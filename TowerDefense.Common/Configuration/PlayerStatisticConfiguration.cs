using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TowerDefense.Common.Models.Player;

namespace TowerDefense.Common.Configuration
{
    public class PlayerStatisticConfiguration : IEntityTypeConfiguration<PlayerStatistic>
    {
        public void Configure(EntityTypeBuilder<PlayerStatistic> builder)
        {
            builder.HasOne(ps => ps.Player)
                   .WithOne(p => p.PlayerStatistics)
                   .HasForeignKey<PlayerStatistic>(ps => ps.IdPlayer)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
