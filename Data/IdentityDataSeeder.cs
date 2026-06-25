using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.공통;
using 홍달.도메인.기사;
using 홍달.도메인.운송;

namespace 홍달.Data
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<HongdalContext>();

            var roles = new[] { 역할명.기사, 역할명.화주, 역할명.서버관리자 };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminId = "qkrcjfdn79";
            var adminEmail = "qkrcjfdn79@hongdal.local";
            var adminUser = await userManager.FindByNameAsync(adminId);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminId,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var createResult = await userManager.CreateAsync(adminUser, "tlstjstns79!");
                if (!createResult.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, 역할명.서버관리자))
            {
                await userManager.AddToRoleAsync(adminUser, 역할명.서버관리자);
            }

            var legacyAdminEmail = "admin@hongdal.local";
            var legacyAdminUser = await userManager.FindByEmailAsync(legacyAdminEmail);
            if (legacyAdminUser != null && !string.Equals(legacyAdminUser.UserName, adminId, StringComparison.OrdinalIgnoreCase))
            {
                if (await userManager.IsInRoleAsync(legacyAdminUser, 역할명.서버관리자))
                {
                    await userManager.RemoveFromRoleAsync(legacyAdminUser, 역할명.서버관리자);
                }

                await userManager.DeleteAsync(legacyAdminUser);
            }

            var driverEmail = "driver1@hongdal.local";
            var driverUser = await userManager.FindByEmailAsync(driverEmail);
            if (driverUser == null)
            {
                driverUser = new ApplicationUser
                {
                    UserName = driverEmail,
                    Email = driverEmail,
                    EmailConfirmed = true,
                };

                var createResult = await userManager.CreateAsync(driverUser, "Driver123!");
                if (!createResult.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(driverUser, 역할명.기사))
            {
                await userManager.AddToRoleAsync(driverUser, 역할명.기사);
            }

            var shipperId = "shipper1";
            var shipperEmail = "shipper1@hongdal.local";
            var shipperUser = await userManager.FindByNameAsync(shipperId) ?? await userManager.FindByEmailAsync(shipperEmail);
            if (shipperUser == null)
            {
                shipperUser = new ApplicationUser
                {
                    UserName = shipperId,
                    Email = shipperEmail,
                    EmailConfirmed = true,
                    BusinessRegistrationNumber = "123-45-67890"
                };

                var createResult = await userManager.CreateAsync(shipperUser, "Shipper123!");
                if (!createResult.Succeeded)
                {
                    return;
                }
            }
            else
            {
                var needsUpdate = false;
                if (!string.Equals(shipperUser.UserName, shipperId, StringComparison.OrdinalIgnoreCase))
                {
                    shipperUser.UserName = shipperId;
                    needsUpdate = true;
                }

                if (!string.Equals(shipperUser.Email, shipperEmail, StringComparison.OrdinalIgnoreCase))
                {
                    shipperUser.Email = shipperEmail;
                    shipperUser.EmailConfirmed = true;
                    needsUpdate = true;
                }

                if (!string.Equals(shipperUser.BusinessRegistrationNumber, "123-45-67890", StringComparison.OrdinalIgnoreCase))
                {
                    shipperUser.BusinessRegistrationNumber = "123-45-67890";
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    await userManager.UpdateAsync(shipperUser);
                }
            }

            if (!await userManager.IsInRoleAsync(shipperUser, 역할명.화주))
            {
                await userManager.AddToRoleAsync(shipperUser, 역할명.화주);
            }

            var driverProfile = await db.용달기사.FirstOrDefaultAsync(d => d.기사Id == driverUser.Id);
            if (driverProfile == null)
            {
                db.용달기사.Add(new 용달기사
                {
                    NotionPageId = Guid.NewGuid().ToString("N"),
                    기사명 = "개발용 기사",
                    기사Id = driverUser.Id,
                    상태 = "활동중",
                    연락처 = "010-0000-0000",
                    차량 = "오토바이",
                    운행상태 = 상태값.기사운행상태.대기,
                    주_활동지역 = "서울",
                    메모 = "개발 시드 데이터",
                    등록일 = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            var 차량단가시드 = new[]
            {
                new { 차량종류 = "오토바이", 기본운임 = 5000m, Km당단가 = 1000m, 최소운임 = 5000m, 야간할증 = 1000m, 우천할증 = 500m },
                new { 차량종류 = "다마스", 기본운임 = 15000m, Km당단가 = 1100m, 최소운임 = 15000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "라보", 기본운임 = 20000m, Km당단가 = 1150m, 최소운임 = 20000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1톤 카고", 기본운임 = 35000m, Km당단가 = 1300m, 최소운임 = 35000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1톤 탑차", 기본운임 = 35000m, Km당단가 = 1350m, 최소운임 = 35000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1톤 윙바디", 기본운임 = 35000m, Km당단가 = 1400m, 최소운임 = 35000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1톤 냉장탑", 기본운임 = 35000m, Km당단가 = 1450m, 최소운임 = 35000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1톤 냉동탑", 기본운임 = 35000m, Km당단가 = 1500m, 최소운임 = 35000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1.4톤 카고", 기본운임 = 45000m, Km당단가 = 1550m, 최소운임 = 45000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1.4톤 탑차", 기본운임 = 45000m, Km당단가 = 1600m, 최소운임 = 45000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "1.4톤 윙바디", 기본운임 = 45000m, Km당단가 = 1650m, 최소운임 = 45000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "2.5톤 카고", 기본운임 = 60000m, Km당단가 = 1700m, 최소운임 = 60000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "2.5톤 탑차", 기본운임 = 60000m, Km당단가 = 1750m, 최소운임 = 60000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "2.5톤 윙바디", 기본운임 = 60000m, Km당단가 = 1800m, 최소운임 = 60000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "3.5톤 카고", 기본운임 = 80000m, Km당단가 = 1850m, 최소운임 = 80000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "5톤 카고", 기본운임 = 100000m, Km당단가 = 1900m, 최소운임 = 100000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "5톤 탑차", 기본운임 = 100000m, Km당단가 = 1950m, 최소운임 = 100000m, 야간할증 = 0m, 우천할증 = 0m },
                new { 차량종류 = "5톤 윙바디", 기본운임 = 100000m, Km당단가 = 2000m, 최소운임 = 100000m, 야간할증 = 0m, 우천할증 = 0m }
            };

            var 기존차량종류 = await db.차량단가
                .Select(x => x.차량종류)
                .ToListAsync();

            var 기존차량종류셋 = new HashSet<string>(기존차량종류, StringComparer.OrdinalIgnoreCase);

            foreach (var row in 차량단가시드)
            {
                if (기존차량종류셋.Contains(row.차량종류))
                {
                    continue;
                }

                db.차량단가.Add(new 차량단가
                {
                    차량종류 = row.차량종류,
                    기본운임 = row.기본운임,
                    Km당단가 = row.Km당단가,
                    최소운임 = row.최소운임,
                    야간할증 = row.야간할증,
                    우천할증 = row.우천할증,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
