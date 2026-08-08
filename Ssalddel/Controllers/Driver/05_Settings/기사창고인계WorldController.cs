using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Driver.Transport;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using 살뜰.도메인.공통;

namespace Ssalddel.Controllers.Driver.Progress05;

[SsalddelApiVersion(SsalddelProductVersion.V3_5)]
[SsalddelApiCapability(SsalddelCapability.TransportExecution)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Api,
    "현재 기사의 운송 화물이 창고 입고 NPC에게 인계되는 World workflow를 제공한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(CargoWarehouseHandoffResponse),
    FlowOrder = 30,
    Boundary = "인증된 기사에게 현재 배정 운송과 연결된 입고의 비민감 상태만 제공한다.")]
[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route(NpcMovementRoutes.DriverWarehouseHandoff)]
public sealed class 기사창고인계WorldController : DriverControllerBase
{
    private readonly ISender sender;

    public 기사창고인계WorldController(ISender sender)
    {
        this.sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 창고화물인계조회(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new 기사창고화물인계조회Query(현재기사Id()), cancellationToken);

        return result is null
            ? this.ToNotFoundProblem("현재 창고에 인계할 운송 화물이 없습니다.")
            : Ok(result);
    }
}
