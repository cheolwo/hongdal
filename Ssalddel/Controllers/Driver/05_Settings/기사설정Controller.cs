using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services;
using Ssalddel.Contracts.Driver.Settings;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Driver.Settings05
{
    [SsalddelApiVersion(SsalddelProductVersion.V1_0)]
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/preferences")]
    public sealed class 기사설정Controller : DriverControllerBase
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
                return this.ToProblemActionResult("request body is required");
            }

            await _callScopeStore.SetNationwideEnabledAsync(driverId, request.NationwideEnabled);
            return Ok(new 기사콜범위응답
            {
                DriverId = driverId,
                NationwideEnabled = request.NationwideEnabled
            });
        }

    }

}
