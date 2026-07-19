using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class HrRoleAssignmentRecordConfiguration : IEntityTypeConfiguration<HrRoleAssignmentRecord>
{
    public void Configure(EntityTypeBuilder<HrRoleAssignmentRecord> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.ScopeType, x.ScopeId, x.RoleCode, x.IsActive });
    }
}
