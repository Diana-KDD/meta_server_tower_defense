using System.ComponentModel.DataAnnotations;

namespace TowerDefense.Common.Models.DTO
{
    public class LoginRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiry { get; set; }
        public PlayerInfo Player { get; set; }
    }

    public class PlayerInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class ResultMatchInfo
    {
        [Required]
        public int IdPlayer_1 {  get; set; }
        [Required]
        public int IdPlayer_2 {  get; set; }
        [Required]
        public int IdPlayer_winner { get; set; }
    }

    public class LeaderboardInfo
    {
        [Required]
        public string Username { get; set; }
        public int Rating { get; set; }
        public int TotalMatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }

}
