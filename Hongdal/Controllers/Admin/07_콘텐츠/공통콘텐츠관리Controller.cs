using Hongdal.Contracts.CommonContents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통콘텐츠;

namespace Hongdal.Controllers.Admin.Content07;

[ApiController]
[Route("api/v1/admin/common-contents")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 공통콘텐츠관리Controller : ControllerBase
{
    private readonly HongdalContext _db;

    public 공통콘텐츠관리Controller(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<관리자공통콘텐츠요약응답>>> 목록조회(CancellationToken cancellationToken)
    {
        var items = await _db.홍달공통콘텐츠
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new 관리자공통콘텐츠요약응답
            {
                Id = x.Id,
                제목 = x.제목,
                콘텐츠유형 = (계약홍달콘텐츠유형)x.콘텐츠유형,
                노출위치 = (계약홍달노출위치)x.노출위치,
                활성화여부 = x.활성화여부,
                생성시각 = x.생성시각
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<관리자공통콘텐츠상세응답>> 상세조회(long id, CancellationToken cancellationToken)
    {
        var entity = await _db.홍달공통콘텐츠
            .AsNoTracking()
            .Include(x => x.보상정책)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(ToDetail(entity));
    }

    [HttpPost]
    public async Task<ActionResult<관리자공통콘텐츠상세응답>> 등록([FromBody] 관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken)
    {
        var entity = new 홍달공통콘텐츠();
        Apply(entity, request);

        _db.홍달공통콘텐츠.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var saved = await _db.홍달공통콘텐츠
            .AsNoTracking()
            .Include(x => x.보상정책)
            .FirstAsync(x => x.Id == entity.Id, cancellationToken);

        return CreatedAtAction(nameof(상세조회), new { id = entity.Id }, ToDetail(saved));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<관리자공통콘텐츠상세응답>> 수정(long id, [FromBody] 관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken)
    {
        var entity = await _db.홍달공통콘텐츠.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        Apply(entity, request);
        await _db.SaveChangesAsync(cancellationToken);

        var saved = await _db.홍달공통콘텐츠
            .AsNoTracking()
            .Include(x => x.보상정책)
            .FirstAsync(x => x.Id == id, cancellationToken);

        return Ok(ToDetail(saved));
    }

    [HttpPatch("{id:long}/active")]
    public async Task<IActionResult> 활성화변경(long id, [FromQuery] bool enabled, CancellationToken cancellationToken)
    {
        var entity = await _db.홍달공통콘텐츠.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.활성화여부 = enabled;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("reward-policies")]
    public async Task<ActionResult<IReadOnlyList<공통콘텐츠보상정책Dto>>> 보상정책목록(CancellationToken cancellationToken)
    {
        var items = await _db.홍달콘텐츠보상정책
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new 공통콘텐츠보상정책Dto
            {
                Id = x.Id,
                보상유형 = (계약홍달보상유형)x.보상유형,
                지급포인트 = x.지급포인트,
                할인율 = x.할인율,
                할인금액 = x.할인금액,
                최소시청초 = x.최소시청초,
                필요시청비율 = x.필요시청비율,
                사용자당1회만지급 = x.사용자당1회만지급,
                최대할인금액 = x.최대할인금액
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("reward-policies")]
    public async Task<ActionResult<공통콘텐츠보상정책Dto>> 보상정책등록([FromBody] 공통콘텐츠보상정책Dto request, CancellationToken cancellationToken)
    {
        var policy = new 홍달콘텐츠보상정책
        {
            보상유형 = (홍달보상유형)request.보상유형,
            지급포인트 = request.지급포인트,
            할인율 = request.할인율,
            할인금액 = request.할인금액,
            최소시청초 = request.최소시청초,
            필요시청비율 = request.필요시청비율,
            사용자당1회만지급 = request.사용자당1회만지급,
            최대할인금액 = request.최대할인금액
        };

        _db.홍달콘텐츠보상정책.Add(policy);
        await _db.SaveChangesAsync(cancellationToken);

        request.Id = policy.Id;
        return CreatedAtAction(nameof(보상정책목록), new { id = policy.Id }, request);
    }

    private static 관리자공통콘텐츠상세응답 ToDetail(홍달공통콘텐츠 entity)
    {
        return new 관리자공통콘텐츠상세응답
        {
            Id = entity.Id,
            제목 = entity.제목,
            설명 = entity.설명,
            콘텐츠유형 = (계약홍달콘텐츠유형)entity.콘텐츠유형,
            이미지Url = entity.이미지Url,
            영상Url = entity.영상Url,
            외부링크Url = entity.외부링크Url,
            노출위치 = (계약홍달노출위치)entity.노출위치,
            기사노출 = entity.기사노출,
            화주노출 = entity.화주노출,
            운영자노출 = entity.운영자노출,
            활성화여부 = entity.활성화여부,
            노출시작시각 = entity.노출시작시각,
            노출종료시각 = entity.노출종료시각,
            생성시각 = entity.생성시각,
            보상정책 = entity.보상정책 is null
                ? null
                : new 공통콘텐츠보상정책Dto
                {
                    Id = entity.보상정책.Id,
                    보상유형 = (계약홍달보상유형)entity.보상정책.보상유형,
                    지급포인트 = entity.보상정책.지급포인트,
                    할인율 = entity.보상정책.할인율,
                    할인금액 = entity.보상정책.할인금액,
                    최소시청초 = entity.보상정책.최소시청초,
                    필요시청비율 = entity.보상정책.필요시청비율,
                    사용자당1회만지급 = entity.보상정책.사용자당1회만지급,
                    최대할인금액 = entity.보상정책.최대할인금액
                }
        };
    }

    private static void Apply(홍달공통콘텐츠 entity, 관리자공통콘텐츠저장요청 request)
    {
        entity.제목 = request.제목.Trim();
        entity.설명 = request.설명.Trim();
        entity.콘텐츠유형 = (홍달콘텐츠유형)request.콘텐츠유형;
        entity.이미지Url = request.이미지Url;
        entity.영상Url = request.영상Url;
        entity.외부링크Url = request.외부링크Url;
        entity.노출위치 = (홍달노출위치)request.노출위치;
        entity.기사노출 = request.기사노출;
        entity.화주노출 = request.화주노출;
        entity.운영자노출 = request.운영자노출;
        entity.활성화여부 = request.활성화여부;
        entity.노출시작시각 = request.노출시작시각;
        entity.노출종료시각 = request.노출종료시각;
        entity.보상정책Id = request.보상정책Id;
    }
}