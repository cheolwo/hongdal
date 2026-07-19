using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.설정;

namespace 살뜰.Infrastructure.Persistence.Configurations.Settings;

public sealed class 사용자Command기능설정Configuration : IEntityTypeConfiguration<사용자Command기능설정>
{
    public void Configure(EntityTypeBuilder<사용자Command기능설정> builder)
    {
        builder.HasIndex(x => new { x.사용자Id, x.CommandName, x.FeatureName }).IsUnique();
    }
}
