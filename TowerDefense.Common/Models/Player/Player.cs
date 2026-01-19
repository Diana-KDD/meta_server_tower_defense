using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TowerDefense.Common.Models.Access;

namespace TowerDefense.Common.Models.Player
{
    public class Player
    {
        //-- Общие
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        //-- Сессия
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;
        public int LoginCount { get; set; } = 0;

        //-- Блокировка
        public bool IsBanned { get; set; } = false;
        public string BanReason { get; set; } = string.Empty;

        //-- Системные
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //-------
        [NotMapped]
        public ICollection<PlayerRole> PlayerRoles { get; set; } = new List<PlayerRole>();
        [NotMapped]
        public PlayerProfile PlayerProfiles { get; set; }
        [NotMapped]
        public PlayerStatistic PlayerStatistics { get; set; }
        [NotMapped]
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        [NotMapped]
        public ICollection<Formation> Formations { get; set; } = new List<Formation>();

    }
}
