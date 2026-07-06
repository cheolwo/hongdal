using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Tests.Contracts.Common.Privacy;

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
        Assert.Contains(plan.Rules, x =>
            x.FieldKey == PersonalDataFieldKey.BankAccountNumber &&
            x.SensitivityCode == PersonalDataSensitivityCode.Restricted);
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
}
