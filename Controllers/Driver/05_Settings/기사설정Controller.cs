using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services;
using Hongdal.Contracts.Driver.Settings;

namespace Hongdal.Controllers.Driver.Settings05
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/preferences")]
    public sealed class 기사설정Controller : ControllerBase
    {
        private readonly IDriverCallScopeStore _callScopeStore;

        public 기사설정Controller(IDriverCallScopeStore callScopeStore)
        {
            _callScopeStore = callScopeStore;
        }

        [HttpGet("call-scope")]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var enabled = await _callScopeStore.IsNationwideEnabledAsync(driverId);
            return Ok(new 기사콜범위응답
            {
                DriverId = driverId,
                NationwideEnabled = enabled
            });
        }

        [HttpPut("call-scope")]
        public async Task<IActionResult> 수정([FromBody] 기사콜범위수정요청 request)
        {
            var driverId = 현재기사Id();
            if (request == null)
            {
                return BadRequest("request body is required");
            }

            await _callScopeStore.SetNationwideEnabledAsync(driverId, request.NationwideEnabled);
            return Ok(new 기사콜범위응답
            {
                DriverId = driverId,
                NationwideEnabled = request.NationwideEnabled
            });
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
