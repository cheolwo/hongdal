using System.Security.Cryptography;

namespace Ssalddel.UnityReview.Api.Configuration;

public sealed class UnityReviewAccessOptions
{
    public const string SectionName = "UnityReviewAccess";

    public string AdminUserName { get; set; } = string.Empty;
    public string AdminPasswordPbkdf2 { get; set; } = string.Empty;
    public string JwtSigningKeyBase64 { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "Ssalddel.UnityReview";
    public string JwtAudience { get; set; } = "Ssalddel.UnityReview.Web";
    public int TokenLifetimeHours { get; set; } = 12;

    public static bool VerifyPassword(string password, string encoded)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(encoded))
        {
            return false;
        }

        var parts = encoded.Split('.', StringSplitOptions.TrimEntries);
        if (parts.Length != 3
            || !int.TryParse(parts[0], out var iterations)
            || iterations is < 100_000 or > 2_000_000)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            if (salt.Length < 16 || expectedHash.Length < 32)
            {
                return false;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

public sealed class UnityReviewDatabaseOptions
{
    public const string SectionName = "UnityReviewDatabase";

    public string ConnectionString { get; set; } = string.Empty;
}
