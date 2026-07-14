using Hongdal.ApiMetadata;
using Hongdal.Application.Driver.Food;
using Hongdal.Controllers;
using Hongdal.Contracts.Driver.Food;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Food;

[HongdalApiVersion(HongdalProductVersion.V3_0)]
[HongdalApiWorkflow(HongdalWorkflow.FoodDelivery)]
[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route("api/v1/driver/food-deliveries")]
public sealed class FoodDeliveryDriverController : DriverControllerBase
{
    private readonly IFoodDeliveryDriverWorkspaceUseCase _workspace;
    private readonly IFoodDeliveryDriverRouteService _routes;
    private readonly I음식배달기사업무Service _driverWork;

    public FoodDeliveryDriverController(
        IFoodDeliveryDriverWorkspaceUseCase workspace,
        IFoodDeliveryDriverRouteService routes,
        I음식배달기사업무Service driverWork)
    {
        _workspace = workspace;
        _routes = routes;
        _driverWork = driverWork;
    }

    [HttpGet("workspace")]
    public async Task<IActionResult> GetWorkspace(CancellationToken cancellationToken)
        => Ok(await _workspace.GetAsync(CurrentDriverId(), cancellationToken));

    [HttpGet("offers")]
    public async Task<IActionResult> GetOffers(CancellationToken cancellationToken)
        => Ok(await _driverWork.제안조회Async(CurrentDriverId(), cancellationToken));

    [HttpPost("offers/{offerId}/accept")]
    public async Task<IActionResult> Accept(string offerId, CancellationToken cancellationToken)
        => this.ToActionResult(await _driverWork.수락Async(CurrentDriverId(), offerId, cancellationToken));

    [HttpPost("offers/{offerId}/reject")]
    public async Task<IActionResult> Reject(string offerId, CancellationToken cancellationToken)
        => this.ToActionResult(await _driverWork.거절Async(CurrentDriverId(), offerId, cancellationToken));

    [HttpPost("bundles/accept")]
    public async Task<IActionResult> AcceptBundle(
        [FromBody] FoodDeliveryBundleAcceptRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _driverWork.묶음수락Async(
            CurrentDriverId(),
            request?.OfferIds ?? [],
            cancellationToken));

    [HttpPost("offers/{offerId}/pickup-complete")]
    public async Task<IActionResult> ConfirmPickup(string offerId, CancellationToken cancellationToken)
        => this.ToActionResult(await _driverWork.픽업완료Async(CurrentDriverId(), offerId, cancellationToken));

    [HttpPost("offers/{offerId}/delivery-complete")]
    public async Task<IActionResult> Complete(string offerId, CancellationToken cancellationToken)
        => this.ToActionResult(await _driverWork.전달완료Async(CurrentDriverId(), offerId, cancellationToken));

    [HttpPost("route")]
    public async Task<IActionResult> GetRoute(
        [FromBody] FoodDeliveryDriverRouteRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _routes.GetRouteAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new FoodDeliveryDriverActionResultDto
            {
                IsSuccess = false,
                Message = ex.Message
            });
        }
    }

}
