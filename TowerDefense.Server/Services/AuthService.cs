using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Security.Permissions;
using TowerDefense.Common.Models.Access;
using TowerDefense.Common.Models.DTO;
using TowerDefense.Common.Models.Player;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<AuthResponse> LogoutAsync(int playerId);
    }
    public class AuthService : IAuthService
    {
        public readonly GameDBContext _context;
        public readonly IConfiguration _configuration;
        public readonly ILogger<AuthService> _logger;
        public readonly IJwtTokenService _jwtTokenService;
        public AuthService(GameDBContext context, IConfiguration configuration,
                           ILogger<AuthService> logger, IJwtTokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _jwtTokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid(request))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Некорректные данные"
                    };
                }
                if (request.Password != request.ConfirmPassword)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Пароли не совпадают"
                    };
                }
                if (await _context.Players.AnyAsync(p => p.Username == request.Username))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Имя пользователя уже занято"
                    };
                }
                if (string.IsNullOrEmpty(request.Email))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Введите Email"
                    };
                }
                if (await _context.Players.AnyAsync(p => p.Email == request.Email))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email уже зарегистрирован"
                    };
                }

                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var player = new Player
                {
                    Username = request.Username.Trim(),
                    Email = request.Email.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    LoginCount = 1
                };

                await _context.Players.AddAsync(player);
                await _context.SaveChangesAsync();

                PlayerProfile playerProfile = new()
                {
                    IdPlayer = player.Id
                };
                PlayerStatistic playerStatistic = new()
                {
                    IdPlayer = player.Id
                };
                PlayerRole playerRole = new()
                {
                    PlayerId = player.Id,
                    RoleId = _context.Roles.FirstOrDefault(r => r.RoleName == "Player")!.Id
                };

                await _context.PlayerProfiles.AddAsync(playerProfile);
                await _context.PlayerStatistics.AddAsync(playerStatistic);
                await _context.PlayerRoles.AddAsync(playerRole);
                await _context.SaveChangesAsync();

                PlayerInfoClaim playerInfoClaim = await MapToPlayerInfoClaim(player, playerProfile, playerStatistic);

                var token = _jwtTokenService.GenerateJwtToken(playerInfoClaim);

                _logger.LogInformation("New player registered: {Username} (ID: {Id})", player.Username, player.Id);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Регистрация успешна",
                    Token = token.Result,
                    RefreshToken = refreshToken,
                    TokenExpiry = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("JwtConfig:ExpirationInHours", 1)),
                    Player = MapToPlayerInfo(player)
                };
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error during registration for {Username}", request.Username);

                if (dbEx.InnerException?.Message?.Contains("RefreshToken") == true &&
                    dbEx.InnerException.Message.Contains("NULL"))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Ошибка конфигурации базы данных"
                    };
                }

                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка при сохранении данных"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Username}", request.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка при регистрации"
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid(request))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Некорректные данные"
                    };
                }

                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Username == request.Username ||
                                            p.Email == request.Email);
                if (player == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Неверное имя пользователя или пароль"
                    };
                }
                if (player.IsBanned)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Аккаунт заблокирован. Причина: {player.BanReason}"
                    };
                }
                if (!BCrypt.Net.BCrypt.Verify(request.Password, player.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Неверное имя пользователя или пароль"
                    };
                }
                
                player.LastLogin = DateTime.UtcNow;
                player.LoginCount++;
                player.UpdatedAt = DateTime.UtcNow;

                PlayerProfile playerProfile = await _context.PlayerProfiles.FindAsync(player.Id);
                PlayerStatistic playerStatistic = await _context.PlayerStatistics.FindAsync(player.Id);

                PlayerInfoClaim playerInfoClaim = await MapToPlayerInfoClaim(player, playerProfile, playerStatistic);

                var token = _jwtTokenService.GenerateJwtToken(playerInfoClaim);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                player.RefreshToken = refreshToken;
                player.RefreshTokenExpiry = request.RememberMe
                    ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Player logger in: {UserName} (ID: {Id})", player.Username, player.Id);
                return new AuthResponse
                {
                    Success = true,
                    Message = "Вход выполнен успешно",
                    Token = token.Result,
                    RefreshToken = refreshToken,
                    TokenExpiry = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("JwtConfig:ExpirationInHours", 1)),
                    Player = MapToPlayerInfo(player)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {UserName}", request.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка при входе в систему"
                };
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Refresh Token обязателен"
                    };
                }

                var jwtToken = _jwtTokenService.ValidateTokenWithoutLifetime(request.Token);
                if (jwtToken == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Недействительный токен"
                    };
                }
                var playerIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(playerIdClaim, out var playerId))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Недействительный токен"
                    };
                }

                var player = await _context.Players.FindAsync(playerId);
                if (player == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Пользователь не найден"
                    };
                }
                if (player.RefreshToken != request.RefreshToken)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Недействительный Refresh Token"
                    };
                }
                if (player.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Refresh Token истек"
                    };
                }

                PlayerProfile playerProfile = await _context.PlayerProfiles.FindAsync(player.Id);
                PlayerStatistic playerStatistic = await _context.PlayerStatistics.FindAsync(player.Id);
                PlayerInfoClaim playerInfoClaim = await MapToPlayerInfoClaim(player, playerProfile, playerStatistic);

                var newToken = _jwtTokenService.GenerateJwtToken(playerInfoClaim);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                player.RefreshToken = newRefreshToken;
                player.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                player.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Token refreshed for player ID: {Id}", playerId);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Токен обновлен",
                    Token = newToken.Result,
                    RefreshToken = newRefreshToken,
                    TokenExpiry = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("JwtConfig:ExpirationInHours", 1)),
                    Player = MapToPlayerInfo(player)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка обновления токена"
                };
            }
        }

        private PlayerInfo MapToPlayerInfo(Player player)
        {
            return new PlayerInfo
            {
                Id = player.Id,
                Username = player.Username,
                Email = player.Email,
                CreatedAt = player.CreatedAt,
                LastLogin = player.LastLogin
            };
        }

        private async Task<PlayerInfoClaim> MapToPlayerInfoClaim(Player player, 
            PlayerProfile playerProfile, PlayerStatistic playerStatistic)
        {
            List<string> rolesName = await _context.PlayerRoles
                                             .Where(pr => pr.PlayerId == player.Id)
                                             .Join(_context.Roles,
                                             pr => pr.RoleId,
                                             r => r.Id,
                                             (pr, r) => r.RoleName)
                                             .ToListAsync();
            List<string> permissionsName = await _context.PlayerRoles
                                         .Where(pr => pr.PlayerId == player.Id)
                                         .Join(_context.RolePermissions,
                                                 pr => pr.RoleId,
                                                 rp => rp.RoleId,
                                                 (pr, rp) => rp.PermissionId)
                                         .Join(_context.Permissions,
                                               perId => perId,
                                               p => p.Id,
                                               (perId, p) => p.PermissionName)
                                         .Distinct()
                                         .ToListAsync();

            return new PlayerInfoClaim
            {
                Id = player.Id,
                Username = player.Username,
                Email = player.Email,
                Level = playerProfile.Level,
                Experience = playerProfile.Experience,
                TotalMatches = playerStatistic.TotalMatches,
                Wins = playerStatistic.Wins,
                Rating = playerStatistic.Rating,
                Roles = rolesName,
                Permissions = permissionsName
            };
        }

        public async Task<AuthResponse> LogoutAsync(int playerId)
        {
            try
            {
                var player = await _context.Players.FindAsync(playerId);
                if (player == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Ошибка при выходе (Пользователь не найден)"
                    };
                }
                player.RefreshToken = _jwtTokenService.GenerateRefreshToken();
                player.RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1);
                player.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Player logged out: ID: {Id}", playerId);
                return new AuthResponse
                {
                    Success = true,
                    Message = "Выход выполнен успешно"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed for player ID: {Id}", playerId);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка при выходе"
                };
            }
        }
    }

    public static class ModelState
    {
        public static bool IsValid(object model)
        {
            var context = new ValidationContext(model);
            var result = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, result, true);
        }
    }

}
