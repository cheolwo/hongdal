using System.Security.Claims;
using Hongdal.Application.Driver.Notification;
using Hongdal.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Contracts.Driver.Notification;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Driver.Notification07
{
    [HongdalApiVersion(HongdalProductVersion.V1_0)]
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/notifications")]
    public sealed class 기사알림Controller : ControllerBase
    {
        private readonly I기사알림UseCase _useCase;

        public 기사알림Controller(I기사알림UseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet("push-token")]
        public async Task<IActionResult> 조회()
        {
            var result = await _useCase.푸시토큰조회Async(현재기사Id());
            return this.ToActionResult(result);
        }

        [HttpPut("push-token")]
        public async Task<IActionResult> 등록([FromBody] 기사푸시토큰등록요청 request)
        {
            var result = await _useCase.푸시토큰등록Async(현재기사Id(), request);
            return this.ToActionResult(result);
        }

        [HttpDelete("push-token")]
        public async Task<IActionResult> 삭제()
        {
            var result = await _useCase.푸시토큰삭제Async(현재기사Id());
            return this.ToNoContentActionResult(result);
        }

        [HttpGet("settings")]
        public async Task<IActionResult> 설정조회()
        {
            var result = await _useCase.설정조회Async(현재기사Id());
            return this.ToActionResult(result);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> 설정수정([FromBody] 기사알림설정수정요청 request)
        {
            var result = await _useCase.설정수정Async(현재기사Id(), request);
            return this.ToActionResult(result);
        }

        private string? 현재기사Id()
            => User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

}
