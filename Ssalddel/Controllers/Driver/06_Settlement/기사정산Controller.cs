using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Ssalddel.Application.Driver.Settlement;
using 살뜰.도메인.공통;
using Ssalddel.Contracts.Driver.Settlement;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Driver.Settlement06
{
    [SsalddelApiVersion(SsalddelProductVersion.V2_0)]
    [SsalddelApiCapability(SsalddelCapability.Settlement)]
    [SsalddelApiOperation(SsalddelOperation.Browse)]
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/settlements")]
    public sealed class 기사정산Controller : DriverControllerBase
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
            var items = await _sender.Send(new Ssalddel.Application.Driver.Settlement.기사정산목록조회Query(driverId));

            return Ok(items);
        }

        [HttpGet("{year:int}/{month:int}")]
        public async Task<IActionResult> 월별조회(int year, int month)
        {
            var driverId = 현재기사Id();
            var settlement = await _sender.Send(new Ssalddel.Application.Driver.Settlement.기사정산월별조회Query(driverId, year, month));
            if (settlement == null)
            {
                return this.ToNotFoundProblem("기사 정산 정보를 찾을 수 없습니다.");
            }

            return Ok(settlement);
        }

        [HttpGet("current-month")]
        public async Task<IActionResult> 현재월조회()
        {
            var driverId = 현재기사Id();
            var settlement = await _sender.Send(new Ssalddel.Application.Driver.Settlement.기사정산현재월조회Query(driverId));
            return Ok(settlement);
        }

    }

}
