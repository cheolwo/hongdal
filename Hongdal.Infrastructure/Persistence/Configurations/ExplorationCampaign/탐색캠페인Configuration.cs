using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.탐색캠페인;

namespace 홍달.Infrastructure.Persistence.Configurations.ExplorationCampaign;

public sealed class 탐색캠페인Configuration : IEntityTypeConfiguration<탐색캠페인>
{
    public void Configure(EntityTypeBuilder<탐색캠페인> builder)
    {
        builder.HasIndex(x => new { x.개시자UserId, x.운행예정일, x.탐색상태 });
    }
}
