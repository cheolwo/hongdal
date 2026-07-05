using FluentResults;
using 홍달.도메인.운송;

namespace Hongdal.Application.Driver.Transport;

public interface I기사운송상태전이Service
{
    Result 상태변경(배송_운송 운송, string 목표상태, DateTime 변경시각);
}

public sealed class 기사운송상태전이Service : I기사운송상태전이Service
{
    private const string 배차대기 = "배차대기";
    private const string 매칭중 = "매칭중";
    private const string 이동중 = "이동중";
    private const string 운송중 = "운송중";
    private const string 상차지도착 = "상차지도착";
    private const string 상차완료 = "상차완료";
    private const string 하차지도착 = "하차지도착";
    private const string 인수완료 = "인수완료";

    private static readonly IReadOnlyDictionary<string, string[]> 허용전이 = new Dictionary<string, string[]>
    {
        [상차지도착] = [배차대기, 매칭중, 이동중],
        [상차완료] = [상차지도착],
        [하차지도착] = [상차완료, 운송중],
        [인수완료] = [하차지도착]
    };

    public Result 상태변경(배송_운송 운송, string 목표상태, DateTime 변경시각)
    {
        if (string.Equals(운송.상태, 목표상태, StringComparison.Ordinal))
        {
            운송.UpdatedAt = 변경시각;
            return Result.Ok();
        }

        if (string.Equals(운송.상태, 인수완료, StringComparison.Ordinal))
        {
            return Result.Fail("이미 완료된 운송입니다.");
        }

        if (!허용전이.TryGetValue(목표상태, out var 이전상태목록)
            || !이전상태목록.Contains(운송.상태, StringComparer.Ordinal))
        {
            return Result.Fail($"현재 상태({운송.상태})에서는 {목표상태} 처리할 수 없습니다.");
        }

        운송.상태 = 목표상태;
        운송.UpdatedAt = 변경시각;

        if (string.Equals(목표상태, 상차완료, StringComparison.Ordinal))
        {
            운송.출발_픽업 ??= 변경시각;
        }
        else if (string.Equals(목표상태, 인수완료, StringComparison.Ordinal))
        {
            운송.도착 ??= 변경시각;
        }

        return Result.Ok();
    }
}
