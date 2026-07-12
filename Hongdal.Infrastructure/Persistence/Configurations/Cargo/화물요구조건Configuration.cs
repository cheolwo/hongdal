using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.화물;

namespace 홍달.Infrastructure.Persistence.Configurations.Cargo;

public sealed class 화물요구조건Configuration : IEntityTypeConfiguration<화물요구조건>
{
    public void Configure(EntityTypeBuilder<화물요구조건> builder)
    {
        builder.ToTable("화물요구조건");

        builder.HasKey(x => x.의뢰Id);

        builder.Property(x => x.의뢰Id).HasColumnName("의뢰Id");
        builder.Property(x => x.화물길이Mm).HasColumnName("화물길이Mm");
        builder.Property(x => x.화물폭Mm).HasColumnName("화물폭Mm");
        builder.Property(x => x.화물높이Mm).HasColumnName("화물높이Mm");
        builder.Property(x => x.화물무게Kg).HasColumnName("화물무게Kg");
        builder.Property(x => x.팔레트개수).HasColumnName("팔레트개수");
        builder.Property(x => x.비맞으면안됨).HasColumnName("비맞으면안됨");
        builder.Property(x => x.냉장필요).HasColumnName("냉장필요");
        builder.Property(x => x.냉동필요).HasColumnName("냉동필요");
        builder.Property(x => x.리프트필요).HasColumnName("리프트필요");
        builder.Property(x => x.측면상하차필요).HasColumnName("측면상하차필요");
        builder.Property(x => x.장재물).HasColumnName("장재물");
        builder.Property(x => x.혼적허용).HasColumnName("혼적허용");
        builder.Property(x => x.독차필수).HasColumnName("독차필수");
        builder.Property(x => x.주의사항).HasColumnName("주의사항").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
