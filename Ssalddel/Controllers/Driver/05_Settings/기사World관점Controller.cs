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
    "인증된 기사에게 도심 물류센터 운송자 관점을 read-only projection으로 제공한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(RolePerspectiveResponse),
    FlowOrder = 30,
    Boundary = "기사 role과 인증 user ID를 서버에서 확정하며 요청 파라미터로 다른 기사나 역할을 선택할 수 없다.")]
[ApiController]
[Authorize(Roles = 역할명.기사)]
[Route(RolePerspectiveRoutes.DriverUrbanLogisticsCenter)]
public sealed class 기사World관점Controller : DriverControllerBase
{
    private readonly ISender sender;

    public 기사World관점Controller(ISender sender)
    {
        this.sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 도심물류센터관점조회(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new 도심물류센터운송자관점조회Query(현재기사Id()),
            cancellationToken);

        return result is null
            ? this.ToNotFoundProblem("현재 운송 정보를 찾을 수 없습니다.")
            : Ok(result);
    }

    [HttpGet("npc-movement")]
    public async Task<IActionResult> 도심물류센터Npc이동조회(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new 도심물류센터운송자Npc이동조회Query(현재기사Id()),
            cancellationToken);

        return result is null
            ? this.ToNotFoundProblem("현재 도심 물류센터에서 표현할 NPC 이동이 없습니다.")
            : Ok(result);
    }
}
