using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.탐색캠페인;

namespace 홍달.Infrastructure.Persistence.Configurations.ExplorationCampaign;

public sealed class 기사화주관계집계Configuration : IEntityTypeConfiguration<기사화주관계집계>
{
    public void Configure(EntityTypeBuilder<기사화주관계집계> builder)
    {
        builder.HasIndex(x => new { x.기사Id, x.화주UserId }).IsUnique();
    }
}
