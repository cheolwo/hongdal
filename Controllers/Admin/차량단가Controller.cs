using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.운송;

namespace Hongdal.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/vehicle-rates")]
    [Authorize]
    public class 차량단가Controller : ControllerBase
    {
        private readonly HongdalContext _db;

        public 차량단가Controller(HongdalContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _db.차량단가.OrderBy(c => c.Id).ToListAsync());
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _db.차량단가.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 생성([FromBody] 차량단가요청 request)
        {
            if (request == null) return BadRequest();

            var entity = new 차량단가
            {
                차량종류 = request.차량종류,
                기본운임 = request.기본운임,
                Km당단가 = request.Km당단가,
                야간할증 = request.야간할증,
                우천할증 = request.우천할증,
                최소운임 = request.최소운임,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.차량단가.AddAsync(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 수정(long id, [FromBody] 차량단가요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _db.차량단가.FindAsync(id);
            if (entity == null) return NotFound();

            entity.차량종류 = request.차량종류;
            entity.기본운임 = request.기본운임;
            entity.Km당단가 = request.Km당단가;
            entity.야간할증 = request.야간할증;
            entity.우천할증 = request.우천할증;
            entity.최소운임 = request.최소운임;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 삭제(long id)
        {
            var entity = await _db.차량단가.FindAsync(id);
            if (entity == null) return NotFound();
            _db.차량단가.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public class 차량단가요청
    {
        public string 차량종류 { get; set; } = string.Empty;
        public decimal 기본운임 { get; set; }
        public decimal Km당단가 { get; set; }
        public decimal 야간할증 { get; set; }
        public decimal 우천할증 { get; set; }
        public decimal 최소운임 { get; set; }
    }
}
