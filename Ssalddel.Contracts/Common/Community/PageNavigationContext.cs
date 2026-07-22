namespace Ssalddel.Contracts.Common.Community;

/// <summary>
/// Web과 모바일 Route Page가 이전 화면의 정확한 로컬 문맥을 안전하게 전달하기 위한 계약입니다.
/// </summary>
public sealed record PageNavigationContext
{
    public const int MaximumReturnPathLength = 2048;
    public const int MaximumFocusTargetLength = 128;

    public string? ReturnPath { get; init; }
    public string? FocusTarget { get; init; }

    public string ResolveReturnPath(string fallbackPath = CommunityPageRoutes.Home)
        => ResolveReturnPath(ReturnPath, fallbackPath);

    public static string ResolveReturnPath(string? returnPath, string fallbackPath)
        => NormalizeReturnPath(returnPath)
           ?? NormalizeReturnPath(fallbackPath)
           ?? CommunityPageRoutes.Home;

    public static string? NormalizeReturnPath(string? returnPath)
    {
        var value = returnPath?.Trim();
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaximumReturnPathLength
            || !value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("//", StringComparison.Ordinal)
            || value.Contains('\\')
            || value.Any(char.IsControl))
        {
            return null;
        }

        try
        {
            var decoded = Uri.UnescapeDataString(value);
            if (decoded.StartsWith("//", StringComparison.Ordinal)
                || decoded.Contains('\\')
                || decoded.Any(char.IsControl))
            {
                return null;
            }
        }
        catch (UriFormatException)
        {
            return null;
        }

        return value;
    }

    public static string? NormalizeFocusTarget(string? focusTarget)
    {
        var value = focusTarget?.Trim();
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaximumFocusTargetLength
            || value.Any(character => !char.IsLetterOrDigit(character)
                                      && character is not '-' and not '_' and not ':' and not '.'))
        {
            return null;
        }

        return value;
    }

    public static string WithReturnPath(string targetPath, string? returnPath)
    {
        var normalizedTarget = NormalizeReturnPath(targetPath)
            ?? throw new ArgumentException("대상 경로는 앱 내부 절대 경로여야 합니다.", nameof(targetPath));
        var normalizedReturnPath = NormalizeReturnPath(returnPath);
        if (normalizedReturnPath is null)
        {
            return normalizedTarget;
        }

        var fragmentIndex = normalizedTarget.IndexOf('#', StringComparison.Ordinal);
        var fragment = fragmentIndex >= 0 ? normalizedTarget[fragmentIndex..] : string.Empty;
        var pathAndQuery = fragmentIndex >= 0 ? normalizedTarget[..fragmentIndex] : normalizedTarget;
        var separator = pathAndQuery.Contains('?') ? '&' : '?';
        return $"{pathAndQuery}{separator}{PageNavigationQueryNames.ReturnPath}={Uri.EscapeDataString(normalizedReturnPath)}{fragment}";
    }
}

public static class PageNavigationQueryNames
{
    public const string ReturnPath = "from";
    public const string FocusTarget = "focus";
}
