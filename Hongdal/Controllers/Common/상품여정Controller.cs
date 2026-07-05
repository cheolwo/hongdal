using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Application.ProductJourney.Queries;

namespace Hongdal.Controllers.Common;

[ApiController]
[Authorize]
[Route("api/v1/products")]
public sealed class 상품여정Controller : ControllerBase
{
    private readonly ISender _sender;

    public 상품여정Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("scan/{code}/journey")]
    public async Task<IActionResult> 스캔코드기반상품여정조회(string code, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new 스캔코드기반상품여정조회Query(code), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
