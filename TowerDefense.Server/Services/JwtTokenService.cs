using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TowerDefense.Common.Models.DTO;

namespace TowerDefense.Server.Services
{
    public interface IJwtTokenService
    {
        string GenerateRefreshToken();
        Task<string> GenerateJwtToken(PlayerInfoClaim playerInfoClaim);
        JwtSecurityToken ValidateTokenWithoutLifetime(string token);
    }
    public class JwtTokenService: IJwtTokenService
    {
        public readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
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

        public async Task<string> GenerateJwtToken(PlayerInfoClaim playerInfoClaim)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!));
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, playerInfoClaim.Id.ToString()),
                new Claim(ClaimTypes.Name, playerInfoClaim.Username),
                new Claim(ClaimTypes.Email, playerInfoClaim.Email),
                new Claim("Level", playerInfoClaim.Level.ToString()),
                new Claim("Experience", playerInfoClaim.Experience.ToString()),
                new Claim("Totalmatches", playerInfoClaim.TotalMatches.ToString()),
                new Claim("Wins", playerInfoClaim.Wins.ToString()),
                new Claim("Rating", playerInfoClaim.Rating.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            foreach(var nameRole in playerInfoClaim.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, nameRole));
            }
            foreach (var namePermission in playerInfoClaim.Permissions)
            {
                claims.Add(new Claim("Permission", namePermission));
            }

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
