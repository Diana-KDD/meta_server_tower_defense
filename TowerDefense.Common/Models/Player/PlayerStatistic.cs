using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TowerDefense.Common.Models.Player
{
    public class PlayerStatistic
    {
        [Key]
        public int IdPlayer { get; set; }

        public int TotalMatches { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Rating { get; set; } = 1000;


        //---------
        [NotMapped]
        public Player Player { get; set; }
    }
}
