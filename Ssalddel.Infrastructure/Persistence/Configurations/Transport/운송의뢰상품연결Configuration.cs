using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.운송;

namespace 살뜰.Infrastructure.Persistence.Configurations.Transport;

public sealed class 운송의뢰상품연결Configuration : IEntityTypeConfiguration<운송의뢰상품연결>
{
    public void Configure(EntityTypeBuilder<운송의뢰상품연결> builder)
    {
        builder.ToTable("운송의뢰상품연결");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.운송의뢰Id).HasColumnName("운송의뢰_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.입고상품Id).HasColumnName("입고상품_id");
        builder.Property(x => x.할당수량).HasColumnName("할당수량");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => new { x.운송의뢰Id, x.입고상품Id });
    }
}
