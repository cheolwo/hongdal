using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.CommonContents.Commands;
using Ssalddel.Contracts.CommonContents;
using Microsoft.EntityFrameworkCore;
using 살뜰.Services.Payments;
using 살뜰.도메인.공통콘텐츠;

namespace Ssalddel.Application.CommonContents.Handlers;

public sealed class 콘텐츠시청시작CommandHandler : IRequestHandler<콘텐츠시청시작Command, 콘텐츠시청시작Result?>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 콘텐츠시청시작CommandHandler(SsalddelContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<콘텐츠시청시작Result?> Handle(콘텐츠시청시작Command request, CancellationToken cancellationToken)
    {
        var 사용자Id = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(사용자Id))
        {
            return null;
        }

        var 콘텐츠존재 = await _db.살뜰공통콘텐츠
            .AnyAsync(x => x.Id == request.콘텐츠Id && x.활성화여부, cancellationToken);
        if (!콘텐츠존재)
        {
            return null;
        }

        var 세션 = new 살뜰콘텐츠시청세션
        {
            사용자Id = 사용자Id,
            콘텐츠Id = request.콘텐츠Id,
            영상전체초 = Math.Max(0, request.영상전체초),
            누적시청초 = 0,
            시작시각 = DateTimeOffset.UtcNow
        };

        _db.살뜰콘텐츠시청세션.Add(세션);
        await _db.SaveChangesAsync(cancellationToken);

        return new 콘텐츠시청시작Result { 세션Id = 세션.Id };
    }
}

public sealed class 콘텐츠시청진행CommandHandler : IRequestHandler<콘텐츠시청진행Command, bool>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 콘텐츠시청진행CommandHandler(SsalddelContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<bool> Handle(콘텐츠시청진행Command request, CancellationToken cancellationToken)
    {
        var 사용자Id = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(사용자Id))
        {
            return false;
        }

        var 세션 = await _db.살뜰콘텐츠시청세션
            .FirstOrDefaultAsync(x => x.Id == request.세션Id && x.사용자Id == 사용자Id, cancellationToken);
        if (세션 is null)
        {
            return false;
        }

        세션.누적시청초 = Math.Max(세션.누적시청초, Math.Max(0, request.현재시청초));
        세션.마지막진행시각 = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class 콘텐츠시청완료CommandHandler : IRequestHandler<콘텐츠시청완료Command, 콘텐츠시청완료Result?>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 콘텐츠시청완료CommandHandler(SsalddelContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<콘텐츠시청완료Result?> Handle(콘텐츠시청완료Command request, CancellationToken cancellationToken)
    {
        var 사용자Id = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(사용자Id))
        {
            return null;
        }

        var 세션 = await _db.살뜰콘텐츠시청세션
            .Include(x => x.콘텐츠)
            .ThenInclude(x => x.보상정책)
            .FirstOrDefaultAsync(x => x.Id == request.세션Id && x.사용자Id == 사용자Id, cancellationToken);

        if (세션 is null)
        {
            return null;
        }

        var 정책 = 세션.콘텐츠.보상정책;
        if (정책 is null || 정책.보상유형 == 살뜰보상유형.없음)
        {
            세션.완료여부 = true;
            세션.완료시각 = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            return new 콘텐츠시청완료Result
            {
                보상지급여부 = false,
                메시지 = "보상 없는 콘텐츠입니다."
            };
        }

        var 시청비율 = 세션.영상전체초 <= 0
            ? 0m
            : (decimal)세션.누적시청초 / 세션.영상전체초;

        var 보상조건충족 = 세션.누적시청초 >= 정책.최소시청초 && 시청비율 >= 정책.필요시청비율;
        if (!보상조건충족)
        {
            return new 콘텐츠시청완료Result
            {
                보상지급여부 = false,
                메시지 = "아직 보상 조건을 충족하지 않았습니다."
            };
        }

        var 이미받음 = await _db.살뜰콘텐츠보상지급
            .AnyAsync(x => x.사용자Id == 사용자Id && x.콘텐츠Id == 세션.콘텐츠Id, cancellationToken);

        if (이미받음 && 정책.사용자당1회만지급)
        {
            return new 콘텐츠시청완료Result
            {
                보상지급여부 = false,
                메시지 = "이미 지급된 보상입니다."
            };
        }

        var 할인금액 = 정책.할인금액;
        if (정책.보상유형 == 살뜰보상유형.할인금액 && 정책.최대할인금액.HasValue)
        {
            할인금액 = Math.Min(정책.할인금액, 정책.최대할인금액.Value);
        }

        var 보상 = new 살뜰콘텐츠보상지급
        {
            사용자Id = 사용자Id,
            콘텐츠Id = 세션.콘텐츠Id,
            보상유형 = 정책.보상유형,
            지급포인트 = 정책.지급포인트,
            할인율 = 정책.할인율,
            할인금액 = 할인금액,
            지급시각 = DateTimeOffset.UtcNow
        };

        세션.완료여부 = true;
        세션.보상지급여부 = true;
        세션.완료시각 = DateTimeOffset.UtcNow;

        _db.살뜰콘텐츠보상지급.Add(보상);
        await _db.SaveChangesAsync(cancellationToken);

        return new 콘텐츠시청완료Result
        {
            보상지급여부 = true,
            메시지 = "보상이 지급되었습니다.",
            지급포인트 = 보상.지급포인트,
            할인율 = 보상.할인율,
            할인금액 = 보상.할인금액
        };
    }
}

public sealed class 결제혜택견적조회QueryHandler : IRequestHandler<결제혜택견적조회Query, 결제혜택견적응답>
{
    private readonly I콘텐츠혜택계산Service _benefitService;

    public 결제혜택견적조회QueryHandler(I콘텐츠혜택계산Service benefitService)
    {
        _benefitService = benefitService;
    }

    public async Task<결제혜택견적응답> Handle(결제혜택견적조회Query request, CancellationToken cancellationToken)
    {
        return await _benefitService.계산Async(request.사용자Id, request.원운임, cancellationToken);
    }
}