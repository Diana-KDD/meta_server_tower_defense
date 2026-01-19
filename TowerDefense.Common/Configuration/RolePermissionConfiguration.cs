using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TowerDefense.Common.Models.Access;

namespace TowerDefense.Common.Configuration
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {

        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasKey(rp => new {rp.RoleId, rp.PermissionId});

            builder.HasOne(rp=>rp.Role)
                   .WithMany(r=>r.RolePermissions)
                   .HasForeignKey(r=>r.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(r => r.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
