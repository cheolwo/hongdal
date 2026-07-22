using MudBlazor;
using Ssalddel.Contracts.Common.Inbound;

namespace Ssalddel.Ui.Common.Areas.App.Components.WarehouseOperations;

public static class InboundReceivingPresentation
{
    public static string Display(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;

    public static string FormatCreatedAt(DateTime value)
        => value == default
            ? "-"
            : $"{value.ToLocalTime():yyyy-MM-dd HH:mm} (기기 시각)";

    public static Color StatusColor(string? status)
        => string.Equals(status, 입고상태코드.예정, StringComparison.Ordinal)
            ? Color.Info
            : Color.Default;

    public static string? WorkBoardHref(string? path, long? inboundId)
    {
        if (string.IsNullOrWhiteSpace(path) || inboundId is not > 0)
        {
            return null;
        }

        var normalizedPath = path.Trim();
        var separator = normalizedPath.Contains('?') ? '&' : '?';
        return $"{normalizedPath}{separator}inboundId={inboundId.Value}";
    }
}
