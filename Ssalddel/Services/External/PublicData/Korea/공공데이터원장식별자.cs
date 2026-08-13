using System.Security.Cryptography;
using System.Text;

namespace 살뜰.Services.External.PublicData.Korea;

internal static class 공공데이터원장식별자
{
    public static Guid 결정적Guid(string canonicalValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalValue);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalValue));
        return new Guid(bytes.AsSpan(0, 16));
    }

    public static string Sha256(string canonicalValue)
    {
        ArgumentNullException.ThrowIfNull(canonicalValue);
        return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonicalValue)))
            .ToLowerInvariant();
    }
}
