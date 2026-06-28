using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.도메인.기사;

namespace Hongdal.Controllers.Driver.Work01
{
    [ApiController]
    [Route("api/v1/drivers/{driverId}/shifts")]
    [Authorize(Roles = 역할명.기사)]
    public class 용달기사근무Controller : ControllerBase
    {
        private readonly HongdalContext _db;

        public 용달기사근무Controller(HongdalContext db)
        {
            _db = db;
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 근무조회(string driverId, long id)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var s = await _db.기사근무.FindAsync(id);
            if (s == null || s.기사Id != driverId) return NotFound();
            return Ok(s);
        }

        private bool 현재기사확인(string driverId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(currentUserId)
                   && string.Equals(currentUserId, driverId, StringComparison.Ordinal);
        }
    }
}
