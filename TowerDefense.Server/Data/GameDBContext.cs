using Microsoft.EntityFrameworkCore;
using TowerDefense.Common.Configuration;
using TowerDefense.Common.Models.Access;
using TowerDefense.Common.Models.Player;
using TowerDefense.Common.Models.Tower;

namespace TowerDefense.Server.Data
{
    public class GameDBContext(DbContextOptions<GameDBContext> options) : DbContext(options)
    {

        public DbSet<Player> Players { get; set; }
        public DbSet<Ttower> Towers { get; set; }
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }
        public DbSet<PlayerStatistic> PlayerStatistics { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Formation> Formations { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<PlayerRole> PlayerRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PlayerConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerProfileConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerStatisticConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryConfiguration());
            modelBuilder.ApplyConfiguration(new FormationConfiguration());
            modelBuilder.ApplyConfiguration(new TowerConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerRoleConfiguration());
        }
    }
}
