using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TowerDefense.Common.Models.Access;

namespace TowerDefense.Common.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
           builder.HasIndex(r=>r.RoleName)
                  .IsUnique();
        }
    }
}
