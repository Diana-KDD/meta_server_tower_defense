using Microsoft.EntityFrameworkCore;
using TowerDefense.Common.Models.Player;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TowerDefense.Common.Configuration
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.HasIndex(p => p.Username)
                   .IsUnique();

            builder.HasIndex(p => p.Email)
                   .IsUnique();
        }
    }
}
