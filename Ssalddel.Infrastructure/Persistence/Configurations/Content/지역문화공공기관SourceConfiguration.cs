using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.Content;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Content;

public sealed class 지역문화공공기관SourceConfiguration
    : IEntityTypeConfiguration<지역문화공공기관Source>
{
    public void Configure(EntityTypeBuilder<지역문화공공기관Source> builder)
    {
        builder.HasIndex(item => new
        {
            item.CountryCode,
            item.JurisdictionLevelCode,
            item.SourceKindCode
        });
        builder.HasIndex(item => new
        {
            item.CountryCode,
            item.IsMachineReadable
        });
    }
}
