using System.Security.Claims;
using Hongdal.Application.Driver.Notification;
using Hongdal.Controllers;
using Hongdal.Contracts.CommandSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Notification07;

[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route("api/v1/driver/command-feature-settings")]
public sealed class 기사Command기능설정Controller : ControllerBase
{
    private readonly ISender _sender;

    public 기사Command기능설정Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<Command기능설정목록응답>> 목록조회(CancellationToken cancellationToken)
    {
        var userId = 현재사용자Id();
        var result = await _sender.Send(new 기사Command기능설정목록조회Query(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{commandName}/{featureName}")]
    public async Task<IActionResult> 수정(string commandName, string featureName, [FromBody] Command기능설정수정요청 request, CancellationToken cancellationToken)
    {
        var userId = 현재사용자Id();
        var result = await _sender.Send(new 기사Command기능설정수정Command(userId, commandName, featureName, request.IsEnabled), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpDelete("{commandName}/{featureName}")]
    public async Task<IActionResult> 기본값으로복원(string commandName, string featureName, CancellationToken cancellationToken)
    {
        var userId = 현재사용자Id();
        var result = await _sender.Send(new 기사Command기능설정기본값복원Command(userId, commandName, featureName), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    private string 현재사용자Id()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
    }
}
