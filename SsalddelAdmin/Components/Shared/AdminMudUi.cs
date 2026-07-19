using MudBlazor;

namespace SsalddelAdmin.Components.Shared;

public static class AdminMudUi
{
    public static Color 색상(string? color)
        => color switch
        {
            "success" => Color.Success,
            "primary" => Color.Primary,
            "danger" => Color.Error,
            "warning" => Color.Warning,
            "info" => Color.Info,
            _ => Color.Default
        };

    public static Color 상태색상(string? status)
    {
        if (관리자화면Ui.포함(status, "검수완료", "결제완료", "배송완료", "하차완료", "완료", "성공", "업로드", "확정"))
        {
            return Color.Success;
        }

        if (관리자화면Ui.포함(status, "실패", "예외", "반려", "취소", "환불", "오류", "보류"))
        {
            return Color.Error;
        }

        if (관리자화면Ui.포함(status, "대기", "미결제", "매칭중", "배차대기", "청구", "검수"))
        {
            return Color.Warning;
        }

        return Color.Default;
    }

    public static Color 우선도색상(string? priority)
        => string.Equals(priority, "높음", StringComparison.OrdinalIgnoreCase)
            ? Color.Error
            : Color.Warning;
}
