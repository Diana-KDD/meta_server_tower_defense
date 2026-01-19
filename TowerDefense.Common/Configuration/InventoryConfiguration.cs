using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TowerDefense.Common.Models.Player;

namespace TowerDefense.Common.Configuration
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.HasKey(i => new { i.IdPlayer, i.IdTower });

            builder.ToTable(t => t.HasCheckConstraint("Ck_Inventory_Quantity", "\"Quantity\" >= 0"));

            builder.HasOne(i=>i.Player)
                   .WithMany(p=>p.Inventories)
                   .HasForeignKey(i => i.IdPlayer)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Tower)
                   .WithMany(t => t.Inventories)
                   .HasForeignKey(i => i.IdTower)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
