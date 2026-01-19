using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TowerDefense.Common.Models.Player;
using TowerDefense.Server.Data;

namespace TowerDefense.Server.Services
{
    public interface IJwtTokenService
    {
        string GenerateRefreshToken();
        string GenerateJwtToken(Player player, PlayerProfile playerProfile);
        JwtSecurityToken ValidateTokenWithoutLifetime(string token);
    }
    public class JwtTokenService: IJwtTokenService
    {
        public readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IConfiguration _configuration;
        private readonly GameDBContext _context;

        public JwtTokenService(IConfiguration configuration, GameDBContext context)
        {
            _configuration = configuration;
            _context = context;

            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtConfig:Issuer"],
                ValidAudience = _configuration["JwtConfig:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rgn = RandomNumberGenerator.Create();
            rgn.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber)
                .Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        public string GenerateJwtToken(Player player, PlayerProfile playerProfile)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!));

            string playersRoles = "";
            List<int> idRolesList = _context.PlayerRoles
                .Where(pr => pr.PlayerId == player.Id).Select(pr => pr.RoleId).ToList();
            for(int i = 0; i < idRolesList.Count; i++)
            {
                playersRoles += _context.Roles.FirstOrDefault(r => r.Id == idRolesList[i])!.RoleName;
                if(i < idRolesList.Count - 1)
                {
                    playersRoles += ", ";
                }
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                new Claim(ClaimTypes.Name, player.Username),
                new Claim(ClaimTypes.Email, player.Email),
                new Claim("rating", playerProfile.Rating.ToString()),
                new Claim("level", playerProfile.Level.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, playersRoles),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var experationHours = _configuration.GetValue<int>("JwtConfig:ExpirationInHour", 1);

            var token = new JwtSecurityToken
                (
                    issuer: _configuration["JwtConfig:Issuer"],
                    audience: _configuration["JwtConfig:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(experationHours),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtSecurityToken ValidateTokenWithoutLifetime(string token)
        {
            Console.WriteLine(token);
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JwtConfig:Issuer"],
                    ValidAudience = _configuration["JwtConfig:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                tokenHandler.ValidateToken(token, parameters, out var validatedToken);
                return validatedToken as JwtSecurityToken;
            }
            catch
            {
                return null;
            }
        }

    }
}
