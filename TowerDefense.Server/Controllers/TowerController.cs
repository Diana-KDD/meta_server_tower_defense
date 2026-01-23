using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Common.Models.Tower;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Controllers
{
    [ ApiController ]
    [Route("api/[controller]")]
    [Authorize]
    public class TowerController:ControllerBase
    {
        private readonly GameDBContext _context;
        private readonly ILogger<TowerController> _logger;

        public TowerController(GameDBContext context, ILogger<TowerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("Add")]
        [Authorize(Policy = "AdminCreate")]
        public async Task<IActionResult> AddTower([FromBody] TowerInfo request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Неккоректные данные"
                });
            }

            await _context.Towers.FirstOrDefaultAsync(t => t.TowerName == request.TowerName);

            if (request == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Башня с таким именем уже существует"
                });
            }
            await _context.Towers.AddAsync(new Ttower
            {
                TowerName = request.TowerName,
                Description = request.Description
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation($"The tower - {request.TowerName} has been added to the table");

            return Ok(new
            {
                Success = true,
                result = request
            });
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetTowers()
        {
            var TowersList = await _context.Towers.Select(t => new {t.TowerName, t.Description}).ToListAsync();

            if(TowersList.Count == 0)
            {
                return NotFound("Список башен пуст");
            }

            string UserNamme = User.FindFirst(ClaimTypes.Name)?.Value!;
            _logger.LogInformation($"The user - {UserNamme} looked at the list of towers");
            return Ok(TowersList);
        }
    }
}


