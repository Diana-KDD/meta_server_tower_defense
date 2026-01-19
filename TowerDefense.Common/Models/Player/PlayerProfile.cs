using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TowerDefense.Common.Models.Player
{
    public class PlayerProfile
    {
        [Key]
        public int IdPlayer { get; set; }

        public string? AvatarUrl { get; set; }
        public int Rating { get; set; } = 1000;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;

        //---------
        [NotMapped]
        public Player Player { get; set; }
    }
}
