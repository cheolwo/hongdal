using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Api,
    "개별수출 원장만 참조하는 공동수출 물류 집계 원장을 생성하고 조회합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(무역확장원장응답),
    FlowOrder = 30,
    Boundary = "개별 신고 원본을 보존하고 포워더 자동 선정·외부 자동 전송을 허용하지 않습니다.")]
[ApiController]
[Authorize]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/orderer/group-export-ledgers")]
public sealed class 공동수출원장Controller : ControllerBase
{
    private readonly I무역확장원장UseCase _useCase;

    public 공동수출원장Controller(I무역확장원장UseCase useCase)
        => _useCase = useCase;

    [HttpPost]
    public async Task<IActionResult> 생성(
        [FromBody] 공동수출원장생성요청 request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        request.요청멱등키 = string.IsNullOrWhiteSpace(idempotencyKey)
            ? request.요청멱등키
            : idempotencyKey.Trim();
        return this.ToActionResult(await _useCase.공동수출생성Async(
            request,
            CurrentUserId(),
            User.IsInRole(역할명.서버관리자),
            cancellationToken));
    }

    [HttpGet("{ledgerId}")]
    public async Task<IActionResult> 조회(string ledgerId, CancellationToken cancellationToken)
        => this.ToActionResult(await _useCase.조회Async(
            ledgerId,
            CurrentUserId(),
            User.IsInRole(역할명.서버관리자),
            cancellationToken));

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? string.Empty;
}
