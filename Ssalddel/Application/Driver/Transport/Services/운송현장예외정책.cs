namespace Ssalddel.Application.Driver.Transport;

public static class 운송현장예외정책
{
    public const string 기본단계 = "운행중";
    public const string 기본예외코드 = "일반문제";

    private static readonly HashSet<string> 관리자확인필수예외코드 = new(StringComparer.Ordinal)
    {
        "상차물건없음",
        "수량불일치",
        "상차담당자부재",
        "하차지부재",
        "화물훼손"
    };

    public static 운송현장예외정리결과 정리(
        string? 단계,
        string? 예외코드,
        string? 사유,
        bool 관리자확인요청)
    {
        var normalizedCode = NormalizeOrDefault(예외코드, 기본예외코드);
        var normalizedStage = NormalizeOrDefault(단계, ResolveStage(normalizedCode));
        var normalizedReason = NormalizeOrDefault(사유, ResolveReason(normalizedCode));
        var nextAction = ResolveNextAction(normalizedCode, normalizedStage);
        var needsAdminReview = 관리자확인요청 || 관리자확인필수예외코드.Contains(normalizedCode);

        return new 운송현장예외정리결과(
            normalizedStage,
            normalizedCode,
            normalizedReason,
            nextAction,
            needsAdminReview);
    }

    private static string NormalizeOrDefault(string? value, string fallback)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
    }

    private static string ResolveStage(string 예외코드)
    {
        if (예외코드.Contains("상차", StringComparison.Ordinal))
        {
            return "상차";
        }

        if (예외코드.Contains("하차", StringComparison.Ordinal))
        {
            return "하차";
        }

        if (예외코드.Contains("증빙", StringComparison.Ordinal)
            || 예외코드.Contains("사진", StringComparison.Ordinal))
        {
            return "증빙";
        }

        if (예외코드.Contains("위치", StringComparison.Ordinal))
        {
            return "위치";
        }

        if (예외코드.Contains("추천", StringComparison.Ordinal)
            || 예외코드.Contains("서버", StringComparison.Ordinal))
        {
            return "추천";
        }

        return 기본단계;
    }

    private static string ResolveReason(string 예외코드)
        => 예외코드 switch
        {
            "상차물건없음" => "상차지에 도착했지만 상차할 물건을 확인하지 못했습니다.",
            "수량불일치" => "현장 수량이 운송 의뢰 수량과 다릅니다.",
            "상차담당자부재" => "상차 담당자와 현장에서 연락 또는 인계를 진행하지 못했습니다.",
            "하차지부재" => "하차지 담당자 또는 수령자를 확인하지 못했습니다.",
            "사진재촬영필요" => "증빙 사진이 흐리거나 대상을 확인하기 어렵습니다.",
            "화물훼손" => "화물 훼손 또는 이상 상태를 확인했습니다.",
            "증빙업로드실패" => "증빙 사진 업로드가 실패했습니다.",
            "위치송신실패" => "운행 위치 송신이 실패했습니다.",
            "추천응답실패" => "배차 추천 응답 전송이 실패했습니다.",
            "서버응답지연" => "서버 응답이 지연되어 처리가 완료되지 않았습니다.",
            _ => "운송 진행 중 현장 확인이 필요한 문제가 발생했습니다."
        };

    private static string ResolveNextAction(string 예외코드, string 단계)
        => 예외코드 switch
        {
            "상차물건없음" => "상차지와 연락을 다시 시도하고, 현장 사진을 남긴 뒤 관리자 확인을 기다려 주세요.",
            "수량불일치" => "수량 차이를 사진과 메모로 남기고, 임의 상차 또는 하차 전에 관리자 확인을 요청해 주세요.",
            "상차담당자부재" => "상차 담당자에게 다시 연락하고, 연락이 되지 않으면 현장 사진과 함께 관리자 확인을 요청해 주세요.",
            "하차지부재" => "수령자에게 다시 연락하고, 하차 위치 사진을 남긴 뒤 관리자 지시를 기다려 주세요.",
            "사진재촬영필요" => "사진을 다시 촬영한 뒤 재업로드해 주세요. 이미 이동했다면 현재 가능한 증빙과 메모를 남겨 주세요.",
            "화물훼손" => "훼손 부위를 촬영하고, 추가 이동 또는 인계 전에 관리자 확인을 요청해 주세요.",
            "증빙업로드실패" => "사진은 앱에 임시 보관하고, 네트워크가 안정되면 다시 업로드해 주세요.",
            "위치송신실패" => "운행은 계속하되 앱 권한과 네트워크 상태를 확인하고, 도착 시각은 사진 증빙으로 남겨 주세요.",
            "추천응답실패" => "추천 카드가 닫혔다면 다시 열린 추천만 처리하고, 동일 운송건이 반복 표시되면 관리자에게 문의해 주세요.",
            "서버응답지연" => "중복 조작을 피하고 잠시 후 다시 시도해 주세요. 계속 지연되면 관리자에게 문의해 주세요.",
            _ when 단계 == "상차" => "현장 상황을 사진과 메모로 남기고, 상차 진행 가능 여부를 관리자에게 확인해 주세요.",
            _ when 단계 == "하차" => "하차지 상황을 사진과 메모로 남기고, 인계 가능 여부를 관리자에게 확인해 주세요.",
            _ => "현장 상황을 메모와 사진으로 남기고, 필요하면 관리자 확인을 요청해 주세요."
        };
}

public sealed record 운송현장예외정리결과(
    string 단계,
    string 예외코드,
    string 사유,
    string 다음행동안내,
    bool 관리자확인필요);
