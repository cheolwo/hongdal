using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.운송;

namespace 홍달.Infrastructure.Persistence.Configurations.Transport;

public sealed class 차량단가Configuration : IEntityTypeConfiguration<차량단가>
{
    public void Configure(EntityTypeBuilder<차량단가> builder)
    {
        builder.ToTable("차량단가");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.차량종류).HasColumnName("차량종류").IsRequired();
        builder.Property(x => x.기본운임).HasColumnName("기본운임");
        builder.Property(x => x.Km당단가).HasColumnName("Km당단가");
        builder.Property(x => x.야간할증).HasColumnName("야간할증");
        builder.Property(x => x.우천할증).HasColumnName("우천할증");
        builder.Property(x => x.최소운임).HasColumnName("최소운임");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
