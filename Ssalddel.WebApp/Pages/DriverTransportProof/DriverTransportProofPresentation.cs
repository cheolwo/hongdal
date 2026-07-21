using MudBlazor;

namespace Ssalddel.WebApp.Pages.DriverTransportProof;

public static class DriverTransportProofPresentation
{
    public static string Display(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    public static string DisplayMoney(decimal? value)
        => value.HasValue ? $"{value.Value:N0}원" : "금액 미정";

    public static Color ResolveTransportStateColor(string? status)
    {
        if (ContainsAny(status, "완료", "인수"))
        {
            return Color.Success;
        }

        if (ContainsAny(status, "예외", "실패", "문제"))
        {
            return Color.Error;
        }

        if (ContainsAny(status, "도착", "상차", "하차"))
        {
            return Color.Info;
        }

        return Color.Default;
    }

    private static bool ContainsAny(string? value, params string[] tokens)
        => !string.IsNullOrWhiteSpace(value)
           && tokens.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
}
