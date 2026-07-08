using System.Security.Claims;
using Hongdal.ApiMetadata;
using Hongdal.Application.Driver.Food;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Driver.Food;

[HongdalApiVersion(HongdalProductVersion.V3_0)]
[ApiController]
[Route("api/v1/drivers/{driverId}/monthly-settlements")]
[Authorize(Roles = 역할명.기사)]
public class 배달기사월정산Controller : ControllerBase
{
    private readonly I배달기사월정산UseCase _useCase;

    public 배달기사월정산Controller(I배달기사월정산UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("current")]
    public async Task<IActionResult> 당월조회(string driverId, CancellationToken cancellationToken)
    {
        var result = await _useCase.당월조회Async(driverId, 현재사용자Id(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{year:int}/{month:int}/mark-paid")]
    public async Task<IActionResult> 결제완료처리(string driverId, int year, int month, CancellationToken cancellationToken)
    {
        var result = await _useCase.결제완료처리Async(driverId, year, month, 현재사용자Id(), cancellationToken);
        return this.ToActionResult(result);
    }

    private string? 현재사용자Id()
        => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
