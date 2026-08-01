namespace Ssalddel.BusinessPackages.AdminUi;

public sealed class BusinessPackageAdminOptions
{
    public const string SectionName = "BusinessPackageAdmin";

    public string PackageCode { get; init; } = string.Empty;
    public string LegacyAdminBaseUrl { get; init; } = "http://localhost:5018";
}
