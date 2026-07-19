using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.공통콘텐츠;

namespace 살뜰.Infrastructure.Persistence.Configurations.Content;

public sealed class 살뜰콘텐츠보상지급Configuration : IEntityTypeConfiguration<살뜰콘텐츠보상지급>
{
    public void Configure(EntityTypeBuilder<살뜰콘텐츠보상지급> builder)
    {
        builder.HasIndex(x => new { x.사용자Id, x.콘텐츠Id }).IsUnique();
        builder.HasIndex(x => new { x.사용자Id, x.결제사용여부, x.지급시각 });
    }
}
