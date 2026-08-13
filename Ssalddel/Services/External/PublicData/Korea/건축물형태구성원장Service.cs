using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed record 건축물형태구성원장Result(
    int 대상건물수,
    int 추가형태Profile수,
    int 기존형태Profile수,
    int 추가시각계획수);

public sealed class 건축물형태구성원장Service
{
    private readonly PublicDataIngestionDbContext _db;

    public 건축물형태구성원장Service(PublicDataIngestionDbContext db)
    {
        _db = db;
    }

    public async Task<건축물형태구성원장Result> 형태와시각계획생성Async(
        string sourceRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRevision);
        var rows = await (
                from building in _db.BuildingRegisterTitles
                join category in _db.BuildingCategoryAssignments
                    on building.Id equals category.BuildingRecordId
                where building.SourceRevision == sourceRevision
                    && building.ValidToUtc == null
                    && category.RuleRevision == 건축물주용도분류Engine.RuleRevision
                    && category.IsPrimary
                orderby building.RegisterManagementPk
                select new { Building = building, category.CategoryCode })
            .ToListAsync(cancellationToken);
        var buildingIds = rows.Select(item => item.Building.Id).ToList();
        var existingProfiles = await _db.건축물형태Profiles
            .Where(item => buildingIds.Contains(item.건축물RecordId)
                && item.규칙개정번호 == 건축물형태분석Engine.규칙개정번호)
            .Include(item => item.건축물Record)
            .ToDictionaryAsync(item => item.건축물RecordId, cancellationToken);

        var addedProfiles = 0;
        var addedPlans = 0;
        foreach (var row in rows)
        {
            if (existingProfiles.ContainsKey(row.Building.Id))
                continue;

            var result = 건축물형태분석Engine.분석(row.Building, row.CategoryCode);
            var profileId = 공공데이터원장식별자.결정적Guid(
                $"massing|{row.Building.Id:N}|{건축물형태분석Engine.규칙개정번호}");
            var profile = new 건축물형태Profile
            {
                Id = profileId,
                건축물RecordId = row.Building.Id,
                관측지상층수 = result.관측지상층수,
                추정지상층수 = result.추정지상층수,
                표현지상층수 = result.표현지상층수,
                공식건폐율Percent = result.공식건폐율Percent,
                공식용적률Percent = result.공식용적률Percent,
                단순건폐비율Percent = result.단순건폐비율Percent,
                단순연면적대지비율Percent = result.단순연면적대지비율Percent,
                대지면적SquareMeters = row.Building.SiteAreaSquareMeters,
                건축면적SquareMeters = row.Building.BuildingAreaSquareMeters,
                연면적SquareMeters = row.Building.TotalFloorAreaSquareMeters,
                높이Meters = row.Building.HeightMeters,
                추정층고Meters = result.추정층고Meters,
                건물바닥면적등급Code = result.건물바닥면적등급Code,
                높이등급Code = result.높이등급Code,
                밀도등급Code = result.밀도등급Code,
                근거종류Code = result.근거종류Code,
                규칙개정번호 = 건축물형태분석Engine.규칙개정번호,
                생성시각Utc = DateTimeOffset.UtcNow,
            };
            profile.ProfileHashSha256 = 공공데이터원장식별자.Sha256(ProfileFingerprint(profile));
            _db.건축물형태Profiles.Add(profile);

            var visual = 건축물형태분석Engine.시각구성(result, row.CategoryCode);
            var plan = new 건축물시각구성계획
            {
                Id = 공공데이터원장식별자.결정적Guid(
                    $"visual-plan|{profileId:N}|{건축물형태분석Engine.규칙개정번호}"),
                건축물형태ProfileId = profileId,
                시각FamilyCode = visual.시각FamilyCode,
                기준층수 = visual.기준층수,
                중간층반복수 = visual.중간층반복수,
                대지점유등급Code = visual.대지점유등급Code,
                주변여백등급Code = visual.주변여백등급Code,
                LOD등급Code = visual.LOD등급Code,
                표현전용 = true,
                규칙개정번호 = 건축물형태분석Engine.규칙개정번호,
                생성시각Utc = DateTimeOffset.UtcNow,
            };
            plan.계획HashSha256 = 공공데이터원장식별자.Sha256(PlanFingerprint(plan));
            _db.건축물시각구성계획들.Add(plan);
            addedProfiles++;
            addedPlans++;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new 건축물형태구성원장Result(
            rows.Count,
            addedProfiles,
            existingProfiles.Count,
            addedPlans);
    }

    private static string ProfileFingerprint(건축물형태Profile profile) =>
        string.Join('|',
            profile.건축물RecordId.ToString("N", CultureInfo.InvariantCulture),
            Format(profile.관측지상층수),
            Format(profile.추정지상층수),
            Format(profile.표현지상층수),
            Format(profile.공식건폐율Percent),
            Format(profile.공식용적률Percent),
            Format(profile.단순건폐비율Percent),
            Format(profile.단순연면적대지비율Percent),
            Format(profile.대지면적SquareMeters),
            Format(profile.건축면적SquareMeters),
            Format(profile.연면적SquareMeters),
            Format(profile.높이Meters),
            Format(profile.추정층고Meters),
            profile.건물바닥면적등급Code,
            profile.높이등급Code,
            profile.밀도등급Code,
            profile.근거종류Code,
            profile.규칙개정번호);

    private static string PlanFingerprint(건축물시각구성계획 plan) =>
        string.Join('|',
            plan.건축물형태ProfileId.ToString("N", CultureInfo.InvariantCulture),
            plan.시각FamilyCode,
            plan.기준층수.ToString(CultureInfo.InvariantCulture),
            plan.중간층반복수.ToString(CultureInfo.InvariantCulture),
            plan.대지점유등급Code,
            plan.주변여백등급Code,
            plan.LOD등급Code,
            plan.표현전용.ToString(CultureInfo.InvariantCulture),
            plan.규칙개정번호);

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}
