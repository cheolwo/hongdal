using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Community;

public sealed class 커뮤니티원장블록투영Configuration : IEntityTypeConfiguration<커뮤니티원장블록투영>
{
    public void Configure(EntityTypeBuilder<커뮤니티원장블록투영> builder)
    {
        builder.ToTable("community_ledger_block_projections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.커뮤니티원장Id).HasMaxLength(120).IsRequired();
        builder.Property(x => x.커뮤니티Id).HasMaxLength(120).IsRequired();
        builder.Property(x => x.원장템플릿Key).HasMaxLength(120).IsRequired();
        builder.Property(x => x.BlockId).HasMaxLength(120).IsRequired();
        builder.Property(x => x.BlockType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.State).HasMaxLength(80);
        builder.Property(x => x.UiSectionHint).HasMaxLength(120);
        builder.Property(x => x.DiagramNodeId).HasMaxLength(120);
        builder.Property(x => x.RelatedRoute).HasMaxLength(400);
        builder.Property(x => x.속성Json).HasColumnType("longtext").IsRequired();

        builder.HasIndex(x => new { x.커뮤니티원장Id, x.BlockId }).IsUnique();
        builder.HasIndex(x => new { x.커뮤니티Id, x.원장템플릿Key, x.BlockType });
        builder.HasIndex(x => new { x.커뮤니티원장Id, x.SortOrder });
        builder.HasIndex(x => x.DiagramNodeId);
    }
}

public sealed class 커뮤니티원장블록관계투영Configuration : IEntityTypeConfiguration<커뮤니티원장블록관계투영>
{
    public void Configure(EntityTypeBuilder<커뮤니티원장블록관계투영> builder)
    {
        builder.ToTable("community_ledger_block_relation_projections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.커뮤니티원장Id).HasMaxLength(120).IsRequired();
        builder.Property(x => x.관계유형).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Cardinality).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FromBlockId).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ToBlockId).HasMaxLength(120).IsRequired();
        builder.Property(x => x.DiagramEdgeId).HasMaxLength(120);
        builder.Property(x => x.Label).HasMaxLength(200);
        builder.Property(x => x.MeaningCode).HasMaxLength(120);
        builder.Property(x => x.조건식Json).HasColumnType("longtext");

        builder.HasOne(x => x.FromBlock)
            .WithMany(x => x.출력관계목록)
            .HasForeignKey(x => x.FromBlockProjectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToBlock)
            .WithMany(x => x.입력관계목록)
            .HasForeignKey(x => x.ToBlockProjectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.커뮤니티원장Id, x.FromBlockId, x.ToBlockId, x.관계유형 });
        builder.HasIndex(x => new { x.커뮤니티원장Id, x.Cardinality });
        builder.HasIndex(x => x.DiagramEdgeId);
    }
}
