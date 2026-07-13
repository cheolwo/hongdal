using Hongdal.ApiMetadata;
using Hongdal.Services.Speech;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Content07;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/admin/speech/typecast/voices")]
[Authorize(Policy = "서버관리자전용")]
public sealed class Typecast음성카탈로그Controller : ControllerBase
{
    private readonly ITypecast음성카탈로그Service _service;

    public Typecast음성카탈로그Controller(ITypecast음성카탈로그Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? model,
        [FromQuery] string? gender,
        [FromQuery] string? age,
        [FromQuery(Name = "use_case")] string? useCase,
        [FromQuery(Name = "voice_type")] string? voiceType,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.목록조회Async(
            new Typecast음성카탈로그검색조건(model, gender, age, useCase, voiceType, activeOnly),
            cancellationToken);
        return Ok(items);
    }

    [HttpGet("{voiceId}")]
    public async Task<IActionResult> 단건조회(string voiceId, CancellationToken cancellationToken)
    {
        var item = await _service.단건조회Async(voiceId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> 동기화(CancellationToken cancellationToken)
        => Ok(await _service.동기화Async(cancellationToken));
}
