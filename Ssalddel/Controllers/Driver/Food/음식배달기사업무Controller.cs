using Ssalddel.ApiMetadata;
using Ssalddel.Application.Driver.Food;
using Ssalddel.Controllers;
using Ssalddel.Contracts.Driver.Food;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.도메인.공통;

namespace Ssalddel.Controllers.Driver.Food;

[SsalddelApiVersion(SsalddelProductVersion.V3_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelApiCapability(SsalddelCapability.FoodDelivery)]
[SsalddelApiOperation(SsalddelOperation.Execute)]
[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route("api/v1/driver/food-deliveries")]
[SsalddelApiContractName("FoodDeliveryDriverController")]
public sealed class 음식배달기사업무Controller : DriverControllerBase
{
    private readonly IFoodDeliveryDriverWorkspaceUseCase _업무공간UseCase;
    private readonly IFoodDeliveryDriverRouteService _경로Service;
    private readonly I음식배달기사업무Service _음식배달기사업무Service;

    public 음식배달기사업무Controller(
        IFoodDeliveryDriverWorkspaceUseCase 업무공간UseCase,
        IFoodDeliveryDriverRouteService 경로Service,
        I음식배달기사업무Service 음식배달기사업무Service)
    {
        _업무공간UseCase = 업무공간UseCase;
        _경로Service = 경로Service;
        _음식배달기사업무Service = 음식배달기사업무Service;
    }

    [HttpGet("workspace")]
    [SsalddelApiContractName("GetWorkspace")]
    public async Task<IActionResult> 업무공간조회(CancellationToken cancellationToken)
        => Ok(await _업무공간UseCase.GetAsync(CurrentDriverId(), cancellationToken));

    [HttpGet("offers")]
    [SsalddelApiContractName("GetOffers")]
    public async Task<IActionResult> 제안목록조회(CancellationToken cancellationToken)
        => Ok(await _음식배달기사업무Service.제안조회Async(CurrentDriverId(), cancellationToken));

    [HttpPost("offers/{offerId}/accept")]
    [SsalddelApiContractName("Accept")]
    public async Task<IActionResult> 제안수락(
        [FromRoute(Name = "offerId")] string 제안Id,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _음식배달기사업무Service.수락Async(
            CurrentDriverId(),
            제안Id,
            cancellationToken));

    [HttpPost("offers/{offerId}/reject")]
    [SsalddelApiContractName("Reject")]
    public async Task<IActionResult> 제안거절(
        [FromRoute(Name = "offerId")] string 제안Id,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _음식배달기사업무Service.거절Async(
            CurrentDriverId(),
            제안Id,
            cancellationToken));

    [HttpPost("bundles/accept")]
    [SsalddelApiContractName("AcceptBundle")]
    public async Task<IActionResult> 묶음제안수락(
        [FromBody] FoodDeliveryBundleAcceptRequest 요청,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _음식배달기사업무Service.묶음수락Async(
            CurrentDriverId(),
            요청?.OfferIds ?? [],
            cancellationToken));

    [HttpPost("offers/{offerId}/pickup-complete")]
    [SsalddelApiContractName("ConfirmPickup")]
    public async Task<IActionResult> 픽업완료(
        [FromRoute(Name = "offerId")] string 제안Id,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _음식배달기사업무Service.픽업완료Async(
            CurrentDriverId(),
            제안Id,
            cancellationToken));

    [HttpPost("offers/{offerId}/delivery-complete")]
    [SsalddelApiContractName("Complete")]
    public async Task<IActionResult> 전달완료(
        [FromRoute(Name = "offerId")] string 제안Id,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _음식배달기사업무Service.전달완료Async(
            CurrentDriverId(),
            제안Id,
            cancellationToken));

    [HttpPost("route")]
    [SsalddelApiContractName("GetRoute")]
    public async Task<IActionResult> 경로조회(
        [FromBody] FoodDeliveryDriverRouteRequestDto 요청,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _경로Service.GetRouteAsync(요청, cancellationToken));
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
