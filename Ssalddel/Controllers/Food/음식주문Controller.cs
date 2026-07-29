using Ssalddel.ApiMetadata;
using Ssalddel.Application.Food;
using Ssalddel.Contracts.Food;
using Ssalddel.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Ssalddel.Security;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Food;

[SsalddelApiVersion(SsalddelProductVersion.V3_0, FeatureKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow, WorkflowKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.FoodDelivery)]
[RequireVersionFeature(VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[ApiController]
[Route("api/v1/food-orders")]
public sealed class 음식주문Controller(
    I음식주문접수UseCase commandUseCase,
    I주문자음식주문조회UseCase readUseCase,
    I음식점음식주문조회UseCase restaurantReadUseCase) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> 목록조회(
        [FromQuery] 주문자음식주문목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await readUseCase.목록Async(request, 현재사용자Id(), cancellationToken));

    [HttpGet("{orderNo}")]
    [Authorize]
    public async Task<IActionResult> 상세조회(string orderNo, CancellationToken cancellationToken)
        => this.ToActionResult(await readUseCase.상세Async(orderNo, 현재사용자Id(), cancellationToken));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<음식주문응답>> 등록([FromBody] 음식주문등록요청 request, CancellationToken cancellationToken)
    {
        request.주문자UserId = 현재사용자Id()
            ?? throw new InvalidOperationException("로그인 사용자 식별자를 확인할 수 없습니다.");
        return Ok(await commandUseCase.등록Async(request, cancellationToken));
    }

    [HttpPost("{orderNo}/receipt-confirmation")]
    [Authorize]
    public async Task<ActionResult<음식주문응답>> 주문자수령확인(
        string orderNo,
        [FromBody] 주문자음식주문수령확인요청 request,
        CancellationToken cancellationToken)
    {
        var ordererUserId = 현재사용자Id();
        if (string.IsNullOrWhiteSpace(ordererUserId))
        {
            return Forbid();
        }

        try
        {
            var order = await commandUseCase.주문자수령확인Async(
                orderNo,
                request,
                ordererUserId,
                cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("restaurant/inbox")]
    [Authorize(Policy = "음식점운영자전용")]
    public ActionResult<음식점주문수신함응답> 음식점수신함(
        [FromQuery] 음식점주문수신함조회요청 request)
    {
        if (!string.IsNullOrWhiteSpace(request.처리상태)
            && !음식점주문수신함처리상태코드.전체목록.Contains(
                request.처리상태.Trim(),
                StringComparer.Ordinal))
        {
            return BadRequest(new { message = "처리상태는 미처리, 완료 또는 전체만 사용할 수 있습니다." });
        }

        var restaurantId = 현재음식점Id();
        return restaurantId is null
            ? Forbid()
            : Ok(restaurantReadUseCase.목록(request, restaurantId.Value));
    }

    [HttpGet("restaurant/inbox/{orderNo}")]
    [Authorize(Policy = "음식점운영자전용")]
    public ActionResult<음식주문응답> 음식점상세(string orderNo)
    {
        var restaurantId = 현재음식점Id();
        if (restaurantId is null)
        {
            return Forbid();
        }

        var order = restaurantReadUseCase.상세(orderNo, restaurantId.Value);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{orderNo}/restaurant-acceptance")]
    [Authorize(Policy = "음식점운영자전용")]
    public async Task<ActionResult<음식주문응답>> 음식점수락(
        string orderNo,
        [FromBody] 음식점주문수락요청 request,
        CancellationToken cancellationToken)
    {
        var restaurantId = 현재음식점Id();
        var actorUserId = 현재사용자Id();
        if (restaurantId is null || string.IsNullOrWhiteSpace(actorUserId))
        {
            return Forbid();
        }

        if (restaurantReadUseCase.상세(orderNo, restaurantId.Value) is null)
        {
            return NotFound();
        }

        try
        {
            var order = await commandUseCase.음식점수락Async(
                orderNo,
                request,
                actorUserId,
                cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{orderNo}/restaurant-progress")]
    [Authorize(Policy = "음식점운영자전용")]
    public async Task<ActionResult<음식주문응답>> 음식점진행변경(
        string orderNo,
        [FromBody] 음식점주문진행변경요청 request,
        CancellationToken cancellationToken)
    {
        var restaurantId = 현재음식점Id();
        var actorUserId = 현재사용자Id();
        if (restaurantId is null || string.IsNullOrWhiteSpace(actorUserId))
        {
            return Forbid();
        }

        if (restaurantReadUseCase.상세(orderNo, restaurantId.Value) is null)
        {
            return NotFound();
        }

        try
        {
            var order = await commandUseCase.음식점진행변경Async(
                orderNo,
                request,
                actorUserId,
                cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? 현재사용자Id()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? User.Identity?.Name;

    private long? 현재음식점Id()
        => 음식점접근범위Resolver.음식점Id조회(User);
}
