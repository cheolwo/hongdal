using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.도메인.공통;
using Hongdal.Contracts.Driver.Recommendation;

namespace Hongdal.Controllers.Driver.Recommendation02
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/requests")]
    public sealed class 기사운송의뢰Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly I차량화물적합성Service _compatibilityService;

        public 기사운송의뢰Controller(HongdalContext db, I차량화물적합성Service compatibilityService)
        {
            _db = db;
            _compatibilityService = compatibilityService;
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> 상세조회(string requestId)
        {
            var driverId = 현재기사Id();
            var request = await _db.화주운송의뢰.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            if (request == null)
            {
                return NotFound("의뢰를 찾을 수 없습니다.");
            }

            var queue = await _db.배차대기.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            var cargoRequirement = await _db.화물요구조건.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
            var vehicle = driver is null
                ? null
                : await _db.차량제원.AsNoTracking().FirstOrDefaultAsync(x => x.차량코드 == driver.차량 || x.차량명 == driver.차량);
            var fit = _compatibilityService.판정(vehicle, request, cargoRequirement);

            return Ok(new 기사운송의뢰상세응답
            {
                의뢰Id = request.의뢰Id,
                화주Id = request.화주Id,
                화물종류 = request.화물종류,
                화물설명 = request.화물설명,
                픽업지 = request.픽업_도로명주소,
                픽업상세지 = request.픽업_상세주소,
                픽업위도 = request.픽업_위도,
                픽업경도 = request.픽업_경도,
                하차지 = request.하차_도로명주소,
                하차상세지 = request.하차_상세주소,
                하차위도 = request.하차_위도,
                하차경도 = request.하차_경도,
                결제상태 = request.결제상태,
                의뢰상태 = request.상태,
                배차상태 = request.배차상태,
                결제수단 = request.결제수단,
                결제예정금액 = request.결제예정금액,
                화물길이Mm = request.화물길이Mm,
                화물폭Mm = request.화물폭Mm,
                화물높이Mm = request.화물높이Mm,
                화물팔레트개수 = request.화물팔레트개수,
                차량적합여부 = fit.적합여부,
                부적합사유 = fit.부적합사유,
                경고 = fit.경고,
                배차대기상태 = queue?.상태,
                생성일시 = request.CreatedAt,
                수정일시 = request.UpdatedAt
            });
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
