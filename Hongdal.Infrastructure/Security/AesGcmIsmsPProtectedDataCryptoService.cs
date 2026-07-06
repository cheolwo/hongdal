using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Hongdal.Contracts.Common.Privacy;

namespace 홍달.Infrastructure.Security;

public sealed class AesGcmIsmsPProtectedDataCryptoService : IIsmsPProtectedDataCryptoService
{
    public const string EncryptionPrefix = "ismp:aes-256-gcm:v1:";
    public const string HashPrefix = "ismp:sha-256:v1:";
    public const string EncryptionAlgorithmCode = "AES-256-GCM";
    public const string HashAlgorithmCode = "SHA-256";

    private readonly byte[] key;
    private readonly string hashSalt;

    public AesGcmIsmsPProtectedDataCryptoService(IOptions<IsmsPProtectedDataOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var value = options.Value;
        key = ResolveKey(value);
        hashSalt = value.HashSalt ?? string.Empty;
    }

    public IsmsPProtectedValue EncryptAtRest(string fieldKey, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);

        if (string.IsNullOrEmpty(value) || value.StartsWith(EncryptionPrefix, StringComparison.Ordinal))
        {
            return new IsmsPProtectedValue(
                fieldKey,
                PersonalDataProtectionActionCode.EncryptAtRest,
                EncryptionAlgorithmCode,
                value);
        }

        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(value);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];
        var associatedData = Encoding.UTF8.GetBytes(fieldKey);

        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag, associatedData);

        var payload = string.Join(
            ".",
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(tag),
            Convert.ToBase64String(cipherBytes));

        return new IsmsPProtectedValue(
            fieldKey,
            PersonalDataProtectionActionCode.EncryptAtRest,
            EncryptionAlgorithmCode,
            EncryptionPrefix + payload);
    }

    public string DecryptAtRest(string fieldKey, string storedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);

        if (string.IsNullOrEmpty(storedValue) || !storedValue.StartsWith(EncryptionPrefix, StringComparison.Ordinal))
        {
            return storedValue;
        }

        var payload = storedValue[EncryptionPrefix.Length..].Split('.');
        if (payload.Length != 3)
        {
            throw new InvalidOperationException("ISMS-P protected data payload is malformed.");
        }

        var nonce = Convert.FromBase64String(payload[0]);
        var tag = Convert.FromBase64String(payload[1]);
        var cipherBytes = Convert.FromBase64String(payload[2]);
        var plainBytes = new byte[cipherBytes.Length];
        var associatedData = Encoding.UTF8.GetBytes(fieldKey);

        using var aes = new AesGcm(key, tag.Length);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes, associatedData);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public IsmsPProtectedValue HashForEvidence(string fieldKey, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);

        if (string.IsNullOrEmpty(value) || value.StartsWith(HashPrefix, StringComparison.Ordinal))
        {
            return new IsmsPProtectedValue(
                fieldKey,
                PersonalDataProtectionActionCode.HashForEvidence,
                HashAlgorithmCode,
                value);
        }

        var input = $"{hashSalt}:{fieldKey}:{value}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return new IsmsPProtectedValue(
            fieldKey,
            PersonalDataProtectionActionCode.HashForEvidence,
            HashAlgorithmCode,
            HashPrefix + Convert.ToHexString(hash).ToLowerInvariant());
    }

    private static byte[] ResolveKey(IsmsPProtectedDataOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Aes256GcmKeyBase64))
        {
            if (options.FailWhenKeyMissing)
            {
                throw new InvalidOperationException("ISMS-P protected data encryption key is missing. Configure IsmsPProtectedData:Aes256GcmKeyBase64 with a 32-byte Base64 key.");
            }

            return SHA256.HashData(Encoding.UTF8.GetBytes("Hongdal.LocalDevelopment.IsmsPProtectedDataKey"));
        }

        byte[] decoded;
        try
        {
            decoded = Convert.FromBase64String(options.Aes256GcmKeyBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("ISMS-P protected data encryption key must be Base64 encoded.", ex);
        }

        if (decoded.Length != 32)
        {
            throw new InvalidOperationException("ISMS-P protected data encryption key must be exactly 32 bytes for AES-256-GCM.");
        }

        return decoded;
    }
}
