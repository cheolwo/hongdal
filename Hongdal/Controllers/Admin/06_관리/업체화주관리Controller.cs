using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Admin.Partners;
using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Controllers.Admin.Master06;

[ApiController]
[Route("api/v1/admin/partners")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 업체화주관리Controller : ControllerBase
{
    private readonly ISender _sender;

    public 업체화주관리Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("companies")]
    public async Task<IActionResult> 업체목록조회([FromQuery] string? 상태)
    {
        var items = await _sender.Send(new 업체목록조회Query(상태));

        return Ok(items);
    }

    [HttpGet("shippers")]
    public async Task<IActionResult> 화주목록조회()
    {
        var items = await _sender.Send(new 화주목록조회Query());

        return Ok(items);
    }
}
