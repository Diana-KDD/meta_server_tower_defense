using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Common.Models.Player;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Controllers
{
    [ ApiController ]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController:ControllerBase
    {
        private readonly GameDBContext _context;
        private readonly ILogger<TowerController> _logger;

        public InventoryController(GameDBContext context, ILogger<TowerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetInventory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!int.TryParse(userIdClaim, out var PlayerId))
            {
                return NotFound($"Пользователь не найден {userIdClaim}");
            }

            var inventoryPlayer = await _context.Inventories.Where(i=>i.IdPlayer == PlayerId)
                                                            .Select(i => new {i.IdTower, i.Quantity})
                                                            .ToListAsync();

            if(inventoryPlayer.Count == 0)
            {
                return NotFound($"Инвентарь пуст");
            }

            _logger.LogInformation($"User - Id = {PlayerId} received inventory");

            return Ok(inventoryPlayer);

        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddTowerInInventory([FromBody] TowerAddInInventory request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Неккоректные данные"
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!int.TryParse(userIdClaim, out var PlayerId))
            {
                return NotFound($"Пользователь не найден {userIdClaim}");
            }

            var player = await _context.Players.FindAsync(PlayerId);
            if(player == null)
            {
                return NotFound($"Пользователь не найден {PlayerId}");
            }

            await _context.Inventories.AddAsync(new Inventory
            {
                IdPlayer = PlayerId,
                IdTower = request.IdTower,
                Quantity = request.Quantity,
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation($"To the user - {player.Username} a tower was added to the inventory " +
                $"[id - {request.IdTower} in the amount of {request.Quantity} pcs]");

            return Ok(new
            {
                Success = true,
                Message = $"Пользователю - {player.Username} в инвентарь была " +
                $"добавлена башня [id - {request.IdTower} в количестве {request.Quantity} шт]"
            });
        }


    }
}

