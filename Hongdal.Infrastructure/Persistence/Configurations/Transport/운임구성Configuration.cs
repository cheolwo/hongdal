using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.운송;

namespace 홍달.Infrastructure.Persistence.Configurations.Transport;

public sealed class 운임구성Configuration : IEntityTypeConfiguration<운임구성>
{
    public void Configure(EntityTypeBuilder<운임구성> builder)
    {
        builder.ToTable("운임구성");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.의뢰Id).HasColumnName("request_id").IsRequired();
        builder.Property(x => x.기본운임).HasColumnName("기본운임");
        builder.Property(x => x.거리운임).HasColumnName("거리운임");
        builder.Property(x => x.할증).HasColumnName("할증");
        builder.Property(x => x.대기료).HasColumnName("대기료");
        builder.Property(x => x.수작업비).HasColumnName("수작업비");
        builder.Property(x => x.최종운임).HasColumnName("최종운임");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
