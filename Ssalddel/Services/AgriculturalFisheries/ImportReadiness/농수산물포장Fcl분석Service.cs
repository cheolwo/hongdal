using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.ImportReadiness;

public sealed record 농수산물포장Fcl분석RunResult(
    int SourceYear,
    int ItemCount,
    int InsertedCount,
    int UpdatedCount,
    DateTime AnalyzedAtUtc);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "KAMIS 품목별 대표 포장과 컨테이너 적재 추정치를 근거 수준과 함께 저장하고 조회한다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(농수산물포장Fcl분석목록Response),
    FlowOrder = 20,
    Boundary = "추정치는 발주, 선적 예약 또는 계약 확정값이 아니며 공급자와 포워더 확인 전에는 실행에 사용할 수 없다.")]
public interface I농수산물포장Fcl분석Service
{
    Task<농수산물포장Fcl분석RunResult> 분석저장Async(
        int sourceYear,
        CancellationToken cancellationToken = default);

    Task<농수산물포장Fcl분석목록Response> 조회Async(
        int? sourceYear,
        string? itemCode,
        string? categoryCode,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "공식 컨테이너 제원과 대표 포장 가정을 결합해 품목별 FCL 적재량을 추정한다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(농수산물포장Fcl분석목록Response),
    FlowOrder = 30,
    Boundary = "KAMIS 가격 비교 단위를 실제 공급 포장 단위로 간주하지 않는다.")]
public sealed class 농수산물포장Fcl분석Service : I농수산물포장Fcl분석Service
{
    internal const string CurrentProfileVersion = "2026.07-v1";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AgriculturalFisheriesDbContext _db;
    private readonly TimeProvider _timeProvider;

    public 농수산물포장Fcl분석Service(
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<농수산물포장Fcl분석RunResult> 분석저장Async(
        int sourceYear,
        CancellationToken cancellationToken = default)
    {
        if (sourceYear is < 1990 or > 2100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceYear),
                "분석 기준 연도는 1990년부터 2100년 사이여야 합니다.");
        }

        var startDate = new DateOnly(sourceYear, 1, 1);
        var endDate = startDate.AddYears(1);
        var observations = await _db.KamisPriceObservations
            .AsNoTracking()
            .Where(item =>
                item.SurveyDate >= startDate
                && item.SurveyDate < endDate
                && item.ItemCode != string.Empty)
            .Select(item => new
            {
                item.CategoryCode,
                item.CategoryName,
                item.ItemCode,
                item.ItemName,
                item.Unit,
                item.KindName,
                item.SurveyDate
            })
            .ToListAsync(cancellationToken);
        if (observations.Count == 0)
        {
            throw new InvalidOperationException(
                $"{sourceYear}년 KAMIS 관측 품목이 없어 포장·FCL 분석을 만들 수 없습니다.");
        }

        var itemSources = observations
            .GroupBy(item => item.ItemCode, StringComparer.Ordinal)
            .Select(group =>
            {
                var latest = group
                    .OrderByDescending(item => item.SurveyDate)
                    .ThenBy(item => item.CategoryCode, StringComparer.Ordinal)
                    .First();
                return new 분석대상품목(
                    latest.CategoryCode,
                    latest.CategoryName,
                    latest.ItemCode,
                    latest.ItemName,
                    group.Select(item => item.Unit)
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                    group.Select(item => item.KindName)
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray());
            })
            .OrderBy(item => item.CategoryCode, StringComparer.Ordinal)
            .ThenBy(item => item.ItemCode, StringComparer.Ordinal)
            .ToArray();

        var analyzedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var existingRows = await _db.PackagingFclAnalysisSnapshots
            .Where(item =>
                item.SourceYear == sourceYear
                && item.ProfileVersion == CurrentProfileVersion)
            .ToListAsync(cancellationToken);
        var existing = existingRows.ToDictionary(
            item => item.AnalysisKey,
            StringComparer.Ordinal);

        var insertedCount = 0;
        var updatedCount = 0;
        foreach (var item in itemSources)
        {
            var profile = 농수산물대표포장추론Catalog.추론(item);
            var estimates = 농수산물포장Fcl계산기.계산(profile);
            var key = BuildAnalysisKey(sourceYear, item.ItemCode);
            if (!existing.TryGetValue(key, out var snapshot))
            {
                snapshot = new 농수산물포장Fcl분석Snapshot
                {
                    AnalysisKey = key
                };
                _db.PackagingFclAnalysisSnapshots.Add(snapshot);
                insertedCount++;
            }
            else
            {
                updatedCount++;
            }

            Apply(
                snapshot,
                sourceYear,
                item,
                profile,
                estimates,
                analyzedAtUtc);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new 농수산물포장Fcl분석RunResult(
            sourceYear,
            itemSources.Length,
            insertedCount,
            updatedCount,
            analyzedAtUtc);
    }

    public async Task<농수산물포장Fcl분석목록Response> 조회Async(
        int? sourceYear,
        string? itemCode,
        string? categoryCode,
        CancellationToken cancellationToken = default)
    {
        var resolvedYear = sourceYear ?? await _db.PackagingFclAnalysisSnapshots
            .AsNoTracking()
            .MaxAsync(item => (int?)item.SourceYear, cancellationToken);
        if (resolvedYear is null)
        {
            return EmptyResponse(0);
        }

        var normalizedItemCode = itemCode?.Trim();
        var normalizedCategoryCode = categoryCode?.Trim();
        var query = _db.PackagingFclAnalysisSnapshots
            .AsNoTracking()
            .Where(item => item.SourceYear == resolvedYear.Value);
        if (!string.IsNullOrWhiteSpace(normalizedItemCode))
        {
            query = query.Where(item => item.ItemCode == normalizedItemCode);
        }
        if (!string.IsNullOrWhiteSpace(normalizedCategoryCode))
        {
            query = query.Where(item => item.CategoryCode == normalizedCategoryCode);
        }

        var snapshots = await query
            .OrderBy(item => item.CategoryCode)
            .ThenBy(item => item.ItemCode)
            .ToListAsync(cancellationToken);
        return new 농수산물포장Fcl분석목록Response
        {
            ProfileVersion = snapshots.FirstOrDefault()?.ProfileVersion ?? CurrentProfileVersion,
            SourceYear = resolvedYear.Value,
            TotalCount = snapshots.Count,
            LatestAnalyzedAtUtc = snapshots.Count == 0
                ? null
                : snapshots.Max(item => item.AnalyzedAtUtc),
            Items = snapshots.Select(Map).ToArray(),
            Notices = BuildNotices()
        };
    }

    private static void Apply(
        농수산물포장Fcl분석Snapshot target,
        int sourceYear,
        분석대상품목 item,
        농수산물대표포장제원 profile,
        IReadOnlyList<농수산물Fcl적재추정Response> estimates,
        DateTime analyzedAtUtc)
    {
        target.ProfileVersion = CurrentProfileVersion;
        target.SourceYear = sourceYear;
        target.CategoryCode = item.CategoryCode;
        target.CategoryName = item.CategoryName;
        target.ItemCode = item.ItemCode;
        target.ItemName = item.ItemName;
        target.KamisPriceComparisonUnitsJson = JsonSerializer.Serialize(
            item.KamisPriceComparisonUnits,
            JsonOptions);
        target.KamisKindNamesJson = JsonSerializer.Serialize(item.KamisKindNames, JsonOptions);
        target.PackageTypeCode = profile.PackageTypeCode;
        target.PackageUnitLabel = profile.PackageUnitLabel;
        target.NetContentWeightKg = profile.NetContentWeightKg;
        target.GrossWeightKg = profile.GrossWeightKg;
        target.UnitsPerPackage = profile.UnitsPerPackage;
        target.UnitCountLabel = profile.UnitCountLabel ?? string.Empty;
        target.LengthMm = profile.LengthMm;
        target.WidthMm = profile.WidthMm;
        target.HeightMm = profile.HeightMm;
        target.TemperatureCode = profile.TemperatureCode;
        target.Stackable = profile.Stackable;
        target.MaxStackLayers = profile.MaxStackLayers;
        target.PackingMethodCode = profile.PackingMethodCode;
        target.EvidenceLevelCode = profile.EvidenceLevelCode;
        target.ConfidenceScore = profile.ConfidenceScore;
        target.IsEstimate = true;
        target.RequiresSupplierConfirmation = true;
        target.AssumptionNote = profile.AssumptionNote;
        target.EvidenceJson = JsonSerializer.Serialize(profile.Evidence, JsonOptions);
        target.ContainerEstimatesJson = JsonSerializer.Serialize(estimates, JsonOptions);
        target.AnalyzedAtUtc = analyzedAtUtc;
        target.UpdatedAtUtc = analyzedAtUtc;
    }

    private static 농수산물포장Fcl분석항목Response Map(
        농수산물포장Fcl분석Snapshot source)
        => new()
        {
            CategoryCode = source.CategoryCode,
            CategoryName = source.CategoryName,
            ItemCode = source.ItemCode,
            ItemName = source.ItemName,
            KamisPriceComparisonUnits = Deserialize<string>(source.KamisPriceComparisonUnitsJson),
            KamisKindNames = Deserialize<string>(source.KamisKindNamesJson),
            RepresentativePackage = new 농수산물대표포장Response
            {
                PackageTypeCode = source.PackageTypeCode,
                PackageUnitLabel = source.PackageUnitLabel,
                NetContentWeightKg = source.NetContentWeightKg,
                GrossWeightKg = source.GrossWeightKg,
                UnitsPerPackage = source.UnitsPerPackage,
                UnitCountLabel = NullIfEmpty(source.UnitCountLabel),
                LengthMm = source.LengthMm,
                WidthMm = source.WidthMm,
                HeightMm = source.HeightMm,
                TemperatureCode = source.TemperatureCode,
                Stackable = source.Stackable,
                MaxStackLayers = source.MaxStackLayers,
                PackingMethodCode = source.PackingMethodCode
            },
            ContainerEstimates = Deserialize<농수산물Fcl적재추정Response>(
                source.ContainerEstimatesJson),
            EvidenceLevelCode = source.EvidenceLevelCode,
            ConfidenceScore = source.ConfidenceScore,
            IsEstimate = source.IsEstimate,
            RequiresSupplierConfirmation = source.RequiresSupplierConfirmation,
            AssumptionNote = source.AssumptionNote,
            Evidence = Deserialize<농수산물포장근거Response>(source.EvidenceJson),
            AnalyzedAtUtc = source.AnalyzedAtUtc
        };

    private static IReadOnlyList<T> Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T[]>(json, JsonOptions) ?? [];

    private static string? NullIfEmpty(string value)
        => value.Length == 0 ? null : value;

    private static string BuildAnalysisKey(int sourceYear, string itemCode)
        => $"{CurrentProfileVersion}:{sourceYear}:KAMIS:{itemCode}";

    private static 농수산물포장Fcl분석목록Response EmptyResponse(int sourceYear)
        => new()
        {
            ProfileVersion = CurrentProfileVersion,
            SourceYear = sourceYear,
            Notices = BuildNotices()
        };

    private static IReadOnlyList<string> BuildNotices()
        =>
        [
            "FCL은 컨테이너 전체를 한 화주가 예약하는 방식이며 법정 최소중량을 뜻하지 않습니다.",
            "PlanningFcl 값은 실무 최대 적재량의 85%를 계획 기준으로 둔 추정치입니다.",
            "KAMIS 가격 비교 단위와 품종명은 공급자의 실제 외포장 제원이 아닙니다.",
            "미국 도로운송 한도는 일반 지침이며 실제 장비·주·지역 규정에 따라 달라집니다.",
            "발주나 선적 예약 전 공급자 포장명세서, 팔레타이징, 포워더 적재계획과 중량 제한을 다시 확인해야 합니다."
        ];

    internal sealed record 분석대상품목(
        string CategoryCode,
        string CategoryName,
        string ItemCode,
        string ItemName,
        IReadOnlyList<string> KamisPriceComparisonUnits,
        IReadOnlyList<string> KamisKindNames);
}

public sealed record 농수산물대표포장제원(
    string PackageTypeCode,
    string PackageUnitLabel,
    decimal NetContentWeightKg,
    decimal GrossWeightKg,
    int? UnitsPerPackage,
    string? UnitCountLabel,
    int LengthMm,
    int WidthMm,
    int HeightMm,
    string TemperatureCode,
    bool Stackable,
    int MaxStackLayers,
    string PackingMethodCode,
    string EvidenceLevelCode,
    decimal ConfidenceScore,
    string AssumptionNote,
    IReadOnlyList<농수산물포장근거Response> Evidence);

internal static class 농수산물대표포장추론Catalog
{
    private const string Rigid = "RigidOrthogonal";
    private const string Flexible = "FlexibleVolume";

    private static readonly HashSet<string> Leafy = new(StringComparer.Ordinal)
    {
        "배추", "양배추", "시금치", "상추", "얼갈이배추", "갓", "열무", "미나리",
        "깻잎", "알배기배추", "브로콜리"
    };

    private static readonly HashSet<string> RootBulb = new(StringComparer.Ordinal)
    {
        "고구마", "감자", "무", "당근", "피마늘", "양파", "생강", "깐마늘(국산)"
    };

    private static readonly HashSet<string> DelicateVegetables = new(StringComparer.Ordinal)
    {
        "딸기", "방울토마토"
    };

    private static readonly HashSet<string> DriedSeafood = new(StringComparer.Ordinal)
    {
        "마른멸치", "북어", "마른오징어", "김", "마른미역", "건다시마"
    };

    public static 농수산물대표포장제원 추론(
        농수산물포장Fcl분석Service.분석대상품목 item)
    {
        var evidence = BaseEvidence();
        var name = item.ItemName;
        if (name == "바나나")
        {
            return Profile(
                "VentilatedCarton",
                "carton",
                18.14m,
                19m,
                null,
                null,
                508,
                330,
                184,
                농수산물포장온도코드.냉장,
                8,
                Rigid,
                농수산물포장근거수준코드.공식대표규격,
                0.62m,
                "FAO가 제시한 바나나 골판지 상자 외형을 사용했다. 순중량은 대표 거래중량 가정이므로 공급자 확인이 필요하다.",
                evidence);
        }

        if (name is "감자" or "고구마")
        {
            evidence.Add(UsdaVegetableEvidence());
            return Profile(
                "VentilatedCarton",
                "carton",
                22.68m,
                23.5m,
                null,
                null,
                600,
                400,
                300,
                농수산물포장온도코드.냉장,
                7,
                Rigid,
                농수산물포장근거수준코드.공식대표규격,
                0.58m,
                "USDA의 50lb 감자 포장과 FAO 600×400mm 모듈을 조합한 대표값이다. 한국 공급자의 실제 상자 치수는 확인되지 않았다.",
                evidence);
        }

        if (name == "양파")
        {
            evidence.Add(UsdaVegetableEvidence());
            return Profile(
                "MeshSackInShippingContainer",
                "shipping carton",
                22.68m,
                23.4m,
                null,
                null,
                600,
                400,
                300,
                농수산물포장온도코드.냉장,
                7,
                Rigid,
                농수산물포장근거수준코드.공식대표규격,
                0.55m,
                "USDA의 10×5lb 망 포장과 FAO 물류 모듈을 결합한 대표 외포장이다.",
                evidence);
        }

        if (name == "오이")
        {
            return Profile(
                "VentilatedCarton",
                "carton",
                13.5m,
                14.3m,
                85,
                "개",
                500,
                300,
                300,
                농수산물포장온도코드.냉장,
                6,
                Rigid,
                농수산물포장근거수준코드.공식대표규격,
                0.58m,
                "FAO 운송포장 표의 500×300×300mm, 오이 85개 대표 규격이다.",
                evidence);
        }

        if (name == "토마토")
        {
            return Profile(
                "VentilatedCarton",
                "carton",
                13.38m,
                14.1m,
                null,
                null,
                500,
                300,
                230,
                농수산물포장온도코드.냉장,
                7,
                Rigid,
                농수산물포장근거수준코드.공식대표규격,
                0.6m,
                "FAO 운송포장 표의 500×300×230mm, 약 29.5lb 토마토 대표 규격이다.",
                evidence);
        }

        if (name is "피망" or "파프리카" or "풋고추" or "붉은고추")
        {
            return Profile(
                "VentilatedCarton",
                "carton",
                11.7m,
                12.4m,
                name == "피망" ? 75 : null,
                name == "피망" ? "개" : null,
                500,
                300,
                300,
                농수산물포장온도코드.냉장,
                6,
                Rigid,
                농수산물포장근거수준코드.공식대표규격,
                0.52m,
                "FAO 고추류 운송포장 표를 같은 품목군의 대표 규격으로 적용했다.",
                evidence);
        }

        if (name == "멜론")
        {
            return Profile(
                "VentilatedCarton",
                "carton",
                8m,
                8.7m,
                5,
                "개",
                500,
                400,
                210,
                농수산물포장온도코드.냉장,
                7,
                Rigid,
                농수산물포장근거수준코드.공식대표규격,
                0.56m,
                "FAO 허니듀 멜론 500×400×210mm, 5개 규격을 멜론 대표값으로 적용했다.",
                evidence);
        }

        if (name == "수박")
        {
            return Profile(
                "VentilatedCarton",
                "carton",
                16m,
                17m,
                2,
                "개",
                600,
                400,
                350,
                농수산물포장온도코드.냉장,
                5,
                Rigid,
                농수산물포장근거수준코드.품목군추론,
                0.35m,
                "대형 과채류용 600×400mm 모듈 상자에 수박 2개를 넣는 보수적 가정이다.",
                evidence);
        }

        if (Leafy.Contains(name))
        {
            var cabbage = name.Contains("배추", StringComparison.Ordinal)
                          || name == "양배추";
            return Profile(
                "VentilatedCrate",
                "crate",
                cabbage ? 18m : 10m,
                cabbage ? 19m : 11m,
                cabbage ? 18 : null,
                cabbage ? "포기" : null,
                600,
                400,
                300,
                농수산물포장온도코드.냉장,
                5,
                Rigid,
                농수산물포장근거수준코드.품목군추론,
                cabbage ? 0.52m : 0.42m,
                cabbage
                    ? "FAO 600×400×300mm 배추류 포장 예시를 적용했다. 포기당 크기에 따라 개수와 중량 차이가 크다."
                    : "FAO 600×400mm 신선 농산물 물류 모듈을 잎채소 품목군에 적용했다.",
                evidence);
        }

        if (RootBulb.Contains(name))
        {
            return Profile(
                "VentilatedCarton",
                "carton",
                20m,
                20.8m,
                null,
                null,
                600,
                400,
                250,
                농수산물포장온도코드.냉장,
                7,
                Rigid,
                농수산물포장근거수준코드.품목군추론,
                0.4m,
                "뿌리·구근류 20kg 대표 포장과 FAO 600×400mm 물류 모듈을 결합한 추정값이다.",
                evidence);
        }

        if (DelicateVegetables.Contains(name))
        {
            return Profile(
                "TrayMasterCarton",
                "master carton",
                5m,
                5.7m,
                null,
                null,
                500,
                300,
                160,
                농수산물포장온도코드.냉장,
                6,
                Rigid,
                농수산물포장근거수준코드.품목군추론,
                0.38m,
                "연약 과채류의 소포장 트레이를 담는 대표 마스터 상자 추정값이다.",
                evidence);
        }

        if (item.CategoryCode == "200")
        {
            var dry = name is "건고추" or "고춧가루";
            return Profile(
                dry ? "DryGoodsCarton" : "VentilatedCarton",
                "carton",
                dry ? 10m : 12m,
                dry ? 10.7m : 12.8m,
                null,
                null,
                500,
                300,
                dry ? 250 : 300,
                dry ? 농수산물포장온도코드.상온 : 농수산물포장온도코드.냉장,
                dry ? 8 : 6,
                Rigid,
                농수산물포장근거수준코드.품목군추론,
                0.34m,
                "채소류의 대표 외포장 추정값으로 실제 품종·선별등급·산지 포장에 따라 달라진다.",
                evidence);
        }

        if (item.CategoryCode == "100")
        {
            return Profile(
                "WovenSack",
                "sack",
                20m,
                20.35m,
                null,
                null,
                600,
                400,
                120,
                농수산물포장온도코드.상온,
                18,
                Flexible,
                농수산물포장근거수준코드.품목군추론,
                0.4m,
                "곡류·두류 20kg 마대의 대표 외형이다. 유연 포장은 직교 상자 적재가 아니라 체적 효율로 계산했다.",
                evidence);
        }

        if (item.CategoryCode == "300")
        {
            var mushroom = name.Contains("버섯", StringComparison.Ordinal);
            return Profile(
                mushroom ? "TrayMasterCarton" : "DryGoodsCarton",
                "carton",
                mushroom ? 5m : 20m,
                mushroom ? 5.7m : 20.7m,
                null,
                null,
                mushroom ? 500 : 600,
                mushroom ? 300 : 400,
                200,
                mushroom ? 농수산물포장온도코드.냉장 : 농수산물포장온도코드.상온,
                mushroom ? 7 : 8,
                Rigid,
                농수산물포장근거수준코드.품목군추론,
                mushroom ? 0.36m : 0.38m,
                mushroom
                    ? "버섯류 소포장을 담는 5kg 마스터 상자 추정값이다."
                    : "종실·견과류 20kg 골판지 외포장의 대표 추정값이다.",
                evidence);
        }

        if (item.CategoryCode == "400")
        {
            if (name is "사과" or "배")
            {
                evidence.Add(UsdaFruitEvidence());
                return Profile(
                    "TrayPackCarton",
                    "carton",
                    name == "배" ? 20.32m : 18.14m,
                    name == "배" ? 21.2m : 19m,
                    null,
                    null,
                    name == "배" ? 400 : 600,
                    name == "배" ? 300 : 400,
                    name == "배" ? 300 : 250,
                    농수산물포장온도코드.냉장,
                    7,
                    Rigid,
                    농수산물포장근거수준코드.공식대표규격,
                    0.58m,
                    name == "배"
                        ? "FAO의 배 400×300×300mm, 44.8lb 대표 규격이다."
                        : "USDA 40lb 사과 포장 중량과 FAO 600×400mm 물류 모듈을 결합했다.",
                    evidence);
            }

            var delicate = name is "복숭아" or "포도" or "체리";
            return Profile(
                delicate ? "TrayPackCarton" : "VentilatedCarton",
                "carton",
                delicate ? 10m : 18m,
                delicate ? 10.8m : 18.9m,
                null,
                null,
                500,
                delicate ? 300 : 400,
                delicate ? 160 : 300,
                농수산물포장온도코드.냉장,
                delicate ? 6 : 7,
                Rigid,
                농수산물포장근거수준코드.품목군추론,
                0.38m,
                "과일류의 대표 수출용 골판지 상자 추정값이며 품종·과수 크기·트레이 수에 따라 달라진다.",
                evidence);
        }

        if (item.CategoryCode == "500")
        {
            if (name == "계란")
            {
                evidence.Add(EggEvidence());
                return Profile(
                    "EggCase",
                    "case",
                    21.6m,
                    23m,
                    360,
                    "개",
                    600,
                    400,
                    300,
                    농수산물포장온도코드.냉장,
                    6,
                    Rigid,
                    농수산물포장근거수준코드.공식대표규격,
                    0.48m,
                    "USDA가 설명하는 30-dozen 표준 계란 케이스와 60g/개 가정을 결합했다. 외형은 대표 물류 모듈 추정이다.",
                    evidence);
            }

            if (name == "우유")
            {
                return Profile(
                    "LiquidCase",
                    "case",
                    12m,
                    13m,
                    12,
                    "1L팩",
                    400,
                    300,
                    250,
                    농수산물포장온도코드.냉장,
                    7,
                    Rigid,
                    농수산물포장근거수준코드.공급자확인필요,
                    0.25m,
                    "소매용 1L팩 12개 마스터 상자를 가정했다. 원유·벌크·멸균유 거래에는 적용할 수 없다.",
                    evidence);
            }

            return Profile(
                "FrozenMeatMasterCarton",
                "carton",
                20m,
                21m,
                null,
                null,
                600,
                400,
                250,
                농수산물포장온도코드.냉동,
                8,
                Rigid,
                농수산물포장근거수준코드.공급자확인필요,
                0.25m,
                "KAMIS 축산 품목은 생체·도체·부위육 의미가 섞일 수 있어 냉동 부위육 20kg 마스터 상자를 대체값으로만 적용했다.",
                evidence);
        }

        if (item.CategoryCode == "600")
        {
            if (DriedSeafood.Contains(name))
            {
                return Profile(
                    "DrySeafoodCarton",
                    "carton",
                    10m,
                    11m,
                    null,
                    null,
                    600,
                    400,
                    300,
                    농수산물포장온도코드.상온,
                    7,
                    Rigid,
                    농수산물포장근거수준코드.품목군추론,
                    0.33m,
                    "건수산물 10kg 마스터 상자 대표 추정값이다. 건조도와 압축 정도가 체적을 크게 바꾼다.",
                    evidence);
            }

            if (name is "새우젓" or "멸치액젓" or "천일염")
            {
                return Profile(
                    name == "천일염" ? "WovenSack" : "LiquidPailCarton",
                    name == "천일염" ? "sack" : "case",
                    20m,
                    21.5m,
                    null,
                    null,
                    name == "천일염" ? 600 : 400,
                    name == "천일염" ? 400 : 300,
                    name == "천일염" ? 150 : 350,
                    농수산물포장온도코드.상온,
                    name == "천일염" ? 15 : 6,
                    name == "천일염" ? Flexible : Rigid,
                    농수산물포장근거수준코드.품목군추론,
                    0.3m,
                    "염장·액상 또는 소금 제품의 20kg 대표 운송포장 추정값이다.",
                    evidence);
            }

            return Profile(
                "InsulatedSeafoodCarton",
                "insulated carton",
                20m,
                22m,
                null,
                null,
                600,
                400,
                250,
                농수산물포장온도코드.냉동,
                7,
                Rigid,
                농수산물포장근거수준코드.공급자확인필요,
                0.28m,
                "냉동 수산물 20kg 보냉 마스터 상자를 대체값으로 적용했다. 생물·활어·빙장 거래에는 적용할 수 없다.",
                evidence);
        }

        return Profile(
            "RepresentativeCarton",
            "carton",
            10m,
            11m,
            null,
            null,
            600,
            400,
            300,
            농수산물포장온도코드.상온,
            6,
            Rigid,
            농수산물포장근거수준코드.공급자확인필요,
            0.2m,
            "품목군을 식별하지 못해 일반 운송상자를 대체값으로 적용했다.",
            evidence);
    }

    private static 농수산물대표포장제원 Profile(
        string packageTypeCode,
        string packageUnitLabel,
        decimal netWeightKg,
        decimal grossWeightKg,
        int? unitsPerPackage,
        string? unitCountLabel,
        int lengthMm,
        int widthMm,
        int heightMm,
        string temperatureCode,
        int maxStackLayers,
        string packingMethodCode,
        string evidenceLevelCode,
        decimal confidence,
        string note,
        IReadOnlyList<농수산물포장근거Response> evidence)
        => new(
            packageTypeCode,
            packageUnitLabel,
            netWeightKg,
            grossWeightKg,
            unitsPerPackage,
            unitCountLabel,
            lengthMm,
            widthMm,
            heightMm,
            temperatureCode,
            true,
            maxStackLayers,
            packingMethodCode,
            evidenceLevelCode,
            confidence,
            note,
            evidence);

    private static List<농수산물포장근거Response> BaseEvidence()
        =>
        [
            Evidence(
                "KAMIS",
                "한국농수산식품유통공사 KAMIS",
                "https://www.kamis.or.kr/customer/price/agricultureRetail/catalogue.do",
                "분석 대상 품목코드, 품목명, 가격 비교단위와 품종명",
                "가격 조사단위는 공급자의 수출 외포장 제원과 같지 않다."),
            Evidence(
                "FAO-PACKAGING-MODULE",
                "FAO Packaging for fruits, vegetables and root crops",
                "https://www.fao.org/4/x5016e/X5016E05.htm",
                "600×400mm 운송포장 모듈과 대표 품목별 포장 외형",
                "국가·산지·업체별 실제 포장명세를 대신하지 않는 참고 규격이다."),
            Evidence(
                "MAERSK-CONTAINER-PAYLOAD",
                "Maersk container cargo weight limits",
                "https://www.maersk.com/support/faqs/2023/10/09/cargo-weight-limit",
                "20·40ft dry/reefer 용적과 최대 payload",
                "국가와 실제 장비에 따라 한도가 달라질 수 있다."),
            Evidence(
                "HAPAG-CONTAINER-DIMENSIONS",
                "Hapag-Lloyd container specifications",
                "https://www.hapag-lloyd.com/en/services-information/cargo-fleet/container.html",
                "대표 컨테이너 내부 길이·폭·높이",
                "제조사와 실제 배정 장비에 따라 치수가 달라질 수 있다."),
            Evidence(
                "OCEMA-US-ROAD-WEIGHT",
                "OCEMA recommended U.S. highway cargo weights",
                "https://www.maersk.com/~/media_sc9/maersk/local-information/files/north-america/united-states-of-america/local-solutions/customs-services/ocema.pdf",
                "미국 도로운송 시 일반 chassis 기준 권장 화물중량",
                "제품·포장·팔레트·고정재를 포함한 일반 지침이며 실제 연방·주·지역 한도와 장비를 확인해야 한다.")
        ];

    private static 농수산물포장근거Response UsdaVegetableEvidence()
        => Evidence(
            "USDA-FRESH-VEGETABLES-2019",
            "USDA AMS Commodity Specification for Fresh Vegetables",
            "https://www.ams.usda.gov/sites/default/files/media/CommoditySpecificationforFreshVegetablesMay2019.pdf",
            "감자 50lb, 양파 10×5lb 등 조달 포장 중량과 48×40in 팔레트",
            "미국 조달 규격이며 한국 공급자의 실제 수출 포장을 확정하지 않는다.");

    private static 농수산물포장근거Response UsdaFruitEvidence()
        => Evidence(
            "USDA-FRESH-FRUIT",
            "USDA fresh fruit procurement references",
            "https://www.ams.usda.gov/selling-food/product-specs",
            "사과 등 신선 과일의 대표 조달 포장 중량",
            "미국 조달 규격이며 실제 공급계약 포장명세를 대신하지 않는다.");

    private static 농수산물포장근거Response EggEvidence()
        => Evidence(
            "USDA-EGG-TERMS",
            "USDA AMS poultry and egg terms",
            "https://www.ams.usda.gov/market-news/livestock-poultry-and-grain-poultry-and-egg-terms",
            "표준 계란 case 30 dozen",
            "계란 크기와 실제 케이스 외형은 공급자별로 달라진다.");

    private static 농수산물포장근거Response Evidence(
        string key,
        string name,
        string url,
        string supports,
        string limitation)
        => new()
        {
            SourceKey = key,
            SourceName = name,
            SourceUrl = url,
            Supports = supports,
            Limitation = limitation,
            RetrievedAtUtc = DateTime.UtcNow
        };
}

public static class 농수산물포장Fcl계산기
{
    private const decimal PlanningFillRate = 0.85m;

    private static readonly IReadOnlyList<ContainerSpec> DryContainers =
    [
        new(
            "20GP",
            "20ft standard dry",
            농수산물포장온도코드.상온,
            5900,
            2352,
            2395,
            33.2m,
            28300m,
            17780m,
            0.9m),
        new(
            "40GP",
            "40ft standard dry",
            농수산물포장온도코드.상온,
            12032,
            2352,
            2395,
            67.7m,
            28870m,
            19960m,
            0.9m),
        new(
            "40HC",
            "40ft high-cube dry",
            농수산물포장온도코드.상온,
            12032,
            2350,
            2700,
            76.4m,
            28690m,
            19820m,
            0.9m)
    ];

    private static readonly IReadOnlyList<ContainerSpec> ReeferContainers =
    [
        new(
            "20RF",
            "20ft reefer",
            농수산물포장온도코드.냉장,
            5450,
            2280,
            2159,
            28.3m,
            27770m,
            15830m,
            0.8m),
        new(
            "40HCRF",
            "40ft high-cube reefer",
            농수산물포장온도코드.냉장,
            11599,
            2290,
            2425,
            67.5m,
            29670m,
            17830m,
            0.8m)
    ];

    public static IReadOnlyList<농수산물Fcl적재추정Response> 계산(
        농수산물대표포장제원 package)
    {
        ArgumentNullException.ThrowIfNull(package);
        Validate(package);

        var containers = package.TemperatureCode == 농수산물포장온도코드.상온
            ? DryContainers
            : ReeferContainers;
        return containers.Select(container => Calculate(package, container)).ToArray();
    }

    private static 농수산물Fcl적재추정Response Calculate(
        농수산물대표포장제원 package,
        ContainerSpec container)
    {
        var spatialLimit = package.PackingMethodCode == "FlexibleVolume"
            ? CalculateFlexibleVolumeLimit(package, container)
            : CalculateRigidGeometryLimit(package, container);
        var oceanWeightLimit = decimal.ToInt32(decimal.Floor(
            container.OceanEquipmentPayloadKg / package.GrossWeightKg));
        var roadWeightLimit = container.UnitedStatesRoadCargoWeightLimitKg.HasValue
            ? decimal.ToInt32(decimal.Floor(
                container.UnitedStatesRoadCargoWeightLimitKg.Value / package.GrossWeightKg))
            : oceanWeightLimit;
        var oceanMaximum = Math.Max(1, Math.Min(spatialLimit, oceanWeightLimit));
        var practicalMaximum = Math.Max(1, Math.Min(oceanMaximum, roadWeightLimit));
        var planningCount = Math.Max(
            1,
            decimal.ToInt32(decimal.Floor(practicalMaximum * PlanningFillRate)));
        var limitingFactor = practicalMaximum == roadWeightLimit
            ? "UnitedStatesRoadWeight"
            : practicalMaximum == oceanWeightLimit
                ? "OceanEquipmentPayload"
                : package.PackingMethodCode == "FlexibleVolume"
                    ? "UsableVolume"
                    : "PackageGeometry";

        return new 농수산물Fcl적재추정Response
        {
            ContainerCode = container.Code,
            ContainerName = container.Name,
            TemperatureCode = container.TemperatureCode,
            InternalLengthMm = container.InternalLengthMm,
            InternalWidthMm = container.InternalWidthMm,
            InternalHeightMm = container.InternalHeightMm,
            NominalCapacityCbm = container.NominalCapacityCbm,
            OceanEquipmentPayloadKg = container.OceanEquipmentPayloadKg,
            UnitedStatesRoadCargoWeightLimitKg = container.UnitedStatesRoadCargoWeightLimitKg,
            LoadingEfficiencyRate = container.LoadingEfficiencyRate,
            OceanMaximumPackageCount = oceanMaximum,
            OceanMaximumNetWeightKg = Round(package.NetContentWeightKg * oceanMaximum),
            OceanMaximumGrossWeightKg = Round(package.GrossWeightKg * oceanMaximum),
            PracticalMaximumPackageCount = practicalMaximum,
            PracticalMaximumNetWeightKg = Round(package.NetContentWeightKg * practicalMaximum),
            PracticalMaximumGrossWeightKg = Round(package.GrossWeightKg * practicalMaximum),
            PracticalMaximumUnitCount = MultiplyUnits(
                package.UnitsPerPackage,
                practicalMaximum),
            PlanningFillRate = PlanningFillRate,
            PlanningFclPackageCount = planningCount,
            PlanningFclNetWeightKg = Round(package.NetContentWeightKg * planningCount),
            PlanningFclGrossWeightKg = Round(package.GrossWeightKg * planningCount),
            PlanningFclUnitCount = MultiplyUnits(package.UnitsPerPackage, planningCount),
            LimitingFactorCode = limitingFactor,
            Warnings =
            [
                "동일 포장만 적재하고 팔레트 없이 floor-loading하는 계산이다.",
                "미국 도로 한도에는 제품·포장·팔레트·고정재 중량이 포함되므로 실제 포장명세서와 chassis를 확인해야 한다.",
                package.TemperatureCode == 농수산물포장온도코드.상온
                    ? "식품용 dry container 적합성, 습기·환기·방충 조건은 별도 확인이 필요하다."
                    : "reefer의 냉기 순환선, 환기, set point와 적재 금지선을 반영한 실제 적재계획이 필요하다."
            ]
        };
    }

    private static int CalculateFlexibleVolumeLimit(
        농수산물대표포장제원 package,
        ContainerSpec container)
    {
        var packageCbm = package.LengthMm / 1000m
                         * (package.WidthMm / 1000m)
                         * (package.HeightMm / 1000m);
        return decimal.ToInt32(decimal.Floor(
            container.NominalCapacityCbm
            * container.LoadingEfficiencyRate
            / packageCbm));
    }

    private static int CalculateRigidGeometryLimit(
        농수산물대표포장제원 package,
        ContainerSpec container)
    {
        var normalPerLayer =
            container.InternalLengthMm / package.LengthMm
            * (container.InternalWidthMm / package.WidthMm);
        var rotatedPerLayer =
            container.InternalLengthMm / package.WidthMm
            * (container.InternalWidthMm / package.LengthMm);
        var layers = Math.Min(
            container.InternalHeightMm / package.HeightMm,
            package.MaxStackLayers);
        var exactGeometryCount = Math.Max(normalPerLayer, rotatedPerLayer) * layers;
        return decimal.ToInt32(decimal.Floor(
            exactGeometryCount * container.LoadingEfficiencyRate));
    }

    private static long? MultiplyUnits(int? unitsPerPackage, int packageCount)
        => unitsPerPackage.HasValue
            ? checked((long)unitsPerPackage.Value * packageCount)
            : null;

    private static void Validate(농수산물대표포장제원 package)
    {
        if (package.NetContentWeightKg <= 0
            || package.GrossWeightKg < package.NetContentWeightKg
            || package.LengthMm <= 0
            || package.WidthMm <= 0
            || package.HeightMm <= 0
            || package.MaxStackLayers <= 0)
        {
            throw new ArgumentException("FCL 계산에는 유효한 포장 중량·외형·적층수가 필요합니다.");
        }
    }

    private static decimal Round(decimal value)
        => decimal.Round(value, 3, MidpointRounding.AwayFromZero);

    private sealed record ContainerSpec(
        string Code,
        string Name,
        string TemperatureCode,
        int InternalLengthMm,
        int InternalWidthMm,
        int InternalHeightMm,
        decimal NominalCapacityCbm,
        decimal OceanEquipmentPayloadKg,
        decimal? UnitedStatesRoadCargoWeightLimitKg,
        decimal LoadingEfficiencyRate);
}
