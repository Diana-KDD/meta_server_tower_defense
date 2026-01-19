using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TowerDefense.Common.Models.Tower;

namespace TowerDefense.Common.Models.Player
{
    public class Inventory
    {
        public int IdPlayer { get; set; }
        [Required]
        public int IdTower { get; set; }
        public int Quantity { get; set; }

        //-----
        [NotMapped]
        public Player Player { get; set; }
        [NotMapped]
        public Ttower Tower { get; set; }
    }
}
