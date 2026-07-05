using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Application.Warehouse;
using 홍달.도메인.사용자;

namespace Hongdal.Controllers.Common;

[ApiController]
[Authorize]
[Route("api/v1/customs")]
public sealed class 통관연동Controller : ControllerBase
{
    private readonly ISender _sender;

    public 통관연동Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("consents")]
    public async Task<IActionResult> 통관조회동의등록([FromBody] 통관조회동의등록요청 request, CancellationToken cancellationToken)
    {
        var command = new 통관조회동의등록Command(
            request.사용자Id,
            request.주문Id,
            request.통관절차Id,
            request.개인통관고유부호,
            request.수취인이름,
            request.휴대폰번호,
            request.우편번호,
            request.참여자Id,
            request.실행역할);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToNoContentActionResult(result);
    }
}

public sealed class 통관조회동의등록요청
{
    public string 사용자Id { get; set; } = string.Empty;
    public long 주문Id { get; set; }
    public long 통관절차Id { get; set; }
    public string 개인통관고유부호 { get; set; } = string.Empty;
    public string 수취인이름 { get; set; } = string.Empty;
    public string 휴대폰번호 { get; set; } = string.Empty;
    public string? 우편번호 { get; set; }
    public string 참여자Id { get; set; } = string.Empty;
    public 홍달역할유형 실행역할 { get; set; } = 홍달역할유형.주문자;
}
