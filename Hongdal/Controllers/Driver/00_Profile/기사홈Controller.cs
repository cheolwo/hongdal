using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Driver.Home;

namespace Hongdal.Controllers.Driver.Profile00
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/home")]
    public sealed class 기사홈Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사홈Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 기사홈조회Query(driverId));

            if (result == null)
            {
                return NotFound("용달기사 정보를 찾을 수 없습니다.");
            }

            return Ok(result);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
