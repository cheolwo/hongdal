using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.기사;

namespace 홍달.Infrastructure.Persistence.Configurations.Driver;

public sealed class 기사월정산Configuration : IEntityTypeConfiguration<기사월정산>
{
    public void Configure(EntityTypeBuilder<기사월정산> builder)
    {
        builder.ToTable("기사월정산");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.기사Id).HasColumnName("driver_id").IsRequired();
        builder.Property(x => x.년도).HasColumnName("year");
        builder.Property(x => x.월).HasColumnName("month");
        builder.Property(x => x.배차건수).HasColumnName("dispatch_count");
        builder.Property(x => x.이용료).HasColumnName("usage_fee");
        builder.Property(x => x.결제완료).HasColumnName("is_paid");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
