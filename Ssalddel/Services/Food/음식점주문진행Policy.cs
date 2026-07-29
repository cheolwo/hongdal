using Ssalddel.Contracts.Food;

namespace Ssalddel.Services.Food;

public sealed record 음식점주문진행판정(
    string 다음상태,
    int? 조리예상분,
    string 이력사유);

public static class 음식점주문진행Policy
{
    public static 음식점주문진행판정 판정(
        string? 현재상태,
        음식점주문진행변경요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var current = 음식주문상태코드.Normalize(현재상태);
        var action = request.작업?.Trim();
        if (!음식점주문진행작업코드.지원여부(action))
        {
            throw new ArgumentException("지원하지 않는 음식점 주문 진행 작업입니다.", nameof(request));
        }

        return action switch
        {
            음식점주문진행작업코드.거절 => Reject(current, request),
            음식점주문진행작업코드.조리시간변경 => ChangePreparationTime(current, request),
            음식점주문진행작업코드.픽업준비 => MarkPickupReady(current),
            _ => throw new ArgumentOutOfRangeException(nameof(request))
        };
    }

    private static 음식점주문진행판정 Reject(
        string current,
        음식점주문진행변경요청 request)
    {
        EnsureCurrent(current, 음식주문상태코드.주문대기, request.작업);
        var reason = request.사유?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("주문 거절 사유가 필요합니다.", nameof(request));
        }

        return new 음식점주문진행판정(
            음식주문상태코드.거절,
            null,
            $"음식점 주문 거절 · {reason}");
    }

    private static 음식점주문진행판정 ChangePreparationTime(
        string current,
        음식점주문진행변경요청 request)
    {
        EnsureCurrent(current, 음식주문상태코드.조리중, request.작업);
        if (request.조리예상분 is null)
        {
            throw new ArgumentException("변경할 조리 예상 시간이 필요합니다.", nameof(request));
        }

        var minutes = 음식점조리시간정책.Clamp(request.조리예상분.Value);
        return new 음식점주문진행판정(
            음식주문상태코드.조리중,
            minutes,
            $"조리 예상 시간 변경 · {minutes}분");
    }

    private static 음식점주문진행판정 MarkPickupReady(string current)
    {
        EnsureCurrent(current, 음식주문상태코드.조리중, 음식점주문진행작업코드.픽업준비);
        return new 음식점주문진행판정(
            음식주문상태코드.픽업대기,
            0,
            "음식점 픽업 준비 완료");
    }

    private static void EnsureCurrent(
        string current,
        string expected,
        string? action)
    {
        if (!string.Equals(current, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{action} 작업이 가능한 주문 상태가 아닙니다. 현재상태={current}, 필요상태={expected}");
        }
    }
}
