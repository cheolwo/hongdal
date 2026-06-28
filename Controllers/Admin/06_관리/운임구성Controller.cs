using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using Hongdal.Contracts.Admin.Management;
using 홍달.도메인.운송;

namespace Hongdal.Controllers.Admin.Master06
{
    [ApiController]
    [Route("api/v1/fare-configurations")]
    [Authorize]
    public class 운임구성Controller : ControllerBase
    {
        private readonly HongdalContext _db;

        public 운임구성Controller(HongdalContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _db.운임구성.OrderBy(c => c.CreatedAt).ToListAsync());
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _db.운임구성.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 생성([FromBody] 운임구성요청 request)
        {
            if (request == null) return BadRequest();

            var entity = new 운임구성
            {
                의뢰Id = request.의뢰Id,
                기본운임 = request.기본운임,
                거리운임 = request.거리운임,
                할증 = request.할증,
                대기료 = request.대기료,
                수작업비 = request.수작업비,
                최종운임 = request.최종운임,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.운임구성.AddAsync(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 수정(long id, [FromBody] 운임구성요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _db.운임구성.FindAsync(id);
            if (entity == null) return NotFound();

            entity.의뢰Id = request.의뢰Id;
            entity.기본운임 = request.기본운임;
            entity.거리운임 = request.거리운임;
            entity.할증 = request.할증;
            entity.대기료 = request.대기료;
            entity.수작업비 = request.수작업비;
            entity.최종운임 = request.최종운임;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 삭제(long id)
        {
            var entity = await _db.운임구성.FindAsync(id);
            if (entity == null) return NotFound();
            _db.운임구성.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

}
