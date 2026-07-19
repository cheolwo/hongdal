using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.공통;

namespace 살뜰.Infrastructure.Persistence.Configurations.Common;

public sealed class 생성이미지작업Configuration : IEntityTypeConfiguration<생성이미지작업>
{
    public void Configure(EntityTypeBuilder<생성이미지작업> builder)
    {
        builder.HasIndex(x => x.작업코드).IsUnique();
        builder.HasIndex(x => x.중복방지키);
        builder.HasIndex(x => new { x.이미지용도, x.대상타입, x.대상식별자, x.상태 });
        builder.HasIndex(x => x.외부TaskId);
    }
}
