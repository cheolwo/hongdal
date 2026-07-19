using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.통관;

namespace 살뜰.Infrastructure.Persistence.Configurations.Customs;

public sealed class 관세사프로필Configuration : IEntityTypeConfiguration<관세사프로필>
{
    public void Configure(EntityTypeBuilder<관세사프로필> builder)
    {
        builder.HasIndex(x => x.참여자Id).IsUnique();
        builder.HasIndex(x => new { x.관리자승인여부, x.수임가능여부 });
    }
}
