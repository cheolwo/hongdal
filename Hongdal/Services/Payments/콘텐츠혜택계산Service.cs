using Hongdal.Contracts.CommonContents;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.공통콘텐츠;

namespace 홍달.Services.Payments;

public interface I콘텐츠혜택계산Service
{
    Task<결제혜택견적응답> 계산Async(string 사용자Id, int 원운임, CancellationToken cancellationToken = default);
    Task<int> 보상사용처리Async(string 사용자Id, CancellationToken cancellationToken = default);
}

public sealed class 콘텐츠혜택계산Service : I콘텐츠혜택계산Service
{
    private readonly HongdalContext _db;

    public 콘텐츠혜택계산Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task<결제혜택견적응답> 계산Async(string 사용자Id, int 원운임, CancellationToken cancellationToken = default)
    {
        var 사용가능보상 = await _db.홍달콘텐츠보상지급
            .Where(x => x.사용자Id == 사용자Id)
            .Where(x => !x.결제사용여부)
            .OrderBy(x => x.지급시각)
            .ToListAsync(cancellationToken);

        var 콘텐츠할인금액 = 0;
        var 포인트사용가능액 = 0;

        foreach (var 보상 in 사용가능보상)
        {
            if (보상.보상유형 == 홍달보상유형.포인트)
            {
                포인트사용가능액 += 보상.지급포인트;
            }

            if (보상.보상유형 == 홍달보상유형.할인금액)
            {
                콘텐츠할인금액 += 보상.할인금액;
            }

            if (보상.보상유형 == 홍달보상유형.할인율)
            {
                콘텐츠할인금액 += (int)Math.Floor(원운임 * 보상.할인율);
            }
        }

        var 총할인 = 콘텐츠할인금액 + 포인트사용가능액;
        var 최종결제금액 = Math.Max(0, 원운임 - 총할인);

        return new 결제혜택견적응답
        {
            원운임 = 원운임,
            포인트사용가능액 = 포인트사용가능액,
            콘텐츠할인금액 = 콘텐츠할인금액,
            최종결제금액 = 최종결제금액
        };
    }

    public async Task<int> 보상사용처리Async(string 사용자Id, CancellationToken cancellationToken = default)
    {
        var rewards = await _db.홍달콘텐츠보상지급
            .Where(x => x.사용자Id == 사용자Id)
            .Where(x => !x.결제사용여부)
            .ToListAsync(cancellationToken);

        if (rewards.Count == 0)
        {
            return 0;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var reward in rewards)
        {
            reward.결제사용여부 = true;
            reward.사용시각 = now;
        }

        return rewards.Count;
    }
}