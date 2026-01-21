using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaderboardController: ControllerBase
    {
        private readonly GameDBContext _context;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(GameDBContext context, ILogger<LeaderboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetLeaderboard()
        {
            var playerStatisticList = await _context.PlayerStatistics.ToListAsync();
            if(playerStatisticList.Count <= 0)
            {
                return BadRequest(new
                {
                    Message = "Ошибка получения статистик игроков"
                });
            }

            var sortedPlayerStatisticList = playerStatisticList.OrderByDescending(p=>p.Rating).ToList();
            List<LeaderboardInfo> result = new();

            foreach (var playerStatistic in sortedPlayerStatisticList)
            {
                var player = await _context.Players.FindAsync(playerStatistic.IdPlayer);
                if(player == null)
                {
                    return BadRequest(new
                    {
                        Message = "Ошибка получения игрока"
                    });
                }
                result.Add(new LeaderboardInfo
                {
                    Username = player!.Username,
                    Rating = playerStatistic.Rating,
                    TotalMatches = playerStatistic.TotalMatches,
                    Wins = playerStatistic.Wins,
                    Losses = playerStatistic.Losses
                });
            }
            _logger.LogInformation("The leaderboard was successfully received");
            return Ok(new { result });
        }

    }
}
