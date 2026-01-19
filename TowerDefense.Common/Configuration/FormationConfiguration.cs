using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TowerDefense.Common.Models.Player;

namespace TowerDefense.Common.Configuration
{
    public class FormationConfiguration : IEntityTypeConfiguration<Formation>
    {
        public void Configure(EntityTypeBuilder<Formation> builder)
        {
            builder.HasKey(f => new { f.IdPlayer, f.SlotIndex });

            builder.HasIndex(f => new { f.IdPlayer, f.SlotIndex, f.IdTower })
                   .IsUnique();

            builder.HasOne(f => f.Player)
                  .WithMany(p => p.Formations)
                  .HasForeignKey(f => f.IdPlayer)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Tower)
                   .WithMany(t => t.Formations)
                   .HasForeignKey(f => f.IdTower)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
