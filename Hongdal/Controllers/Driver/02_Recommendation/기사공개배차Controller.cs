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
    [Route("api/v1/driver/public-dispatches")]
    public sealed class 기사공개배차Controller : ControllerBase
    {
        private readonly I기사배차추천UseCase _useCase;

        public 기사공개배차Controller(I기사배차추천UseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var items = await _useCase.공개배차조회Async(driverId);
            return Ok(items);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
