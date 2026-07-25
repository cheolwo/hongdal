using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.ImportReadiness;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[Route("api/v1/agricultural-fisheries/import-readiness")]
[SsalddelApiContractName("MeatImportReadinessController")]
public sealed class 육류수입준비Controller : CommunityControllerBase
{
    private readonly IMeatImportReadinessService _육류수입준비Service;

    public 육류수입준비Controller(IMeatImportReadinessService 육류수입준비Service)
    {
        _육류수입준비Service = 육류수입준비Service;
    }

    [HttpGet("diagram")]
    [AllowAnonymous]
    [SsalddelApiContractName("GetDiagram")]
    public ActionResult<MeatImportReadinessDiagramResponse> 절차도조회()
        => Ok(_육류수입준비Service.GetDiagram());

    [HttpGet("cases/mine")]
    [Authorize]
    [SsalddelApiContractName("ListMine")]
    public Task<ActionResult<MeatImportReadinessCaseListResponse>> 내사례목록조회(CancellationToken cancellationToken)
        => ExecuteAsync(() => _육류수입준비Service.ListMineAsync(CurrentUserId(), cancellationToken));

    [HttpGet("cases/{caseId}")]
    [Authorize]
    [SsalddelApiContractName("GetCase")]
    public async Task<ActionResult<MeatImportReadinessCaseResponse>> 사례조회(
        string caseId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _육류수입준비Service.GetCaseAsync(caseId, CurrentUserId(), cancellationToken);
            return result is null ? NotFoundProblem("육류 수입 준비 작업공간을 찾을 수 없습니다.") : Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ForbiddenProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpPost("cases")]
    [Authorize]
    [SsalddelApiContractName("CreateCase")]
    public async Task<ActionResult<MeatImportReadinessCaseResponse>> 사례생성(
        [FromBody] CreateMeatImportReadinessCaseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _육류수입준비Service.CreateCaseAsync(
                request,
                CurrentUserId(),
                CurrentDisplayName(),
                cancellationToken);
            return CreatedAtAction(nameof(사례조회), new { caseId = result.CaseId }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ForbiddenProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpPut("cases/{caseId}/steps/{stepCode}/status")]
    [Authorize]
    [SsalddelApiContractName("UpdateStepStatus")]
    public Task<ActionResult<MeatImportReadinessCaseResponse>> 단계상태수정(
        string caseId,
        string stepCode,
        [FromBody] UpdateMeatImportReadinessStepStatusRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(() => _육류수입준비Service.UpdateStepStatusAsync(
            caseId,
            stepCode,
            request,
            CurrentUserId(),
            CurrentDisplayName(),
            cancellationToken));

    [HttpPost("cases/{caseId}/steps/{stepCode}/evidences")]
    [Authorize]
    [SsalddelApiContractName("AddEvidence")]
    public Task<ActionResult<MeatImportReadinessCaseResponse>> 근거추가(
        string caseId,
        string stepCode,
        [FromBody] AddMeatImportReadinessEvidenceRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(() => _육류수입준비Service.AddEvidenceAsync(
            caseId,
            stepCode,
            request,
            CurrentUserId(),
            CurrentDisplayName(),
            cancellationToken));

    [HttpPost("cases/{caseId}/steps/{stepCode}/discussions")]
    [Authorize]
    [SsalddelApiContractName("AddDiscussion")]
    public Task<ActionResult<MeatImportReadinessCaseResponse>> 논의추가(
        string caseId,
        string stepCode,
        [FromBody] AddMeatImportReadinessDiscussionRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(() => _육류수입준비Service.AddDiscussionAsync(
            caseId,
            stepCode,
            request,
            CurrentUserId(),
            CurrentDisplayName(),
            cancellationToken));

    [HttpPost("cases/{caseId}/steps/{stepCode}/discussions/{discussionId}/resolve")]
    [Authorize]
    [SsalddelApiContractName("ResolveDiscussion")]
    public Task<ActionResult<MeatImportReadinessCaseResponse>> 논의해결(
        string caseId,
        string stepCode,
        string discussionId,
        [FromBody] ResolveMeatImportReadinessDiscussionRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(() => _육류수입준비Service.ResolveDiscussionAsync(
            caseId,
            stepCode,
            discussionId,
            request,
            CurrentUserId(),
            CurrentDisplayName(),
            cancellationToken));

    [HttpPost("cases/{caseId}/steps/{stepCode}/acknowledgements")]
    [Authorize]
    [SsalddelApiContractName("AcknowledgeStep")]
    public Task<ActionResult<MeatImportReadinessCaseResponse>> 단계확인(
        string caseId,
        string stepCode,
        [FromBody] AcknowledgeMeatImportReadinessStepRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(() => _육류수입준비Service.AcknowledgeStepAsync(
            caseId,
            stepCode,
            request,
            CurrentUserId(),
            CurrentDisplayName(),
            cancellationToken));

    private async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return Ok(await action());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ForbiddenProblem(ex.Message);
        }
        catch (MeatImportReadinessConcurrencyException ex)
        {
            return ConflictProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? string.Empty;

    private string CurrentDisplayName()
        => User.Identity?.Name
           ?? User.FindFirstValue("name")
           ?? "참여자";

    private ObjectResult BadRequestProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "육류 수입 준비 요청이 올바르지 않습니다.",
            detail: detail);

    private ObjectResult NotFoundProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "육류 수입 준비 정보를 찾을 수 없습니다.",
            detail: detail);

    private ObjectResult ForbiddenProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "이 육류 수입 준비 작업공간에 접근할 수 없습니다.",
            detail: detail);

    private ObjectResult ConflictProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "육류 수입 준비 정보가 이미 변경되었습니다.",
            detail: detail);
}
