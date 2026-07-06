using System.Reflection;

namespace Hongdal.Contracts.Common.Privacy;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class IsmsPProtectedDataAttribute : Attribute
{
    public IsmsPProtectedDataAttribute(
        string fieldKey,
        string purpose,
        params string[] requiredActionCodes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        FieldKey = fieldKey;
        Purpose = purpose;
        RequiredActionCodes = requiredActionCodes ?? [];
    }

    public string FieldKey { get; }

    public string Purpose { get; }

    public string[] RequiredActionCodes { get; }

    public bool IsPersonalData { get; set; } = true;

    public bool IsContractData { get; set; }

    public string DomainCode { get; set; } = IsmsPDomainCode.PrivacyLifecycle;

    public string ProtectionNote { get; set; } = string.Empty;
}

public sealed record IsmsPProtectedDataMember(
    Type DeclaringType,
    string PropertyName,
    string FieldKey,
    string Purpose,
    string DomainCode,
    bool IsPersonalData,
    bool IsContractData,
    IReadOnlyList<string> RequiredActionCodes,
    PersonalDataFieldProtectionRule? FieldRule,
    string ProtectionNote);

public static class IsmsPProtectedDataAttributeReader
{
    public static IReadOnlyList<IsmsPProtectedDataMember> Read(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(property => property
                .GetCustomAttributes<IsmsPProtectedDataAttribute>(inherit: true)
                .Select(attribute => CreateMember(type, property, attribute)))
            .ToArray();
    }

    public static IReadOnlyList<IsmsPProtectedDataMember> Read<T>()
        => Read(typeof(T));

    public static PersonalDataFieldProtectionPlan BuildFieldProtectionPlan(Type type)
    {
        var fieldKeys = Read(type)
            .Where(x => x.IsPersonalData)
            .Select(x => x.FieldKey);

        return PersonalDataFieldProtectionCatalog.PlanFor(fieldKeys);
    }

    public static PersonalDataContractFeatureProfile BuildFeatureProfile(
        Type type,
        string featureName,
        string owner,
        bool hasPurposeAndLegalBasis,
        bool hasRetentionAndDestructionRule,
        bool hasConsentOrNotice,
        bool hasRoleBasedAccessControl,
        bool hasMaskingOrEncryption,
        bool hasAuditLog,
        bool hasThirdPartyOrOutsourcingReview,
        bool hasIncidentResponseOwner,
        bool hasBackupOrRecoveryPlan,
        bool hasSecureDevelopmentReview,
        bool hasContractTermsReview)
    {
        var members = Read(type);
        return new PersonalDataContractFeatureProfile(
            FeatureName: featureName,
            Owner: owner,
            ProcessesPersonalData: members.Any(x => x.IsPersonalData),
            ProcessesContractData: members.Any(x => x.IsContractData),
            HasPurposeAndLegalBasis: hasPurposeAndLegalBasis,
            HasDataMinimization: members.Any(x => x.IsPersonalData),
            HasRetentionAndDestructionRule: hasRetentionAndDestructionRule,
            HasConsentOrNotice: hasConsentOrNotice,
            HasRoleBasedAccessControl: hasRoleBasedAccessControl,
            HasMaskingOrEncryption: hasMaskingOrEncryption,
            HasAuditLog: hasAuditLog,
            HasThirdPartyOrOutsourcingReview: hasThirdPartyOrOutsourcingReview,
            HasIncidentResponseOwner: hasIncidentResponseOwner,
            HasBackupOrRecoveryPlan: hasBackupOrRecoveryPlan,
            HasSecureDevelopmentReview: hasSecureDevelopmentReview,
            HasContractTermsReview: hasContractTermsReview,
            PersonalDataFieldKeys: members
                .Where(x => x.IsPersonalData)
                .Select(x => x.FieldKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    private static IsmsPProtectedDataMember CreateMember(
        Type type,
        PropertyInfo property,
        IsmsPProtectedDataAttribute attribute)
        => new(
            type,
            property.Name,
            attribute.FieldKey,
            attribute.Purpose,
            attribute.DomainCode,
            attribute.IsPersonalData,
            attribute.IsContractData,
            attribute.RequiredActionCodes,
            PersonalDataFieldProtectionCatalog.Find(attribute.FieldKey),
            attribute.ProtectionNote);
}
