using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Server.Data;
using TowerDefense.Server.Services;

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
                return NotFound($"Пользователь не найден {userId}");
            }

            var player = await _context.Players.FindAsync(userId);
            var profilePlayer = await _context.PlayerProfiles.FindAsync(userId);
            _logger.LogInformation($"User: {userId} get profile");
            return Ok(new
            {
                UserName = player.Username,
                AvatarUrl = profilePlayer.AvatarUrl,
                Email = player.Email,
                Level = profilePlayer.Level,
                Experience = profilePlayer.Experience,
                Rating = profilePlayer.Rating
            });
        }

        //[HttpGet("debug")]
        //public IActionResult DebugClaims()
        //{
        //    var claims = User.Claims.Select(c => new
        //    {
        //        Type = c.Type,
        //        Value = c.Value,
        //        ValueType = c.ValueType
        //    }).ToList();

        //    return Ok(claims);
        //}
    }
}
