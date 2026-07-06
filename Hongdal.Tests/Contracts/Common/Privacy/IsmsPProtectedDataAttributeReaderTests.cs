using Hongdal.Contracts.Common.ContractManagement;
using Hongdal.Contracts.Common.Hr;
using Hongdal.Contracts.Common.Privacy;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Tests.Contracts.Common.Privacy;

public sealed class IsmsPProtectedDataAttributeReaderTests
{
    [Fact]
    public void Read_ImportFoodGroupPurchaseContractDraft_ExposesIsmsPContractAndPrivacyFields()
    {
        var members = IsmsPProtectedDataAttributeReader.Read<ImportFoodGroupPurchaseContractDraft>();

        Assert.Contains(members, x =>
            x.PropertyName == nameof(ImportFoodGroupPurchaseContractDraft.ContractNumber) &&
            x.FieldKey == PersonalDataFieldKey.ContractDocument &&
            x.IsContractData &&
            !x.IsPersonalData);
        Assert.Contains(members, x =>
            x.PropertyName == nameof(ImportFoodGroupPurchaseContractDraft.OrdererGroupScopeName) &&
            x.FieldKey == PersonalDataFieldKey.OrdererGroupScope &&
            x.FieldRule is not null);
        Assert.Contains(members, x =>
            x.PropertyName == nameof(ImportFoodGroupPurchaseContractDraft.PaymentPolicy) &&
            x.DomainCode == IsmsPDomainCode.ManagementSystem);
    }

    [Fact]
    public void BuildFieldProtectionPlan_ShipperAddress_UsesCatalogRules()
    {
        var plan = IsmsPProtectedDataAttributeReader.BuildFieldProtectionPlan(typeof(AddressDTO));

        Assert.False(plan.HasUnknownFields);
        Assert.Contains(PersonalDataProtectionActionCode.MaskByDefault, plan.RequiredActionCodes);
        Assert.Contains(PersonalDataProtectionActionCode.AuditOnAccess, plan.RequiredActionCodes);
        Assert.Contains(plan.Rules, x => x.FieldKey == PersonalDataFieldKey.DetailedAddress);
        Assert.Contains(plan.Rules, x => x.FieldKey == PersonalDataFieldKey.LocationCoordinate);
    }

    [Fact]
    public void BuildFeatureProfile_HrDraftRequest_CanFeedReadinessPlanner()
    {
        var profile = IsmsPProtectedDataAttributeReader.BuildFeatureProfile(
            typeof(HrEmploymentContractDraftRequest),
            featureName: "HR 근로계약 초안",
            owner: "인력 관리자",
            hasPurposeAndLegalBasis: true,
            hasRetentionAndDestructionRule: true,
            hasConsentOrNotice: true,
            hasRoleBasedAccessControl: true,
            hasMaskingOrEncryption: true,
            hasAuditLog: true,
            hasThirdPartyOrOutsourcingReview: true,
            hasIncidentResponseOwner: true,
            hasBackupOrRecoveryPlan: true,
            hasSecureDevelopmentReview: true,
            hasContractTermsReview: true);

        var plan = IsmsPReadinessPlanner.Plan(profile);

        Assert.True(profile.ProcessesPersonalData);
        Assert.True(profile.ProcessesContractData);
        Assert.True(plan.IsReadyForInternalReview);
        Assert.NotNull(profile.PersonalDataFieldKeys);
        Assert.Contains(PersonalDataFieldKey.BankAccountNumber, profile.PersonalDataFieldKeys!);
    }
}
