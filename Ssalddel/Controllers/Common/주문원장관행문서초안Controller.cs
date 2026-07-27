using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Documents;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Filters;
using Ssalddel.Services.Community;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[ApiController]
[Authorize]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Api,
    "같이 주문·같이 수입 원장에서 관행 문서 검토용 초안을 조회합니다.",
    ContractType = typeof(원장관행문서초안묶음응답),
    FlowOrder = 39,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "문서 발행·서명·신고·외부 전송 없이 HTML과 구조화된 검토용 초안만 반환합니다.")]
[Route("api/v1/community/order-ledgers/{원장Id}/document-drafts")]
public sealed class 주문원장관행문서초안Controller : CommunityControllerBase
{
    private readonly I원장관행문서초안UseCase _useCase;
    private readonly I원장관행문서보관UseCase _보관UseCase;

    public 주문원장관행문서초안Controller(
        I원장관행문서초안UseCase useCase,
        I원장관행문서보관UseCase 보관UseCase)
    {
        _useCase = useCase;
        _보관UseCase = 보관UseCase;
    }

    [HttpGet("catalog")]
    [ProducesResponseType(typeof(원장관행문서카탈로그응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 카탈로그(
        string 원장Id,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _useCase.카탈로그조회Async(
            원장Id,
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(원장관행문서초안묶음응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 생성(
        string 원장Id,
        [FromQuery] string? documentTypeCode,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _useCase.생성Async(
            원장Id,
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            documentTypeCode,
            cancellationToken));

    [HttpPost("{documentTypeCode}/archive")]
    [ProducesResponseType(typeof(원장관행문서보관응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 보관(
        string 원장Id,
        string documentTypeCode,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _보관UseCase.보관Async(
            원장Id,
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            documentTypeCode,
            cancellationToken));
}
