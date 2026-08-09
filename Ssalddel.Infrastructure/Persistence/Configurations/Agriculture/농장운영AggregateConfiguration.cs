using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.농업;

namespace 살뜰.Infrastructure.Persistence.Configurations.Agriculture;

public sealed class 농장Configuration : IEntityTypeConfiguration<농장>
{
    public void Configure(EntityTypeBuilder<농장> builder)
    {
        builder.HasIndex(item => item.StableId).IsUnique();
        builder.HasIndex(item => new { item.소유자UserId, item.운영상태Code });
    }
}

public sealed class 농장구획Configuration : IEntityTypeConfiguration<농장구획>
{
    public void Configure(EntityTypeBuilder<농장구획> builder)
    {
        builder.HasIndex(item => item.StableId).IsUnique();
        builder.HasIndex(item => new { item.농장Id, item.구획명 }).IsUnique();
        builder.HasOne(item => item.농장).WithMany(item => item.구획들)
            .HasForeignKey(item => item.농장Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 재배작기Configuration : IEntityTypeConfiguration<재배작기>
{
    public void Configure(EntityTypeBuilder<재배작기> builder)
    {
        builder.HasIndex(item => item.StableId).IsUnique();
        builder.HasIndex(item => new { item.농장구획Id, item.생육상태Code });
        builder.HasOne(item => item.농장구획).WithMany(item => item.재배작기들)
            .HasForeignKey(item => item.농장구획Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 농업센서Configuration : IEntityTypeConfiguration<농업센서>
{
    public void Configure(EntityTypeBuilder<농업센서> builder)
    {
        builder.HasIndex(item => item.StableId).IsUnique();
        builder.HasIndex(item => new { item.농장구획Id, item.상태Code });
        builder.HasOne(item => item.농장구획).WithMany(item => item.센서들)
            .HasForeignKey(item => item.농장구획Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 농업센서관측Configuration : IEntityTypeConfiguration<농업센서관측>
{
    public void Configure(EntityTypeBuilder<농업센서관측> builder)
    {
        builder.HasIndex(item => new { item.농업센서Id, item.관측시각Utc });
        builder.HasOne(item => item.농업센서).WithMany(item => item.관측들)
            .HasForeignKey(item => item.농업센서Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 농장작업Configuration : IEntityTypeConfiguration<농장작업>
{
    public void Configure(EntityTypeBuilder<농장작업> builder)
    {
        builder.HasIndex(item => item.StableId).IsUnique();
        builder.HasIndex(item => new { item.농장Id, item.NpcStableId });
        builder.HasOne(item => item.농장).WithMany(item => item.작업들)
            .HasForeignKey(item => item.농장Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.농장구획).WithMany()
            .HasForeignKey(item => item.농장구획Id).OnDelete(DeleteBehavior.SetNull);
    }
}
