namespace Ssalddel.Security;

public sealed class PersonalDataProtectionOptions
{
    public const string SectionName = "PersonalDataProtection";

    public string ApplicationName { get; set; } = "Ssalddel";

    public string KeyRingPath { get; set; } = "App_Data/DataProtection-Keys";

    public bool RequireCertificate { get; set; }

    public string CertificatePath { get; set; } = string.Empty;

    public string CertificatePassword { get; set; } = string.Empty;
}
