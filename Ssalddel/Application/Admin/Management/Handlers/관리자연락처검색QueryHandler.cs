using Ssalddel.Contracts.Admin.Management;
using 살뜰.도메인.사용자;
using 살뜰.도메인.창고;

namespace Ssalddel.Application.Admin.Management;

public sealed class 관리자연락처검색QueryHandler : IRequestHandler<관리자연락처검색Query, 관리자연락처검색응답>
{
    private static readonly string[] CompletedRequestStatuses = ["완료", "취소", "취소환불", "환불됨"];

    private readonly SsalddelContext _db;

    public 관리자연락처검색QueryHandler(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<관리자연락처검색응답> Handle(관리자연락처검색Query request, CancellationToken cancellationToken)
    {
        var last8 = OnlyDigits(request.전화번호뒤8자리);
        if (last8.Length != 8)
        {
            return new 관리자연락처검색응답
            {
                전화번호뒤8자리 = last8,
                조회일시Utc = DateTime.UtcNow
            };
        }

        var matchedUserIds = new HashSet<string>(StringComparer.Ordinal);
        var contactSources = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        var users = await _db.Users
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email,
                x.PhoneNumber,
                x.BusinessRegistrationNumber
            })
            .ToListAsync(cancellationToken);

        foreach (var user in users.Where(x => EndsWithLast8(x.PhoneNumber, last8)))
        {
            AddMatch(matchedUserIds, contactSources, user.Id, "계정 전화번호");
        }

        var drivers = await _db.용달기사
            .AsNoTracking()
            .Select(x => new
            {
                x.기사Id,
                x.기사명,
                x.연락처,
                x.차량,
                x.운행상태,
                x.주_활동지역,
                x.등록일
            })
            .ToListAsync(cancellationToken);

        foreach (var driver in drivers.Where(x => EndsWithLast8(x.연락처, last8)))
        {
            AddMatch(matchedUserIds, contactSources, driver.기사Id, "기사 프로필 연락처");
        }

        var ordererProfiles = await _db.Set<주문자프로필>()
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.표시명,
                x.연락처,
                x.기본주소
            })
            .ToListAsync(cancellationToken);

        foreach (var profile in ordererProfiles.Where(x => EndsWithLast8(x.연락처, last8)))
        {
            AddMatch(matchedUserIds, contactSources, profile.UserId, "주문자 프로필 연락처");
        }

        var warehouseContactMatches = await _db.창고
            .AsNoTracking()
            .Where(x => x.소유자UserId != string.Empty)
            .Select(x => new
            {
                x.Id,
                x.소유자UserId,
                x.창고명,
                x.연락처
            })
            .ToListAsync(cancellationToken);

        var matchedWarehouseIds = warehouseContactMatches
            .Where(x => EndsWithLast8(x.연락처, last8))
            .Select(x => x.Id)
            .ToHashSet();

        foreach (var warehouse in warehouseContactMatches.Where(x => matchedWarehouseIds.Contains(x.Id)))
        {
            AddMatch(matchedUserIds, contactSources, warehouse.소유자UserId, $"창고 연락처: {warehouse.창고명}");
        }

        if (matchedWarehouseIds.Count > 0)
        {
            var warehouseUsersFromContact = await _db.창고사용자
                .AsNoTracking()
                .Where(x => matchedWarehouseIds.Contains(x.창고Id))
                .Select(x => new { x.UserId, x.창고Id, x.역할명 })
                .ToListAsync(cancellationToken);

            foreach (var warehouseUser in warehouseUsersFromContact)
            {
                AddMatch(matchedUserIds, contactSources, warehouseUser.UserId, $"창고 담당자 연결: {warehouseUser.역할명}");
            }
        }

        if (matchedUserIds.Count == 0)
        {
            return new 관리자연락처검색응답
            {
                전화번호뒤8자리 = last8,
                조회일시Utc = DateTime.UtcNow
            };
        }

        var userIds = matchedUserIds.ToArray();
        var userMap = users
            .Where(x => matchedUserIds.Contains(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var roleRows = await (
                from userRole in _db.UserRoles.AsNoTracking()
                join role in _db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, RoleName = role.Name ?? string.Empty })
            .ToListAsync(cancellationToken);

        var rolesByUser = roleRows
            .GroupBy(x => x.UserId, StringComparer.Ordinal)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<string>)x.Select(r => r.RoleName).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().OrderBy(r => r).ToArray(),
                StringComparer.Ordinal);

        var driversByUser = drivers
            .Where(x => matchedUserIds.Contains(x.기사Id))
            .ToDictionary(x => x.기사Id, StringComparer.Ordinal);

        var profilesByUser = ordererProfiles
            .Where(x => matchedUserIds.Contains(x.UserId))
            .ToDictionary(x => x.UserId, StringComparer.Ordinal);

        var shipperStatRows = await _db.화주운송의뢰
            .AsNoTracking()
            .Where(x => userIds.Contains(x.화주Id))
            .GroupBy(x => x.화주Id)
            .Select(x => new
            {
                UserId = x.Key,
                Count = x.Count(),
                ActiveCount = x.Count(r => !CompletedRequestStatuses.Contains(r.상태)),
                RecentAt = x.Max(r => (DateTime?)r.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        var shipperStats = shipperStatRows.ToDictionary(x => x.UserId, StringComparer.Ordinal);

        var recentRequests = await _db.화주운송의뢰
            .AsNoTracking()
            .Where(x => userIds.Contains(x.화주Id))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.화주Id,
                x.의뢰Id,
                x.화물종류,
                x.상태,
                x.결제상태,
                x.배차상태,
                x.픽업_도로명주소,
                x.하차_도로명주소,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var recentRequestsByUser = recentRequests
            .GroupBy(x => x.화주Id, StringComparer.Ordinal)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<관리자연락처최근의뢰응답>)x.Take(5).Select(r => new 관리자연락처최근의뢰응답
                {
                    의뢰Id = r.의뢰Id,
                    화물종류 = r.화물종류,
                    의뢰상태 = r.상태,
                    결제상태 = r.결제상태,
                    배차상태 = r.배차상태,
                    픽업지 = r.픽업_도로명주소,
                    하차지 = r.하차_도로명주소,
                    생성일시 = r.CreatedAt
                }).ToArray(),
                StringComparer.Ordinal);

        var warehouseMemberships = await (
                from warehouseUser in _db.창고사용자.AsNoTracking()
                join warehouse in _db.창고.AsNoTracking() on warehouseUser.창고Id equals warehouse.Id
                where userIds.Contains(warehouseUser.UserId)
                select new
                {
                    warehouseUser.UserId,
                    Item = new 관리자연락처창고참여응답
                    {
                        창고Id = warehouse.Id,
                        창고명 = warehouse.창고명,
                        역할명 = warehouseUser.역할명,
                        주담당여부 = warehouseUser.IsPrimary,
                        창고유형 = warehouse.창고유형,
                        주소 = warehouse.주소,
                        담당자명 = warehouse.담당자명,
                        연락처 = warehouse.연락처
                    }
                })
            .ToListAsync(cancellationToken);

        var ownedWarehouses = await _db.창고
            .AsNoTracking()
            .Where(x => userIds.Contains(x.소유자UserId))
            .Select(x => new
            {
                x.소유자UserId,
                Item = new 관리자연락처창고참여응답
                {
                    창고Id = x.Id,
                    창고명 = x.창고명,
                    역할명 = "창고 소유자",
                    주담당여부 = x.기본창고여부,
                    창고유형 = x.창고유형,
                    주소 = x.주소,
                    담당자명 = x.담당자명,
                    연락처 = x.연락처
                }
            })
            .ToListAsync(cancellationToken);

        var warehousesByUser = new Dictionary<string, List<관리자연락처창고참여응답>>(StringComparer.Ordinal);
        foreach (var membership in warehouseMemberships)
        {
            AddWarehouse(warehousesByUser, membership.UserId, membership.Item);
        }

        foreach (var owned in ownedWarehouses)
        {
            AddWarehouse(warehousesByUser, owned.소유자UserId, owned.Item);
        }

        var people = matchedUserIds
            .Select(userId =>
            {
                userMap.TryGetValue(userId, out var user);
                rolesByUser.TryGetValue(userId, out var roles);
                contactSources.TryGetValue(userId, out var sources);
                driversByUser.TryGetValue(userId, out var driver);
                profilesByUser.TryGetValue(userId, out var profile);
                shipperStats.TryGetValue(userId, out var shipperStat);
                recentRequestsByUser.TryGetValue(userId, out var userRecentRequests);
                warehousesByUser.TryGetValue(userId, out var userWarehouses);

                return new 관리자연락처인물응답
                {
                    UserId = userId,
                    사용자명 = user?.UserName ?? profile?.표시명 ?? driver?.기사명 ?? userId,
                    이메일 = user?.Email ?? string.Empty,
                    연락처 = FirstNotBlank(user?.PhoneNumber, driver?.연락처, profile?.연락처),
                    전화번호뒤8자리 = last8,
                    사업자번호 = user?.BusinessRegistrationNumber ?? string.Empty,
                    역할목록 = roles ?? [],
                    연락처출처목록 = sources?.OrderBy(x => x).ToArray() ?? [],
                    기사정보 = driver is null
                        ? null
                        : new 관리자연락처기사정보응답
                        {
                            기사명 = driver.기사명,
                            연락처 = driver.연락처,
                            차량 = driver.차량,
                            운행상태 = driver.운행상태,
                            활동지역 = driver.주_활동지역,
                            등록일 = driver.등록일
                        },
                    주문자프로필 = profile is null
                        ? null
                        : new 관리자연락처주문자프로필응답
                        {
                            Id = profile.Id,
                            표시명 = profile.표시명,
                            연락처 = profile.연락처,
                            기본주소 = profile.기본주소
                        },
                    화주정보 = shipperStat is null
                        ? null
                        : new 관리자연락처화주요약응답
                        {
                            의뢰건수 = shipperStat.Count,
                            진행중의뢰건수 = shipperStat.ActiveCount,
                            최근의뢰일시 = shipperStat.RecentAt
                        },
                    창고참여목록 = userWarehouses?.DistinctBy(x => $"{x.창고Id}:{x.역할명}").OrderBy(x => x.창고명).ToArray() ?? [],
                    최근의뢰목록 = userRecentRequests ?? []
                };
            })
            .OrderBy(x => x.사용자명)
            .ToArray();

        return new 관리자연락처검색응답
        {
            전화번호뒤8자리 = last8,
            검색결과수 = people.Length,
            조회일시Utc = DateTime.UtcNow,
            인물목록 = people
        };
    }

    private static void AddMatch(
        ISet<string> matchedUserIds,
        IDictionary<string, HashSet<string>> contactSources,
        string userId,
        string source)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        matchedUserIds.Add(userId);
        if (!contactSources.TryGetValue(userId, out var sources))
        {
            sources = new HashSet<string>(StringComparer.Ordinal);
            contactSources[userId] = sources;
        }

        sources.Add(source);
    }

    private static void AddWarehouse(
        IDictionary<string, List<관리자연락처창고참여응답>> warehousesByUser,
        string userId,
        관리자연락처창고참여응답 item)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (!warehousesByUser.TryGetValue(userId, out var items))
        {
            items = [];
            warehousesByUser[userId] = items;
        }

        items.Add(item);
    }

    private static bool EndsWithLast8(string? phoneNumber, string last8)
    {
        var digits = OnlyDigits(phoneNumber);
        return digits.Length >= 8 && digits.EndsWith(last8, StringComparison.Ordinal);
    }

    private static string OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static string FirstNotBlank(params string?[] values)
        => values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
}
