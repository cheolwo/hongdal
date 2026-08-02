using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.Content;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Content;

public sealed class 앱문맥이미지자산Configuration : IEntityTypeConfiguration<앱문맥이미지자산>
{
    public void Configure(EntityTypeBuilder<앱문맥이미지자산> builder)
    {
        builder.ToTable("app_context_image_assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.장면Key).HasColumnName("scene_key").HasMaxLength(120).IsRequired();
        builder.Property(x => x.앱PackId).HasColumnName("app_pack_id").HasMaxLength(80).IsRequired();
        builder.Property(x => x.장면번호).HasColumnName("scene_number");
        builder.Property(x => x.PromptVersion).HasColumnName("prompt_version");
        builder.Property(x => x.제목).HasColumnName("title").HasMaxLength(240).IsRequired();
        builder.Property(x => x.대체Text).HasColumnName("alt_text").HasMaxLength(500).IsRequired();
        builder.Property(x => x.이미지Url).HasColumnName("image_url").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.StorageContainer).HasColumnName("storage_container").HasMaxLength(160).IsRequired();
        builder.Property(x => x.StorageObjectName).HasColumnName("storage_object_name").HasMaxLength(700).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(80).IsRequired();
        builder.Property(x => x.화면비율).HasColumnName("aspect_ratio").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Sha256).HasColumnName("sha256").HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.RouteRefsJson).HasColumnName("route_refs_json").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.품질상태).HasColumnName("quality_status");
        builder.Property(x => x.활성화여부).HasColumnName("is_active");
        builder.Property(x => x.생성시각).HasColumnName("created_at");
        builder.Property(x => x.수정시각).HasColumnName("updated_at");
        builder.HasIndex(x => x.장면Key).IsUnique();
        builder.HasIndex(x => new { x.앱PackId, x.활성화여부, x.장면번호 });
    }
}
