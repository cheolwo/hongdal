using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed record 공개사업장건축물연결Result(
    int 대상사업장수,
    int 연결수,
    int 복수후보수,
    int 건물후보없음수,
    int 주소부족수,
    int 기존판정수);

public sealed class 공개사업장건축물연결Service
{
    private readonly PublicDataIngestionDbContext _db;

    public 공개사업장건축물연결Service(PublicDataIngestionDbContext db)
    {
        _db = db;
    }

    public async Task<공개사업장건축물연결Result> 정확한도로명주소로연결Async(
        string businessSourceRevision,
        string buildingSourceRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessSourceRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingSourceRevision);

        var buildings = await _db.BuildingRegisterTitles
            .Where(item => item.SourceRevision == buildingSourceRevision && item.ValidToUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var building in buildings.Where(item => item.NormalizedRoadAddressKey == null))
            building.NormalizedRoadAddressKey = 공개사업장주소정규화Engine.NormalizeRoadAddress(building.RoadAddress);
        await _db.SaveChangesAsync(cancellationToken);

        var candidatesByAddress = buildings
            .Where(item => item.NormalizedRoadAddressKey is not null)
            .GroupBy(item => item.NormalizedRoadAddressKey!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Id).Order().ToArray(), StringComparer.Ordinal);
        var businesses = await _db.공개인허가사업장Records
            .Where(item => item.SourceRevision == businessSourceRevision)
            .OrderBy(item => item.OpenServiceId)
            .ThenBy(item => item.ManagementNumber)
            .ToListAsync(cancellationToken);
        var businessIds = businesses.Select(item => item.Id).ToList();
        var existingIds = await _db.공개사업장건축물Assignments
            .Where(item => businessIds.Contains(item.BusinessRecordId)
                && item.RuleRevision == 공개사업장주소정규화Engine.RuleRevision)
            .Select(item => item.BusinessRecordId)
            .ToListAsync(cancellationToken);
        var existingSet = existingIds.ToHashSet();
        var linked = 0;
        var multiple = 0;
        var none = 0;
        var insufficient = 0;

        foreach (var business in businesses.Where(item => !existingSet.Contains(item.Id)))
        {
            var normalized = business.NormalizedRoadAddressKey;
            var candidates = normalized is not null && candidatesByAddress.TryGetValue(normalized, out var found)
                ? found
                : [];
            var status = 공개사업장주소정규화Engine.DecideStatus(normalized, candidates.Length);
            switch (status)
            {
                case 공개사업장연결상태Codes.연결됨: linked++; break;
                case 공개사업장연결상태Codes.복수후보: multiple++; break;
                case 공개사업장연결상태Codes.건물후보없음: none++; break;
                default: insufficient++; break;
            }

            _db.공개사업장건축물Assignments.Add(new 공개사업장건축물Assignment
            {
                Id = 공공데이터원장식별자.결정적Guid(
                    $"business-building|{business.Id:N}|{공개사업장주소정규화Engine.RuleRevision}"),
                BusinessRecordId = business.Id,
                BuildingRecordId = candidates.Length == 1 ? candidates[0] : null,
                AssignmentStatusCode = status,
                AssignmentMethodCode = candidates.Length == 1
                    ? 공개사업장연결방법Codes.정확한정규화도로명주소
                    : null,
                ConfidenceCode = candidates.Length == 1 ? "DerivedHigh" : "Unresolved",
                CandidateBuildingCount = candidates.Length,
                RuleRevision = 공개사업장주소정규화Engine.RuleRevision,
                EvaluatedAtUtc = DateTimeOffset.UtcNow,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new 공개사업장건축물연결Result(
            businesses.Count,
            linked,
            multiple,
            none,
            insufficient,
            existingSet.Count);
    }

    public async Task<int> 건물별집계생성Async(
        string businessSourceRevision,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
                from assignment in _db.공개사업장건축물Assignments
                join business in _db.공개인허가사업장Records
                    on assignment.BusinessRecordId equals business.Id
                where assignment.BuildingRecordId != null
                    && assignment.AssignmentStatusCode == 공개사업장연결상태Codes.연결됨
                    && assignment.RuleRevision == 공개사업장주소정규화Engine.RuleRevision
                    && business.SourceRevision == businessSourceRevision
                select new
                {
                    BuildingRecordId = assignment.BuildingRecordId!.Value,
                    business.BusinessStatusName,
                    business.DetailedStatusName,
                })
            .ToListAsync(cancellationToken);
        var buildingIds = rows.Select(item => item.BuildingRecordId).Distinct().ToList();
        var existing = await _db.건축물공개사업장Aggregates
            .Where(item => buildingIds.Contains(item.BuildingRecordId)
                && item.SourceRevision == businessSourceRevision
                && item.RuleRevision == 공개사업장주소정규화Engine.RuleRevision)
            .ToDictionaryAsync(item => item.BuildingRecordId, cancellationToken);

        foreach (var group in rows.GroupBy(item => item.BuildingRecordId))
        {
            if (!existing.TryGetValue(group.Key, out var aggregate))
            {
                aggregate = new 건축물공개사업장Aggregate
                {
                    Id = 공공데이터원장식별자.결정적Guid(
                        $"building-business-aggregate|{group.Key:N}|{businessSourceRevision}|{공개사업장주소정규화Engine.RuleRevision}"),
                    BuildingRecordId = group.Key,
                    SourceRevision = businessSourceRevision,
                    RuleRevision = 공개사업장주소정규화Engine.RuleRevision,
                };
                _db.건축물공개사업장Aggregates.Add(aggregate);
            }

            var statuses = group
                .Select(item => 공개사업장영업상태Engine.분류(
                    item.BusinessStatusName,
                    item.DetailedStatusName))
                .ToArray();
            aggregate.TotalBusinessCount = statuses.Length;
            aggregate.OpenBusinessCount = statuses.Count(item =>
                item == 공개사업장영업상태Codes.영업);
            aggregate.SuspendedBusinessCount = statuses.Count(item =>
                item == 공개사업장영업상태Codes.휴업);
            aggregate.ClosedBusinessCount = statuses.Count(item =>
                item == 공개사업장영업상태Codes.폐업);
            aggregate.UnresolvedStatusCount = statuses.Count(item =>
                item == 공개사업장영업상태Codes.미확인);
            aggregate.AggregateHashSha256 = 공공데이터원장식별자.Sha256(string.Join('|',
                group.Key.ToString("N", CultureInfo.InvariantCulture),
                businessSourceRevision,
                aggregate.TotalBusinessCount,
                aggregate.OpenBusinessCount,
                aggregate.SuspendedBusinessCount,
                aggregate.ClosedBusinessCount,
                aggregate.UnresolvedStatusCount,
                공개사업장주소정규화Engine.RuleRevision));
            aggregate.GeneratedAtUtc = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return buildingIds.Count;
    }

}
