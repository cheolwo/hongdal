using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class HrEmploymentContractRecordConfiguration : IEntityTypeConfiguration<HrEmploymentContractRecord>
{
    public void Configure(EntityTypeBuilder<HrEmploymentContractRecord> builder)
    {
        builder.HasIndex(x => new { x.WorkerUserId, x.EmployerScopeType, x.EmployerScopeId, x.ContractStatus });
    }
}
