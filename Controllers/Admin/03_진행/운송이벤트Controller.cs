using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using Hongdal.Contracts.Admin.Progress;
using 홍달.도메인.운송;

namespace Hongdal.Controllers.Admin.Progress03
{
    [ApiController]
    [Route("api/v1/transport-events")]
    public class 운송이벤트Controller : ControllerBase
    {
        private readonly HongdalContext _db;

        public 운송이벤트Controller(HongdalContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _db.운송이벤트.OrderBy(e => e.이벤트시각).ToListAsync());
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _db.운송이벤트.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> 생성([FromBody] 운송이벤트요청 request)
        {
            if (request == null) return BadRequest();

            var entity = new 운송이벤트
            {
                의뢰Id = request.의뢰Id,
                이벤트타입 = request.이벤트타입,
                이벤트시각 = request.이벤트시각 == default ? DateTime.UtcNow : request.이벤트시각,
                메타데이터 = request.메타데이터
            };

            await _db.운송이벤트.AddAsync(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> 수정(long id, [FromBody] 운송이벤트요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _db.운송이벤트.FindAsync(id);
            if (entity == null) return NotFound();

            entity.의뢰Id = request.의뢰Id;
            entity.이벤트타입 = request.이벤트타입;
            entity.이벤트시각 = request.이벤트시각 == default ? entity.이벤트시각 : request.이벤트시각;
            entity.메타데이터 = request.메타데이터;

            await _db.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> 삭제(long id)
        {
            var entity = await _db.운송이벤트.FindAsync(id);
            if (entity == null) return NotFound();
            _db.운송이벤트.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

}
