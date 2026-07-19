namespace SsalddelAdmin.Components.Shared;

public static class 관리자문서Routes
{
    public const string 목록 = "/documents";
    public const string 업로드 = "/documents/upload";
    public const string 정책목록 = "/documents/policies";
    public const string 로그목록 = "/documents/logs";

    public static string 정책상세(string documentCode)
        => $"{정책목록}/{Uri.EscapeDataString(documentCode)}";

    public static string 로그(long documentId)
        => $"{로그목록}?documentId={documentId}";

    public static string 로그인ReturnUrl(string route)
        => $"/login?returnUrl={Uri.EscapeDataString(route)}";
}
