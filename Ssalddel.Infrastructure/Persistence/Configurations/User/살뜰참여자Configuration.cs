using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class 살뜰참여자Configuration : IEntityTypeConfiguration<살뜰참여자>
{
    public void Configure(EntityTypeBuilder<살뜰참여자> builder)
    {
        builder.HasIndex(x => x.활성화여부);
    }
}
