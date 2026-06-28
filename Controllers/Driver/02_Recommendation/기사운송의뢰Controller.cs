using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using 홍달.도메인.공통;
using Hongdal.Application.Driver.Recommendation;

namespace Hongdal.Controllers.Driver.Recommendation02
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/requests")]
    public sealed class 기사운송의뢰Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사운송의뢰Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> 상세조회(string requestId)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운송의뢰상세조회Query(driverId, requestId));
            return Ok(result);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
