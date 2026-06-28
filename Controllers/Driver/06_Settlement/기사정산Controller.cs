using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Driver.Settlement;
using 홍달.도메인.공통;
using Hongdal.Contracts.Driver.Settlement;

namespace Hongdal.Controllers.Driver.Settlement06
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/settlements")]
    public sealed class 기사정산Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사정산Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            var driverId = 현재기사Id();
            var items = await _sender.Send(new Hongdal.Application.Driver.Settlement.기사정산목록조회Query(driverId));

            return Ok(items);
        }

        [HttpGet("{year:int}/{month:int}")]
        public async Task<IActionResult> 월별조회(int year, int month)
        {
            var driverId = 현재기사Id();
            var settlement = await _sender.Send(new Hongdal.Application.Driver.Settlement.기사정산월별조회Query(driverId, year, month));
            if (settlement == null)
            {
                return NotFound();
            }

            return Ok(settlement);
        }

        [HttpGet("current-month")]
        public async Task<IActionResult> 현재월조회()
        {
            var driverId = 현재기사Id();
            var settlement = await _sender.Send(new Hongdal.Application.Driver.Settlement.기사정산현재월조회Query(driverId));
            return Ok(settlement);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
