using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

            if(player == null|| profilePlayer == null)
            {
                return NotFound($"Пользователь не найден {userId}");
            }

            _logger.LogInformation($"User: {userId} get profile");

            return Ok(new
            {
                UserName = player.Username,
                AvatarUrl = profilePlayer.AvatarUrl,
                Email = player.Email,
                Level = profilePlayer.Level,
                Experience = profilePlayer.Experience,
            });
        }
    }
}
