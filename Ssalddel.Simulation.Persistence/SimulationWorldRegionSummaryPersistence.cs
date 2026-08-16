using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorld지역표현요약ProfileEntity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string ProfileRevision { get; set; } = string.Empty;
    public string ProfileHashSha256 { get; set; } = string.Empty;
    public int Seed { get; set; }
    public decimal MaximumCategoryShare { get; set; }
    public int L0TotalSlots { get; set; }
    public int L1TotalSlots { get; set; }
    public int L2TotalSlots { get; set; }
    public string BudgetJson { get; set; } = string.Empty;
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorld지역표현요약RunEntity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public long ProfileId { get; set; }
    public string RegionStableId { get; set; } = string.Empty;
    public string? TileKey { get; set; }
    public string LodCode { get; set; } = string.Empty;
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string SummaryHashSha256 { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
    public int TotalCandidateCount { get; set; }
    public int SelectedItemCount { get; set; }
    public int TotalRepresentedRecordCount { get; set; }
    public int SelectedRepresentedRecordCount { get; set; }
    public int OmittedRepresentedRecordCount { get; set; }
    public int RequestedVisualSlotCount { get; set; }
    public int AllocatedVisualSlotCount { get; set; }
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
    public SimulationWorld지역표현요약ProfileEntity Profile { get; set; } = null!;
}

public sealed class SimulationWorld지역표현요약ItemEntity
{
    public long Id { get; set; }
    public long SummaryRunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string SourceObjectStableId { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string ObjectTypeCode { get; set; } = string.Empty;
    public string SelectionReasonCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string VisualKey { get; set; } = string.Empty;
    public int RepresentedRecordCount { get; set; }
    public decimal? RepresentedAreaSquareMeters { get; set; }
    public int VisualSlotCount { get; set; }
    public int MinimumVisibleCount { get; set; }
    public bool HasPublicDetail { get; set; }
    public bool PresentationOnly { get; set; }
    public SimulationWorld지역표현요약RunEntity SummaryRun { get; set; } = null!;
}

public sealed class SimulationWorld지역표현요약CategoryReportEntity
{
    public long Id { get; set; }
    public long SummaryRunId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public int CandidateCount { get; set; }
    public int TotalRepresentedRecordCount { get; set; }
    public int SelectedRepresentedRecordCount { get; set; }
    public int OmittedRepresentedRecordCount { get; set; }
    public decimal TotalRepresentedAreaSquareMeters { get; set; }
    public decimal SelectedRepresentedAreaSquareMeters { get; set; }
    public int AllocatedVisualSlotCount { get; set; }
    public SimulationWorld지역표현요약RunEntity SummaryRun { get; set; } = null!;
}

internal static class SimulationWorld지역표현요약PersistenceBuilder
{
    private static readonly string[] ExcludedNodeKinds =
    {
        "Area",
        "LandscapeCompletionArea",
        "SpatialTile",
        "DataGap",
        SimulationWorldRegionProjectionCodes.LegalRegion,
        SimulationWorldRegionProjectionCodes.AdministrativeRegion,
    };

    public static void Add지역표현요약(
        this SimulationWorld파생DbContext dbContext,
        SimulationWorld파생RunEntity run,
        SimulationWorld파생원장 ledger)
    {
        var profile = SimulationWorld지역표현요약Profile.CreateDefault();
        var profileEntity = new SimulationWorld지역표현요약ProfileEntity
        {
            Run = run,
            ProfileRevision = profile.ProfileRevision,
            ProfileHashSha256 = profile.ComputeHash(),
            Seed = profile.Seed,
            MaximumCategoryShare = profile.MaximumCategoryShare,
            L0TotalSlots = profile.GetBudget(SimulationWorld지역표현요약LodCodes.L0).TotalSlots,
            L1TotalSlots = profile.GetBudget(SimulationWorld지역표현요약LodCodes.L1).TotalSlots,
            L2TotalSlots = profile.GetBudget(SimulationWorld지역표현요약LodCodes.L2).TotalSlots,
            BudgetJson = System.Text.Json.JsonSerializer.Serialize(profile.Budgets),
        };
        dbContext.RegionSummaryProfiles.Add(profileEntity);

        var candidates = CreateCandidates(ledger);
        var areaSetCandidates = candidates
            .Select(item => CopyForRegion(item, ledger.AreaSetStableId))
            .ToArray();
        foreach (var lodCode in new[]
                 {
                     SimulationWorld지역표현요약LodCodes.L0,
                     SimulationWorld지역표현요약LodCodes.L1,
                     SimulationWorld지역표현요약LodCodes.L2,
                 })
        {
            AddResult(dbContext, run, profileEntity, SimulationWorld지역표현요약Engine.Generate(
                profile,
                ledger.AreaSetStableId,
                null,
                lodCode,
                areaSetCandidates,
                ledger.InputFingerprintSha256,
                ledger.GeneratedAtUtc));
        }

        foreach (var regionGroup in candidates
                     .Where(item => !string.Equals(
                         item.RegionStableId, ledger.AreaSetStableId, StringComparison.Ordinal))
                     .GroupBy(item => item.RegionStableId, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            foreach (var lodCode in new[]
                     {
                         SimulationWorld지역표현요약LodCodes.L0,
                         SimulationWorld지역표현요약LodCodes.L1,
                         SimulationWorld지역표현요약LodCodes.L2,
                     })
            {
                AddResult(dbContext, run, profileEntity, SimulationWorld지역표현요약Engine.Generate(
                    profile,
                    regionGroup.Key,
                    null,
                    lodCode,
                    regionGroup,
                    ledger.InputFingerprintSha256,
                    ledger.GeneratedAtUtc));
            }
        }

        foreach (var tileGroup in candidates
                     .Where(item => !string.IsNullOrWhiteSpace(item.TileKey))
                     .GroupBy(item => new { item.RegionStableId, item.TileKey })
                     .OrderBy(group => group.Key.TileKey, StringComparer.Ordinal)
                     .ThenBy(group => group.Key.RegionStableId, StringComparer.Ordinal))
        {
            foreach (var lodCode in new[]
                     {
                         SimulationWorld지역표현요약LodCodes.L0,
                         SimulationWorld지역표현요약LodCodes.L1,
                         SimulationWorld지역표현요약LodCodes.L2,
                     })
            {
                AddResult(dbContext, run, profileEntity, SimulationWorld지역표현요약Engine.Generate(
                    profile,
                    tileGroup.Key.RegionStableId,
                    tileGroup.Key.TileKey,
                    lodCode,
                    tileGroup,
                    ledger.InputFingerprintSha256,
                    ledger.GeneratedAtUtc));
            }
        }

        var candidateTileKeys = candidates
            .Where(item => !string.IsNullOrWhiteSpace(item.TileKey))
            .Select(item => item.TileKey!)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var tileKey in ledger.UnityTileManifests
                     .Select(item => item.TileKey)
                     .Where(item => !candidateTileKeys.Contains(item))
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(item => item, StringComparer.Ordinal))
        {
            foreach (var lodCode in new[]
                     {
                         SimulationWorld지역표현요약LodCodes.L0,
                         SimulationWorld지역표현요약LodCodes.L1,
                         SimulationWorld지역표현요약LodCodes.L2,
                     })
            {
                AddResult(dbContext, run, profileEntity, SimulationWorld지역표현요약Engine.Generate(
                    profile,
                    ledger.AreaSetStableId,
                    tileKey,
                    lodCode,
                    Array.Empty<SimulationWorld지역표현요약Candidate>(),
                    ledger.InputFingerprintSha256,
                    ledger.GeneratedAtUtc));
            }
        }
    }

    private static SimulationWorld지역표현요약Candidate CopyForRegion(
        SimulationWorld지역표현요약Candidate source,
        string regionStableId) => new()
    {
        StableId = source.StableId,
        RegionStableId = regionStableId,
        TileKey = source.TileKey,
        CategoryCode = source.CategoryCode,
        ObjectTypeCode = source.ObjectTypeCode,
        EvidenceKindCode = source.EvidenceKindCode,
        VisualKey = source.VisualKey,
        RepresentedRecordCount = source.RepresentedRecordCount,
        RepresentedAreaSquareMeters = source.RepresentedAreaSquareMeters,
        QualityScore = source.QualityScore,
        SpatialBucketCode = source.SpatialBucketCode,
        RegionalShare = source.RegionalShare,
        BaselineShare = source.BaselineShare,
        GameplayPriority = source.GameplayPriority,
        HasPublicDetail = source.HasPublicDetail,
    };

    private static IReadOnlyList<SimulationWorld지역표현요약Candidate> CreateCandidates(
        SimulationWorld파생원장 ledger)
    {
        var nodeById = ledger.Nodes.ToDictionary(item => item.StableId, StringComparer.Ordinal);
        var incomingByTarget = ledger.Relations
            .GroupBy(item => item.ToNodeStableId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var publiclyLinkedBusinessIds = ledger.Relations
            .Where(item => string.Equals(
                item.RelationCode, "HostsPublicLicensedBusiness", StringComparison.Ordinal)
                && item.Confidence > 0m)
            .Select(item => item.ToNodeStableId)
            .ToHashSet(StringComparer.Ordinal);
        return ledger.Nodes
            .Where(node => !ExcludedNodeKinds.Contains(node.NodeKindCode, StringComparer.Ordinal))
            .Select(node =>
            {
                var regionStableId = ResolveRegionStableId(
                    node, ledger.AreaSetStableId, nodeById, incomingByTarget);
                var categoryCode = string.IsNullOrWhiteSpace(node.RepresentativeGroupCode)
                    ? node.NodeKindCode
                    : node.RepresentativeGroupCode!;
                return new SimulationWorld지역표현요약Candidate
                {
                    StableId = node.StableId,
                    RegionStableId = regionStableId,
                    TileKey = node.TileKey,
                    CategoryCode = categoryCode,
                    ObjectTypeCode = node.NodeKindCode,
                    EvidenceKindCode = node.EvidenceKindCode,
                    VisualKey = SemanticVisualKey(node.NodeKindCode, categoryCode),
                    RepresentedRecordCount = Math.Max(1, node.RepresentedRecordCount ?? 1),
                    QualityScore = (!string.IsNullOrWhiteSpace(node.DisplayName) ? 100 : 0)
                                   + (!string.IsNullOrWhiteSpace(node.SourceRecordStableId) ? 10 : 0)
                                   + (node.RepresentativeRank.HasValue ? 5 : 0),
                    SpatialBucketCode = node.TileKey ?? node.AreaStableId ?? regionStableId,
                    GameplayPriority = string.Equals(
                        node.EvidenceKindCode, SimulationWorld근거종류Codes.시나리오,
                        StringComparison.Ordinal) ? 100 : 0,
                    HasPublicDetail = string.Equals(
                        node.NodeKindCode, "PublicLicensedBusiness", StringComparison.Ordinal)
                        && !string.IsNullOrWhiteSpace(node.DisplayName)
                        && publiclyLinkedBusinessIds.Contains(node.StableId),
                };
            })
            .ToArray();
    }

    private static string ResolveRegionStableId(
        SimulationWorld파생Node node,
        string areaSetStableId,
        IReadOnlyDictionary<string, SimulationWorld파생Node> nodeById,
        IReadOnlyDictionary<string, SimulationWorld파생Relation[]> incomingByTarget)
    {
        if (!string.IsNullOrWhiteSpace(node.RegionCode))
            return "region:kr:bjd:" + node.RegionCode;

        var queue = new Queue<string>();
        var visited = new HashSet<string>(StringComparer.Ordinal) { node.StableId };
        queue.Enqueue(node.StableId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!incomingByTarget.TryGetValue(current, out var incoming)) continue;
            foreach (var relation in incoming.OrderBy(item => item.FromNodeStableId, StringComparer.Ordinal))
            {
                if (!visited.Add(relation.FromNodeStableId)
                    || !nodeById.TryGetValue(relation.FromNodeStableId, out var parent)) continue;
                if ((string.Equals(parent.NodeKindCode, SimulationWorldRegionProjectionCodes.LegalRegion,
                         StringComparison.Ordinal)
                     || string.Equals(parent.NodeKindCode,
                         SimulationWorldRegionProjectionCodes.AdministrativeRegion,
                         StringComparison.Ordinal))
                    && !string.IsNullOrWhiteSpace(parent.SourceRecordStableId))
                    return parent.SourceRecordStableId!;
                if (!string.IsNullOrWhiteSpace(parent.RegionCode))
                    return "region:kr:bjd:" + parent.RegionCode;
                queue.Enqueue(parent.StableId);
            }
        }

        return !string.IsNullOrWhiteSpace(node.AreaStableId)
            ? node.AreaStableId!
            : areaSetStableId;
    }

    private static string SemanticVisualKey(string objectTypeCode, string categoryCode) =>
        "summary." + NormalizeKey(objectTypeCode) + "." + NormalizeKey(categoryCode);

    private static string NormalizeKey(string value) => new(
        value.ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '-').ToArray());

    private static void AddResult(
        SimulationWorld파생DbContext dbContext,
        SimulationWorld파생RunEntity run,
        SimulationWorld지역표현요약ProfileEntity profile,
        SimulationWorld지역표현요약Result result)
    {
        var summaryRun = new SimulationWorld지역표현요약RunEntity
        {
            Run = run,
            Profile = profile,
            RegionStableId = result.RegionStableId,
            TileKey = result.TileKey,
            LodCode = result.LodCode,
            InputFingerprintSha256 = result.InputFingerprintSha256,
            SummaryHashSha256 = result.SummaryHashSha256,
            StatusCode = result.StatusCode,
            GeneratedAtUtc = result.GeneratedAtUtc,
            StoredAtUtc = DateTimeOffset.UtcNow,
            TotalCandidateCount = result.TotalCandidateCount,
            SelectedItemCount = result.SelectedItemCount,
            TotalRepresentedRecordCount = result.TotalRepresentedRecordCount,
            SelectedRepresentedRecordCount = result.SelectedRepresentedRecordCount,
            OmittedRepresentedRecordCount = result.OmittedRepresentedRecordCount,
            RequestedVisualSlotCount = result.RequestedVisualSlotCount,
            AllocatedVisualSlotCount = result.AllocatedVisualSlotCount,
        };
        dbContext.RegionSummaryRuns.Add(summaryRun);
        dbContext.RegionSummaryItems.AddRange(result.Items.Select(item =>
            new SimulationWorld지역표현요약ItemEntity
            {
                SummaryRun = summaryRun,
                StableId = item.StableId,
                SourceObjectStableId = item.SourceObjectStableId,
                CategoryCode = item.CategoryCode,
                ObjectTypeCode = item.ObjectTypeCode,
                SelectionReasonCode = item.SelectionReasonCode,
                EvidenceKindCode = item.EvidenceKindCode,
                VisualKey = item.VisualKey,
                RepresentedRecordCount = item.RepresentedRecordCount,
                RepresentedAreaSquareMeters = item.RepresentedAreaSquareMeters,
                VisualSlotCount = item.VisualSlotCount,
                MinimumVisibleCount = item.MinimumVisibleCount,
                HasPublicDetail = item.HasPublicDetail,
                PresentationOnly = item.PresentationOnly,
            }));
        dbContext.RegionSummaryCategoryReports.AddRange(result.CategoryReports.Select(item =>
            new SimulationWorld지역표현요약CategoryReportEntity
            {
                SummaryRun = summaryRun,
                CategoryCode = item.CategoryCode,
                CandidateCount = item.CandidateCount,
                TotalRepresentedRecordCount = item.TotalRepresentedRecordCount,
                SelectedRepresentedRecordCount = item.SelectedRepresentedRecordCount,
                OmittedRepresentedRecordCount = item.OmittedRepresentedRecordCount,
                TotalRepresentedAreaSquareMeters = item.TotalRepresentedAreaSquareMeters,
                SelectedRepresentedAreaSquareMeters = item.SelectedRepresentedAreaSquareMeters,
                AllocatedVisualSlotCount = item.AllocatedVisualSlotCount,
            }));
    }
}

public sealed class SimulationWorld지역표현요약Reader(
    SimulationWorld파생DbContext dbContext) : ISimulationWorld지역표현요약Reader
{
    public async Task<SimulationWorld지역표현요약Snapshot?> 지역요약조회Async(
        string regionStableId,
        string lodCode,
        CancellationToken cancellationToken)
    {
        var summaryRunId = await dbContext.RegionSummaryRuns.AsNoTracking()
            .Where(item => item.RegionStableId == regionStableId
                && item.TileKey == null
                && item.LodCode == lodCode)
            .OrderByDescending(item => item.GeneratedAtUtc)
            .ThenByDescending(item => item.Id)
            .Select(item => (long?)item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return summaryRunId.HasValue
            ? await ReadSummaryAsync(summaryRunId.Value, cancellationToken)
            : null;
    }

    public async Task<SimulationWorld지역표현요약Snapshot?> 타일요약조회Async(
        string tileKey,
        string lodCode,
        CancellationToken cancellationToken)
    {
        var summaryRunId = await dbContext.RegionSummaryRuns.AsNoTracking()
            .Where(item => item.TileKey == tileKey && item.LodCode == lodCode)
            .OrderByDescending(item => item.GeneratedAtUtc)
            .ThenByDescending(item => item.Id)
            .Select(item => (long?)item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return summaryRunId.HasValue
            ? await ReadSummaryAsync(summaryRunId.Value, cancellationToken)
            : null;
    }

    public async Task<SimulationWorld공개객체상세Snapshot?> 공개객체상세조회Async(
        string objectStableId,
        CancellationToken cancellationToken)
    {
        var node = await dbContext.Nodes.AsNoTracking()
            .Where(item => item.StableId == objectStableId
                && item.NodeKindCode == "PublicLicensedBusiness"
                && item.DisplayName != null
                && item.DisplayName != string.Empty)
            .OrderByDescending(item => item.Run.GeneratedAtUtc)
            .ThenByDescending(item => item.Id)
            .Select(item => new
            {
                item.RunId,
                item.StableId,
                item.DisplayName,
                item.RepresentativeGroupCode,
                item.NodeKindCode,
                item.EvidenceKindCode,
                item.SourceStableId,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (node is null || string.IsNullOrWhiteSpace(node.SourceStableId)) return null;

        var hasVerifiedBuildingLink = await dbContext.Relations.AsNoTracking()
            .AnyAsync(item => item.RunId == node.RunId
                && item.ToNodeStableId == node.StableId
                && item.RelationCode == "HostsPublicLicensedBusiness"
                && item.Confidence > 0m,
                cancellationToken);
        if (!hasVerifiedBuildingLink) return null;

        var source = await dbContext.Sources.AsNoTracking()
            .Where(item => item.RunId == node.RunId && item.SourceStableId == node.SourceStableId)
            .Select(item => new { item.SourceRevision, item.ReferenceTimeUtc })
            .FirstOrDefaultAsync(cancellationToken);
        return new SimulationWorld공개객체상세Snapshot(
            node.StableId,
            node.DisplayName!,
            node.RepresentativeGroupCode ?? node.NodeKindCode,
            node.EvidenceKindCode,
            node.SourceStableId!,
            source?.SourceRevision ?? string.Empty,
            source?.ReferenceTimeUtc);
    }

    private async Task<SimulationWorld지역표현요약Snapshot> ReadSummaryAsync(
        long summaryRunId,
        CancellationToken cancellationToken)
    {
        var run = await dbContext.RegionSummaryRuns.AsNoTracking()
            .Where(item => item.Id == summaryRunId)
            .Select(item => new
            {
                item.RegionStableId,
                item.TileKey,
                item.LodCode,
                item.Profile.ProfileRevision,
                item.Profile.ProfileHashSha256,
                item.SummaryHashSha256,
                item.StatusCode,
                item.GeneratedAtUtc,
                item.TotalCandidateCount,
                item.SelectedItemCount,
                item.TotalRepresentedRecordCount,
                item.SelectedRepresentedRecordCount,
                item.OmittedRepresentedRecordCount,
                item.RequestedVisualSlotCount,
                item.AllocatedVisualSlotCount,
            })
            .SingleAsync(cancellationToken);
        var items = await dbContext.RegionSummaryItems.AsNoTracking()
            .Where(item => item.SummaryRunId == summaryRunId)
            .OrderBy(item => item.SelectionReasonCode)
            .ThenBy(item => item.CategoryCode)
            .ThenBy(item => item.StableId)
            .Select(item => new SimulationWorld지역표현요약ItemSnapshot(
                item.StableId,
                item.SourceObjectStableId,
                item.CategoryCode,
                item.ObjectTypeCode,
                item.SelectionReasonCode,
                item.EvidenceKindCode,
                item.VisualKey,
                item.RepresentedRecordCount,
                item.RepresentedAreaSquareMeters,
                item.VisualSlotCount,
                item.MinimumVisibleCount,
                item.HasPublicDetail))
            .ToArrayAsync(cancellationToken);
        var reports = await dbContext.RegionSummaryCategoryReports.AsNoTracking()
            .Where(item => item.SummaryRunId == summaryRunId)
            .OrderBy(item => item.CategoryCode)
            .Select(item => new SimulationWorld지역표현요약CategoryReportSnapshot(
                item.CategoryCode,
                item.CandidateCount,
                item.TotalRepresentedRecordCount,
                item.SelectedRepresentedRecordCount,
                item.OmittedRepresentedRecordCount,
                item.TotalRepresentedAreaSquareMeters,
                item.SelectedRepresentedAreaSquareMeters,
                item.AllocatedVisualSlotCount))
            .ToArrayAsync(cancellationToken);
        return new SimulationWorld지역표현요약Snapshot(
            run.RegionStableId,
            run.TileKey,
            run.LodCode,
            run.ProfileRevision,
            run.ProfileHashSha256,
            run.SummaryHashSha256,
            run.StatusCode,
            run.GeneratedAtUtc,
            run.TotalCandidateCount,
            run.SelectedItemCount,
            run.TotalRepresentedRecordCount,
            run.SelectedRepresentedRecordCount,
            run.OmittedRepresentedRecordCount,
            run.RequestedVisualSlotCount,
            run.AllocatedVisualSlotCount,
            items,
            reports);
    }
}

internal sealed class SimulationWorld지역표현요약ProfileConfiguration
    : IEntityTypeConfiguration<SimulationWorld지역표현요약ProfileEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld지역표현요약ProfileEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_지역표현요약프로필");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.ProfileRevision }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ProfileRevision), "요약프로필개정번호", 120);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ProfileHashSha256), "요약프로필SHA256", 64);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Seed), "결정적배치Seed");
        builder.Property(item => item.MaximumCategoryShare).HasColumnName("분류별최대표현비율").HasPrecision(8, 6);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.L0TotalSlots), "L0표현슬롯수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.L1TotalSlots), "L1표현슬롯수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.L2TotalSlots), "L2표현슬롯수");
        builder.Property(item => item.BudgetJson).HasColumnName("LOD별표현예산JSON").HasColumnType("longtext");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld지역표현요약RunConfiguration
    : IEntityTypeConfiguration<SimulationWorld지역표현요약RunEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld지역표현요약RunEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_지역표현요약실행");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.RegionStableId, item.TileKey, item.LodCode }).IsUnique();
        builder.HasIndex(item => new { item.RegionStableId, item.TileKey, item.LodCode });
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.ProfileId), "요약프로필식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.RegionStableId), "지역고유식별자", 240);
        builder.Property(item => item.TileKey).HasColumnName("타일키").HasMaxLength(120);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.LodCode), "세부표현단계코드", 20);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.InputFingerprintSha256), "입력지문SHA256", 64);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SummaryHashSha256), "요약결과SHA256", 64);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StatusCode), "요약상태코드", 50);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.GeneratedAtUtc), "생성일시UTC");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.StoredAtUtc), "저장일시UTC");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.TotalCandidateCount), "전체후보수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.SelectedItemCount), "선정항목수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.TotalRepresentedRecordCount), "전체대표원본수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.SelectedRepresentedRecordCount), "선정대표원본수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.OmittedRepresentedRecordCount), "화면생략대표원본수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RequestedVisualSlotCount), "요청표현슬롯수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.AllocatedVisualSlotCount), "배정표현슬롯수");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Profile).WithMany().HasForeignKey(item => item.ProfileId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld지역표현요약ItemConfiguration
    : IEntityTypeConfiguration<SimulationWorld지역표현요약ItemEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld지역표현요약ItemEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_지역표현요약항목");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.SummaryRunId, item.StableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.SummaryRunId), "요약실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "요약항목고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SourceObjectStableId), "원본객체고유식별자", 240);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.CategoryCode), "표현분류코드", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ObjectTypeCode), "객체종류코드", 120);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SelectionReasonCode), "선정이유코드", 80);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.EvidenceKindCode), "근거수준코드", 80);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.VisualKey), "시각의미키", 240);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RepresentedRecordCount), "대표원본수");
        builder.Property(item => item.RepresentedAreaSquareMeters).HasColumnName("대표면적제곱미터").HasPrecision(18, 4);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.VisualSlotCount), "표현슬롯수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.MinimumVisibleCount), "최소가시표현수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.HasPublicDetail), "공개상세연결여부");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.PresentationOnly), "표현전용여부");
        builder.HasOne(item => item.SummaryRun).WithMany().HasForeignKey(item => item.SummaryRunId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld지역표현요약CategoryReportConfiguration
    : IEntityTypeConfiguration<SimulationWorld지역표현요약CategoryReportEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld지역표현요약CategoryReportEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_지역표현요약분류보고서");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.SummaryRunId, item.CategoryCode }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.SummaryRunId), "요약실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.CategoryCode), "표현분류코드", 160);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.CandidateCount), "후보수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.TotalRepresentedRecordCount), "전체대표원본수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.SelectedRepresentedRecordCount), "선정대표원본수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.OmittedRepresentedRecordCount), "화면생략대표원본수");
        builder.Property(item => item.TotalRepresentedAreaSquareMeters).HasColumnName("전체대표면적제곱미터").HasPrecision(18, 4);
        builder.Property(item => item.SelectedRepresentedAreaSquareMeters).HasColumnName("선정대표면적제곱미터").HasPrecision(18, 4);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.AllocatedVisualSlotCount), "배정표현슬롯수");
        builder.HasOne(item => item.SummaryRun).WithMany().HasForeignKey(item => item.SummaryRunId).OnDelete(DeleteBehavior.Cascade);
    }
}
