using System.Security.Claims;
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
    [SsalddelApiVersion(SsalddelProductVersion.V2_0)]
    [SsalddelApiAudience(SsalddelActor.Driver)]
    [SsalddelApiCapability(SsalddelCapability.DriverWork)]
    [SsalddelApiOperation(SsalddelOperation.Execute)]
    [ApiController]
    [Route("api/v1/drivers/{driverId}/shifts")]
    [Authorize(Roles = 역할명.기사)]
    public class 용달기사근무Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 용달기사근무Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 근무조회(string driverId, long id)
        {
            if (!현재기사확인(driverId)) return this.ToForbiddenProblem("다른 기사의 근무 정보는 조회할 수 없습니다.");

            var s = await _sender.Send(new Ssalddel.Application.Driver.Work.기사근무상세조회Query(driverId, id));
            return s == null ? this.ToNotFoundProblem("기사 근무 정보를 찾을 수 없습니다.") : Ok(s);
        }

        private bool 현재기사확인(string driverId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(currentUserId)
                   && string.Equals(currentUserId, driverId, StringComparison.Ordinal);
        }
    }
}
