using Ssalddel.Domain.Speech;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Speech;

public sealed class Typecast음성Configuration : IEntityTypeConfiguration<Typecast음성>
{
    public void Configure(EntityTypeBuilder<Typecast음성> builder)
    {
        builder.ToTable("typecast_voices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VoiceId).HasColumnName("voice_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.이름).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.성별).HasColumnName("gender").HasMaxLength(30).IsRequired();
        builder.Property(x => x.연령대).HasColumnName("age_group").HasMaxLength(30).IsRequired();
        builder.Property(x => x.음성유형).HasColumnName("voice_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.활성화여부).HasColumnName("is_active");
        builder.Property(x => x.마지막동기화일시Utc).HasColumnName("last_synced_at_utc");
        builder.Property(x => x.생성일시Utc).HasColumnName("created_at_utc");
        builder.Property(x => x.수정일시Utc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.VoiceId).IsUnique();
        builder.HasIndex(x => new { x.활성화여부, x.음성유형, x.성별, x.연령대 });
    }
}

public sealed class Typecast음성모델Configuration : IEntityTypeConfiguration<Typecast음성모델>
{
    public void Configure(EntityTypeBuilder<Typecast음성모델> builder)
    {
        builder.ToTable("typecast_voice_models");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Typecast음성Id).HasColumnName("typecast_voice_id");
        builder.Property(x => x.버전).HasColumnName("model_version").HasMaxLength(30).IsRequired();
        builder.Property(x => x.지원감정Json).HasColumnName("emotions_json").HasColumnType("text").IsRequired();

        builder.HasOne(x => x.Typecast음성)
            .WithMany(x => x.지원모델)
            .HasForeignKey(x => x.Typecast음성Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Typecast음성Id, x.버전 }).IsUnique();
        builder.HasIndex(x => x.버전);
    }
}

public sealed class Typecast음성용도Configuration : IEntityTypeConfiguration<Typecast음성용도>
{
    public void Configure(EntityTypeBuilder<Typecast음성용도> builder)
    {
        builder.ToTable("typecast_voice_use_cases");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Typecast음성Id).HasColumnName("typecast_voice_id");
        builder.Property(x => x.이름).HasColumnName("name").HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.Typecast음성)
            .WithMany(x => x.용도)
            .HasForeignKey(x => x.Typecast음성Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Typecast음성Id, x.이름 }).IsUnique();
        builder.HasIndex(x => x.이름);
    }
}
