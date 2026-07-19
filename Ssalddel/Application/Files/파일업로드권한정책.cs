using 살뜰.도메인.운송;

namespace Ssalddel.Application.Files;

public static class 파일업로드권한정책
{
    private static readonly string[] 운송증빙CommandNames =
    [
        "TransportPickupComplete",
        "TransportDropoffComplete",
        "TransportIssueEvidence",
        "운송상차완료Command",
        "운송인수완료Command",
        "운송문제신고Command"
    ];

    public static bool 운송증빙업로드인가(string? commandName)
        => !string.IsNullOrWhiteSpace(commandName)
           && 운송증빙CommandNames.Contains(commandName.Trim(), StringComparer.Ordinal);

    public static bool 운송증빙업로드권한있음(운송원장 transport, string? userId, string? role)
        => string.Equals(role, 역할명.서버관리자, StringComparison.OrdinalIgnoreCase)
           || (!string.IsNullOrWhiteSpace(userId)
               && string.Equals(transport.기사_운송자, userId, StringComparison.Ordinal));
}
