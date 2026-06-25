using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.도메인.배차;

namespace Hongdal.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/dispatch/wait")]
    public class 배차대기Controller : ControllerBase
    {
        private readonly HongdalContext _db;

        public 배차대기Controller(HongdalContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _db.배차대기.OrderBy(q => q.CreatedAt).ToListAsync());
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _db.배차대기.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> 생성([FromBody] 배차대기요청 request)
        {
            if (request == null) return BadRequest();

            var entity = new 배차대기
            {
                의뢰Id = request.의뢰Id,
                화주Id = request.화주Id,
                픽업_도로명주소 = request.픽업_도로명주소,
                픽업_상세주소 = request.픽업_상세주소,
                픽업_위도 = request.픽업_위도,
                픽업_경도 = request.픽업_경도,
                하차_도로명주소 = request.하차_도로명주소,
                하차_상세주소 = request.하차_상세주소,
                하차_위도 = request.하차_위도,
                하차_경도 = request.하차_경도,
                상태 = string.IsNullOrWhiteSpace(request.상태) ? 상태값.배차대기상태.대기 : request.상태,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.배차대기.AddAsync(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> 수정(long id, [FromBody] 배차대기요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _db.배차대기.FindAsync(id);
            if (entity == null) return NotFound();

            entity.의뢰Id = request.의뢰Id;
            entity.화주Id = request.화주Id;
            entity.픽업_도로명주소 = request.픽업_도로명주소;
            entity.픽업_상세주소 = request.픽업_상세주소;
            entity.픽업_위도 = request.픽업_위도;
            entity.픽업_경도 = request.픽업_경도;
            entity.하차_도로명주소 = request.하차_도로명주소;
            entity.하차_상세주소 = request.하차_상세주소;
            entity.하차_위도 = request.하차_위도;
            entity.하차_경도 = request.하차_경도;
            entity.상태 = string.IsNullOrWhiteSpace(request.상태) ? entity.상태 : request.상태;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> 삭제(long id)
        {
            var entity = await _db.배차대기.FindAsync(id);
            if (entity == null) return NotFound();
            _db.배차대기.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public class 배차대기요청
    {
        public string 의뢰Id { get; set; } = string.Empty;
        public string 화주Id { get; set; } = string.Empty;
        public string 픽업_도로명주소 { get; set; } = string.Empty;
        public string 픽업_상세주소 { get; set; } = string.Empty;
        public decimal? 픽업_위도 { get; set; }
        public decimal? 픽업_경도 { get; set; }
        public string 하차_도로명주소 { get; set; } = string.Empty;
        public string 하차_상세주소 { get; set; } = string.Empty;
        public decimal? 하차_위도 { get; set; }
        public decimal? 하차_경도 { get; set; }
        public string 상태 { get; set; } = string.Empty;
    }
}
