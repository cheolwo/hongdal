using MudBlazor;
using Ssalddel.Contracts.Common.Orderer;
using Color = MudBlazor.Color;

namespace OrdererApp.Components.GroupPurchase;

internal static class GroupPurchasePresentation
{
    public static string Money(decimal value) => $"{value:N0}원";

    public static string UsdPerKg(decimal? value)
        => value.HasValue ? $"${value.Value:N2}/kg" : "-";

    public static string KrwPerKg(decimal? value)
        => value.HasValue ? $"{value.Value:N0} KRW/kg" : "-";

    public static string KrwTotal(decimal? value)
        => value.HasValue ? $"{value.Value:N0} KRW" : "-";

    public static string TemperatureLabel(string value)
        => value switch
        {
            공동구매온도코드.냉동 => "냉동",
            공동구매온도코드.냉장 => "냉장",
            공동구매온도코드.상온 => "상온",
            _ => value
        };

    public static string PriorityLabel(string value)
        => value switch
        {
            공동구매활성화우선순위코드.냉장냉동먹거리중심 => "냉동/냉장 우선",
            공동구매활성화우선순위코드.먹거리중심 => "먹거리 우선",
            _ => "일반 후보"
        };

    public static Color PriorityColor(string value)
        => value switch
        {
            공동구매활성화우선순위코드.냉장냉동먹거리중심 => Color.Success,
            공동구매활성화우선순위코드.먹거리중심 => Color.Primary,
            _ => Color.Default
        };

    public static Color MarketSignalColor(string value)
        => value switch
        {
            "Attractive" => Color.Success,
            "Viable" => Color.Primary,
            "ThinMargin" => Color.Warning,
            "LossRisk" => Color.Error,
            "BelowMarketImportAverage" => Color.Success,
            "NearMarketImportAverage" => Color.Info,
            "AboveMarketImportAverage" => Color.Warning,
            _ => Color.Default
        };

    public static Color CostStageColor(string statusCode)
        => statusCode switch
        {
            수입도착원가단계상태코드.차단 => Color.Error,
            수입도착원가단계상태코드.검토필요 => Color.Warning,
            수입도착원가단계상태코드.추정 => Color.Primary,
            _ => Color.Info
        };

    public static string PaymentStatusLabel(string value)
        => value switch
        {
            공동구매결제단계상태코드.요청가능 => "요청 가능",
            공동구매결제단계상태코드.지급완료 => "지급 완료",
            공동구매결제단계상태코드.차단 => "대기",
            _ => "대기"
        };

    public static Color PaymentStatusColor(string value)
        => value switch
        {
            공동구매결제단계상태코드.요청가능 => Color.Primary,
            공동구매결제단계상태코드.지급완료 => Color.Success,
            _ => Color.Default
        };

    public static string ApplicationStatusLabel(string value)
        => value switch
        {
            주문자집단개설신청상태코드.승인대기 => "승인 대기",
            주문자집단개설신청상태코드.ApprovedGroupReady => "집단 생성",
            주문자집단개설신청상태코드.반려 => "반려",
            _ => "작성 중"
        };

    public static string AutoGroupStatusLabel(string value)
        => value switch
        {
            공동구매자동집단상태코드.수요수집중 => "수요 수집 중",
            공동구매자동집단상태코드.확정대기 => "확정 검토 대기",
            공동구매자동집단상태코드.확정 => "집단 확정",
            _ => value
        };

    public static string TransportDocumentLabel(string value)
        => value switch
        {
            공동구매선적문서유형코드.항공화물운송장 => "AWB",
            _ => "B/L"
        };

    public static string DateLabel(DateTime? value)
        => value.HasValue ? DateLabel(value.Value) : "-";

    public static string DateLabel(DateTime value)
        => value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    public static string ShipmentStatusLabel(string value)
        => value switch
        {
            공동구매선적상태코드.문서등록 => "문서 등록",
            공동구매선적상태코드.해외포장완료 => "해외 포장",
            공동구매선적상태코드.선박항공편적재 => "선적 완료",
            공동구매선적상태코드.운송중 => "운송 중",
            공동구매선적상태코드.항만도착 => "도착",
            공동구매선적상태코드.통관진행중 => "통관 중",
            공동구매선적상태코드.통관완료 => "통관 완료",
            공동구매선적상태코드.국내창고입고 => "국내 입고",
            공동구매선적상태코드.국내기사상차 => "국내 기사 상차",
            공동구매선적상태코드.공동주택하차 => "공동주택 거점 하차",
            공동구매선적상태코드.분배진행중 => "분배 중",
            공동구매선적상태코드.완료 => "완료",
            공동구매선적상태코드.예외 => "예외",
            _ => value
        };

    public static Color ShipmentStatusColor(string value)
        => value switch
        {
            공동구매선적상태코드.완료 => Color.Success,
            공동구매선적상태코드.예외 => Color.Error,
            공동구매선적상태코드.통관완료 => Color.Info,
            공동구매선적상태코드.국내창고입고 => Color.Info,
            공동구매선적상태코드.국내기사상차 => Color.Primary,
            공동구매선적상태코드.공동주택하차 => Color.Success,
            _ => Color.Default
        };
}
