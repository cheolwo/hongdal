using Hongdal.FoodApi.Contracts;
using Hongdal.FoodApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.FoodApi.Controllers.Common;

[ApiController]
[Route("api/v1/food-orders/dispatch/address-form")]
[Obsolete("Dispatch address endpoints are moving to the main Hongdal server. Use Hongdal.Controllers.Food endpoints.", false)]
public sealed class 배차주소Controller : ControllerBase
{
    private readonly 배차주소샘플Store _store;
    private readonly IKakao좌표변환Service _kakaoGeoService;

    public 배차주소Controller(배차주소샘플Store store, IKakao좌표변환Service kakaoGeoService)
    {
        _store = store;
        _kakaoGeoService = kakaoGeoService;
    }

    [HttpPost]
    public async Task<ActionResult<배차주소저장응답>> 저장([FromBody] 배차주소저장요청 request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.상차지기본주소) || string.IsNullOrWhiteSpace(request.하차지기본주소))
        {
            return BadRequest(new 배차주소저장응답 { 메시지 = "상차지/하차지 기본주소는 필수입니다." });
        }

        if (string.IsNullOrWhiteSpace(request.사업자등록번호) || request.사업자등록번호.Length != 10)
        {
            return BadRequest(new 배차주소저장응답 { 메시지 = "사업자등록번호는 숫자 10자리여야 합니다." });
        }

        var 상차좌표 = await _kakaoGeoService.도로명주소좌표변환Async(request.상차지기본주소, cancellationToken);
        var 하차좌표 = await _kakaoGeoService.도로명주소좌표변환Async(request.하차지기본주소, cancellationToken);

        return Ok(_store.저장(request, 상차좌표, 하차좌표));
    }
}
