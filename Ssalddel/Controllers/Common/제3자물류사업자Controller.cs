using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Services.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[Route("api/v1/operations/third-party-logistics/providers")]
[SsalddelApiContractName("ThirdPartyLogisticsProvidersController")]
public sealed class 제3자물류사업자Controller : ControllerBase
{
    private readonly IThirdPartyLogisticsProviderDirectoryService _제3자물류사업자Service;

    public 제3자물류사업자Controller(
        IThirdPartyLogisticsProviderDirectoryService 제3자물류사업자Service)
    {
        _제3자물류사업자Service = 제3자물류사업자Service;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("Get")]
    public ActionResult<ThirdPartyLogisticsProviderDirectoryResponse> 목록조회(
        [FromQuery] string? q = null,
        [FromQuery] string? capabilityCode = null,
        [FromQuery] string? segmentCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var response = _제3자물류사업자Service.Search(new ThirdPartyLogisticsProviderDirectoryQuery
        {
            SearchText = q,
            CapabilityCode = capabilityCode,
            SegmentCode = segmentCode,
            Page = page,
            PageSize = pageSize
        });

        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet("collective-purchase")]
    [AllowAnonymous]
    [SsalddelApiContractName("GetForCollectivePurchase")]
    public ActionResult<CollectivePurchaseLogisticsDirectoryResponse>
        공동구매물류조회(
            [FromQuery] string? q = null,
            [FromQuery] string? stageCode = null,
            [FromQuery] string? productHandlingCode = null,
            [FromQuery] string? engagementSignalCode = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
    {
        var response = _제3자물류사업자Service.SearchForCollectivePurchase(
            new CollectivePurchaseLogisticsDirectoryQuery
            {
                SearchText = q,
                StageCode = stageCode,
                ProductHandlingCode = productHandlingCode,
                EngagementSignalCode = engagementSignalCode,
                Page = page,
                PageSize = pageSize
            });

        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet("bonded-to-door")]
    [AllowAnonymous]
    [SsalddelApiContractName("GetBondedToDoor")]
    public ActionResult<BondedToDoorLogisticsDirectoryResponse> 보세창고문앞배송조회(
        [FromQuery] string? q = null,
        [FromQuery] string? stageCode = null,
        [FromQuery] string? storageModelCode = null,
        [FromQuery] string? stateCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var response = _제3자물류사업자Service.SearchBondedToDoor(
            new BondedToDoorLogisticsDirectoryQuery
            {
                SearchText = q,
                StageCode = stageCode,
                StorageModelCode = storageModelCode,
                StateCode = stateCode,
                Page = page,
                PageSize = pageSize
            });

        return response.Success ? Ok(response) : NotFound(response);
    }
}
