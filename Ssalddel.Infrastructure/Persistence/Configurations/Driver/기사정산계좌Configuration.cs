using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.기사;

namespace 살뜰.Infrastructure.Persistence.Configurations.Driver;

public sealed class 기사정산계좌Configuration : IEntityTypeConfiguration<기사정산계좌>
{
    public void Configure(EntityTypeBuilder<기사정산계좌> builder)
    {
        builder.ToTable("기사정산계좌");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.기사Id).HasColumnName("driver_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.국가코드).HasColumnName("country_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.은행명).HasColumnName("bank_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.예금주명).HasColumnName("account_holder_name").HasMaxLength(500).IsRequired();
        builder.Property(x => x.계좌번호).HasColumnName("account_number").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.확인상태).HasColumnName("verification_status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.기사Id).IsUnique();
    }
}
