using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.운송;

namespace 살뜰.Infrastructure.Persistence.Configurations.Transport;

public sealed class 운송이벤트Configuration : IEntityTypeConfiguration<운송이벤트>
{
    public void Configure(EntityTypeBuilder<운송이벤트> builder)
    {
        builder.ToTable("운송이벤트");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.의뢰Id).HasColumnName("request_id").IsRequired();
        builder.Property(x => x.이벤트타입).HasColumnName("event_type").IsRequired();
        builder.Property(x => x.이벤트시각).HasColumnName("event_time");
        builder.Property(x => x.메타데이터).HasColumnName("metadata").IsRequired();
    }
}
