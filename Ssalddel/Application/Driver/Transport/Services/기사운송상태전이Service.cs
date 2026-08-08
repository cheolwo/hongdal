using FluentResults;
using 살뜰.도메인.공통;
using 살뜰.도메인.운송;

namespace Ssalddel.Application.Driver.Transport;

public interface I기사운송상태전이Service
{
    Result 상태변경(운송원장 운송, string 목표상태, DateTime 변경시각);
}

public static class 기사운송상태전이Policy
{
    private static readonly IReadOnlyDictionary<string, string[]> 허용전이 = new Dictionary<string, string[]>
    {
        [기사운송상태코드.상차지도착] =
        [
            기사운송상태코드.배차대기,
            기사운송상태코드.매칭중,
            기사운송상태코드.배차확정,
            상태값.배차대기상태.확정,
            기사운송상태코드.이동중
        ],
        [기사운송상태코드.상차완료] = [기사운송상태코드.상차지도착],
        [기사운송상태코드.하차지도착] = [기사운송상태코드.상차완료, 기사운송상태코드.운송중],
        [기사운송상태코드.인수완료] = [기사운송상태코드.하차지도착]
    };

    public static bool 가능한가(string 현재상태, string 목표상태)
    {
        return string.Equals(현재상태, 목표상태, StringComparison.Ordinal)
            || (허용전이.TryGetValue(목표상태, out var 이전상태목록)
                && 이전상태목록.Contains(현재상태, StringComparer.Ordinal));
    }
}

public sealed class 기사운송상태전이Service : I기사운송상태전이Service
{

    public Result 상태변경(운송원장 운송, string 목표상태, DateTime 변경시각)
    {
        if (string.Equals(운송.상태, 목표상태, StringComparison.Ordinal))
        {
            운송.UpdatedAt = 변경시각;
            return Result.Ok();
        }

        if (string.Equals(운송.상태, 기사운송상태코드.인수완료, StringComparison.Ordinal))
        {
            return Result.Fail("이미 완료된 운송입니다.");
        }

        if (!기사운송상태전이Policy.가능한가(운송.상태, 목표상태))
        {
            return Result.Fail($"현재 상태({운송.상태})에서는 {목표상태} 처리할 수 없습니다.");
        }

        운송.상태 = 목표상태;
        운송.UpdatedAt = 변경시각;

        if (string.Equals(목표상태, 기사운송상태코드.상차완료, StringComparison.Ordinal))
        {
            운송.출발_픽업 ??= 변경시각;
        }
        else if (string.Equals(목표상태, 기사운송상태코드.인수완료, StringComparison.Ordinal))
        {
            운송.도착 ??= 변경시각;
        }

        return Result.Ok();
    }
}
