using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Tests.Contracts.Common.Privacy;

public sealed class IsmsPProtectedDataAttributeReaderTests
{
    [Fact]
    public void Read_수입식품공동주문계약초안_ExposesIsmsPContractAndPrivacyFields()
    {
        var members = IsmsPProtectedDataAttributeReader.Read<수입식품공동주문계약초안>();

        Assert.Contains(members, x =>
            x.PropertyName == nameof(수입식품공동주문계약초안.ContractNumber) &&
            x.FieldKey == PersonalDataFieldKey.ContractDocument &&
            x.IsContractData &&
            !x.IsPersonalData);
        Assert.Contains(members, x =>
            x.PropertyName == nameof(수입식품공동주문계약초안.주문자집단배송권명) &&
            x.FieldKey == PersonalDataFieldKey.OrdererGroupScope &&
            x.FieldRule is not null);
        Assert.Contains(members, x =>
            x.PropertyName == nameof(수입식품공동주문계약초안.PaymentPolicy) &&
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
