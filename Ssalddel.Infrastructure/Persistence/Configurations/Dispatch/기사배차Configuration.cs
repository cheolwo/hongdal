using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.배차;

namespace 살뜰.Infrastructure.Persistence.Configurations.Dispatch;

public sealed class 기사배차Configuration : IEntityTypeConfiguration<기사배차>
{
    public void Configure(EntityTypeBuilder<기사배차> builder)
    {
        builder.ToTable("기사배차");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.배차Id).HasColumnName("배차Id");
        builder.Property(x => x.배차명).HasColumnName("배차명").IsRequired();
        builder.Property(x => x.상태).HasColumnName("상태").IsRequired();
        builder.Property(x => x.배차일).HasColumnName("배차일");
        builder.Property(x => x.용달기사_id).HasColumnName("배달기사_id");
        builder.Property(x => x.픽업지).HasColumnName("픽업지").IsRequired();
        builder.Property(x => x.배송지).HasColumnName("배송지").IsRequired();
        builder.Property(x => x.기본요금).HasColumnName("기본요금");
        builder.Property(x => x.거리추가_요금).HasColumnName("거리추가_요금");
        builder.Property(x => x.주문Id).HasColumnName("주문Id");
        builder.Property(x => x.기사Id).HasColumnName("기사Id");
        builder.Property(x => x.잠금여부).HasColumnName("잠금여부");
        builder.Property(x => x.잠금시각).HasColumnName("잠금시각");
        builder.Property(x => x.시도횟수).HasColumnName("시도횟수");
        builder.Property(x => x.픽업거리_m).HasColumnName("픽업거리_m");
        builder.Property(x => x.픽업예상시간_sec).HasColumnName("픽업예상시간_sec");
        builder.Property(x => x.배차점수).HasColumnName("배차점수");
        builder.Property(x => x.실패사유).HasColumnName("실패사유").IsRequired();
        builder.Property(x => x.메모).HasColumnName("메모").IsRequired();
        builder.Property(x => x.배차생성시각).HasColumnName("배차생성시각");
        builder.Property(x => x.배차완료시각).HasColumnName("배차완료시각");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
