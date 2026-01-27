using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly GameDBContext _context;
        private readonly ILogger<ProfileController> _logger;
        public ProfileController(GameDBContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!int.TryParse(userIdClaim, out int userId))
            {
                return NotFound($"Пользователь не найден {userIdClaim}");
            }

            var player = await _context.Players.FindAsync(userId);
            var profilePlayer = await _context.PlayerProfiles.FindAsync(userId);
            var playerStatistic = await _context.PlayerStatistics.FindAsync(userId);

            if(player == null|| profilePlayer == null || playerStatistic == null)
            {
                return NotFound($"Пользователь не найден {userId}");
            }

            ProfileResponse profileResponse = new ProfileResponse()
            {
                Id = player.Id,
                UserName = player.Username,
                Email = player.Email,
                AvatarUrl = profilePlayer.AvatarUrl,
                Role = await _context.PlayerRoles
                            .Where(pr => pr.PlayerId == player.Id)
                            .Join(_context.Roles,
                            pr => pr.RoleId,
                            r => r.Id,
                            (pr, r) => r.RoleName)
                            .ToListAsync(),
                LastLogin = player.LastLogin,
                Level = profilePlayer.Level,
                Experience = profilePlayer.Experience,
                TotalMatches = playerStatistic.TotalMatches,
                Wins = playerStatistic.Wins,
                Losses = playerStatistic.Losses,
                Rating = playerStatistic.Rating
            };

        _logger.LogInformation($"User: {userId} get profile");

            return Ok(profileResponse);
        }
    }
}
