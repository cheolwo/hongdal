using Microsoft.AspNetCore.Components;

namespace HongdalAdmin.Components.Shared;

public static class 관리자화면Ui
{
    public static RenderFragment 정보칸(string label, string? value) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "border rounded p-2 h-100");
        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "text-muted small");
        builder.AddContent(4, label);
        builder.CloseElement();
        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", "fw-semibold text-break");
        builder.AddContent(7, 표시값(value));
        builder.CloseElement();
        builder.CloseElement();
    };

    public static string 색상Badge(string? color)
        => color switch
        {
            "success" => "badge rounded-pill text-bg-success",
            "primary" => "badge rounded-pill text-bg-primary",
            "danger" => "badge rounded-pill text-bg-danger",
            "warning" => "badge rounded-pill text-bg-warning",
            _ => "badge rounded-pill text-bg-secondary"
        };

    public static string 상태Badge(string? status)
    {
        if (포함(status, "검수완료", "결제완료", "배송완료", "하차완료", "완료", "성공", "업로드", "확정"))
        {
            return "badge rounded-pill text-bg-success";
        }

        if (포함(status, "실패", "예외", "반려", "취소", "환불", "오류", "보류"))
        {
            return "badge rounded-pill text-bg-danger";
        }

        if (포함(status, "대기", "미결제", "매칭중", "배차대기", "청구", "검수"))
        {
            return "badge rounded-pill text-bg-warning";
        }

        return "badge rounded-pill text-bg-secondary";
    }

    public static string 표시값(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;

    public static bool 포함(string? value, params string[] tokens)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return tokens.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
    }
}
