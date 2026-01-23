using BCrypt.Net;
using DotNetEnv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TowerDefense.Common.Models.Access;
using TowerDefense.Common.Models.Player;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Services
{
    public class AuthorizationSeedService
    {
        private readonly GameDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenService _tokenService;

        public AuthorizationSeedService(GameDBContext context, 
                                        IConfiguration configuration,
                                        IJwtTokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task SeedAsync()
        {
            var options = _configuration.GetSection("AuthorizationOptions")
                                       .Get<AuthorizationOptions>();
            if (options == null) return;

            //-- Creating or getting roles
            foreach (var RP in options.RolePermissions)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == RP.Role);
                if (role == null)
                {
                    role = new Role { RoleName = RP.Role };
                    await _context.Roles.AddAsync(role);
                    await _context.SaveChangesAsync();
                }

                //-- For each role, we create or receive the right
                foreach (var permName in RP.Permissions)
                {
                    var permission = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.PermissionName == permName);

                    if (permission == null)
                    {
                        permission = new Permission { PermissionName = permName };
                        await _context.Permissions.AddAsync(permission);
                        await _context.SaveChangesAsync();
                    }

                    //--  Creating a connection if there is none
                    bool exists = await _context.RolePermissions
                        .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                    if (!exists)
                    {
                        await _context.RolePermissions.AddAsync(
                            new RolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = permission.Id
                            }
                        );
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    
        public async Task SeedAdmin()
        {
            var UserName = Environment.GetEnvironmentVariable("ADMIN_NAME");
            var Email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            List<string> RoleList = new List<string>() { _configuration["Admin:DefaultRole"], "Player" };
            var Password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if(string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Email))
            {
                Console.WriteLine("Admin credentials are missing!");
                return;
            }
            //-- We create an admin if there is none
            var existingPlayer = await _context.Players
                                .FirstOrDefaultAsync(p => p.Username == UserName || p.Email == Email);
            if (existingPlayer != null)
            {
                Console.WriteLine($"Admin already exists: {existingPlayer.Username}");
                return;
            }
            Player player = new Player
            {
                Username = UserName,
                Email = Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
                RefreshToken = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                LoginCount = 1
            };
            await _context.AddAsync(player);
            await _context.SaveChangesAsync();
            await _context.PlayerProfiles.AddAsync(new PlayerProfile { IdPlayer = player.Id });
            await _context.PlayerStatistics.AddAsync(new PlayerStatistic { IdPlayer = player.Id });
            
            foreach(var Role in RoleList)
            {
                var adminRole = await _context.Roles
                            .FirstOrDefaultAsync(r => r.RoleName == Role);

                if (adminRole == null)
                {
                    Console.WriteLine($"Role '{Role}' not found!");
                    return;
                }

                await _context.PlayerRoles.AddAsync(new PlayerRole
                {
                    PlayerId = player.Id,
                    RoleId = adminRole.Id
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}
