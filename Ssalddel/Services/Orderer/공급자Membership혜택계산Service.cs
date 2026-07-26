using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface I공급자Membership혜택계산Service
{
    SupplierMembershipBenefitPreviewResponse 미리보기(
        SupplierMembershipBenefitPreviewRequest request);
}

/// <summary>
/// 공급자가 제시한 구독료와 혜택을 주문 금액과 비교합니다.
/// 계산 결과는 설명용 미리보기이며 구독료 결제나 주문 확정을 수행하지 않습니다.
/// </summary>
public sealed class 공급자Membership혜택계산Service : I공급자Membership혜택계산Service
{
    public SupplierMembershipBenefitPreviewResponse 미리보기(
        SupplierMembershipBenefitPreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SupplierKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SupplierDisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CurrencyCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TermsVersion);

        if (request.MonthlyFeeAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.MonthlyFeeAmount),
                "월 구독료는 0 이상이어야 합니다.");
        }

        if (request.OrderSubtotalAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.OrderSubtotalAmount),
                "주문 상품 금액은 0 이상이어야 합니다.");
        }

        if (request.DiscountRatePercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.DiscountRatePercent),
                "할인율은 0% 이상 100% 이하여야 합니다.");
        }

        if (request.FixedDiscountAmount < 0 ||
            request.MaximumDiscountAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.FixedDiscountAmount),
                "할인 금액과 할인 한도는 0 이상이어야 합니다.");
        }

        ValidateCodes(request);

        var benefitOfferEligible =
            request.ProductEligible &&
            request.SupplierBenefitOfferConfirmed &&
            request.SupplierEvidenceVerified;
        var potentialDiscount = benefitOfferEligible
            ? CalculatePotentialDiscount(request)
            : 0m;
        var isActive = string.Equals(
            request.MembershipStatusCode,
            SupplierMembershipStatusCodes.Active,
            StringComparison.Ordinal);
        var appliedDiscount = isActive ? potentialDiscount : 0m;
        var estimatedOrderAmount = Math.Max(
            0m,
            request.OrderSubtotalAmount - appliedDiscount);
        var potentialNetBenefit = potentialDiscount - request.MonthlyFeeAmount;

        return new SupplierMembershipBenefitPreviewResponse
        {
            SupplierKey = request.SupplierKey.Trim(),
            SupplierDisplayName = request.SupplierDisplayName.Trim(),
            SupplierPartyTypeCode = request.SupplierPartyTypeCode,
            AudienceTypeCode = request.AudienceTypeCode,
            MembershipStatusCode = request.MembershipStatusCode,
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            MonthlyFeeAmount = request.MonthlyFeeAmount,
            OrderSubtotalAmount = request.OrderSubtotalAmount,
            PotentialDiscountAmount = potentialDiscount,
            AppliedDiscountAmount = appliedDiscount,
            EstimatedOrderAmount = estimatedOrderAmount,
            PotentialNetBenefitAfterMonthlyFee = potentialNetBenefit,
            EstimatedOrdersToBreakEven = CalculateBreakEvenOrderCount(
                request.MonthlyFeeAmount,
                potentialDiscount),
            BenefitOfferEligible = benefitOfferEligible,
            BenefitApplied = appliedDiscount > 0,
            RequiresMembershipActivation = potentialDiscount > 0 && !isActive,
            RequiresEachGroupMemberConsent = string.Equals(
                request.AudienceTypeCode,
                SupplierRelationshipAudienceTypeCodes.DeliveryScopeGroup,
                StringComparison.Ordinal),
            MembershipChargeExecutionAllowed = false,
            OrderExecutionAllowed = false,
            TermsVersion = request.TermsVersion.Trim(),
            BenefitReason = ResolveBenefitReason(
                benefitOfferEligible,
                isActive,
                appliedDiscount),
            GuidanceMessage =
                "무료 관심 구독, 유료 멤버십 가입과 개별 주문 동의는 서로 분리합니다. " +
                "이 미리보기는 결제·자동 갱신·주문·배송을 실행하지 않습니다."
        };
    }

    private static decimal CalculatePotentialDiscount(
        SupplierMembershipBenefitPreviewRequest request)
    {
        var discount = request.BenefitTypeCode switch
        {
            SupplierMembershipBenefitTypeCodes.PercentageDiscount =>
                decimal.Round(
                    request.OrderSubtotalAmount * request.DiscountRatePercent / 100m,
                    2,
                    MidpointRounding.AwayFromZero),
            SupplierMembershipBenefitTypeCodes.FixedAmountDiscount =>
                request.FixedDiscountAmount,
            _ => 0m
        };

        if (request.MaximumDiscountAmount is not null)
        {
            discount = Math.Min(discount, request.MaximumDiscountAmount.Value);
        }

        return Math.Min(request.OrderSubtotalAmount, discount);
    }

    private static int? CalculateBreakEvenOrderCount(
        decimal monthlyFee,
        decimal discountPerOrder)
        => discountPerOrder <= 0
            ? null
            : (int)Math.Ceiling(monthlyFee / discountPerOrder);

    private static string ResolveBenefitReason(
        bool benefitOfferEligible,
        bool isActive,
        decimal appliedDiscount)
    {
        if (!benefitOfferEligible)
        {
            return "공급자 확인, 업체 근거와 대상 상품 조건이 모두 확인되어야 혜택을 계산할 수 있습니다.";
        }

        if (!isActive)
        {
            return "가입 전 예상 혜택입니다. 약관과 자동 갱신 여부를 확인하고 별도로 동의해야 적용됩니다.";
        }

        return appliedDiscount > 0
            ? "활성 멤버십과 대상 상품 조건이 확인되어 예상 할인에 반영했습니다."
            : "활성 멤버십이지만 이번 주문에서 적용할 할인 금액이 없습니다.";
    }

    private static void ValidateCodes(SupplierMembershipBenefitPreviewRequest request)
    {
        if (request.SupplierPartyTypeCode is not
            SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness and not
            SupplierRelationshipPartyTypeCodes.OverseasFoodManufacturer)
        {
            throw new ArgumentException(
                "지원하지 않는 공급자 유형입니다.",
                nameof(request.SupplierPartyTypeCode));
        }

        if (request.AudienceTypeCode is not
            SupplierRelationshipAudienceTypeCodes.IndividualOrderer and not
            SupplierRelationshipAudienceTypeCodes.DeliveryScopeGroup)
        {
            throw new ArgumentException(
                "지원하지 않는 구독 대상 유형입니다.",
                nameof(request.AudienceTypeCode));
        }

        if (request.MembershipStatusCode is not
            SupplierMembershipStatusCodes.InterestFollowing and not
            SupplierMembershipStatusCodes.EnrollmentDraft and not
            SupplierMembershipStatusCodes.Active and not
            SupplierMembershipStatusCodes.Paused and not
            SupplierMembershipStatusCodes.Cancelled)
        {
            throw new ArgumentException(
                "지원하지 않는 멤버십 상태입니다.",
                nameof(request.MembershipStatusCode));
        }

        if (request.BenefitTypeCode is not
            SupplierMembershipBenefitTypeCodes.PercentageDiscount and not
            SupplierMembershipBenefitTypeCodes.FixedAmountDiscount)
        {
            throw new ArgumentException(
                "지원하지 않는 혜택 유형입니다.",
                nameof(request.BenefitTypeCode));
        }
    }
}
