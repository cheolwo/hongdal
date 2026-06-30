using Hongdal.Application.Exploration;
using Hongdal.Contracts.Common.Exploration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using 홍달.Data;

namespace Hongdal.Controllers.Driver.Recommendation02;

[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route("api/v1/driver/exploration-campaigns")]
public sealed class 기사탐색캠페인Controller : ControllerBase
{
    private readonly ISender _sender;

    public 기사탐색캠페인Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 목록()
    {
        var items = await _sender.Send(new 탐색캠페인목록조회Query(현재사용자Id(), 역할명.기사));
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> 생성([FromBody] 탐색캠페인생성요청 request)
    {
        request.개시자역할 = 역할명.기사;
        request.대상역할 = string.IsNullOrWhiteSpace(request.대상역할) ? 역할명.화주 : request.대상역할;
        var result = await _sender.Send(new 탐색캠페인생성Command(request));
        return result.IsSuccess ? CreatedAtAction(nameof(상세), new { id = result.Value.Id }, result.Value) : BadRequest(new { errors = result.Errors.Select(x => x.Message).ToArray() });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> 상세(long id)
    {
        var item = await _sender.Send(new 탐색캠페인상세조회Query(현재사용자Id(), 역할명.기사, id));
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:long}/recommendations")]
    public async Task<IActionResult> 추천대상(long id)
    {
        var items = await _sender.Send(new 탐색캠페인추천대상조회Query(현재사용자Id(), 역할명.기사, id));
        return Ok(items);
    }

    [HttpPost("{id:long}/send")]
    public async Task<IActionResult> 발송(long id, [FromBody] 탐색캠페인발송요청 request)
    {
        var result = await _sender.Send(new 탐색캠페인발송Command(현재사용자Id(), 역할명.기사, id, request));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors.Select(x => x.Message).ToArray() });
    }

    private string 현재사용자Id()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
    }
}
