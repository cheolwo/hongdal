using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.사용자;

namespace 홍달.Infrastructure.Persistence.Configurations.User;

public sealed class HrEmploymentContractRecordConfiguration : IEntityTypeConfiguration<HrEmploymentContractRecord>
{
    public void Configure(EntityTypeBuilder<HrEmploymentContractRecord> builder)
    {
        builder.HasIndex(x => new { x.WorkerUserId, x.EmployerScopeType, x.EmployerScopeId, x.ContractStatus });
    }
}
