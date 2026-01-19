using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TowerDefense.Common.Models.Player;

namespace TowerDefense.Common.Configuration
{
    public class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
    {
        public void Configure(EntityTypeBuilder<PlayerProfile> builder)
        {

            builder.HasOne(pp => pp.Player)
                   .WithOne(p => p.PlayerProfiles)
                   .HasForeignKey<PlayerProfile>(pp => pp.IdPlayer)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
