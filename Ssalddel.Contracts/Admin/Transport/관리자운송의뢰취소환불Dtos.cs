namespace Ssalddel.Contracts.Admin.Transport;

public sealed class 관리자운송의뢰취소환불요청
{
    public string 확인의뢰Id { get; set; } = string.Empty;
    public string 사유 { get; set; } = string.Empty;
}

public sealed record 관리자운송의뢰취소환불판단(
    bool 처리가능,
    bool 환불상태기록필요,
    string 처리명,
    string 안내문구);

public static class 관리자운송의뢰취소환불정책
{
    private static readonly HashSet<string> 처리가능의뢰상태 = new(StringComparer.OrdinalIgnoreCase)
    {
        "생성됨",
        "접수",
        "대기"
    };

    private static readonly HashSet<string> 처리가능배차상태 = new(StringComparer.OrdinalIgnoreCase)
    {
        "미시작",
        "대기",
        "배차대기",
        "매칭중",
        "추천대기",
        "추천중",
        "공개대기",
        "공개중"
    };

    private static readonly HashSet<string> 환불필요결제상태 = new(StringComparer.OrdinalIgnoreCase)
    {
        "결제완료",
        "승인완료"
    };

    public static 관리자운송의뢰취소환불판단 평가(
        string? 의뢰상태,
        string? 결제상태,
        string? 정산상태,
        string? 배차상태)
    {
        if (포함(의뢰상태, "취소", "환불")
            || 포함(결제상태, "환불", "취소")
            || 포함(정산상태, "정산취소"))
        {
            return 불가("이미 취소 또는 환불 상태가 기록된 의뢰입니다.");
        }

        if (!처리가능의뢰상태.Contains(정규화(의뢰상태)))
        {
            return 불가("현재 의뢰 상태에서는 자동 취소할 수 없습니다. 운송 진행 여부를 먼저 확인해 주세요.");
        }

        if (포함(정산상태, "정산완료"))
        {
            return 불가("정산이 완료된 의뢰는 이 화면에서 취소하거나 환불할 수 없습니다.");
        }

        if (!처리가능배차상태.Contains(정규화(배차상태)))
        {
            return 불가("배차가 확정되었거나 운송이 시작된 의뢰입니다. 별도 운영 절차가 필요합니다.");
        }

        var 환불필요 = 환불필요결제상태.Contains(정규화(결제상태));
        return new 관리자운송의뢰취소환불판단(
            true,
            환불필요,
            환불필요 ? "취소 및 환불 상태 기록" : "의뢰 및 결제 취소 상태 기록",
            환불필요
                ? "외부 PG를 호출하지 않고 Simulation 원장에 취소·환불 상태를 기록합니다."
                : "결제 승인 전 의뢰와 결제 대기 상태를 Simulation 원장에서 취소합니다.");
    }

    public static string? 명시적확인오류(
        string? requestId,
        string? 확인의뢰Id,
        string? 사유)
    {
        if (string.IsNullOrWhiteSpace(requestId)
            || !string.Equals(requestId.Trim(), 확인의뢰Id?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return "확인용 의뢰 ID가 현재 의뢰와 일치하지 않습니다.";
        }

        if (string.IsNullOrWhiteSpace(사유))
        {
            return "취소 또는 환불 사유를 입력해 주세요.";
        }

        return 사유.Trim().Length > 300
            ? "취소 또는 환불 사유는 300자 이하여야 합니다."
            : null;
    }

    private static 관리자운송의뢰취소환불판단 불가(string message)
        => new(false, false, "처리 불가", message);

    private static string 정규화(string? value)
        => value?.Trim() ?? string.Empty;

    private static bool 포함(string? value, params string[] candidates)
    {
        var normalized = 정규화(value);
        return candidates.Any(candidate =>
            normalized.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }
}
