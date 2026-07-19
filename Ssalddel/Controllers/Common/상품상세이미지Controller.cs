using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Application.Warehouse;
using 살뜰.도메인.사용자;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[ApiController]
[Authorize]
[Route("api/v1/product-detail-images")]
public sealed class 상품상세이미지Controller : ControllerBase
{
    private readonly ISender _sender;

    public 상품상세이미지Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("requests")]
    public async Task<IActionResult> 생성요청([FromBody] 상품상세이미지생성요청 request, CancellationToken cancellationToken)
    {
        var command = new 상품상세이미지생성요청Command(
            request.요청자Id,
            request.상품Id,
            request.주문Id,
            request.통관절차Id,
            request.참여자Id,
            request.실행역할);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}

public sealed class 상품상세이미지생성요청
{
    public string 요청자Id { get; set; } = string.Empty;
    public long 상품Id { get; set; }
    public long? 주문Id { get; set; }
    public long? 통관절차Id { get; set; }
    public string 참여자Id { get; set; } = string.Empty;
    public 살뜰역할유형 실행역할 { get; set; } = 살뜰역할유형.판매자;
}
