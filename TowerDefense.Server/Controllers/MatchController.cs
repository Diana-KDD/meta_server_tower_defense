using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Common.Models.Player;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchController:ControllerBase
    {
        private readonly GameDBContext _context;
        private readonly ILogger<MatchController> _logger;
        public MatchController(GameDBContext context, ILogger<MatchController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPatch("save_result")]
        public async Task<IActionResult> SaveResultMath([FromBody] ResultMatchInfo request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    error = ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage)
                });
            }

            if(request.IdPlayer_1 == request.IdPlayer_2 || 
                (request.IdPlayer_winner != request.IdPlayer_1 
                && request.IdPlayer_winner != request.IdPlayer_2))
            {
                return BadRequest(request);
            }

            PlayerStatistic? playerWinner;
            PlayerStatistic? playerLoser;

            if (request.IdPlayer_1 == request.IdPlayer_winner)
            {
                playerWinner = await _context.PlayerStatistics.FindAsync(request.IdPlayer_1);
                playerLoser = await _context.PlayerStatistics.FindAsync(request.IdPlayer_2);
                
            }
            else
            {
                playerWinner = await _context.PlayerStatistics.FindAsync(request.IdPlayer_2);
                playerLoser = await _context.PlayerStatistics.FindAsync(request.IdPlayer_1);
            }

            if (playerWinner != null && playerLoser != null)
            {
                playerWinner.TotalMatches += 1;
                playerWinner.Wins += 1;

                playerLoser.TotalMatches += 1;
                playerLoser.Losses += 1;

                var listRating = CalculatingTheRating(playerWinner, playerLoser);

                playerWinner.Rating = listRating[0];
                playerLoser.Rating = listRating[1];

                await _context.SaveChangesAsync();
                _logger.LogInformation($"the results of the match between {request.IdPlayer_1} and {request.IdPlayer_2} were saved");
                return Ok(new
                {
                    Success = true,
                    Message = "Результаты успешно сохранены"
                });
            }
            return BadRequest(new
            {
                Success = false,
                Message = "Ошибка получения статистик игроков"
            });

        }

        private List<int> CalculatingTheRating(PlayerStatistic playerWinner, PlayerStatistic playerLoser)
        {
            var player_w_rating = playerWinner.Rating;
            var player_l_rating = playerLoser.Rating;

            int k = 32;

            int s1 = 1;
            int s2 = 0;

            double e1 = 1.0f / (1 + Math.Pow(10, ((player_w_rating - player_l_rating) / 400)));
            double e2 = 1 - e1;
            Console.WriteLine(e1);
            Console.WriteLine(e2);

            double r1 = k * (s1 - e1);
            double r2 = k * (s2 - e2);

            Console.WriteLine(r1);
            Console.WriteLine(r2);

            return new List<int>() { player_w_rating + (int)r1, player_l_rating + (int)r2 };
        }
    }
}
