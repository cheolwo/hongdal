namespace Hongdal.Application.CommandProcessing;

public sealed record Command기능대상(string CommandName, string DisplayName, string Category);

public static class Command기능대상Catalog
{
    public static readonly IReadOnlyList<Command기능대상> DriverCommands =
    [
        new("배차수락Command", "배차 수락", "배차 액션"),
        new("배차거절Command", "배차 거절", "배차 액션"),
        new("운송상차지도착Command", "상차지 도착", "운송 진행"),
        new("운송상차완료Command", "상차 완료", "운송 진행"),
        new("운송하차지도착Command", "하차지 도착", "운송 진행"),
        new("운송인수완료Command", "운송 완료", "운송 진행"),
        new("운송문제신고Command", "운송 문제 신고", "운송 진행")
    ];

    public static string 표시명(string commandName)
    {
        return DriverCommands.FirstOrDefault(x => string.Equals(x.CommandName, commandName, StringComparison.Ordinal))?.DisplayName
            ?? commandName;
    }
}
