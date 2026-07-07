using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Hongdal.Contracts.Common.Privacy;

namespace 홍달.Infrastructure.Security;

public sealed class RsaOaepAesGcmClientTransportProtectionService : IIsmsPClientTransportProtectionService
{
    private const int AesGcmNonceBytes = 12;
    private const int AesGcmTagBytes = 16;

    private readonly IsmsPProtectedDataOptions options;

    public RsaOaepAesGcmClientTransportProtectionService(IOptions<IsmsPProtectedDataOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options.Value;
    }

    public IsmsPClientEncryptionPublicKeyResponse GetPublicKey()
    {
        if (string.IsNullOrWhiteSpace(options.TransportPublicKeyPem))
        {
            throw new InvalidOperationException("ISMS-P transport public key is missing. Configure IsmsPProtectedData:TransportPublicKeyPem.");
        }

        var issuedAtUtc = DateTimeOffset.UtcNow;
        var ttl = TimeSpan.FromMinutes(Math.Max(1, options.TransportPublicKeyTtlMinutes));

        return new IsmsPClientEncryptionPublicKeyResponse(
            options.TransportKeyId,
            IsmsPTransportEncryptionAlgorithmCode.RsaOaepSha256Aes256Gcm,
            options.TransportPublicKeyPem,
            issuedAtUtc,
            issuedAtUtc.Add(ttl));
    }

    public IsmsPDecryptedTransportPayload Decrypt(IsmsPEncryptedTransportEnvelope envelope)
    {
        ValidateEnvelope(envelope);

        if (string.IsNullOrWhiteSpace(options.TransportPrivateKeyPem))
        {
            throw new InvalidOperationException("ISMS-P transport private key is missing. Configure IsmsPProtectedData:TransportPrivateKeyPem.");
        }

        if (!string.Equals(envelope.KeyId, options.TransportKeyId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("ISMS-P transport key id does not match the configured server key.");
        }

        if (!string.Equals(envelope.AlgorithmCode, IsmsPTransportEncryptionAlgorithmCode.RsaOaepSha256Aes256Gcm, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Unsupported ISMS-P transport encryption algorithm.");
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(options.TransportPrivateKeyPem);

        var encryptedKey = Convert.FromBase64String(envelope.EncryptedKeyBase64);
        var aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
        var nonce = Convert.FromBase64String(envelope.NonceBase64);
        var cipherTextWithTag = Convert.FromBase64String(envelope.CipherTextBase64);

        if (aesKey.Length != 32)
        {
            throw new InvalidOperationException("ISMS-P transport AES key must be 32 bytes.");
        }

        if (nonce.Length != AesGcmNonceBytes)
        {
            throw new InvalidOperationException("ISMS-P transport nonce must be 12 bytes for AES-GCM.");
        }

        if (cipherTextWithTag.Length <= AesGcmTagBytes)
        {
            throw new InvalidOperationException("ISMS-P transport cipher text is too short.");
        }

        var cipherTextLength = cipherTextWithTag.Length - AesGcmTagBytes;
        var cipherText = cipherTextWithTag[..cipherTextLength];
        var tag = cipherTextWithTag[cipherTextLength..];
        var plainText = new byte[cipherTextLength];
        var associatedData = string.IsNullOrWhiteSpace(envelope.AssociatedData)
            ? null
            : Encoding.UTF8.GetBytes(envelope.AssociatedData);

        using var aes = new AesGcm(aesKey, AesGcmTagBytes);
        aes.Decrypt(nonce, cipherText, tag, plainText, associatedData);

        CryptographicOperations.ZeroMemory(aesKey);

        return new IsmsPDecryptedTransportPayload(
            Encoding.UTF8.GetString(plainText),
            DateTimeOffset.UtcNow);
    }

    private static void ValidateEnvelope(IsmsPEncryptedTransportEnvelope envelope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.KeyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.AlgorithmCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.EncryptedKeyBase64);
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.NonceBase64);
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.CipherTextBase64);
    }
}
