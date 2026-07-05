using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Images;

namespace Hongdal.Controllers.Common;

[ApiController]
[Route("api/v1/kie-ai")]
public class KieAiCallbackController : ControllerBase
{
    private readonly HongdalContext _db;
    private readonly I샘플이미지생성Service _sampleImageGenerationService;

    public KieAiCallbackController(HongdalContext db, I샘플이미지생성Service sampleImageGenerationService)
    {
        _db = db;
        _sampleImageGenerationService = sampleImageGenerationService;
    }

    [HttpPost("callback")]
    public async Task<IActionResult> 콜백([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        var rawJson = payload.GetRawText();
        var taskId = ResolveTaskId(payload);
        if (string.IsNullOrWhiteSpace(taskId))
        {
            return Ok(new { accepted = true, processed = false });
        }

        var job = await _db.생성이미지작업.FirstOrDefaultAsync(x => x.외부TaskId == taskId, cancellationToken);
        if (job is null)
        {
            return Ok(new { accepted = true, processed = false });
        }

        var processed = await _sampleImageGenerationService.작업후처리Async(job.Id, rawJson, cancellationToken);
        return Ok(new { accepted = true, processed });
    }

    private static string? ResolveTaskId(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (payload.TryGetProperty("taskId", out var taskIdElement))
        {
            return taskIdElement.GetString();
        }

        if (payload.TryGetProperty("data", out var dataElement)
            && dataElement.ValueKind == JsonValueKind.Object
            && dataElement.TryGetProperty("taskId", out var nestedTaskIdElement))
        {
            return nestedTaskIdElement.GetString();
        }

        return null;
    }
}
