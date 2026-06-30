using Hongdal.Application.Exploration;
using Hongdal.Contracts.Common.Exploration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using 홍달.Data;

namespace Hongdal.Controllers.Shipper.Request01;

[ApiController]
[Authorize(Roles = 역할명.화주)]
[Route("api/v1/shipper/exploration-inbox")]
public sealed class 화주탐색문의Controller : ControllerBase
{
    private readonly ISender _sender;

    public 화주탐색문의Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 목록()
    {
        var items = await _sender.Send(new 탐색문의목록조회Query(현재사용자Id(), 역할명.화주));
        return Ok(items);
    }

    [HttpGet("{campaignId:long}")]
    public async Task<IActionResult> 상세(long campaignId)
    {
        var item = await _sender.Send(new 탐색문의상세조회Query(현재사용자Id(), 역할명.화주, campaignId));
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{campaignId:long}/reply")]
    public async Task<IActionResult> 응답(long campaignId, [FromBody] 탐색문의응답요청 request)
    {
        var result = await _sender.Send(new 탐색문의응답Command(현재사용자Id(), 역할명.화주, campaignId, request));
        return result.IsSuccess ? Ok() : BadRequest(new { errors = result.Errors.Select(x => x.Message).ToArray() });
    }

    private string 현재사용자Id()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("화주 인증 정보가 없습니다.");
    }
}
