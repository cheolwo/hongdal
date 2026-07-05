using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Shipper.ImportFood;
using Hongdal.Contracts.Shipper.ImportFood;

namespace Hongdal.Controllers.Shipper
{
    [ApiController]
    [Route("api/v1/shipper/import-food/oversea-manufacturers")]
    [Authorize(Roles = 역할명.화주)]
    public sealed class 수입식품해외제조업소Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 수입식품해외제조업소Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 조회(
            [FromQuery(Name = "pageNo")] int 페이지번호 = 1,
            [FromQuery(Name = "numOfRows")] int 한페이지결과수 = 10,
            [FromQuery(Name = "type")] string 데이터형식 = "xml",
            [FromQuery(Name = "OCTR_MNFT_BSSH_NM")] string? 해외제조업소명 = null,
            [FromQuery(Name = "FOOD_SE_NM")] string? 식품구분명 = null,
            [FromQuery(Name = "NATN_NM")] string? 국가명 = null)
        {
            var 응답 = await _sender.Send(new 해외제조업소조회Query(페이지번호, 한페이지결과수, 데이터형식, 해외제조업소명, 식품구분명, 국가명));
            return Ok(응답);
        }
    }

}
