namespace Ssalddel.Contracts.Common.Privacy;

public static class IsmsPTransportEncryptionAlgorithmCode
{
    public const string RsaOaepSha256Aes256Gcm = "RSA-OAEP-256+A256GCM";
}

public sealed record IsmsPClientEncryptionPublicKeyResponse(
    string KeyId,
    string AlgorithmCode,
    string PublicKeyPem,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc);

public sealed record IsmsPEncryptedTransportEnvelope(
    string KeyId,
    string AlgorithmCode,
    string EncryptedKeyBase64,
    string NonceBase64,
    string CipherTextBase64,
    string? AssociatedData = null);

public sealed record IsmsPDecryptedTransportPayload(
    string JsonPayload,
    DateTimeOffset DecryptedAtUtc);
