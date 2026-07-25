using Ssalddel.Application.Exploration;
using Ssalddel.Controllers;
using Ssalddel.Contracts.Common.Exploration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Data;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Driver.Recommendation02;

[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[SsalddelApiCapability(SsalddelCapability.Dispatch)]
[SsalddelApiOperation(SsalddelOperation.Request)]
[SsalddelApiOperation(SsalddelOperation.Manage)]
[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route("api/v1/driver/exploration-campaigns")]
public sealed class 기사탐색캠페인Controller : DriverControllerBase
{
    private readonly ISender _sender;

    public 기사탐색캠페인Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 목록()
    {
        var items = await _sender.Send(new 탐색캠페인목록조회Query(현재기사Id(), 역할명.기사));
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> 생성([FromBody] 탐색캠페인생성요청 request)
    {
        request.개시자역할 = 역할명.기사;
        request.대상역할 = string.IsNullOrWhiteSpace(request.대상역할) ? 역할명.화주 : request.대상역할;
        var result = await _sender.Send(new 탐색캠페인생성Command(request));
        return result.IsSuccess ? CreatedAtAction(nameof(상세), new { id = result.Value.Id }, result.Value) : this.ToProblemActionResult(result.Errors.Select(x => x.Message));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> 상세(long id)
    {
        var item = await _sender.Send(new 탐색캠페인상세조회Query(현재기사Id(), 역할명.기사, id));
        return item is null ? this.ToNotFoundProblem("탐색 캠페인을 찾을 수 없습니다.") : Ok(item);
    }

    [HttpGet("{id:long}/recommendations")]
    public async Task<IActionResult> 추천대상(long id)
    {
        var items = await _sender.Send(new 탐색캠페인추천대상조회Query(현재기사Id(), 역할명.기사, id));
        return Ok(items);
    }

    [HttpPost("{id:long}/send")]
    public async Task<IActionResult> 발송(long id, [FromBody] 탐색캠페인발송요청 request)
    {
        var result = await _sender.Send(new 탐색캠페인발송Command(현재기사Id(), 역할명.기사, id, request));
        return result.IsSuccess ? Ok(result.Value) : this.ToProblemActionResult(result.Errors.Select(x => x.Message));
    }

}
