using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Admin.Food;
using Ssalddel.Contracts.Admin.Food;

namespace Ssalddel.Controllers.Admin.Food;

[SsalddelApiVersion(SsalddelProductVersion.V3_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/food-orders")]
[SsalddelApiContractName("FoodOrderOperationsTraceController")]
public sealed class 음식주문운영추적Controller(I음식주문운영추적UseCase useCase) : ControllerBase
{
    [HttpGet("{orderNo}/operations-trace")]
    [SsalddelApiContractName("GetOperationsTrace")]
    public async Task<IActionResult> 조회(
        string orderNo,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await useCase.조회Async(orderNo, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message, StatusCodes.Status400BadRequest);
        }
    }
}
