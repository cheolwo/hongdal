namespace 살뜰.Infrastructure.Security;

public sealed class IsmsPProtectedDataOptions
{
    public const string SectionName = "IsmsPProtectedData";

    public string? Aes256GcmKeyBase64 { get; set; }

    public string? HashSalt { get; set; }

    public string TransportKeyId { get; set; } = "ssalddel-isms-p-transport-v1";

    public string? TransportPublicKeyPem { get; set; }

    public string? TransportPrivateKeyPem { get; set; }

    public int TransportPublicKeyTtlMinutes { get; set; } = 43200;

    public bool FailWhenKeyMissing { get; set; } = true;
}
