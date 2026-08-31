using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Content;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin.Content;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V3_5)]
[Route(개체시각대응Codes.Route)]
[Authorize(Policy = 개체시각대응Codes.Policy)]
[SsalddelCodeMetadata(개체시각대응Codes.Feature, SsalddelCodeLayer.Api,
    "관리자가 권한 있는 대상의 시각 대응·검토·선택·이력을 조회한다.", StepKey = "api", FlowOrder = 10,
    Boundary = "원천 수집·자산 업로드·Unity 배치·공개 게시 API가 아니다.")]
public sealed class 개체시각대응Controller(개체시각대응UseCase useCase, 개체시각목록UseCase assets,
    게임객체시각구성UseCase compositions, 게임객체WI참여UseCase wiUses, 보유시각자산목록UseCase inventory) : ControllerBase
{
    [HttpPost("inventory/import")]
    [RequestSizeLimit(16 * 1024 * 1024)]
    public async Task<ActionResult<보유시각자산반입Result>> 보유목록반입([FromBody] 보유시각자산반입Request request, CancellationToken ct)
    {
        var result = await inventory.ImportAsync(User, request, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Persisted"), result);
    }
    [HttpGet("inventory")]
    public async Task<ActionResult<보유시각자산목록Result>> 보유목록조회([FromQuery] string? group, [FromQuery] string? kind,
        [FromQuery] string? name, [FromQuery] string? visualKey, [FromQuery] string? revision, [FromQuery] int skip, CancellationToken ct,
        [FromQuery] string? taxonomyPath = null, [FromQuery] string? classificationState = null,
        [FromQuery] string? trait = null, [FromQuery] string? taxonomyHash = null)
    {
        var result = await inventory.ListAsync(User, group, kind, name, visualKey, revision, skip, ct, taxonomyPath, classificationState, trait, taxonomyHash);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    [HttpPost("inventory/classifications")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<ActionResult<보유시각분류Result>> 보유분류반입([FromBody] 보유시각분류반입Request request, CancellationToken ct)
    {
        var result = await inventory.ImportClassificationsAsync(User, request, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Persisted"), result);
    }
    [HttpPost("wi-uses/import")]
    public async Task<ActionResult<게임객체WI추출Result>> WI참여가져오기([FromBody] 게임객체WI추출Request request, CancellationToken ct)
    {
        var result = await wiUses.ImportAsync(User, request, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Persisted"), result);
    }
    [HttpGet("wi-uses")]
    public async Task<ActionResult<게임객체WI조회Result>> WI참여조회([FromQuery] string? wi, [FromQuery] string? definitionId, [FromQuery] int skip, CancellationToken ct)
    {
        var result = await wiUses.ListAsync(User, wi, definitionId, skip, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    [HttpGet("wi-inventory")]
    public async Task<ActionResult<게임객체WI목록Result>> WI대조목록조회(CancellationToken ct)
    {
        var result = await wiUses.InventoryAsync(User, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    [HttpPost("compositions")]
    public async Task<ActionResult<게임객체시각구성Result>> 구성초안저장([FromBody] 게임객체시각구성Request request, CancellationToken ct)
    {
        var result = await compositions.SaveAsync(User, request, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Persisted"), result);
    }
    [HttpGet("compositions")]
    public async Task<ActionResult<게임객체시각구성목록Result>> 구성목록조회([FromQuery] int skip, CancellationToken ct)
    {
        var result = await compositions.ListAsync(User, skip, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    [HttpGet("compositions/{definitionId}")]
    public async Task<ActionResult<게임객체시각구성Result>> 구성판본조회(string definitionId, [FromQuery] long? revision, CancellationToken ct)
    {
        var result = await compositions.GetAsync(User, definitionId, revision, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    [HttpGet("assets")]
    public async Task<ActionResult<개체시각목록Result>> Assets([FromQuery] string? visualKey, [FromQuery] int skip, CancellationToken ct)
    {
        var result = await assets.ListAsync(User, visualKey, skip, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    [HttpPost("assets/import")]
    public async Task<ActionResult<개체시각목록Result>> ImportAssets([FromBody] 개체시각자산입력[] inputs, CancellationToken ct)
    {
        var result = await assets.ImportAsync(User, inputs, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Persisted"), result);
    }
    [HttpPost]
    public async Task<ActionResult<개체시각대응Result>> Save([FromBody] 개체시각대응Request request, CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(User, request, ct);
        return StatusCode(Status(result.Diagnostic, result.Success), result);
    }
    [HttpGet]
    public async Task<ActionResult<개체시각대응목록Result>> List([FromQuery] 개체시각대상Query query, CancellationToken ct)
    {
        var result = await useCase.ListAsync(User, query, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    [HttpGet("resolve")]
    public async Task<ActionResult<개체시각선택Result>> Resolve([FromQuery] 개체시각대상Query query, CancellationToken ct)
    {
        var result = await useCase.ResolveAsync(User, query, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic is "Selected" or "Unmapped"), result);
    }
    [HttpGet("{bindingId}/history")]
    public async Task<ActionResult<개체시각이력Result>> History(string bindingId, [FromQuery] 개체시각대상Query query, CancellationToken ct)
    {
        var result = await useCase.HistoryAsync(User, bindingId, query, ct);
        return StatusCode(Status(result.Diagnostic, result.Diagnostic == "Found"), result);
    }
    private static int Status(string diagnostic, bool success) => success ? 200 : diagnostic switch
    {
        "Unauthorized" => 401,
        "Forbidden" or "PrincipalMismatch" => 403,
        "FeatureDisabled" => 503,
        "NotFound" or "NotFoundOrNotAuthorized" or "NotFoundOrOutsideAuthorizedWindow" => 404,
        "SourceAccessOrQueryFailed" or "StorageWriteFailedOrConflict" or "CompositionStorageConflictOrFailure" or "ExtractionStorageConflictOrFailure" or "SourceUnavailable" or "InventoryStorageConflictOrFailure" => 503,
        "BindingConflict" or "CatalogConflict" or "SourceConflict" or "ContextChanged" or "RevisionConflict" or "IdempotencyConflict" or "SourceDrift" or "DefinitionConflict" or "RelationConflict" or "InventoryRevisionConflict" or "InventoryFileDrift" => 409,
        _ => 400
    };
}
