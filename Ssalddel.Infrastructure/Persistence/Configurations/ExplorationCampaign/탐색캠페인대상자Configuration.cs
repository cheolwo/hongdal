using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.탐색캠페인;

namespace 살뜰.Infrastructure.Persistence.Configurations.ExplorationCampaign;

public sealed class 탐색캠페인대상자Configuration : IEntityTypeConfiguration<탐색캠페인대상자>
{
    public void Configure(EntityTypeBuilder<탐색캠페인대상자> builder)
    {
        builder.HasIndex(x => new { x.탐색캠페인Id, x.대상UserId }).IsUnique();
    }
}
