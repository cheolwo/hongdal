using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Ssalddel.Application.Admin.Progress;
using Ssalddel.Contracts.Admin.Progress;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin.Progress03;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Route("api/v1/admin/transports")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 운송진행관리Controller : ControllerBase
{
    private readonly ISender _sender;

    public 운송진행관리Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 운송목록조회([FromQuery] string? 상태)
    {
        var items = await _sender.Send(new 관리자운송목록조회Query(상태));

        return Ok(items);
    }

    [HttpGet("events")]
    public async Task<IActionResult> 운송이벤트조회([FromQuery] string? requestId)
    {
        var items = await _sender.Send(new 관리자운송이벤트조회Query(requestId));

        return Ok(items);
    }
}
