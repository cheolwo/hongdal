using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Admin.Operating;
using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Controllers.Admin.Progress03;

[ApiController]
[Route("api/v1/admin/dispatch-plans")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 배차계획관리Controller : ControllerBase
{
    private readonly ISender _sender;

    public 배차계획관리Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? 기사Id,
        [FromQuery] string? 상태,
        [FromQuery] DateTime? 신청일From,
        [FromQuery] DateTime? 신청일To)
    {
        var items = await _sender.Send(new 배차계획목록조회Query(기사Id, 상태, 신청일From, 신청일To));

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> 단건조회(long id)
    {
        var item = await _sender.Send(new 배차계획단건조회Query(id));

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}
