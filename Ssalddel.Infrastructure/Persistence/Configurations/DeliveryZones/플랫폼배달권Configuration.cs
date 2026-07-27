using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.배달권;

namespace 살뜰.Infrastructure.Persistence.Configurations.DeliveryZones;

public sealed class 플랫폼배달권Configuration : IEntityTypeConfiguration<플랫폼배달권>
{
    public void Configure(EntityTypeBuilder<플랫폼배달권> builder)
    {
        builder.ToTable("플랫폼배달권");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.배달권키).HasColumnName("delivery_zone_key").HasMaxLength(120).IsRequired();
        builder.Property(x => x.배달권명).HasColumnName("delivery_zone_name").HasMaxLength(160).IsRequired();
        builder.Property(x => x.판정방식).HasColumnName("classification_method").HasMaxLength(40).IsRequired();
        builder.Property(x => x.법정동코드).HasColumnName("legal_district_code").HasMaxLength(20);
        builder.Property(x => x.시도명).HasColumnName("province_name").HasMaxLength(80);
        builder.Property(x => x.시군구명).HasColumnName("district_name").HasMaxLength(80);
        builder.Property(x => x.대표건물명).HasColumnName("representative_building_name").HasMaxLength(160);
        builder.Property(x => x.대표건물주소).HasColumnName("representative_building_address").HasMaxLength(300);
        builder.Property(x => x.대표위도).HasColumnName("representative_latitude").HasPrecision(10, 7);
        builder.Property(x => x.대표경도).HasColumnName("representative_longitude").HasPrecision(10, 7);
        builder.Property(x => x.인접배달권키Json).HasColumnName("adjacent_zone_keys_json").HasColumnType("json").IsRequired();
        builder.Property(x => x.활성).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.배달권키)
            .IsUnique()
            .HasDatabaseName("ux_플랫폼배달권_key");
    }
}

public sealed class 원장배달권투영Configuration : IEntityTypeConfiguration<원장배달권투영>
{
    public void Configure(EntityTypeBuilder<원장배달권투영> builder)
    {
        builder.ToTable("원장배달권투영");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.배달권Id).HasColumnName("delivery_zone_id");
        builder.Property(x => x.원장유형코드).HasColumnName("ledger_type_code").HasMaxLength(40).IsRequired();
        builder.Property(x => x.원장Id).HasColumnName("ledger_id").HasMaxLength(120).IsRequired();
        builder.Property(x => x.역할코드).HasColumnName("role_code").HasMaxLength(40).IsRequired();
        builder.Property(x => x.생성근거).HasColumnName("projection_basis").HasMaxLength(120).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(x => x.배달권)
            .WithMany()
            .HasForeignKey(x => x.배달권Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.원장유형코드, x.원장Id, x.역할코드 })
            .IsUnique()
            .HasDatabaseName("ux_원장배달권투영_ledger_role");
        builder.HasIndex(x => x.배달권Id)
            .HasDatabaseName("ix_원장배달권투영_zone");
    }
}
