using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Controllers.Platform;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[ApiController]
[Route("api/v1/public-data/modules")]
[SsalddelApiContractName("PublicDataPortalActiveApiModuleController")]
[SsalddelCodeMetadata(
    공공데이터포털활용ApiModuleFeature.Key,
    SsalddelCodeLayer.Api,
    "활용 중 공공데이터 API의 업무 모듈과 구현 연결 상태 조회 API",
    ContractType = typeof(공공데이터포털활용ApiModuleResponse),
    FlowOrder = 4,
    Boundary = "활용계정 식별자와 인증키 없이 공개 데이터 ID, 업무 경계와 client 연결 상태만 반환")]
public sealed class 공공데이터포털활용ApiModuleController : ControllerBase
{
    private readonly I공공데이터포털활용ApiModuleCatalog _moduleCatalog;

    public 공공데이터포털활용ApiModuleController(I공공데이터포털활용ApiModuleCatalog moduleCatalog)
    {
        _moduleCatalog = moduleCatalog;
    }

    [HttpGet]
    public ActionResult<공공데이터포털활용ApiModuleResponse> Get(
        [FromQuery] string? moduleKey,
        [FromQuery] string? implementationStatusCode)
    {
        return Ok(_moduleCatalog.GetCatalog(new 공공데이터포털활용ApiModuleQuery
        {
            ModuleKey = moduleKey,
            ImplementationStatusCode = implementationStatusCode
        }));
    }
}
