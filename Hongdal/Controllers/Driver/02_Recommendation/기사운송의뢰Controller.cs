using System.Security.Claims;
using Hongdal.Application.Driver.Recommendation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.도메인.공통;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Driver.Recommendation02
{
    [HongdalApiVersion(HongdalProductVersion.V1_0)]
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/requests")]
    public sealed class 기사운송의뢰Controller : ControllerBase
    {
        private readonly I기사배차추천UseCase _useCase;

        public 기사운송의뢰Controller(I기사배차추천UseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> 상세조회(string requestId)
        {
            var driverId = 현재기사Id();
            var result = await _useCase.운송의뢰상세조회Async(driverId, requestId);
            return Ok(result);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
