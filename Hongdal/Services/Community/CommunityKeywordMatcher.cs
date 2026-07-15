using System.Text;
using Hongdal.Domain.Community;

namespace Hongdal.Services.Community;

public interface ICommunityKeywordMatcher
{
    string NormalizeAndValidate(string keyword);
    bool IsMatch(string normalizedKeyword, PlatformCommunityPost post);
}

public sealed class CommunityKeywordMatcher : ICommunityKeywordMatcher
{
    public string NormalizeAndValidate(string keyword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);
        if (keyword.Any(character => char.IsControl(character) && !char.IsWhiteSpace(character)))
        {
            throw new ArgumentException("키워드에는 제어 문자를 사용할 수 없습니다.", nameof(keyword));
        }

        var normalized = NormalizeText(keyword);
        if (normalized.Length is < 1 or > 40)
        {
            throw new ArgumentOutOfRangeException(nameof(keyword), "키워드는 1자 이상 40자 이하여야 합니다.");
        }

        if (!normalized.Any(char.IsLetterOrDigit))
        {
            throw new ArgumentException("키워드에는 문자 또는 숫자가 하나 이상 포함되어야 합니다.", nameof(keyword));
        }

        return normalized;
    }

    public bool IsMatch(string normalizedKeyword, PlatformCommunityPost post)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedKeyword);
        ArgumentNullException.ThrowIfNull(post);

        return IsMatch(normalizedKeyword, post.Title)
               || IsMatch(normalizedKeyword, post.Body)
               || IsMatch(normalizedKeyword, post.Category)
               || IsMatch(normalizedKeyword, post.WorkflowTag)
               || IsMatch(normalizedKeyword, post.RoleTag);
    }

    private static bool IsMatch(string keyword, string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        var text = NormalizeText(source);
        if (!UsesAsciiWordBoundary(keyword))
        {
            return text.Contains(keyword, StringComparison.Ordinal);
        }

        var searchFrom = 0;
        while (searchFrom <= text.Length - keyword.Length)
        {
            var index = text.IndexOf(keyword, searchFrom, StringComparison.Ordinal);
            if (index < 0)
            {
                return false;
            }

            var startsAtBoundary = index == 0 || !char.IsAsciiLetterOrDigit(text[index - 1]);
            var end = index + keyword.Length;
            var endsAtBoundary = end == text.Length || !char.IsAsciiLetterOrDigit(text[end]);
            if (startsAtBoundary && endsAtBoundary)
            {
                return true;
            }

            searchFrom = index + 1;
        }

        return false;
    }

    private static bool UsesAsciiWordBoundary(string keyword)
        => keyword.All(character => char.IsAsciiLetterOrDigit(character) || char.IsWhiteSpace(character))
           && keyword.Any(char.IsAsciiLetterOrDigit);

    private static string NormalizeText(string value)
    {
        var compatibilityNormalized = value.Normalize(NormalizationForm.FormKC).Trim();
        var builder = new StringBuilder(compatibilityNormalized.Length);
        var previousWasWhitespace = false;
        foreach (var character in compatibilityNormalized)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                previousWasWhitespace = true;
                continue;
            }

            builder.Append(char.ToLowerInvariant(character));
            previousWasWhitespace = false;
        }

        return builder.ToString().TrimEnd();
    }
}
