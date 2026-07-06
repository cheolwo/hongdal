using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Tests.Contracts.Common.Privacy;

public sealed class IsmsPReadinessPlannerTests
{
    [Fact]
    public void Plan_PersonalAndContractFeatureWithAllControls_IsReady()
    {
        var profile = CreateCompleteProfile();

        var plan = IsmsPReadinessPlanner.Plan(profile);

        Assert.True(plan.IsReadyForInternalReview);
        Assert.Empty(plan.MissingRequiredCodes);
        Assert.Equal(plan.RequiredCount, plan.SatisfiedRequiredCount);
        Assert.Contains(plan.Items, x => x.Code == IsmsPReadinessControlCode.PurposeAndLegalBasis);
        Assert.Contains(plan.Items, x => x.Code == IsmsPReadinessControlCode.ContractTermsReview);
    }

    [Fact]
    public void Plan_MissingRetentionAndEncryption_BlocksInternalReview()
    {
        var profile = CreateCompleteProfile() with
        {
            HasRetentionAndDestructionRule = false,
            HasMaskingOrEncryption = false
        };

        var plan = IsmsPReadinessPlanner.Plan(profile);

        Assert.False(plan.IsReadyForInternalReview);
        Assert.Contains(IsmsPReadinessControlCode.RetentionAndDestruction, plan.MissingRequiredCodes);
        Assert.Contains(IsmsPReadinessControlCode.MaskingOrEncryption, plan.MissingRequiredCodes);
        Assert.Contains("보완 필요", plan.Summary);
    }

    [Fact]
    public void Plan_UnknownPersonalDataField_BlocksFieldCatalogReadiness()
    {
        var profile = CreateCompleteProfile() with
        {
            PersonalDataFieldKeys =
            [
                PersonalDataFieldKey.PhoneNumber,
                "unknown-sensitive-field"
            ]
        };

        var plan = IsmsPReadinessPlanner.Plan(profile);

        Assert.False(plan.IsReadyForInternalReview);
        Assert.Contains(IsmsPReadinessControlCode.PersonalDataFieldCatalog, plan.MissingRequiredCodes);
        Assert.NotNull(plan.FieldProtectionPlan);
        Assert.True(plan.FieldProtectionPlan.HasUnknownFields);
        Assert.Contains("unknown-sensitive-field", plan.FieldProtectionPlan.UnknownFieldKeys);
    }

    [Fact]
    public void Plan_ContractOnlyFeature_RequiresContractAccessAuditAndRecoveryControls()
    {
        var profile = new PersonalDataContractFeatureProfile(
            FeatureName: "운송 계약 정산 조건",
            Owner: "계약 운영자",
            ProcessesPersonalData: false,
            ProcessesContractData: true,
            HasRoleBasedAccessControl: true,
            HasAuditLog: true,
            HasIncidentResponseOwner: true,
            HasBackupOrRecoveryPlan: true,
            HasSecureDevelopmentReview: true,
            HasContractTermsReview: false);

        var plan = IsmsPReadinessPlanner.Plan(profile);

        Assert.False(plan.IsReadyForInternalReview);
        Assert.DoesNotContain(IsmsPReadinessControlCode.PurposeAndLegalBasis, plan.Items.Select(x => x.Code));
        Assert.Contains(IsmsPReadinessControlCode.ContractTermsReview, plan.MissingRequiredCodes);
        Assert.Contains(plan.Items, x => x.Code == IsmsPReadinessControlCode.AuditLog && x.IsSatisfied);
    }

    private static PersonalDataContractFeatureProfile CreateCompleteProfile()
        => new(
            FeatureName: "수입 식품 공동 주문 계약서",
            Owner: "플랫폼 운영자",
            ProcessesPersonalData: true,
            ProcessesContractData: true,
            HasPurposeAndLegalBasis: true,
            HasDataMinimization: true,
            HasRetentionAndDestructionRule: true,
            HasConsentOrNotice: true,
            HasRoleBasedAccessControl: true,
            HasMaskingOrEncryption: true,
            HasAuditLog: true,
            HasThirdPartyOrOutsourcingReview: true,
            HasIncidentResponseOwner: true,
            HasBackupOrRecoveryPlan: true,
            HasSecureDevelopmentReview: true,
            HasContractTermsReview: true,
            PersonalDataFieldKeys:
            [
                PersonalDataFieldKey.DisplayName,
                PersonalDataFieldKey.PhoneNumber,
                PersonalDataFieldKey.DetailedAddress,
                PersonalDataFieldKey.PaymentMethod,
                PersonalDataFieldKey.ContractDocument
            ]);
}
