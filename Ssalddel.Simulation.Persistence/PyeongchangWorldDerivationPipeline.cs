using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed record 평창군공간파생PipelineResult(
    string 상태코드,
    string 파생실행고유식별자,
    bool 새실행본저장여부,
    int 건축물수,
    int 대표건축물수,
    int 대표군수,
    int 표현제외건축물수,
    int 공개사업장수,
    int 대표공개사업장수,
    int 사업장건물연결수,
    int 미배치건축물수,
    int Unity공간변환Profile수,
    int Unity타일Manifest수,
    int Unity산출물수,
    string 출력해시SHA256);

public sealed record 대표건축물선정항목(
    Ssalddel.Domain.PublicData.Korea.건축물대장표제부Record Building,
    string GroupCode,
    int RepresentedRecordCount,
    int Rank);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.SimulationWorldDerivation,
    SsalddelCodeLayer.Application,
    "평창군 공공데이터를 읽어 대표 건물·공간 관계·Unity 타일 계획을 결정적으로 조립한다.",
    StepKey = "application.pyeongchang-derivation",
    DependsOnStepKeys = new string[] { "domain.derived-world-ledger" },
    ExecutionStage = SsalddelCodeExecutionStage.Projection,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ReadsFrom = SsalddelCodeDataScope.SharedPublicData,
    WritesTo = SsalddelCodeDataScope.DerivedWorld,
    FlowOrder = 20,
    Boundary = "공유 공공데이터는 읽기 전용이며 건물 도형이나 DEM이 없으면 임의 좌표를 생성하지 않는다.")]
[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI가 요구하는 AreaSet·Graph·배치·통행 문맥의 파생 근거를 제공한다.",
    Boundary = "파생 공간을 저장해도 WI의 권위 전이·Task·Effect·결과 발현을 증명하지 않는다.")]
public sealed class 평창군공간파생Pipeline(
    PublicDataIngestionDbContext publicDataDb,
    ISimulationWorld파생원장Store derivedStore)
{
    public const string 완료 = "Completed";
    public const string 자료부족 = "InsufficientSourceData";
    private const string 평창군Code = "51760";
    private const string SourceStableId = "source:shared-public-data:pyeongchang-building-business";
    private const string 공간RecipeRevision = "pyeongchang-completion-area-pipeline.v7";
    private const string 공간RuleRevision = "region-first-world-projection.v7";

    public Task<평창군공간파생PipelineResult> 실행Async(CancellationToken cancellationToken)
        => 실행Async(null, null, cancellationToken);

    public async Task<평창군공간파생PipelineResult> 실행Async(
        string? tileManifestPath,
        CancellationToken cancellationToken)
        => await 실행Async(tileManifestPath, null, cancellationToken);

    public async Task<평창군공간파생PipelineResult> 실행Async(
        string? tileManifestPath,
        string? spatialArtifactManifestPath,
        CancellationToken cancellationToken)
    {
        var buildings = await publicDataDb.BuildingRegisterTitles.AsNoTracking()
            .Where(item => item.SigunguCode == 평창군Code && item.ValidToUtc == null)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var buildingIds = buildings.Select(item => item.Id).ToList();
        var regionAssignments = await publicDataDb.BuildingRegionAssignments.AsNoTracking()
            .Where(item => buildingIds.Contains(item.BuildingRecordId) && item.ValidToUtc == null)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var administrativeRegionIds = regionAssignments
            .Select(item => item.AdministrativeRegionStableId)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var administrativeAggregates = await publicDataDb.AdministrativeBuildingCategoryAggregates
            .AsNoTracking()
            .Where(item => administrativeRegionIds.Contains(item.AdministrativeRegionStableId))
            .OrderBy(item => item.AdministrativeRegionStableId)
            .ThenBy(item => item.CategoryCode)
            .ToListAsync(cancellationToken);
        var categories = await publicDataDb.BuildingCategoryAssignments.AsNoTracking()
            .Where(item => buildingIds.Contains(item.BuildingRecordId) && item.IsPrimary)
            .ToDictionaryAsync(item => item.BuildingRecordId, item => item.CategoryCode, cancellationToken);
        var profiles = await publicDataDb.건축물형태Profiles.AsNoTracking()
            .Where(item => buildingIds.Contains(item.건축물RecordId))
            .ToDictionaryAsync(item => item.건축물RecordId, cancellationToken);
        var profileIds = profiles.Values.Select(item => item.Id).ToList();
        var compositions = await publicDataDb.건축물시각구성계획들.AsNoTracking()
            .Where(item => profileIds.Contains(item.건축물형태ProfileId))
            .ToDictionaryAsync(item => item.건축물형태ProfileId, cancellationToken);
        var assignments = await publicDataDb.공개사업장건축물Assignments.AsNoTracking()
            .Where(item => item.BuildingRecordId != null && buildingIds.Contains(item.BuildingRecordId.Value))
            .Include(item => item.BusinessRecord)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var representativeBuildings = SelectOneRepresentativePerBuildingCategory(buildings, categories);
        var representativeBuildingIds = representativeBuildings
            .Select(item => item.Building.Id).ToHashSet();
        var representativeAssignments = assignments
            .Where(item => item.BuildingRecordId.HasValue
                && representativeBuildingIds.Contains(item.BuildingRecordId.Value))
            .ToArray();

        var tileManifest = string.IsNullOrWhiteSpace(tileManifestPath)
            ? null
            : await ReadTileManifestAsync(tileManifestPath, cancellationToken);
        var spatialArtifacts = string.IsNullOrWhiteSpace(spatialArtifactManifestPath)
            ? null
            : await ReadSpatialArtifactManifestAsync(spatialArtifactManifestPath, cancellationToken);
        if (spatialArtifacts != null
            && (tileManifest == null
                || !tileManifest.Tiles.Any(item => item.TileKey == spatialArtifacts.TileKey)))
            throw new InvalidDataException("PyeongchangSpatialArtifactTileManifestMissing");
        var publicDataSourceHash = SourceHash(
            buildings,
            categories,
            profiles,
            compositions,
            assignments,
            regionAssignments,
            administrativeAggregates);
        var sourceHash = tileManifest == null
            ? publicDataSourceHash
            : Sha256(publicDataSourceHash + "|tile-manifest:" + tileManifest.ManifestFileHashSha256);
        if (spatialArtifacts != null)
            sourceHash = Sha256(sourceHash + "|spatial-artifacts:" + spatialArtifacts.ManifestFileHashSha256);
        var spatialInputFingerprint = Sha256(
            sourceHash + "|schema:2|recipe:" + 공간RecipeRevision + "|rule:" + 공간RuleRevision);
        var generatedAt = buildings.Select(item => item.ObservedAtUtc)
            .Concat(assignments.Select(item => item.BusinessRecord.ObservedAtUtc))
            .Concat(administrativeAggregates.Select(item => item.GeneratedAtUtc))
            .DefaultIfEmpty(DateTimeOffset.UnixEpoch)
            .Max();
        var status = buildings.Count == 0 ? 자료부족 : 완료;
        var ledger = BuildLedger(
            representativeBuildings,
            representativeAssignments,
            regionAssignments,
            administrativeAggregates,
            tileManifest,
            spatialArtifacts,
            publicDataSourceHash,
            spatialInputFingerprint,
            generatedAt,
            status);
        var stored = await derivedStore.저장Async(ledger, cancellationToken);

        return new 평창군공간파생PipelineResult(
            status,
            stored.BuildStableId,
            stored.Inserted,
            buildings.Count,
            representativeBuildings.Count,
            representativeBuildings.Select(item => item.GroupCode).Distinct(StringComparer.Ordinal).Count(),
            buildings.Count - representativeBuildings.Count,
            assignments.Select(item => item.BusinessRecordId).Distinct().Count(),
            representativeAssignments.Select(item => item.BusinessRecordId).Distinct().Count(),
            representativeAssignments.Length,
            representativeBuildings.Count,
            stored.UnityTransformProfileCount,
            stored.UnityTileManifestCount,
            stored.UnityArtifactCount,
            stored.OutputHashSha256);
    }

    private static SimulationWorld파생원장 BuildLedger(
        IReadOnlyList<대표건축물선정항목> representativeBuildings,
        IReadOnlyList<Ssalddel.Domain.PublicData.Korea.공개사업장건축물Assignment> assignments,
        IReadOnlyList<건축물행정구역Assignment> regionAssignments,
        IReadOnlyList<행정동건축물CategoryAggregate> administrativeAggregates,
        PyeongchangTileManifest? tileManifest,
        PyeongchangSpatialArtifactManifest? spatialArtifacts,
        string publicDataSourceHash,
        string sourceHash,
        DateTimeOffset generatedAt,
        string status)
    {
        var nodes = AreaNodes().ToList();
        var relations = new List<SimulationWorld파생Relation>();
        Add경관완결영역(nodes, relations, tileManifest);
        Add지역Projection(
            nodes,
            relations,
            representativeBuildings,
            regionAssignments,
            administrativeAggregates);
        var regionAssignmentByBuilding = regionAssignments
            .GroupBy(item => item.BuildingRecordId)
            .ToDictionary(group => group.Key, group => group.First());
        foreach (var representative in representativeBuildings)
        {
            var building = representative.Building;
            var buildingNode = "building:public-data:" + building.Id.ToString("N");
            var areaNode = AreaNodeFor(building.LegalDongCode);
            nodes.Add(new SimulationWorld파생Node
            {
                StableId = buildingNode,
                NodeKindCode = "Building",
                SourceStableId = SourceStableId,
                SourceRecordStableId = building.Id.ToString("N"),
                EvidenceKindCode = SimulationWorld근거종류Codes.관측,
                RegionCode = building.LegalDongCode.Length == 10
                    ? building.LegalDongCode
                    : building.SigunguCode + building.LegalDongCode,
                AreaStableId = areaNode,
                DisplayName = building.BuildingName ?? building.MainPurposeName,
                RepresentativeGroupCode = representative.GroupCode,
                RepresentedRecordCount = representative.RepresentedRecordCount,
                RepresentativeRank = representative.Rank,
            });
            if (areaNode != null)
            {
                relations.Add(new SimulationWorld파생Relation
                {
                    StableId = "relation:" + areaNode + ":contains:" + buildingNode,
                    FromNodeStableId = areaNode,
                    RelationCode = "Contains",
                    ToNodeStableId = buildingNode,
                    EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                    SourceStableId = SourceStableId,
                    Confidence = 1m,
                });
            }
            if (regionAssignmentByBuilding.TryGetValue(building.Id, out var regionAssignment))
                Add건물지역관계(relations, buildingNode, regionAssignment);
        }

        foreach (var assignment in assignments)
        {
            var business = assignment.BusinessRecord;
            var buildingNode = "building:public-data:" + assignment.BuildingRecordId!.Value.ToString("N");
            var businessNode = "business:public-license:" + business.Id.ToString("N");
            nodes.Add(new SimulationWorld파생Node
            {
                StableId = businessNode,
                NodeKindCode = "PublicLicensedBusiness",
                SourceStableId = SourceStableId,
                SourceRecordStableId = business.Id.ToString("N"),
                EvidenceKindCode = SimulationWorld근거종류Codes.관측,
                DisplayName = business.BusinessName,
            });
            relations.Add(new SimulationWorld파생Relation
            {
                StableId = "relation:" + buildingNode + ":hosts:" + businessNode,
                FromNodeStableId = buildingNode,
                RelationCode = "HostsPublicLicensedBusiness",
                ToNodeStableId = businessNode,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                SourceStableId = SourceStableId,
                Confidence = Confidence(assignment.ConfidenceCode),
            });
        }

        if (status == 자료부족)
        {
            nodes.Add(new SimulationWorld파생Node
            {
                StableId = "data-gap:pyeongchang:building-business",
                NodeKindCode = "DataGap",
                SourceStableId = SourceStableId,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                RegionCode = 평창군Code,
                DisplayName = "평창군 건축물·공개 인허가 사업장 원본 자료 부족",
            });
        }

        return new SimulationWorld파생원장
        {
            SchemaVersion = 2,
            BuildStableId = "world-build:pyeongchang:" + sourceHash[..16],
            AreaSetStableId = "area-set:sim:pyeongchang:farm-hub-town.v1",
            RecipeRevision = 공간RecipeRevision,
            RuleRevision = 공간RuleRevision,
            Seed = 51760,
            InputFingerprintSha256 = sourceHash,
            GeneratedAtUtc = generatedAt == DateTimeOffset.UnixEpoch
                ? DateTimeOffset.Parse("2026-08-13T00:00:00Z")
                : generatedAt,
            Sources = SourceLineage(publicDataSourceHash, generatedAt, tileManifest, spatialArtifacts),
            Nodes = nodes,
            Relations = relations,
            BuildingPlacements = Array.Empty<SimulationWorld건물배치계획>(),
            GraphicsPlans = Array.Empty<SimulationWorld그래픽표현계획>(),
            UnityTransformProfiles = new[]
            {
                tileManifest == null
                    ? PendingUnityTransformProfile()
                    : TileManifestTransformProfile(tileManifest, spatialArtifacts),
            },
            UnityTileManifests = tileManifest?.Tiles ?? Array.Empty<SimulationWorldUnity타일Manifest>(),
            UnityArtifacts = spatialArtifacts?.ToDomainArtifacts()
                ?? Array.Empty<SimulationWorldUnity산출물>(),
            VisualPlacements = Array.Empty<SimulationWorld시각배치계획>(),
        };
    }

    private static void Add지역Projection(
        ICollection<SimulationWorld파생Node> nodes,
        ICollection<SimulationWorld파생Relation> relations,
        IReadOnlyList<대표건축물선정항목> representativeBuildings,
        IReadOnlyList<건축물행정구역Assignment> assignments,
        IReadOnlyList<행정동건축물CategoryAggregate> aggregates)
    {
        var representativeByBuilding = representativeBuildings
            .ToDictionary(item => item.Building.Id, item => item.Building);
        foreach (var group in assignments
                     .GroupBy(item => item.LegalRegionStableId, StringComparer.Ordinal)
                     .OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var relatedArea = group
                .Where(item => representativeByBuilding.ContainsKey(item.BuildingRecordId))
                .Select(item => AreaNodeFor(representativeByBuilding[item.BuildingRecordId].LegalDongCode))
                .FirstOrDefault(item => item != null);
            nodes.Add(new SimulationWorld파생Node
            {
                StableId = RegionNodeStableId("legal", group.Key),
                NodeKindCode = SimulationWorldRegionProjectionCodes.LegalRegion,
                SourceStableId = SourceStableId,
                SourceRecordStableId = group.Key,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                AreaStableId = relatedArea,
                DisplayName = group.Key,
            });
        }

        foreach (var regionStableId in assignments
                     .Select(item => item.AdministrativeRegionStableId)
                     .Where(item => !string.IsNullOrWhiteSpace(item))
                     .Cast<string>()
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(item => item, StringComparer.Ordinal))
        {
            nodes.Add(new SimulationWorld파생Node
            {
                StableId = RegionNodeStableId("administrative", regionStableId),
                NodeKindCode = SimulationWorldRegionProjectionCodes.AdministrativeRegion,
                SourceStableId = SourceStableId,
                SourceRecordStableId = regionStableId,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                DisplayName = regionStableId,
            });
        }

        foreach (var pair in assignments
                     .Where(item => !string.IsNullOrWhiteSpace(item.AdministrativeRegionStableId))
                     .Select(item => new
                     {
                         item.LegalRegionStableId,
                         AdministrativeRegionStableId = item.AdministrativeRegionStableId!,
                     })
                     .Distinct()
                     .OrderBy(item => item.LegalRegionStableId, StringComparer.Ordinal)
                     .ThenBy(item => item.AdministrativeRegionStableId, StringComparer.Ordinal))
        {
            var legalNode = RegionNodeStableId("legal", pair.LegalRegionStableId);
            var administrativeNode = RegionNodeStableId(
                "administrative", pair.AdministrativeRegionStableId);
            relations.Add(new SimulationWorld파생Relation
            {
                StableId = RelationStableId(
                    "region-crosswalk", legalNode, administrativeNode),
                FromNodeStableId = legalNode,
                RelationCode = "LegalAdministrativeRegionCrosswalk",
                ToNodeStableId = administrativeNode,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                SourceStableId = SourceStableId,
                Confidence = 1m,
            });
        }

        foreach (var aggregate in aggregates.Where(item => item.BuildingCount > 0))
        {
            var administrativeNode = RegionNodeStableId(
                "administrative", aggregate.AdministrativeRegionStableId);
            var aggregateNode = "region-aggregate:building-category:"
                                + aggregate.Id.ToString("N");
            nodes.Add(new SimulationWorld파생Node
            {
                StableId = aggregateNode,
                NodeKindCode = "AdministrativeRegionBuildingCategoryAggregate",
                SourceStableId = SourceStableId,
                SourceRecordStableId = aggregate.Id.ToString("N"),
                EvidenceKindCode = aggregate.EvidenceKindCode,
                DisplayName = aggregate.CategoryCode + " 건축물 집계",
                RepresentativeGroupCode = aggregate.CategoryCode,
                RepresentedRecordCount = aggregate.BuildingCount > int.MaxValue
                    ? int.MaxValue
                    : (int)aggregate.BuildingCount,
                RepresentativeRank = 1,
            });
            relations.Add(new SimulationWorld파생Relation
            {
                StableId = RelationStableId(
                    "region-building-category", administrativeNode, aggregateNode),
                FromNodeStableId = administrativeNode,
                RelationCode = "HasBuildingCategoryAggregate",
                ToNodeStableId = aggregateNode,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                SourceStableId = SourceStableId,
                Confidence = 1m,
            });
        }
    }

    private static void Add건물지역관계(
        ICollection<SimulationWorld파생Relation> relations,
        string buildingNode,
        건축물행정구역Assignment assignment)
    {
        var legalNode = RegionNodeStableId("legal", assignment.LegalRegionStableId);
        relations.Add(new SimulationWorld파생Relation
        {
            StableId = RelationStableId("building-legal-region", buildingNode, legalNode),
            FromNodeStableId = buildingNode,
            RelationCode = "LocatedInLegalRegion",
            ToNodeStableId = legalNode,
            EvidenceKindCode = SimulationWorld근거종류Codes.파생,
            SourceStableId = SourceStableId,
            Confidence = Confidence(assignment.ConfidenceCode),
        });
        if (string.IsNullOrWhiteSpace(assignment.AdministrativeRegionStableId)) return;
        var administrativeNode = RegionNodeStableId(
            "administrative", assignment.AdministrativeRegionStableId);
        relations.Add(new SimulationWorld파생Relation
        {
            StableId = RelationStableId(
                "building-administrative-region", buildingNode, administrativeNode),
            FromNodeStableId = buildingNode,
            RelationCode = "AggregatedInAdministrativeRegion",
            ToNodeStableId = administrativeNode,
            EvidenceKindCode = SimulationWorld근거종류Codes.파생,
            SourceStableId = SourceStableId,
            Confidence = Confidence(assignment.ConfidenceCode),
        });
    }

    private static string RegionNodeStableId(string regionKind, string regionStableId)
        => "region-projection:" + regionKind + ':' + Sha256(regionStableId)[..20];

    private static string RelationStableId(string kind, string from, string to)
        => "relation:" + kind + ':' + Sha256(from + '|' + to)[..24];

    private static IEnumerable<SimulationWorld파생Node> AreaNodes() =>
    [
        Area("area:sim:pyeongchang:daegwallyeong-farm", "5176038000", "대관령면 Farm"),
        Area("area:sim:pyeongchang:jinbu-hub", "5176036000", "진부면 Hub"),
        Area("area:sim:pyeongchang:pyeongchang-town", "5176025000", "평창읍 Town"),
    ];

    private static SimulationWorld파생Node Area(string id, string regionCode, string name) => new()
    {
        StableId = id,
        NodeKindCode = "Area",
        EvidenceKindCode = SimulationWorld근거종류Codes.시나리오,
        RegionCode = regionCode,
        AreaStableId = id,
        DisplayName = name,
    };

    private static void Add경관완결영역(
        ICollection<SimulationWorld파생Node> nodes,
        ICollection<SimulationWorld파생Relation> relations,
        PyeongchangTileManifest? tileManifest)
    {
        var completionAreaId = PyeongchangSimulationWorldStableIds.대관령Farm경관완결영역;
        var farmAreaId = PyeongchangSimulationWorldStableIds.대관령Farm영역;
        nodes.Add(new SimulationWorld파생Node
        {
            StableId = completionAreaId,
            NodeKindCode = "LandscapeCompletionArea",
            EvidenceKindCode = SimulationWorld근거종류Codes.시나리오,
            RegionCode = "5176038000",
            TileKey = PyeongchangSimulationWorldStableIds.대관령Farm경관완결L2타일키[0],
            AreaStableId = farmAreaId,
            DisplayName = "대관령면 Farm 경관 완결 영역 v1",
        });
        relations.Add(new SimulationWorld파생Relation
        {
            StableId = $"relation:{farmAreaId}:contains:{completionAreaId}",
            FromNodeStableId = farmAreaId,
            RelationCode = "ContainsLandscapeCompletionArea",
            ToNodeStableId = completionAreaId,
            EvidenceKindCode = SimulationWorld근거종류Codes.시나리오,
            SourceStableId = SourceStableId,
            Confidence = 1m,
        });

        if (tileManifest == null)
            return;

        var taskTileKeys = PyeongchangSimulationWorldStableIds.대관령Farm경관완결L2타일키
            .ToHashSet(StringComparer.Ordinal);
        foreach (var tile in tileManifest.Tiles.Where(item => taskTileKeys.Contains(item.TileKey)))
        {
            var tileNodeId = "spatial-tile:" + tile.TileKey;
            nodes.Add(new SimulationWorld파생Node
            {
                StableId = tileNodeId,
                NodeKindCode = "SpatialTile",
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                RegionCode = "5176038000",
                TileKey = tile.TileKey,
                AreaStableId = completionAreaId,
                DisplayName = tile.TileKey,
            });
            relations.Add(new SimulationWorld파생Relation
            {
                StableId = $"relation:{completionAreaId}:contains:{tileNodeId}",
                FromNodeStableId = completionAreaId,
                RelationCode = "ContainsSpatialTile",
                ToNodeStableId = tileNodeId,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                SourceStableId = "source:shared-public-data:pyeongchang-spatial-tile-manifest",
                Confidence = 1m,
            });
        }
    }

    private static SimulationWorld원본계보[] SourceLineage(
        string publicDataSourceHash,
        DateTimeOffset generatedAt,
        PyeongchangTileManifest? tileManifest,
        PyeongchangSpatialArtifactManifest? spatialArtifacts)
    {
        var sources = new List<SimulationWorld원본계보>
        {
            new()
            {
                SourceStableId = SourceStableId,
                SourceDatabaseCode = "SharedPublicData",
                DatasetCode = "pyeongchang-building-public-license",
                SourceRevision = "current-source-snapshot",
                SourceHashSha256 = publicDataSourceHash,
                ReferenceTimeUtc = generatedAt == DateTimeOffset.UnixEpoch ? null : generatedAt,
            },
        };
        if (tileManifest != null)
        {
            sources.Add(new SimulationWorld원본계보
            {
                SourceStableId = "source:shared-public-data:pyeongchang-spatial-tile-manifest",
                SourceDatabaseCode = "SharedPublicData",
                DatasetCode = "pyeongchang-spatial-tile-manifest",
                SourceRevision = tileManifest.SchemaVersion,
                SourceHashSha256 = tileManifest.ManifestFileHashSha256,
                ReferenceTimeUtc = tileManifest.GeneratedAtUtc,
            });
            sources.Add(new SimulationWorld원본계보
            {
                SourceStableId = "source:public-spatial:pyeongchang-worldcover",
                SourceDatabaseCode = "PrivateObjectStorage",
                DatasetCode = "pyeongchang-worldcover-2021-v200-epsg5186",
                SourceRevision = "2021-v200",
                SourceHashSha256 = tileManifest.RasterSourceHashSha256,
                ReferenceTimeUtc = DateTimeOffset.Parse("2021-12-31T00:00:00Z"),
            });
        }
        if (spatialArtifacts != null)
        {
            sources.Add(new SimulationWorld원본계보
            {
                SourceStableId = "source:public-spatial:pyeongchang-copernicus-dem",
                SourceDatabaseCode = "PrivateObjectStorage",
                DatasetCode = "copernicus-dem-glo30-epsg5186",
                SourceRevision = spatialArtifacts.ElevationSource.SourceRevision,
                SourceHashSha256 = spatialArtifacts.ElevationSource.SourceHashSha256,
            });
            sources.Add(new SimulationWorld원본계보
            {
                SourceStableId = "source:public-spatial:pyeongchang-l2-artifact-manifest",
                SourceDatabaseCode = "DerivedWorldArtifactStorage",
                DatasetCode = spatialArtifacts.SchemaVersion,
                SourceRevision = spatialArtifacts.RuleRevision,
                SourceHashSha256 = spatialArtifacts.ManifestFileHashSha256,
            });
        }
        return sources.ToArray();
    }

    private static string? AreaNodeFor(string legalDongCode)
    {
        if (legalDongCode is "380" or "38000" or "5176038000")
            return "area:sim:pyeongchang:daegwallyeong-farm";
        if (legalDongCode is "360" or "36000" or "5176036000")
            return "area:sim:pyeongchang:jinbu-hub";
        if (legalDongCode is "250" or "25000" or "5176025000")
            return "area:sim:pyeongchang:pyeongchang-town";
        return null;
    }

    private static SimulationWorldUnity공간변환Profile PendingUnityTransformProfile()
    {
        const string stableId = "unity-transform:area-set:pyeongchang:farm-hub-town.v1";
        const string ruleRevision = "unity-space-transform.epsg5186.v1";
        var hash = Sha256(stableId + "|EPSG:5186|EastingToX-NorthingToZ-ElevationToY|" +
            "origin:pending|reference-elevation:pending|horizontal:1|vertical:1|meters-per-unit:1|" +
            ruleRevision + "|WaitingForTileManifest");
        return new SimulationWorldUnity공간변환Profile
        {
            StableId = stableId,
            AreaSetStableId = "area-set:sim:pyeongchang:farm-hub-town.v1",
            SourceCrsCode = "EPSG:5186",
            AxisMappingCode = "EastingToX-NorthingToZ-ElevationToY",
            HorizontalScale = 1m,
            VerticalExaggeration = 1m,
            MetersPerUnityUnit = 1m,
            RuleRevision = ruleRevision,
            StatusCode = SimulationWorldUnity변환상태Codes.타일Manifest대기,
            ProfileHashSha256 = hash,
        };
    }

    private static SimulationWorldUnity공간변환Profile TileManifestTransformProfile(
        PyeongchangTileManifest manifest,
        PyeongchangSpatialArtifactManifest? spatialArtifacts)
    {
        const string stableId = "unity-transform:area-set:pyeongchang:farm-hub-town.v1";
        const string ruleRevision = "unity-space-transform.epsg5186.v1";
        var originEasting = spatialArtifacts?.MinEastingMeters
            ?? manifest.Tiles.Min(item => item.MinEastingMeters);
        var originNorthing = spatialArtifacts?.MinNorthingMeters
            ?? manifest.Tiles.Min(item => item.MinNorthingMeters);
        var referenceElevation = spatialArtifacts?.MinimumPhysicalElevationMeters;
        var status = referenceElevation == null
            ? SimulationWorldUnity변환상태Codes.자료부족
            : SimulationWorldUnity변환상태Codes.변환가능;
        return new SimulationWorldUnity공간변환Profile
        {
            StableId = stableId,
            AreaSetStableId = "area-set:sim:pyeongchang:farm-hub-town.v1",
            SourceCrsCode = "EPSG:5186",
            AxisMappingCode = "EastingToX-NorthingToZ-ElevationToY",
            OriginEastingMeters = originEasting,
            OriginNorthingMeters = originNorthing,
            ReferenceElevationMeters = referenceElevation,
            HorizontalScale = 1m,
            VerticalExaggeration = 1m,
            MetersPerUnityUnit = 1m,
            RuleRevision = ruleRevision,
            StatusCode = status,
            ProfileHashSha256 = Sha256(
                $"{stableId}|EPSG:5186|{originEasting}|{originNorthing}|reference-elevation:{referenceElevation}|" +
                $"horizontal:1|vertical:1|meters-per-unit:1|{ruleRevision}|{status}|{manifest.ManifestFileHashSha256}|" +
                $"{spatialArtifacts?.ManifestFileHashSha256}"),
        };
    }

    private static async Task<PyeongchangSpatialArtifactManifest> ReadSpatialArtifactManifestAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("PyeongchangSpatialArtifactManifestMissing", fullPath);
        await using var stream = File.OpenRead(fullPath);
        var fileHash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken));
        stream.Position = 0;
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var tileKey = RequiredString(root, "tileKey");
        var crs = RequiredString(root, "coordinateReferenceSystem");
        if (crs != "EPSG:5186")
            throw new InvalidDataException("PyeongchangSpatialArtifactCrsUnsupported");
        var core = root.GetProperty("coreBounds");
        var sources = root.GetProperty("sources");
        var artifacts = root.GetProperty("artifacts");
        var statistics = root.GetProperty("statistics");
        return new PyeongchangSpatialArtifactManifest(
            RequiredString(root, "schemaVersion"),
            RequiredString(root, "ruleRevision"),
            fileHash,
            tileKey,
            crs,
            core.GetProperty("minEasting").GetDecimal(),
            core.GetProperty("minNorthing").GetDecimal(),
            statistics.GetProperty("minimumPhysicalElevationMeters").GetDecimal(),
            root.GetProperty("sampleSpacingMeters").GetDecimal(),
            ReadSpatialSource(sources.GetProperty("elevation")),
            ReadSpatialSource(sources.GetProperty("landCover")),
            ReadSpatialArtifact(artifacts.GetProperty("elevation")),
            ReadSpatialArtifact(artifacts.GetProperty("landCover")),
            ReadSpatialArtifact(artifacts.GetProperty("placementMask")));
    }

    private static PyeongchangSpatialSource ReadSpatialSource(JsonElement source) => new(
        RequiredString(source, "sourceRevision"),
        RequiredString(source, "sha256"),
        RequiredString(source, "horizontalCrs"),
        source.TryGetProperty("verticalDatum", out var datum) ? datum.GetString() : null,
        source.GetProperty("resolutionMeters").GetDecimal(),
        source.GetProperty("noDataValue").GetRawText(),
        source.TryGetProperty("sourceReferenceDate", out var date) && date.ValueKind != JsonValueKind.Null
            ? date.GetString()
            : null);

    private static PyeongchangSpatialArtifact ReadSpatialArtifact(JsonElement artifact) => new(
        RequiredString(artifact, "relativePath"),
        RequiredString(artifact, "sha256"),
        RequiredString(artifact, "formatCode"),
        artifact.GetProperty("byteLength").GetInt64(),
        artifact.GetProperty("width").GetInt32(),
        artifact.GetProperty("height").GetInt32());

    private static string RequiredString(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw new InvalidDataException("PyeongchangSpatialArtifactPropertyMissing:" + propertyName);

    private static async Task<PyeongchangTileManifest> ReadTileManifestAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("PyeongchangTileManifestMissing", fullPath);
        await using var stream = File.OpenRead(fullPath);
        var fileHash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken))
            .ToLowerInvariant();
        stream.Position = 0;
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var schemaVersion = root.GetProperty("schemaVersion").GetString()
            ?? throw new InvalidDataException("PyeongchangTileManifestSchemaMissing");
        var generatedAt = DateTimeOffset.Parse(
            root.GetProperty("generatedAt").GetString()!,
            System.Globalization.CultureInfo.InvariantCulture);
        var rasterHash = root.GetProperty("source").GetProperty("sha256").GetString()
            ?? throw new InvalidDataException("PyeongchangTileManifestSourceHashMissing");
        var tiles = new List<SimulationWorldUnity타일Manifest>();
        const string transformId = "unity-transform:area-set:pyeongchang:farm-hub-town.v1";
        foreach (var tile in root.GetProperty("tiles").EnumerateArray())
        {
            var tileKey = tile.GetProperty("tileKey").GetString()!;
            var core = tile.GetProperty("coreBounds");
            var inputFingerprint = tile.GetProperty("fingerprint").GetString()!;
            var size = tile.GetProperty("sizeMeters").GetDecimal();
            var halo = tile.GetProperty("haloMeters").GetDecimal();
            var minEasting = core.GetProperty("minEasting").GetDecimal();
            var minNorthing = core.GetProperty("minNorthing").GetDecimal();
            var maxEasting = core.GetProperty("maxEasting").GetDecimal();
            var maxNorthing = core.GetProperty("maxNorthing").GetDecimal();
            tiles.Add(new SimulationWorldUnity타일Manifest
            {
                StableId = "unity-tile:" + tileKey,
                TransformProfileStableId = transformId,
                TileKey = tileKey,
                Level = tile.GetProperty("level").GetInt32(),
                SizeMeters = size,
                HaloMeters = halo,
                MinEastingMeters = minEasting,
                MinNorthingMeters = minNorthing,
                MaxEastingMeters = maxEasting,
                MaxNorthingMeters = maxNorthing,
                InputFingerprintSha256 = inputFingerprint,
                ManifestHashSha256 = Sha256(
                    $"{tileKey}|{size}|{halo}|{minEasting}|{minNorthing}|{maxEasting}|{maxNorthing}|{inputFingerprint}"),
                StatusCode = SimulationWorldUnity변환상태Codes.변환가능,
            });
        }
        if (tiles.Count == 0)
            throw new InvalidDataException("PyeongchangTileManifestEmpty");
        return new PyeongchangTileManifest(
            schemaVersion,
            generatedAt,
            fileHash,
            rasterHash,
            Select경관완결영역Tiles(tiles));
    }

    public static SimulationWorldUnity타일Manifest[] Select경관완결영역Tiles(
        IEnumerable<SimulationWorldUnity타일Manifest> tiles)
    {
        var source = tiles.ToArray();
        var taskTileKeys = PyeongchangSimulationWorldStableIds.대관령Farm경관완결L2타일키
            .ToHashSet(StringComparer.Ordinal);
        var selectedTileKeys = taskTileKeys
            .Concat(PyeongchangSimulationWorldStableIds.대관령Farm경관완결상위타일키)
            .ToHashSet(StringComparer.Ordinal);
        if (!selectedTileKeys.All(key => source.Any(item => item.TileKey == key)))
            return source;

        return source
            .Where(item => selectedTileKeys.Contains(item.TileKey))
            .OrderBy(item => item.Level)
            .ThenBy(item => item.TileKey, StringComparer.Ordinal)
            .ToArray();
    }

    private static decimal Confidence(string code) => code switch
    {
        "DerivedHigh" => 0.9m,
        "Observed" => 1m,
        _ => 0.6m,
    };

    public static IReadOnlyList<대표건축물선정항목> SelectOneRepresentativePerBuildingCategory(
        IReadOnlyList<Ssalddel.Domain.PublicData.Korea.건축물대장표제부Record> buildings,
        IReadOnlyDictionary<Guid, string> categories) =>
        지역대표건축물Selector.SelectOnePerCategory(
            "region:kr:sigungu:" + 평창군Code,
            buildings,
            categories);

    private static string SourceHash(
        IEnumerable<Ssalddel.Domain.PublicData.Korea.건축물대장표제부Record> buildings,
        IReadOnlyDictionary<Guid, string> categories,
        IReadOnlyDictionary<Guid, Ssalddel.Domain.PublicData.Korea.건축물형태Profile> profiles,
        IReadOnlyDictionary<Guid, Ssalddel.Domain.PublicData.Korea.건축물시각구성계획> compositions,
        IEnumerable<Ssalddel.Domain.PublicData.Korea.공개사업장건축물Assignment> assignments,
        IEnumerable<건축물행정구역Assignment> regionAssignments,
        IEnumerable<행정동건축물CategoryAggregate> administrativeAggregates)
    {
        var canonical = new StringBuilder(
            "pyeongchang-completion-area-pipeline.v7|region-first-world-projection|unity-transform:pending");
        foreach (var item in buildings)
            canonical.Append("|B:").Append(item.Id).Append(':').Append(item.SourceRevision)
                .Append(':').Append(item.ObservedAtUtc.ToUniversalTime().ToString("O"));
        foreach (var item in categories.OrderBy(item => item.Key))
            canonical.Append("|C:").Append(item.Key).Append(':').Append(item.Value);
        foreach (var item in profiles.OrderBy(item => item.Key))
            canonical.Append("|P:").Append(item.Key).Append(':').Append(item.Value.ProfileHashSha256);
        foreach (var item in compositions.OrderBy(item => item.Key))
            canonical.Append("|V:").Append(item.Key).Append(':').Append(item.Value.계획HashSha256);
        foreach (var item in assignments)
            canonical.Append("|A:").Append(item.Id).Append(':').Append(item.BusinessRecord.SourceHashSha256)
                .Append(':').Append(item.RuleRevision);
        foreach (var item in regionAssignments.OrderBy(item => item.Id))
            canonical.Append("|RA:").Append(item.Id).Append(':').Append(item.LegalRegionStableId)
                .Append(':').Append(item.AdministrativeRegionStableId).Append(':')
                .Append(item.SourceVintage).Append(':').Append(item.RuleRevision);
        foreach (var item in administrativeAggregates
                     .OrderBy(item => item.AdministrativeRegionStableId, StringComparer.Ordinal)
                     .ThenBy(item => item.CategoryCode, StringComparer.Ordinal))
            canonical.Append("|AG:").Append(item.Id).Append(':')
                .Append(item.AdministrativeRegionStableId).Append(':').Append(item.CategoryCode)
                .Append(':').Append(item.BuildingCount).Append(':')
                .Append(item.AggregateHashSha256).Append(':').Append(item.RuleRevision);
        return Sha256(canonical.ToString());
    }


    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

internal sealed record PyeongchangTileManifest(
    string SchemaVersion,
    DateTimeOffset GeneratedAtUtc,
    string ManifestFileHashSha256,
    string RasterSourceHashSha256,
    SimulationWorldUnity타일Manifest[] Tiles);

internal sealed record PyeongchangSpatialSource(
    string SourceRevision,
    string SourceHashSha256,
    string HorizontalCrsCode,
    string? VerticalDatumCode,
    decimal ResolutionMeters,
    string NoDataValue,
    string? SourceReferenceDate);

internal sealed record PyeongchangSpatialArtifact(
    string RelativePath,
    string ArtifactHashSha256,
    string FormatCode,
    long ByteLength,
    int Width,
    int Height);

internal sealed record PyeongchangSpatialArtifactManifest(
    string SchemaVersion,
    string RuleRevision,
    string ManifestFileHashSha256,
    string TileKey,
    string HorizontalCrsCode,
    decimal MinEastingMeters,
    decimal MinNorthingMeters,
    decimal MinimumPhysicalElevationMeters,
    decimal SampleSpacingMeters,
    PyeongchangSpatialSource ElevationSource,
    PyeongchangSpatialSource LandCoverSource,
    PyeongchangSpatialArtifact Elevation,
    PyeongchangSpatialArtifact LandCover,
    PyeongchangSpatialArtifact PlacementMask)
{
    public SimulationWorldUnity산출물[] ToDomainArtifacts() => new[]
    {
        Create("elevation", Elevation, ElevationSource),
        Create("land-cover", LandCover, LandCoverSource),
        Create("placement-mask", PlacementMask, new PyeongchangSpatialSource(
            RuleRevision,
            Hash(ElevationSource.SourceHashSha256 + "|" + LandCoverSource.SourceHashSha256 + "|" + RuleRevision),
            HorizontalCrsCode,
            ElevationSource.VerticalDatumCode,
            SampleSpacingMeters,
            "0",
            null)),
    };

    private SimulationWorldUnity산출물 Create(
        string layerCode,
        PyeongchangSpatialArtifact artifact,
        PyeongchangSpatialSource source) => new()
        {
            StableId = $"unity-artifact:{TileKey}:{layerCode}",
            TileManifestStableId = "unity-tile:" + TileKey,
            ArtifactKindCode = layerCode,
            LodCode = "L2",
            StorageObjectKey = artifact.RelativePath,
            ArtifactHashSha256 = artifact.ArtifactHashSha256,
            SourceRevision = source.SourceRevision,
            SourceHashSha256 = source.SourceHashSha256,
            SourceReferenceDate = source.SourceReferenceDate,
            HorizontalCrsCode = source.HorizontalCrsCode,
            VerticalDatumCode = source.VerticalDatumCode,
            ResolutionMeters = source.ResolutionMeters,
            NoDataValue = source.NoDataValue,
            ArtifactFormatCode = artifact.FormatCode,
            ArtifactByteLength = artifact.ByteLength,
            SampleWidth = artifact.Width,
            SampleHeight = artifact.Height,
            StatusCode = SimulationWorldUnity산출물상태Codes.완료,
        };

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

public static class 평창군공간파생PipelineRegistration
{
    public static IServiceCollection Add평창군공간파생Pipeline(this IServiceCollection services)
    {
        services.AddScoped<평창군공간파생Pipeline>();
        return services;
    }
}
