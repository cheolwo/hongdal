using Hongdal.Contracts.Common.PublicData;

namespace 홍달.Services.External.PublicData;

public interface IRoadAddressLookupService
{
    Task<PublicDataLookupResponse<RoadAddressItem>> SearchAsync(
        RoadAddressSearchRequest request,
        CancellationToken cancellationToken = default);
}

public interface IApartmentComplexLookupService
{
    Task<PublicDataLookupResponse<ApartmentComplexItem>> SearchAsync(
        ApartmentComplexSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<PublicDataLookupResponse<ApartmentComplexBasicItem>> GetBasicInfoAsync(
        ApartmentComplexBasicRequest request,
        CancellationToken cancellationToken = default);
}

public interface IApartmentManagementFeeLookupService
{
    Task<PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>> GetSnapshotAsync(
        ApartmentManagementFeeSnapshotRequest request,
        CancellationToken cancellationToken = default);

    Task<ApartmentGroupCommerceOffsetSimulationResult> SimulateGroupCommerceOffsetAsync(
        ApartmentGroupCommerceOffsetSimulationRequest request,
        CancellationToken cancellationToken = default);
}

public interface IOrdererGroupScopeLookupService
{
    PublicDataLookupResponse<OrdererGroupScopeCandidateItem> FindCandidates(
        OrdererGroupScopeLookupRequest request);
}
