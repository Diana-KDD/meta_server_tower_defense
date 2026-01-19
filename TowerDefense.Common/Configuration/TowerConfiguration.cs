using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TowerDefense.Common.Models.Tower;

namespace TowerDefense.Common.Configuration
{
    public class TowerConfiguration : IEntityTypeConfiguration<Ttower>
    {
        public void Configure(EntityTypeBuilder<Ttower> builder)
        {
            builder.HasIndex(t=>t.TowerName)
                   .IsUnique();
        }
    }
}
