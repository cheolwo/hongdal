using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{

public sealed record SimulationWorld지역표현요약Snapshot(
    string RegionStableId,
    string? TileKey,
    string LodCode,
    string ProfileRevision,
    string ProfileHashSha256,
    string SummaryHashSha256,
    string StatusCode,
    DateTimeOffset GeneratedAtUtc,
    int TotalCandidateCount,
    int SelectedItemCount,
    int TotalRepresentedRecordCount,
    int SelectedRepresentedRecordCount,
    int OmittedRepresentedRecordCount,
    int RequestedVisualSlotCount,
    int AllocatedVisualSlotCount,
    IReadOnlyList<SimulationWorld지역표현요약ItemSnapshot> Items,
    IReadOnlyList<SimulationWorld지역표현요약CategoryReportSnapshot> CategoryReports);

public sealed record SimulationWorld지역표현요약ItemSnapshot(
    string SummaryItemStableId,
    string SourceObjectStableId,
    string CategoryCode,
    string ObjectTypeCode,
    string SelectionReasonCode,
    string EvidenceKindCode,
    string VisualKey,
    int RepresentedRecordCount,
    decimal? RepresentedAreaSquareMeters,
    int VisualSlotCount,
    int MinimumVisibleCount,
    bool HasPublicDetail);

public sealed record SimulationWorld지역표현요약CategoryReportSnapshot(
    string CategoryCode,
    int CandidateCount,
    int TotalRepresentedRecordCount,
    int SelectedRepresentedRecordCount,
    int OmittedRepresentedRecordCount,
    decimal TotalRepresentedAreaSquareMeters,
    decimal SelectedRepresentedAreaSquareMeters,
    int AllocatedVisualSlotCount);

public sealed record SimulationWorld공개객체상세Snapshot(
    string ObjectStableId,
    string PublicDisplayName,
    string CategoryCode,
    string EvidenceKindCode,
    string SourceStableId,
    string SourceRevision,
    DateTimeOffset? ObservedAtUtc);

public interface ISimulationWorld지역표현요약Reader
{
    Task<SimulationWorld지역표현요약Snapshot?> 지역요약조회Async(
        string regionStableId,
        string lodCode,
        CancellationToken cancellationToken);

    Task<SimulationWorld지역표현요약Snapshot?> 타일요약조회Async(
        string tileKey,
        string lodCode,
        CancellationToken cancellationToken);

    Task<SimulationWorld공개객체상세Snapshot?> 공개객체상세조회Async(
        string objectStableId,
        CancellationToken cancellationToken);
}

public sealed class DisabledSimulationWorld지역표현요약Reader
    : ISimulationWorld지역표현요약Reader
{
    public Task<SimulationWorld지역표현요약Snapshot?> 지역요약조회Async(
        string regionStableId,
        string lodCode,
        CancellationToken cancellationToken) => Task.FromResult<SimulationWorld지역표현요약Snapshot?>(null);

    public Task<SimulationWorld지역표현요약Snapshot?> 타일요약조회Async(
        string tileKey,
        string lodCode,
        CancellationToken cancellationToken) => Task.FromResult<SimulationWorld지역표현요약Snapshot?>(null);

    public Task<SimulationWorld공개객체상세Snapshot?> 공개객체상세조회Async(
        string objectStableId,
        CancellationToken cancellationToken) => Task.FromResult<SimulationWorld공개객체상세Snapshot?>(null);
}

public sealed class SimulationWorld지역표현요약Service
{
    private readonly ISimulationWorld지역표현요약Reader reader;

    public SimulationWorld지역표현요약Service(
        ISimulationWorld지역표현요약Reader reader)
    {
        this.reader = reader;
    }

    public async Task<SimulationWorld지역표현요약Response> 지역요약조회Async(
        string regionStableId,
        string lodCode,
        CancellationToken cancellationToken)
    {
        ValidateLod(lodCode);
        var snapshot = await reader.지역요약조회Async(regionStableId, lodCode, cancellationToken)
            ?? throw new SimulationNotFoundException("SimulationWorldRegionSummaryNotFound");
        return ToResponse(snapshot);
    }

    public async Task<SimulationWorld지역표현요약Response> 타일요약조회Async(
        string tileKey,
        string lodCode,
        CancellationToken cancellationToken)
    {
        ValidateLod(lodCode);
        var snapshot = await reader.타일요약조회Async(tileKey, lodCode, cancellationToken)
            ?? throw new SimulationNotFoundException("SimulationWorldTileSummaryNotFound");
        return ToResponse(snapshot);
    }

    public async Task<SimulationWorld공개객체상세Response> 공개객체상세조회Async(
        string objectStableId,
        CancellationToken cancellationToken)
    {
        var snapshot = await reader.공개객체상세조회Async(objectStableId, cancellationToken)
            ?? throw new SimulationNotFoundException("SimulationWorldPublicObjectDetailNotFound");
        return new SimulationWorld공개객체상세Response
        {
            ObjectStableId = snapshot.ObjectStableId,
            PublicDisplayName = snapshot.PublicDisplayName,
            CategoryCode = snapshot.CategoryCode,
            EvidenceKindCode = snapshot.EvidenceKindCode,
            SourceStableId = snapshot.SourceStableId,
            SourceRevision = snapshot.SourceRevision,
            ObservedAtUtc = snapshot.ObservedAtUtc,
            DisclosureNotice = "공개 공공데이터에서 확인된 상호명만 제공하며 대표자·연락처·사업자등록번호는 포함하지 않습니다.",
            IsOperationalState = false,
        };
    }

    private static void ValidateLod(string lodCode)
    {
        if (!SimulationWorld지역표현요약LodCodes.IsSupported(lodCode))
            throw new SimulationContractException("SimulationWorldRegionSummaryLodUnsupported");
    }

    private static SimulationWorld지역표현요약Response ToResponse(
        SimulationWorld지역표현요약Snapshot snapshot) => new()
    {
        RegionStableId = snapshot.RegionStableId,
        TileKey = snapshot.TileKey,
        LodCode = snapshot.LodCode,
        ProfileRevision = snapshot.ProfileRevision,
        ProfileHashSha256 = snapshot.ProfileHashSha256,
        SummaryHashSha256 = snapshot.SummaryHashSha256,
        StatusCode = snapshot.StatusCode,
        GeneratedAtUtc = snapshot.GeneratedAtUtc,
        TotalCandidateCount = snapshot.TotalCandidateCount,
        SelectedItemCount = snapshot.SelectedItemCount,
        TotalRepresentedRecordCount = snapshot.TotalRepresentedRecordCount,
        SelectedRepresentedRecordCount = snapshot.SelectedRepresentedRecordCount,
        OmittedRepresentedRecordCount = snapshot.OmittedRepresentedRecordCount,
        RequestedVisualSlotCount = snapshot.RequestedVisualSlotCount,
        AllocatedVisualSlotCount = snapshot.AllocatedVisualSlotCount,
        Items = snapshot.Items.Select(item => new SimulationWorld지역표현요약ItemResponse
        {
            SummaryItemStableId = item.SummaryItemStableId,
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
            PresentationOnly = true,
        }).ToArray(),
        CategoryReports = snapshot.CategoryReports.Select(item =>
            new SimulationWorld지역표현요약CategoryReportResponse
            {
                CategoryCode = item.CategoryCode,
                CandidateCount = item.CandidateCount,
                TotalRepresentedRecordCount = item.TotalRepresentedRecordCount,
                SelectedRepresentedRecordCount = item.SelectedRepresentedRecordCount,
                OmittedRepresentedRecordCount = item.OmittedRepresentedRecordCount,
                TotalRepresentedAreaSquareMeters = item.TotalRepresentedAreaSquareMeters,
                SelectedRepresentedAreaSquareMeters = item.SelectedRepresentedAreaSquareMeters,
                AllocatedVisualSlotCount = item.AllocatedVisualSlotCount,
            }).ToArray(),
        PresentationOnly = true,
        IsOperationalState = false,
    };
}
}
