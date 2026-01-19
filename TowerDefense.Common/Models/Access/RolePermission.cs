using System.ComponentModel.DataAnnotations.Schema;

namespace TowerDefense.Common.Models.Access
{
    public class RolePermission
    {
        public int RoleId {  get; set; }
        public int PermissionId {  get; set; }

        //-------
        [NotMapped]
        public Role Role { get; set; }
        [NotMapped]
        public Permission Permission { get; set; }
    }
}
