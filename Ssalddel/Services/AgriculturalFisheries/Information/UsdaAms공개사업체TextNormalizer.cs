using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal static partial class UsdaAms공개사업체TextNormalizer
{
    public static string NormalizeSearchText(string value)
        => CollapseWhitespace(value).ToUpperInvariant();

    public static string CreateProductKey(string value)
    {
        var builder = new StringBuilder();
        var needsSeparator = false;
        foreach (var character in value.Normalize(NormalizationForm.FormKC))
        {
            if (char.IsLetterOrDigit(character))
            {
                if (needsSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(character));
                needsSeparator = false;
            }
            else
            {
                needsSeparator = true;
            }

            if (builder.Length >= 200)
            {
                break;
            }
        }

        return builder.ToString().TrimEnd('-');
    }

    public static string CreateSha256(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    public static string CollapseWhitespace(string? value)
        => WhitespaceRegex().Replace(value?.Trim() ?? string.Empty, " ");

    public static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
