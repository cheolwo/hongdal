using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.기사;

namespace 홍달.Infrastructure.Persistence.Configurations.Driver;

public sealed class 기사위치기록Configuration : IEntityTypeConfiguration<기사위치기록>
{
    public void Configure(EntityTypeBuilder<기사위치기록> builder)
    {
        builder.ToTable("driver_location_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.기사Id).HasColumnName("driver_id").IsRequired();
        builder.Property(x => x.위도).HasColumnName("latitude");
        builder.Property(x => x.경도).HasColumnName("longitude");
        builder.Property(x => x.정확도_m).HasColumnName("accuracy_m");
        builder.Property(x => x.기록시각).HasColumnName("recorded_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
