using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.기사;
using 홍달.도메인.업체;
using 홍달.도메인.화주;
using 홍달.Infrastructure.Security;

namespace 홍달.Infrastructure.Persistence
{
    public static class PersonalDataProtectionModelBuilderExtensions
    {
        public static void ApplyPersonalDataProtection(this ModelBuilder modelBuilder, IPersonalDataEncryptionService protector)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            ArgumentNullException.ThrowIfNull(protector);

            ConfigurePersonalData(modelBuilder, protector);
        }

        private static void ConfigurePersonalData(ModelBuilder modelBuilder, IPersonalDataEncryptionService protector)
        {
            modelBuilder.Entity<ApplicationUser>()
                .Property(x => x.BusinessRegistrationNumber)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<업체>()
                .Property(x => x.대표_연락처)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<업체>()
                .Property(x => x.담당자)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<업체>()
                .Property(x => x.이메일)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<업체>()
                .Property(x => x.주소)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<배달기사>()
                .Property(x => x.연락처)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<용달기사>()
                .Property(x => x.연락처)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.픽업_도로명주소)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.픽업_상세주소)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.픽업_연락처_이름)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.픽업_연락처_전화번호)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.하차_도로명주소)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.하차_상세주소)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.하차_연락처_이름)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));

            modelBuilder.Entity<화주운송의뢰>()
                .Property(x => x.하차_연락처_전화번호)
                .HasConversion(v => protector.Protect(v), v => protector.Unprotect(v));
        }
    }
}