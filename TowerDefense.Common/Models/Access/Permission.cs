using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TowerDefense.Common.Models.Access
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string PermissionName { get; set; }
        //----------
        [NotMapped]
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
