using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ledger,
    SsalddelModuleKind.Api,
    "지도 마커와 연결된 원장의 공개 집계와 본인·참여자 최소 projection 조회 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.ClosedLoop,
    Boundary = "운영자·검토자 권한은 query가 아니라 서버 authorization policy로만 결정하며 사용자별 응답을 공유 cache에 저장하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Api,
    "지도 marker와 template별 원장 projection을 권한·페이지 범위에 맞게 조회",
    ContractType = typeof(I커뮤니티세계지도원장ProjectionUseCase),
    FlowOrder = 31,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "공개 요청은 임계값 집계만 반환하고 원장 ID·개인정보·상세 위치·거래 원문을 반환하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[AllowAnonymous]
[Route(커뮤니티세계지도Routes.LedgerProjectionApi)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[SsalddelApiContractName("CommunityWorldMapLedgerProjectionsController")]
public sealed class 커뮤니티세계지도원장ProjectionController(
    I커뮤니티세계지도원장ProjectionUseCase useCase,
    IAuthorizationService authorizationService) : ControllerBase
{
    private const int DefaultLimit = 50;
    private const int MaximumLimit = 50;
    private const int MaximumOffset = 200;

    [HttpGet]
    [SsalddelApiContractName("GetLedgerProjections")]
    public async Task<ActionResult<커뮤니티세계지도원장ProjectionBatchDto>> 조회(
        [FromQuery] string templateKey,
        [FromQuery] string markerId,
        [FromQuery] string? administrativeRegionKey = null,
        [FromQuery] string? countryCode = null,
        [FromQuery] string evidenceFreshnessCode = 커뮤니티세계지도FreshnessCodes.Unknown,
        [FromQuery] string? evidenceSnapshotVersion = null,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        if (!CommunityLedgerTemplateKeys.All.Contains(templateKey?.Trim() ?? string.Empty, StringComparer.Ordinal)
            || !IsValidStableValue(markerId, 160)
            || !IsValidOptionalValue(administrativeRegionKey, 120)
            || !IsValidOptionalValue(countryCode, 8)
            || !IsValidOptionalValue(evidenceSnapshotVersion, 200)
            || !IsKnownFreshness(evidenceFreshnessCode)
            || offset is < 0 or > MaximumOffset
            || limit is < 1 or > MaximumLimit)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "지도 원장 projection 조회 조건을 확인해 주세요",
                Detail = "등록된 원장 template, marker, 공개 위치·근거 version과 0~200 offset, 1~50 limit을 사용해야 합니다."
            });
        }

        var authenticated = User.Identity?.IsAuthenticated == true;
        var viewerUserId = authenticated
            ? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
            : null;
        var operatorAuthorized = authenticated
            && (await authorizationService.AuthorizeAsync(
                User,
                resource: null,
                policyName: "물류운영사용자전용")).Succeeded;
        var reviewerAuthorized = authenticated
            && (await authorizationService.AuthorizeAsync(
                User,
                resource: null,
                policyName: "서버관리자전용")).Succeeded;

        var projections = await useCase.조회Async(
            new 커뮤니티세계지도원장ProjectionQuery(
                templateKey.Trim(),
                markerId.Trim(),
                viewerUserId,
                operatorAuthorized,
                reviewerAuthorized,
                Clean(administrativeRegionKey),
                Clean(countryCode)?.ToUpperInvariant(),
                evidenceFreshnessCode,
                Clean(evidenceSnapshotVersion)),
            cancellationToken);

        var sourceMayBeTruncated = projections.Count >= MaximumOffset
                                   || projections.Any(projection => string.Equals(
                                       projection.AggregateBucketCode,
                                       커뮤니티세계지도원장집계BucketCodes.Coarsened,
                                       StringComparison.Ordinal));
        var items = projections.Skip(offset).Take(limit).ToArray();
        return Ok(new 커뮤니티세계지도원장ProjectionBatchDto
        {
            Items = items,
            Offset = offset,
            Limit = limit,
            ReturnedCount = items.Length,
            AvailableCount = projections.Count,
            HasMore = offset + items.Length < projections.Count || sourceMayBeTruncated,
            SourceMayBeTruncated = sourceMayBeTruncated
        });
    }

    private static bool IsKnownFreshness(string? value)
        => value is 커뮤니티세계지도FreshnessCodes.Fresh
            or 커뮤니티세계지도FreshnessCodes.Stale
            or 커뮤니티세계지도FreshnessCodes.Expired
            or 커뮤니티세계지도FreshnessCodes.Unknown;

    private static bool IsValidStableValue(string? value, int maximumLength)
        => !string.IsNullOrWhiteSpace(value)
           && value.Trim().Length <= maximumLength
           && !value.Any(char.IsControl);

    private static bool IsValidOptionalValue(string? value, int maximumLength)
        => value is null || IsValidStableValue(value, maximumLength);

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
