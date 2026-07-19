using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.통관;

namespace 살뜰.Infrastructure.Persistence.Configurations.Customs;

public sealed class 통관수임Configuration : IEntityTypeConfiguration<통관수임>
{
    public void Configure(EntityTypeBuilder<통관수임> builder)
    {
        builder.Property(x => x.상태).HasConversion<int>();
        builder.HasIndex(x => new { x.통관절차Id, x.관세사참여자Id, x.상태 });
    }
}
