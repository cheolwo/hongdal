using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V3_5)]
[SsalddelApiAudience(SsalddelActor.ShipperOrSeller)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PotatoProductionDistributionWorld,
    SsalddelCodeLayer.Api,
    "인증 생산자에게 감자 상품·가격과 검증 가능한 source 관계만 포함한 World vertical slice를 제공한다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    ContractType = typeof(감자생산유통WorldResponse),
    FlowOrder = 40,
    Boundary = "현재 operational 응답은 ProductOnly 또는 Unverified만 반환한다. 화물·창고·마트를 상품명으로 연결하지 않는다.")]
[ApiController]
[Authorize]
[Route(감자생산유통WorldRoutes.조회)]
public sealed class 감자생산유통WorldController(
    I감자생산유통World조회UseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 조회(
        [FromQuery] 감자생산유통World조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await useCase.조회Async(request, cancellationToken));
}
