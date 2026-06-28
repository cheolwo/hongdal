using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.업체;
using 홍달.도메인.결제;
using 홍달.도메인.차량;
using 홍달.도메인.화물;
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

        // 배차
        public DbSet<배차대기> 배차대기 { get; set; } = null!;
        public DbSet<배차계획신청> 배차계획신청 { get; set; } = null!;
        public DbSet<기사배차> 기사배차 { get; set; } = null!;

        // 운송
        public DbSet<화주운송의뢰> 화주운송의뢰 { get; set; } = null!;
        public DbSet<화물요구조건> 화물요구조건 { get; set; } = null!;
        public DbSet<배송_운송> 배송_운송 { get; set; } = null!;
        public DbSet<운송이벤트> 운송이벤트 { get; set; } = null!;

        // 운임/결제
        public DbSet<운임구성> 운임구성 { get; set; } = null!;
        public DbSet<차량단가> 차량단가 { get; set; } = null!;
        public DbSet<결제> 결제 { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DataAnnotations ([Table], [Column]) are used on entities.
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyPersonalDataProtection(_personalDataProtector);
        }
    }
}
