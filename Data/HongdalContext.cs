using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.배차;
using 홍달.도메인.운송;
using 홍달.도메인.기사;
using 홍달.도메인.업체;
using 홍달.도메인.화주;
using 홍달.도메인.결제;

namespace 홍달.Data
{
    public class HongdalContext : IdentityDbContext<ApplicationUser>
    {
        public HongdalContext(DbContextOptions<HongdalContext> options) : base(options)
        {
        }

        public DbSet<기사배차> 기사배차 { get; set; } = null!;
        public DbSet<배송_운송> 배송_운송 { get; set; } = null!;

/// <summary>
/// 관리자 관리
/// </summary>
        public DbSet<운임구성> 운임구성 { get; set; } = null!;
        public DbSet<차량단가> 차량단가 { get; set; } = null!;

        public DbSet<운송이벤트> 운송이벤트 { get; set; } = null!;

        public DbSet<배달기사> 배달기사 { get; set; } = null!;
        public DbSet<용달기사> 용달기사 { get; set; } = null!;
        public DbSet<배차계획신청> 배차계획신청 { get; set; } = null!;
        public DbSet<기사근무> 기사근무 { get; set; } = null!;
        public DbSet<기사위치기록> 기사위치기록 { get; set; } = null!;
        public DbSet<기사월정산> 기사월정산 { get; set; } = null!;

        public DbSet<업체> 업체 { get; set; } = null!;
        public DbSet<배차대기> 배차대기 { get; set; } = null!;
        public DbSet<화주운송의뢰> 화주운송의뢰 { get; set; } = null!;
        public DbSet<결제> 결제 { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DataAnnotations ([Table], [Column]) are used on entities.
            base.OnModelCreating(modelBuilder);
        }
    }
}
