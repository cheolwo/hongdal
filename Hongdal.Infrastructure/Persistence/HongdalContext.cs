using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hongdal.Domain.Community;
using Hongdal.Domain.HumanResources;
using Hongdal.Domain.HsCodes;
using 홍달.도메인.기사;
using 홍달.도메인.업체;
using 홍달.도메인.배차;
using 홍달.도메인.결제;
using 홍달.도메인.차량;
using 홍달.도메인.화물;
using 홍달.도메인.탐색캠페인;
using 홍달.도메인.설정;
using 홍달.도메인.공통;
using 홍달.도메인.사용자;
using 홍달.도메인.운송;
using 홍달.도메인.창고;
using 홍달.도메인.판매;
using 홍달.도메인.화주;
using 홍달.도메인.공통콘텐츠;
using 홍달.도메인.통관;
using 홍달.도메인.정산;
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

        public DbSet<업체> 업체 { get; set; } = null!;
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

        public DbSet<배차대기> 배차대기 { get; set; } = null!;
        public DbSet<배차계획신청> 배차계획신청 { get; set; } = null!;
        public DbSet<기사배차> 기사배차 { get; set; } = null!;

        public DbSet<화주운송의뢰> 화주운송의뢰 { get; set; } = null!;
        public DbSet<화물요구조건> 화물요구조건 { get; set; } = null!;
        public DbSet<배송_운송> 배송_운송 { get; set; } = null!;
        public DbSet<운송이벤트> 운송이벤트 { get; set; } = null!;
        public DbSet<운송의뢰상품연결> 운송의뢰상품연결 { get; set; } = null!;

        public DbSet<운임구성> 운임구성 { get; set; } = null!;
        public DbSet<차량단가> 차량단가 { get; set; } = null!;
        public DbSet<결제> 결제 { get; set; } = null!;

        public DbSet<사용자Command기능설정> 사용자Command기능설정 { get; set; } = null!;
        public DbSet<Command알림Outbox> Command알림Outbox { get; set; } = null!;
        public DbSet<배차추천알림Outbox> 배차추천알림Outbox { get; set; } = null!;
        public DbSet<결제승인완료Outbox> 결제승인완료Outbox { get; set; } = null!;
        public DbSet<플랫폼View정책> 플랫폼View정책 { get; set; } = null!;
        public DbSet<사용자View설정> 사용자View설정 { get; set; } = null!;
        public DbSet<사용자행위로그> 사용자행위로그 { get; set; } = null!;
        public DbSet<생성이미지작업> 생성이미지작업 { get; set; } = null!;

        public DbSet<주문자프로필> 주문자프로필 { get; set; } = null!;
        public DbSet<홍달참여자> 홍달참여자 { get; set; } = null!;
        public DbSet<홍달참여자역할> 홍달참여자역할 { get; set; } = null!;
        public DbSet<HrRoleAssignmentRecord> HrRoleAssignments { get; set; } = null!;
        public DbSet<HrEmploymentContractRecord> HrEmploymentContracts { get; set; } = null!;
        public DbSet<HrPayrollScheduleRecord> HrPayrollSchedules { get; set; } = null!;
        public DbSet<WorkRelationshipSnapshotRecord> WorkRelationshipSnapshots { get; set; } = null!;
        public DbSet<인연연결요청> 인연연결요청 { get; set; } = null!;
        public DbSet<연락처공개동의> 연락처공개동의 { get; set; } = null!;
        public DbSet<관세사프로필> 관세사프로필 { get; set; } = null!;

        public DbSet<창고> 창고 { get; set; } = null!;
        public DbSet<창고사용자> 창고사용자 { get; set; } = null!;
        public DbSet<입고요청> 입고요청 { get; set; } = null!;
        public DbSet<입고상품> 입고상품 { get; set; } = null!;
        public DbSet<재고이력> 재고이력 { get; set; } = null!;
        public DbSet<출고예정> 출고예정 { get; set; } = null!;
        public DbSet<재고이동> 재고이동 { get; set; } = null!;
        public DbSet<통관절차> 통관절차 { get; set; } = null!;
        public DbSet<통관수임> 통관수임 { get; set; } = null!;
        public DbSet<통관조회연동> 통관조회연동 { get; set; } = null!;
        public DbSet<HsCodeCatalogVersion> HsCodeCatalogVersions { get; set; } = null!;
        public DbSet<HsCodeEntry> HsCodeEntries { get; set; } = null!;
        public DbSet<HsCodeEntryRiskTag> HsCodeEntryRiskTags { get; set; } = null!;
        public DbSet<HsCodeClassificationCase> HsCodeClassificationCases { get; set; } = null!;
        public DbSet<HsCodePlatformAgencyExperience> HsCodePlatformAgencyExperiences { get; set; } = null!;

        public DbSet<판매채널계정> 판매채널계정 { get; set; } = null!;
        public DbSet<판매상품> 판매상품 { get; set; } = null!;
        public DbSet<채널출품> 채널출품 { get; set; } = null!;
        public DbSet<상품식별코드맵> 상품식별코드맵 { get; set; } = null!;
        public DbSet<상품물류자산> 상품물류자산 { get; set; } = null!;
        public DbSet<상품상세이미지생성작업> 상품상세이미지생성작업 { get; set; } = null!;
        public DbSet<상품판매이미지초안> 상품판매이미지초안 { get; set; } = null!;
        public DbSet<감사메시지> 감사메시지 { get; set; } = null!;

        public DbSet<홍달공통콘텐츠> 홍달공통콘텐츠 { get; set; } = null!;
        public DbSet<홍달콘텐츠보상정책> 홍달콘텐츠보상정책 { get; set; } = null!;
        public DbSet<홍달콘텐츠시청세션> 홍달콘텐츠시청세션 { get; set; } = null!;
        public DbSet<홍달콘텐츠보상지급> 홍달콘텐츠보상지급 { get; set; } = null!;
        public DbSet<PlatformRevenueEntryRecord> PlatformRevenueEntries { get; set; } = null!;
        public DbSet<PlatformProfitReturnPolicyRecord> PlatformProfitReturnPolicies { get; set; } = null!;
        public DbSet<PlatformProfitReturnScheduleRecord> PlatformProfitReturnSchedules { get; set; } = null!;
        public DbSet<PlatformCommunityPost> PlatformCommunityPosts { get; set; } = null!;
        public DbSet<PlatformCommunityPostAttachment> PlatformCommunityPostAttachments { get; set; } = null!;
        public DbSet<PlatformCommunityPostAttachmentComment> PlatformCommunityPostAttachmentComments { get; set; } = null!;
        public DbSet<PlatformCommunityPostComment> PlatformCommunityPostComments { get; set; } = null!;
        public DbSet<PlatformCommunityPostRecommendation> PlatformCommunityPostRecommendations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyPersonalDataProtection(_personalDataProtector);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HongdalContext).Assembly);

            modelBuilder.Entity<사용자Command기능설정>()
                .HasIndex(x => new { x.사용자Id, x.CommandName, x.FeatureName })
                .IsUnique();

            modelBuilder.Entity<Command알림Outbox>()
                .HasIndex(x => new { x.Status, x.CreatedAt });

            modelBuilder.Entity<결제승인완료Outbox>()
                .HasIndex(x => new { x.처리상태, x.CreatedAt });

            modelBuilder.Entity<결제승인완료Outbox>()
                .HasIndex(x => x.결제레코드Id)
                .IsUnique();

            modelBuilder.Entity<플랫폼View정책>()
                .HasIndex(x => new { x.AppKey, x.ViewKey, x.RoleName })
                .IsUnique();

            modelBuilder.Entity<사용자View설정>()
                .HasIndex(x => new { x.UserId, x.AppKey, x.ViewKey })
                .IsUnique();

            modelBuilder.Entity<생성이미지작업>()
                .HasIndex(x => x.작업코드)
                .IsUnique();

            modelBuilder.Entity<생성이미지작업>()
                .HasIndex(x => x.중복방지키);

            modelBuilder.Entity<생성이미지작업>()
                .HasIndex(x => new { x.이미지용도, x.대상타입, x.대상식별자, x.상태 });

            modelBuilder.Entity<생성이미지작업>()
                .HasIndex(x => x.외부TaskId);

            modelBuilder.Entity<주문자프로필>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<홍달참여자>()
                .HasIndex(x => x.활성화여부);

            modelBuilder.Entity<홍달참여자역할>()
                .Property(x => x.역할유형)
                .HasConversion<int>();

            modelBuilder.Entity<홍달참여자역할>()
                .HasIndex(x => new { x.참여자Id, x.역할유형, x.활성화여부 });

            modelBuilder.Entity<홍달참여자역할>()
                .HasOne(x => x.참여자)
                .WithMany(x => x.역할목록)
                .HasForeignKey(x => x.참여자Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HrRoleAssignmentRecord>()
                .HasIndex(x => new { x.UserId, x.ScopeType, x.ScopeId, x.RoleCode, x.IsActive });

            modelBuilder.Entity<HrEmploymentContractRecord>()
                .HasIndex(x => new { x.WorkerUserId, x.EmployerScopeType, x.EmployerScopeId, x.ContractStatus });

            modelBuilder.Entity<HrPayrollScheduleRecord>()
                .HasIndex(x => new { x.WorkerUserId, x.ScheduledPaymentDate, x.Status });

            modelBuilder.Entity<HrPayrollScheduleRecord>()
                .HasOne(x => x.Contract)
                .WithMany(x => x.PayrollSchedules)
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<관세사프로필>()
                .HasIndex(x => x.참여자Id)
                .IsUnique();

            modelBuilder.Entity<관세사프로필>()
                .HasIndex(x => new { x.관리자승인여부, x.수임가능여부 });

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

            modelBuilder.Entity<통관절차>()
                .Property(x => x.물류거래방향)
                .HasConversion<int>();

            modelBuilder.Entity<통관절차>()
                .Property(x => x.상태)
                .HasConversion<int>();

            modelBuilder.Entity<통관절차>()
                .HasIndex(x => new { x.주문Id, x.주문참조번호, x.상태 });

            modelBuilder.Entity<통관절차>()
                .HasIndex(x => new { x.출고예정Id, x.입고요청Id });

            modelBuilder.Entity<통관수임>()
                .Property(x => x.상태)
                .HasConversion<int>();

            modelBuilder.Entity<통관수임>()
                .HasIndex(x => new { x.통관절차Id, x.관세사참여자Id, x.상태 });

            modelBuilder.Entity<판매채널계정>()
                .HasIndex(x => new { x.UserId, x.채널종류, x.상점명 });

            modelBuilder.Entity<판매상품>()
                .HasIndex(x => new { x.입고상품Id, x.판매SKU })
                .IsUnique();

            modelBuilder.Entity<판매상품>()
                .HasIndex(x => new { x.샘플데이터여부, x.이미지생성상태, x.UpdatedAt });

            modelBuilder.Entity<채널출품>()
                .HasIndex(x => new { x.판매상품Id, x.판매채널계정Id })
                .IsUnique();

            modelBuilder.Entity<운송의뢰상품연결>()
                .HasIndex(x => new { x.운송의뢰Id, x.입고상품Id });

            modelBuilder.Entity<PlatformRevenueEntryRecord>()
                .HasIndex(x => new { x.RevenueSource, x.OccurredAtUtc });

            modelBuilder.Entity<PlatformRevenueEntryRecord>()
                .HasIndex(x => new { x.SourceReferenceType, x.SourceReferenceId });

            modelBuilder.Entity<PlatformProfitReturnPolicyRecord>()
                .HasIndex(x => new { x.TargetParticipantCategory, x.IsActive, x.EffectiveStartDate });

            modelBuilder.Entity<PlatformProfitReturnScheduleRecord>()
                .HasIndex(x => new { x.ParticipantUserId, x.ScheduledPaymentDate, x.Status });

            modelBuilder.Entity<PlatformProfitReturnScheduleRecord>()
                .HasOne(x => x.Policy)
                .WithMany()
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
