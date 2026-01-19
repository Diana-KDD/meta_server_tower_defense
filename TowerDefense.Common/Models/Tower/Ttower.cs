using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TowerDefense.Common.Models.Player;

namespace TowerDefense.Common.Models.Tower
{
    public class Ttower
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string TowerName { get; set; }

        public string Description { get; set; }

        //-----
        [NotMapped]
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        [NotMapped]
        public ICollection<Formation> Formations { get; set; } = new List<Formation>();
    }
}
