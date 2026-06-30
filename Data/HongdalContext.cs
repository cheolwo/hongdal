using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.업체;
using 홍달.도메인.결제;
using 홍달.도메인.차량;
using 홍달.도메인.화물;
using 홍달.도메인.탐색캠페인;
using 홍달.도메인.설정;
using 홍달.도메인.사용자;
using 홍달.도메인.운송;
using 홍달.도메인.창고;
using 홍달.도메인.판매;
using 홍달.Infrastructure.Persistence;
using 홍달.Infrastructure.Security;

namespace 홍달.Data
{
    public class HongdalContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IPersonalDataEncryptionService _personalDataProtector;

        public HongdalContext(DbContextOptions<HongdalContext> options, IPersonalDataEncryptionService personalDataProtector) : base(options)
        {
            _personalDataProtector = personalDataProtector;
        }

        // 업체
        public DbSet<업체> 업체 { get; set; } = null!;

        // 기사
        public DbSet<배달기사> 배달기사 { get; set; } = null!;
        public DbSet<용달기사> 용달기사 { get; set; } = null!;
        public DbSet<기사근무> 기사근무 { get; set; } = null!;
        public DbSet<기사위치기록> 기사위치기록 { get; set; } = null!;
        public DbSet<기사월정산> 기사월정산 { get; set; } = null!;
        public DbSet<차량제원> 차량제원 { get; set; } = null!;
        public DbSet<탐색캠페인> 탐색캠페인 { get; set; } = null!;
        public DbSet<탐색캠페인대상자> 탐색캠페인대상자 { get; set; } = null!;
        public DbSet<탐색캠페인응답> 탐색캠페인응답 { get; set; } = null!;
        public DbSet<기사화주관계집계> 기사화주관계집계 { get; set; } = null!;

        // 배차
        public DbSet<배차대기> 배차대기 { get; set; } = null!;
        public DbSet<배차계획신청> 배차계획신청 { get; set; } = null!;
        public DbSet<기사배차> 기사배차 { get; set; } = null!;

        // 운송
        public DbSet<화주운송의뢰> 화주운송의뢰 { get; set; } = null!;
        public DbSet<화물요구조건> 화물요구조건 { get; set; } = null!;
        public DbSet<배송_운송> 배송_운송 { get; set; } = null!;
        public DbSet<운송이벤트> 운송이벤트 { get; set; } = null!;
        public DbSet<운송의뢰상품연결> 운송의뢰상품연결 { get; set; } = null!;

        // 운임/결제
        public DbSet<운임구성> 운임구성 { get; set; } = null!;
        public DbSet<차량단가> 차량단가 { get; set; } = null!;
        public DbSet<결제> 결제 { get; set; } = null!;

        // 설정
        public DbSet<사용자Command기능설정> 사용자Command기능설정 { get; set; } = null!;
        public DbSet<Command알림Outbox> Command알림Outbox { get; set; } = null!;
        public DbSet<플랫폼View정책> 플랫폼View정책 { get; set; } = null!;
        public DbSet<사용자View설정> 사용자View설정 { get; set; } = null!;
        public DbSet<사용자행위로그> 사용자행위로그 { get; set; } = null!;

        // 사용자
        public DbSet<주문자프로필> 주문자프로필 { get; set; } = null!;

        // 창고/입고/재고
        public DbSet<창고> 창고 { get; set; } = null!;
        public DbSet<창고사용자> 창고사용자 { get; set; } = null!;
        public DbSet<입고요청> 입고요청 { get; set; } = null!;
        public DbSet<입고상품> 입고상품 { get; set; } = null!;
        public DbSet<재고이력> 재고이력 { get; set; } = null!;

        // 판매
        public DbSet<판매채널계정> 판매채널계정 { get; set; } = null!;
        public DbSet<판매상품> 판매상품 { get; set; } = null!;
        public DbSet<채널출품> 채널출품 { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DataAnnotations ([Table], [Column]) are used on entities.
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyPersonalDataProtection(_personalDataProtector);

            modelBuilder.Entity<사용자Command기능설정>()
                .HasIndex(x => new { x.사용자Id, x.CommandName, x.FeatureName })
                .IsUnique();

            modelBuilder.Entity<Command알림Outbox>()
                .HasIndex(x => new { x.Status, x.CreatedAt });

            modelBuilder.Entity<플랫폼View정책>()
                .HasIndex(x => new { x.AppKey, x.ViewKey, x.RoleName })
                .IsUnique();

            modelBuilder.Entity<사용자View설정>()
                .HasIndex(x => new { x.UserId, x.AppKey, x.ViewKey })
                .IsUnique();

            modelBuilder.Entity<주문자프로필>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<탐색캠페인>()
                .HasIndex(x => new { x.개시자UserId, x.운행예정일, x.탐색상태 });

            modelBuilder.Entity<탐색캠페인대상자>()
                .HasIndex(x => new { x.탐색캠페인Id, x.대상UserId })
                .IsUnique();

            modelBuilder.Entity<탐색캠페인응답>()
                .HasIndex(x => new { x.탐색캠페인Id, x.응답자UserId })
                .IsUnique();

            modelBuilder.Entity<기사화주관계집계>()
                .HasIndex(x => new { x.기사Id, x.화주UserId })
                .IsUnique();

            modelBuilder.Entity<창고>()
                .HasIndex(x => new { x.소유자UserId, x.창고명 });

            modelBuilder.Entity<창고사용자>()
                .HasIndex(x => new { x.창고Id, x.UserId, x.역할명 })
                .IsUnique();

            modelBuilder.Entity<입고요청>()
                .HasIndex(x => new { x.창고Id, x.주문자UserId, x.상태 });

            modelBuilder.Entity<입고상품>()
                .HasIndex(x => new { x.창고Id, x.소유자UserId, x.상태 });

            modelBuilder.Entity<재고이력>()
                .HasIndex(x => new { x.입고상품Id, x.처리일시 });

            modelBuilder.Entity<판매채널계정>()
                .HasIndex(x => new { x.UserId, x.채널종류, x.상점명 });

            modelBuilder.Entity<판매상품>()
                .HasIndex(x => new { x.입고상품Id, x.판매SKU })
                .IsUnique();

            modelBuilder.Entity<채널출품>()
                .HasIndex(x => new { x.판매상품Id, x.판매채널계정Id })
                .IsUnique();

            modelBuilder.Entity<운송의뢰상품연결>()
                .HasIndex(x => new { x.운송의뢰Id, x.입고상품Id });
        }
    }
}
