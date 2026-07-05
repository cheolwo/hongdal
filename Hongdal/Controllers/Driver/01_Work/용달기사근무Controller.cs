using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Driver.Work;
using Hongdal.Contracts.Driver.Work;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Work01
{
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
            if (!현재기사확인(driverId)) return Forbid();

            var s = await _sender.Send(new Hongdal.Application.Driver.Work.기사근무상세조회Query(driverId, id));
            return s == null ? NotFound() : Ok(s);
        }

        private bool 현재기사확인(string driverId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(currentUserId)
                   && string.Equals(currentUserId, driverId, StringComparison.Ordinal);
        }
    }
}
