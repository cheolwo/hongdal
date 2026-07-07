using Hongdal.Controllers;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[Route("api/v1/orderer/group-purchase-overseas-shipments")]
public sealed class GroupPurchaseOverseasShipmentTrackingController : ControllerBase
{
    private readonly IGroupPurchaseOverseasShipmentTrackingStore _store;

    public GroupPurchaseOverseasShipmentTrackingController(IGroupPurchaseOverseasShipmentTrackingStore store)
    {
        _store = store;
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(
        [FromQuery] string documentManagementNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetByDocumentManagementNumberAsync(documentManagementNumber, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("문서관리번호에 해당하는 공동주문 해외 선적 정보를 찾을 수 없습니다.")
                : Ok(GroupPurchaseOverseasShipmentTrackingProjection.ToPublicDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "문서관리번호가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
