using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Ssalddel.Application.Driver.Work;
using Ssalddel.Contracts.Driver.Work;
using 살뜰.도메인.공통;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Driver.Work01
{
    [SsalddelApiVersion(SsalddelProductVersion.V1_0)]
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/shifts")]
    public sealed class 기사근무Controller : DriverControllerBase
    {
        private readonly ISender _sender;

        public 기사근무Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            var driverId = 현재기사Id();
            var items = await _sender.Send(new Ssalddel.Application.Driver.Work.기사근무목록조회Query(driverId));

            return Ok(items);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 상세조회(long id)
        {
            var driverId = 현재기사Id();
            var shift = await _sender.Send(new Ssalddel.Application.Driver.Work.기사근무상세조회Query(driverId, id));
            if (shift == null)
            {
                return this.ToNotFoundProblem("기사 근무 정보를 찾을 수 없습니다.");
            }

            return Ok(shift);
        }

    }
}
