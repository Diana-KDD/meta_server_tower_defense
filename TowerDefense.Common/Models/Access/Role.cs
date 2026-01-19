using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TowerDefense.Common.Models.Player;

namespace TowerDefense.Common.Models.Access
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string RoleName { get; set; }

        //----------
        [NotMapped]
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        [NotMapped]
        public ICollection<PlayerRole> PlayerRoles { get; set; } = new List<PlayerRole>();
        [NotMapped]
        public Player.Player Player { get; set; }
    }
}
