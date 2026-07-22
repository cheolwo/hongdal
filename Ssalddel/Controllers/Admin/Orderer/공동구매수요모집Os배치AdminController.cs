using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.Orderer;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_0,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[Route("api/v1/admin/orderer/group-purchase-demand-os/batch-workloads")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandOperatingSystem,
    SsalddelCodeLayer.Api,
    "관리자가 1.0 OS 내부·공유 배치의 등록 상태, 일정, 선행 작업, 출처와 실행 경계를 읽습니다.",
    ContractType = typeof(I공동구매수요모집Os배치Catalog),
    FlowOrder = 16,
    Effects = SsalddelCodeEffect.None,
    Boundary = "관리자 읽기 전용 bootstrap API이며 기능 플래그가 꺼져 있어도 상태를 확인할 수 있지만 작업 실행이나 설정 변경은 하지 않습니다.")]
public sealed class 공동구매수요모집Os배치AdminController : ControllerBase
{
    private readonly I공동구매수요모집Os배치Catalog _catalog;

    public 공동구매수요모집Os배치AdminController(
        I공동구매수요모집Os배치Catalog catalog)
    {
        _catalog = catalog;
    }

    [HttpGet]
    [ProducesResponseType(typeof(공동구매수요모집Os배치Catalog응답), StatusCodes.Status200OK)]
    public ActionResult<공동구매수요모집Os배치Catalog응답> 조회()
        => Ok(_catalog.조회());
}
