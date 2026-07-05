using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Admin.Settlement;
using Hongdal.Contracts.Admin.Settlement;

namespace Hongdal.Controllers.Admin.Settlement05;

[ApiController]
[Route("api/v1/admin/driver-settlements")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 기사월정산관리Controller : ControllerBase
{
    private readonly ISender _sender;

    public 기사월정산관리Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회([FromQuery] int? year, [FromQuery] int? month, [FromQuery] string? driverId)
    {
        var items = await _sender.Send(new 기사월정산관리목록조회Query(year, month, driverId));

        return Ok(items);
    }
}
