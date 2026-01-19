using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TowerDefense.Common.Models.Tower;

namespace TowerDefense.Common.Models.Player
{
    public class Formation
    {
        public int IdPlayer { get; set; }

        [Required]
        public int SlotIndex { get; set; }

        [Required]
        public int IdTower { get; set; }

        //-----
        [NotMapped]
        public Player Player { get; set; }
        [NotMapped]
        public Ttower Tower { get; set; }
    }
}
