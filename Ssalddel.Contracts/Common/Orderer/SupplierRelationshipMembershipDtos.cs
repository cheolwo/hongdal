namespace Ssalddel.Contracts.Common.Orderer;

public static class SupplierRelationshipPartyTypeCodes
{
    public const string DomesticAgriculturalBusiness = "DomesticAgriculturalBusiness";
    public const string OverseasFoodManufacturer = "OverseasFoodManufacturer";
}

public static class SupplierRelationshipAudienceTypeCodes
{
    public const string IndividualOrderer = "IndividualOrderer";
    public const string DeliveryScopeGroup = "DeliveryScopeGroup";
}

public static class SupplierMembershipStatusCodes
{
    public const string InterestFollowing = "InterestFollowing";
    public const string EnrollmentDraft = "EnrollmentDraft";
    public const string Active = "Active";
    public const string Paused = "Paused";
    public const string Cancelled = "Cancelled";
}

public static class SupplierMembershipBenefitTypeCodes
{
    public const string PercentageDiscount = "PercentageDiscount";
    public const string FixedAmountDiscount = "FixedAmountDiscount";
}

public sealed class SupplierInterestSubscriptionDraftRequest
{
    public string SupplierKey { get; set; } = string.Empty;

    public string SupplierDisplayName { get; set; } = string.Empty;

    public string SupplierPartyTypeCode { get; set; } =
        SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness;

    public string AudienceTypeCode { get; set; } =
        SupplierRelationshipAudienceTypeCodes.IndividualOrderer;

    public string? DeliveryScopeKey { get; set; }

    public IReadOnlyList<string> InterestedProductTags { get; set; } = [];

    public bool ReceiveSupplierUpdates { get; set; }

    public bool CurrentMemberConsentConfirmed { get; set; }

    public string TermsVersion { get; set; } = string.Empty;
}

public sealed class SupplierInterestSubscriptionDraftResponse
{
    public Guid DraftId { get; set; }

    public string OwnerUserId { get; set; } = string.Empty;

    public string SupplierKey { get; set; } = string.Empty;

    public string SupplierDisplayName { get; set; } = string.Empty;

    public string SupplierPartyTypeCode { get; set; } = string.Empty;

    public string AudienceTypeCode { get; set; } = string.Empty;

    public string? DeliveryScopeKey { get; set; }

    public IReadOnlyList<string> InterestedProductTags { get; set; } = [];

    public bool ReceiveSupplierUpdates { get; set; }

    public string StatusCode { get; set; } =
        SupplierMembershipStatusCodes.InterestFollowing;

    public string TermsVersion { get; set; } = string.Empty;

    public bool PaymentRequired { get; set; }

    public bool SupplierContactDetailsDisclosed { get; set; }

    public bool MembershipActivated { get; set; }

    public bool IsDurablyPersisted { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string GuidanceMessage { get; set; } = string.Empty;
}

/// <summary>
/// 농업경영체 또는 해외 제조업체가 제안한 멤버십 혜택을 주문 전에 비교하기 위한 요청입니다.
/// 이 계약은 구독료 결제, 주문 확정 또는 배송 실행을 일으키지 않습니다.
/// </summary>
public sealed class SupplierMembershipBenefitPreviewRequest
{
    public string SupplierKey { get; set; } = string.Empty;

    public string SupplierDisplayName { get; set; } = string.Empty;

    public string SupplierPartyTypeCode { get; set; } =
        SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness;

    public string AudienceTypeCode { get; set; } =
        SupplierRelationshipAudienceTypeCodes.IndividualOrderer;

    public string? DeliveryScopeKey { get; set; }

    public string MembershipStatusCode { get; set; } =
        SupplierMembershipStatusCodes.InterestFollowing;

    public string BenefitTypeCode { get; set; } =
        SupplierMembershipBenefitTypeCodes.PercentageDiscount;

    public decimal MonthlyFeeAmount { get; set; }

    public string CurrencyCode { get; set; } = "KRW";

    public decimal DiscountRatePercent { get; set; }

    public decimal FixedDiscountAmount { get; set; }

    public decimal? MaximumDiscountAmount { get; set; }

    public decimal OrderSubtotalAmount { get; set; }

    public bool ProductEligible { get; set; }

    public bool SupplierBenefitOfferConfirmed { get; set; }

    public bool SupplierEvidenceVerified { get; set; }

    public string TermsVersion { get; set; } = string.Empty;
}

public sealed class SupplierMembershipBenefitPreviewResponse
{
    public string SupplierKey { get; set; } = string.Empty;

    public string SupplierDisplayName { get; set; } = string.Empty;

    public string SupplierPartyTypeCode { get; set; } = string.Empty;

    public string AudienceTypeCode { get; set; } = string.Empty;

    public string MembershipStatusCode { get; set; } = string.Empty;

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal MonthlyFeeAmount { get; set; }

    public decimal OrderSubtotalAmount { get; set; }

    public decimal PotentialDiscountAmount { get; set; }

    public decimal AppliedDiscountAmount { get; set; }

    public decimal EstimatedOrderAmount { get; set; }

    public decimal PotentialNetBenefitAfterMonthlyFee { get; set; }

    public int? EstimatedOrdersToBreakEven { get; set; }

    public bool BenefitOfferEligible { get; set; }

    public bool BenefitApplied { get; set; }

    public bool RequiresMembershipActivation { get; set; }

    public bool RequiresSeparateMembershipConsent { get; set; } = true;

    public bool RequiresSeparateOrderConsent { get; set; } = true;

    public bool RequiresEachGroupMemberConsent { get; set; }

    public bool MembershipChargeExecutionAllowed { get; set; }

    public bool OrderExecutionAllowed { get; set; }

    public string TermsVersion { get; set; } = string.Empty;

    public string BenefitReason { get; set; } = string.Empty;

    public string GuidanceMessage { get; set; } = string.Empty;
}
