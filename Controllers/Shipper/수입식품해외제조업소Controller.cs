using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Contracts.Shipper.ImportFood;

namespace Hongdal.Controllers.Shipper
{
    [ApiController]
    [Route("api/v1/shipper/import-food/oversea-manufacturers")]
    [Authorize(Roles = 역할명.화주)]
    public sealed class 수입식품해외제조업소Controller : ControllerBase
    {
        private readonly I해외제조업소조회Service _해외제조업소조회Service;

        public 수입식품해외제조업소Controller(I해외제조업소조회Service 해외제조업소조회Service)
        {
            _해외제조업소조회Service = 해외제조업소조회Service;
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
            var 요청 = new 홍달.Services.External.Mfds.해외제조업소조회요청
            {
                페이지번호 = 페이지번호,
                한페이지결과수 = 한페이지결과수,
                데이터형식 = 데이터형식,
                해외제조업소명 = 해외제조업소명,
                식품구분명 = 식품구분명,
                국가명 = 국가명
            };

            var 응답 = await _해외제조업소조회Service.조회Async(요청);
            var 결과목록 = 응답.본문?.아이템?.항목 ?? [];

            return Ok(new 해외제조업소조회화면응답
            {
                결과코드 = 응답.헤더?.결과코드 ?? string.Empty,
                결과메시지 = 응답.헤더?.결과메시지 ?? string.Empty,
                페이지번호 = 응답.본문?.페이지번호 ?? 페이지번호,
                한페이지결과수 = 응답.본문?.한페이지결과수 ?? 한페이지결과수,
                전체결과수 = 응답.본문?.전체결과수 ?? 0,
                항목목록 = 결과목록.Select(항목 => new 해외제조업소조회화면항목
                {
                    제조업소코드 = 항목.해외제조업소코드 ?? string.Empty,
                    제조업소명 = 항목.해외제조업소명 ?? string.Empty,
                    제조업소주소 = 항목.해외제조업소주소 ?? string.Empty,
                    국가명 = 항목.국가명,
                    지역명 = 항목.지역명,
                    식품구분명 = 항목.식품구분명,
                    영업구분명 = 항목.영업구분명,
                    식품안전관리인증여부 = string.Equals(항목.식품안전관리시스템인증여부, "Y", StringComparison.OrdinalIgnoreCase),
                    인증명 = 항목.인증명,
                    인증기관명 = 항목.인증기관명,
                    인증일 = 항목.인증기관인증일,
                    인증만료일 = 항목.인증기관만료일,
                    주의필요여부 = 항목.주의필요여부,
                    주의사유 = 항목.주의사유,
                    취소중단명 = 항목.취소중단명,
                    수입중단번호 = 항목.수입중단번호
                }).ToList()
            });
        }
    }

}
