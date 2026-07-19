using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Tests.Contracts.Common.Privacy;

public sealed class PersonalDataFieldProtectionCatalogTests
{
    [Fact]
    public void PlanFor_BankAccountAndDetailedAddress_RequiresRestrictedFieldProtections()
    {
        var plan = PersonalDataFieldProtectionCatalog.PlanFor(
        [
            PersonalDataFieldKey.BankAccountNumber,
            PersonalDataFieldKey.DetailedAddress
        ]);

        Assert.False(plan.HasUnknownFields);
        Assert.Contains(PersonalDataProtectionActionCode.MaskByDefault, plan.RequiredActionCodes);
        Assert.Contains(PersonalDataProtectionActionCode.EncryptAtRest, plan.RequiredActionCodes);
        Assert.Contains(PersonalDataProtectionActionCode.AuditOnAccess, plan.RequiredActionCodes);
        Assert.Contains(PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview, plan.RequiredActionCodes);
        Assert.True(plan.RequiresTransportEncryption);
        Assert.True(plan.RequiresAtRestEncryption);
        Assert.Contains(plan.Rules, x =>
            x.FieldKey == PersonalDataFieldKey.BankAccountNumber &&
            x.SensitivityCode == PersonalDataSensitivityCode.Restricted &&
            x.StorageProtectionCode == PersonalDataStorageProtectionCode.EncryptAtRest);
    }

    [Fact]
    public void PlanFor_DeduplicatesKnownFieldsAndReportsUnknownFields()
    {
        var plan = PersonalDataFieldProtectionCatalog.PlanFor(
        [
            PersonalDataFieldKey.PhoneNumber,
            PersonalDataFieldKey.PhoneNumber.ToUpperInvariant(),
            "legacy-free-text"
        ]);

        Assert.True(plan.HasUnknownFields);
        Assert.Single(plan.Rules);
        Assert.Single(plan.UnknownFieldKeys);
        Assert.Contains("legacy-free-text", plan.UnknownFieldKeys);
        Assert.Contains("알 수 없는 개인정보 필드", plan.Summary);
    }

    [Fact]
    public void Find_BlankFieldKey_ReturnsNull()
    {
        var rule = PersonalDataFieldProtectionCatalog.Find(" ");

        Assert.Null(rule);
    }

    [Fact]
    public void PlanFor_ElectronicSignatureEvidence_RequiresRestrictedEvidenceProtections()
    {
        var plan = PersonalDataFieldProtectionCatalog.PlanFor(
        [
            PersonalDataFieldKey.ElectronicSignatureEvidence
        ]);

        Assert.False(plan.HasUnknownFields);
        Assert.Contains(PersonalDataProtectionActionCode.ConsentOrNotice, plan.RequiredActionCodes);
        Assert.Contains(PersonalDataProtectionActionCode.EncryptAtRest, plan.RequiredActionCodes);
        Assert.Contains(PersonalDataProtectionActionCode.AuditOnAccess, plan.RequiredActionCodes);
        Assert.Contains(plan.Rules, x =>
            x.FieldKey == PersonalDataFieldKey.ElectronicSignatureEvidence &&
            x.SensitivityCode == PersonalDataSensitivityCode.Restricted &&
            x.StorageProtectionCode == PersonalDataStorageProtectionCode.EncryptAtRest);
    }

    [Fact]
    public void PlanFor_PaymentMethod_ClassifiesWithoutAtRestEncryption()
    {
        var plan = PersonalDataFieldProtectionCatalog.PlanFor(
        [
            PersonalDataFieldKey.PaymentMethod
        ]);

        Assert.DoesNotContain(PersonalDataProtectionActionCode.EncryptAtRest, plan.RequiredActionCodes);
        Assert.True(plan.RequiresTransportEncryption);
        Assert.False(plan.RequiresAtRestEncryption);
        Assert.False(plan.RequiresEvidenceHash);
        Assert.Contains(plan.Rules, x =>
            x.FieldKey == PersonalDataFieldKey.PaymentMethod &&
            x.StorageProtectionCode == PersonalDataStorageProtectionCode.ClassifiedOnly);
    }

    [Fact]
    public void PlanFor_IpAddress_UsesEvidenceHashStorage()
    {
        var plan = PersonalDataFieldProtectionCatalog.PlanFor(
        [
            PersonalDataFieldKey.IpAddress
        ]);

        Assert.Contains(PersonalDataProtectionActionCode.HashForEvidence, plan.RequiredActionCodes);
        Assert.False(plan.RequiresTransportEncryption);
        Assert.False(plan.RequiresAtRestEncryption);
        Assert.True(plan.RequiresEvidenceHash);
        Assert.Contains(plan.Rules, x =>
            x.FieldKey == PersonalDataFieldKey.IpAddress &&
            x.StorageProtectionCode == PersonalDataStorageProtectionCode.HashForEvidence);
    }
}
