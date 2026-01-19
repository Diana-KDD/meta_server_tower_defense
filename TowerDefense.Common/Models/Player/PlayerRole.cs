using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Common.Models.Access;

namespace TowerDefense.Common.Models.Player
{
    public class PlayerRole
    {
        public int PlayerId {  get; set; }
        public int RoleId { get; set; }

        //-----
        public Player Player { get; set; }
        public Role Role { get; set; }
    }
}
