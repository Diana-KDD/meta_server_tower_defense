using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Server.Data;
using TowerDefense.Server.Services;

namespace TowerDefense.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController:ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]TowerDefense.Common.Models.DTO.RegisterRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    succes = false,
                    errors = ModelState.Values
                            .SelectMany(v=>v.Errors)
                            .Select(e=>e.ErrorMessage)
                });
            }
            var result = await _authService.RegisterAsync(request);
            if (result.Success)
            {
                _logger.LogInformation($"User registered: {request.Username}");
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]TowerDefense.Common.Models.DTO.LoginRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    succes = false,
                    errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                });
            }

            var result = await _authService.LoginAsync(request);
            if(result.Success)
            {
                _logger.LogInformation($"User logged in: {request.Username}");
                return Ok(result);
            }
            return Unauthorized(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody]RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    succes = false,
                    errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                });
            }

            var result = await _authService.RefreshTokenAsync(request);
            if (result.Success)
            {
                _logger.LogInformation($"UserId refresh tokens in: {result.Player.Id}");
                return Ok(result);
            }
            return Unauthorized(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(int playerId)
        {
            var result = await _authService.LogoutAsync(playerId);
            if(result.Success)
            {
                _logger.LogInformation($"UserId logged out: {playerId}");
                return Ok(result);
            }
            return Unauthorized(result);
        }
    }
}
