using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Contracts.Common.Transport;
using 살뜰.도메인.기사;

namespace 살뜰.Infrastructure.Persistence.Configurations.Driver;

public sealed class 기사근무Configuration : IEntityTypeConfiguration<기사근무>
{
    public void Configure(EntityTypeBuilder<기사근무> builder)
    {
        builder.ToTable("driver_shifts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.기사Id).HasColumnName("driver_id").IsRequired();
        builder.Property(x => x.시작모드).HasColumnName("start_mode").IsRequired();
        builder.Property(x => x.시작시각).HasColumnName("started_at");
        builder.Property(x => x.시작위치).HasColumnName("start_location").IsRequired();
        builder.Property(x => x.운송실행유형)
            .HasColumnName("transport_execution_type")
            .HasMaxLength(32)
            .HasDefaultValue(운송실행유형코드.화물운송)
            .IsRequired();
        builder.Property(x => x.복귀지).HasColumnName("return_destination");
        builder.Property(x => x.오늘의복귀지주소).HasColumnName("today_return_destination");
        builder.Property(x => x.오늘의복귀지위도).HasColumnName("today_return_latitude");
        builder.Property(x => x.오늘의복귀지경도).HasColumnName("today_return_longitude");
        builder.Property(x => x.복귀지출처).HasColumnName("return_destination_source").IsRequired();
        builder.Property(x => x.복귀지입력일시).HasColumnName("return_destination_recorded_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
