using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Contracts.Common.ContractManagement;

public static class 수입식품공동주문계약역할코드
{
    public const string ApplicantOrderer = "ApplicantOrderer";
    public const string SupplierOrShipper = "SupplierOrShipper";
    public const string PlatformOperator = "PlatformOperator";
    public const string WarehouseOperator = "WarehouseOperator";
    public const string CustomsBroker = "CustomsBroker";
}

public static class 수입식품공동주문계약상태코드
{
    public const string 초안 = "초안";
    public const string Blocked = "Blocked";
    public const string PendingReview = "PendingReview";
    public const string ReadyToSign = "ReadyToSign";
    public const string Signed = "Signed";
}

public sealed record 수입식품공동주문계약당사자(
    string PartyId,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "수입 식품 공동 주문 계약 당사자 식별",
        IsContractData = true,
        ProtectionNote = "계약 목록에서는 역할명과 표시명 중심으로 노출하고 상세 당사자 정보는 권한이 있는 사용자에게만 제공")]
    string DisplayName,
    string RoleCode,
    bool IsRequiredSigner = true);

public sealed record 수입식품공동주문계약보호프로필(
    bool HasPurposeAndLegalBasis = false,
    bool HasDataMinimization = false,
    bool HasRetentionAndDestructionRule = false,
    bool HasConsentOrNotice = false,
    bool HasRoleBasedAccessControl = false,
    bool HasMaskingOrEncryption = false,
    bool HasAuditLog = false,
    bool HasThirdPartyOrOutsourcingReview = false,
    bool HasIncidentResponseOwner = false,
    bool HasBackupOrRecoveryPlan = false,
    bool HasSecureDevelopmentReview = false)
{
    public static 수입식품공동주문계약보호프로필 AllReviewed()
        => new(
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
            HasSecureDevelopmentReview: true);
}

public sealed record 수입식품공동주문계약초안(
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.ContractDocument,
        "계약 문서 식별과 서명/검토 상태 추적",
        IsPersonalData = false,
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem,
        ProtectionNote = "문서번호는 계약 상태와 함께 감사 로그 대상")]
    string ContractNumber,
    string GroupPurchaseId,
    HS먹거리공동구매상품카드 상품카드,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.OrdererGroupScope,
        "주문자 집단 범위 확인",
        IsContractData = true,
        ProtectionNote = "집단 내부 상세 거주 단서가 노출되지 않도록 승인된 범위 키만 사용")]
    string 주문자집단배송권키,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.OrdererGroupScope,
        "주문자 집단 표시",
        IsContractData = true,
        ProtectionNote = "다른 주문자에게는 집단 표시명만 공개하고 상세 주소는 분배 단계까지 지연")]
    string 주문자집단배송권명,
    decimal TargetQuantityKg,
    decimal EstimatedUnitPrice,
    IReadOnlyList<수입식품공동주문계약당사자> Parties,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.PaymentMethod,
        "상차/하차/분배 확인 마일스톤 지급 조건 관리",
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem,
        ProtectionNote = "지급 조건 변경과 조회는 감사 로그 대상")]
    공동구매결제단계정책 PaymentPolicy,
    bool HasNonBindingDemandNotice,
    bool HasImportFoodReviewClause,
    bool HasColdChainHandlingClause,
    bool HasMilestonePaymentClause,
    bool HasDistributionConfirmationClause,
    bool HasRefundAndCancellationClause,
    string Currency = "KRW",
    수입식품공동주문계약보호프로필? ProtectionProfile = null,
    ContractElectronicSignatureBundle? SignatureBundle = null);

public sealed record 수입식품공동주문계약검토계획(
    수입식품공동주문계약초안 초안,
    bool IsFoodHS코드,
    bool CanProceedToReview,
    string 제안상태,
    IReadOnlyList<string> RequiredClauses,
    IReadOnlyList<string> MissingItems,
    IsmsPReadinessPlan PrivacyAndContractReadiness,
    ContractElectronicSignaturePlan? SignaturePlan,
    string 요약);

public static class 수입식품공동주문계약검토계획기
{
    public static 수입식품공동주문계약검토계획 계획(수입식품공동주문계약초안 draft)
        => 계획(draft, DateTimeOffset.UtcNow);

    public static 수입식품공동주문계약검토계획 계획(
        수입식품공동주문계약초안 draft,
        DateTimeOffset 평가시각Utc)
    {
        Validate(draft);

        var isFoodHS코드 = IsHsFoodCandidate(draft.상품카드.HS코드);
        var requiredClauses = ResolveRequiredClauses(draft);
        var missingItems = ResolveMissingItems(draft, isFoodHS코드);
        var privacyAndContractReadiness = BuildPrivacyAndContractReadiness(draft, missingItems);
        var signaturePlan = draft.SignatureBundle is null
            ? null
            : ContractElectronicSignaturePlanner.Plan(draft.SignatureBundle, 평가시각Utc);
        var canProceedToReview = isFoodHS코드 &&
            HasRole(draft, 수입식품공동주문계약역할코드.ApplicantOrderer) &&
            HasRole(draft, 수입식품공동주문계약역할코드.PlatformOperator) &&
            draft.HasNonBindingDemandNotice;
        var status = ResolveStatus(canProceedToReview, missingItems, privacyAndContractReadiness, signaturePlan);

        return new 수입식품공동주문계약검토계획(
            draft,
            isFoodHS코드,
            canProceedToReview,
            status,
            requiredClauses,
            missingItems,
            privacyAndContractReadiness,
            signaturePlan,
            Build요약(draft, status));
    }

    private static void Validate(수입식품공동주문계약초안 draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.ContractNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.GroupPurchaseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.상품카드.상품카드Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.상품카드.HS코드);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.주문자집단배송권키);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.주문자집단배송권명);

        if (draft.TargetQuantityKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.TargetQuantityKg), draft.TargetQuantityKg, "Target quantity must be greater than zero.");
        }

        if (draft.EstimatedUnitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.EstimatedUnitPrice), draft.EstimatedUnitPrice, "Estimated unit price must be greater than zero.");
        }

        if (draft.Parties.Count == 0)
        {
            throw new ArgumentException("At least one contract party is required.", nameof(draft.Parties));
        }

        NormalizePolicy(draft.PaymentPolicy);
    }

    private static IReadOnlyList<string> ResolveRequiredClauses(수입식품공동주문계약초안 draft)
    {
        var clauses = new List<string>
        {
            "비구속 수요 확인 고지",
            "HS 식품 코드와 수입식품 검토",
            "마일스톤 지급 조건",
            "분배 확인 기준",
            "환불/취소/미달성 처리"
        };

        if (draft.상품카드.온도코드 is 공동구매온도코드.냉동 or 공동구매온도코드.냉장)
        {
            clauses.Add("냉장/냉동 보관 및 운송 조건");
        }

        return clauses;
    }

    private static IReadOnlyList<string> ResolveMissingItems(
        수입식품공동주문계약초안 draft,
        bool isFoodHS코드)
    {
        var missing = new List<string>();

        if (!isFoodHS코드)
        {
            missing.Add("HS 식품 코드 확인");
        }

        if (!HasRole(draft, 수입식품공동주문계약역할코드.ApplicantOrderer))
        {
            missing.Add("개설 신청 주문자 당사자");
        }

        if (!HasRole(draft, 수입식품공동주문계약역할코드.SupplierOrShipper))
        {
            missing.Add("공급자 또는 화주 당사자");
        }

        if (!HasRole(draft, 수입식품공동주문계약역할코드.PlatformOperator))
        {
            missing.Add("플랫폼 운영자 당사자");
        }

        if (!draft.HasNonBindingDemandNotice)
        {
            missing.Add("비구속 수요 확인 고지 조항");
        }

        if (!draft.HasImportFoodReviewClause)
        {
            missing.Add("수입식품 신고/검역 검토 조항");
        }

        if (draft.상품카드.온도코드 is 공동구매온도코드.냉동 or 공동구매온도코드.냉장 &&
            !draft.HasColdChainHandlingClause)
        {
            missing.Add("냉장/냉동 보관 및 운송 조항");
        }

        if (!draft.HasMilestonePaymentClause)
        {
            missing.Add("상차/하차/분배 확인 마일스톤 지급 조항");
        }

        if (!draft.HasDistributionConfirmationClause)
        {
            missing.Add("분배 확인율 기준 조항");
        }

        if (!draft.HasRefundAndCancellationClause)
        {
            missing.Add("환불/취소/목표 미달성 처리 조항");
        }

        return missing;
    }

    private static string ResolveStatus(
        bool canProceedToReview,
        IReadOnlyList<string> missingItems,
        IsmsPReadinessPlan privacyAndContractReadiness,
        ContractElectronicSignaturePlan? signaturePlan)
    {
        if (!canProceedToReview)
        {
            return 수입식품공동주문계약상태코드.Blocked;
        }

        if (missingItems.Count == 0 &&
            privacyAndContractReadiness.IsReadyForInternalReview &&
            signaturePlan?.IsFullySigned == true)
        {
            return 수입식품공동주문계약상태코드.Signed;
        }

        return missingItems.Count == 0 && privacyAndContractReadiness.IsReadyForInternalReview
            ? 수입식품공동주문계약상태코드.ReadyToSign
            : 수입식품공동주문계약상태코드.PendingReview;
    }

    private static IsmsPReadinessPlan BuildPrivacyAndContractReadiness(
        수입식품공동주문계약초안 draft,
        IReadOnlyList<string> missingItems)
    {
        var protection = draft.ProtectionProfile ?? new 수입식품공동주문계약보호프로필();

        return IsmsPReadinessPlanner.Plan(new PersonalDataContractFeatureProfile(
            FeatureName: "수입 식품 공동 주문 계약서",
            Owner: "플랫폼 운영자",
            ProcessesPersonalData: true,
            ProcessesContractData: true,
            HasPurposeAndLegalBasis: protection.HasPurposeAndLegalBasis,
            HasDataMinimization: protection.HasDataMinimization,
            HasRetentionAndDestructionRule: protection.HasRetentionAndDestructionRule,
            HasConsentOrNotice: protection.HasConsentOrNotice,
            HasRoleBasedAccessControl: protection.HasRoleBasedAccessControl,
            HasMaskingOrEncryption: protection.HasMaskingOrEncryption,
            HasAuditLog: protection.HasAuditLog,
            HasThirdPartyOrOutsourcingReview: protection.HasThirdPartyOrOutsourcingReview,
            HasIncidentResponseOwner: protection.HasIncidentResponseOwner,
            HasBackupOrRecoveryPlan: protection.HasBackupOrRecoveryPlan,
            HasSecureDevelopmentReview: protection.HasSecureDevelopmentReview,
            HasContractTermsReview: missingItems.Count == 0,
            PersonalDataFieldKeys: ResolvePersonalDataFieldKeys()));
    }

    private static IReadOnlyList<string> ResolvePersonalDataFieldKeys()
        =>
        [
            PersonalDataFieldKey.DisplayName,
            PersonalDataFieldKey.PhoneNumber,
            PersonalDataFieldKey.OrdererGroupScope,
            PersonalDataFieldKey.DetailedAddress,
            PersonalDataFieldKey.PaymentMethod,
            PersonalDataFieldKey.ContractDocument,
            PersonalDataFieldKey.ElectronicSignatureEvidence,
            PersonalDataFieldKey.CustomsClearanceReference
        ];

    private static bool HasRole(수입식품공동주문계약초안 draft, string roleCode)
        => draft.Parties.Any(x => string.Equals(x.RoleCode, roleCode, StringComparison.OrdinalIgnoreCase));

    private static bool IsHsFoodCandidate(string? hsCode)
    {
        var digits = new string((hsCode ?? string.Empty).Where(char.IsDigit).Take(2).ToArray());
        return digits.Length == 2 &&
            int.TryParse(digits, out var chapter) &&
            chapter is >= 1 and <= 24;
    }

    private static void NormalizePolicy(공동구매결제단계정책 policy)
    {
        var totalRate = policy.상차1차지급비율 +
            policy.하차2차지급비율 +
            policy.분배최종지급비율;
        if (totalRate != 1m)
        {
            throw new ArgumentException("Payment milestone rates must sum to 1.", nameof(policy));
        }

        if (policy.분배확인기준비율 is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(policy.분배확인기준비율), policy.분배확인기준비율, "Distribution confirmation threshold must be between 0 and 1.");
        }
    }

    private static string Build요약(
        수입식품공동주문계약초안 draft,
        string status)
        => $"{draft.주문자집단배송권명} {draft.상품카드.상품명} 수입 식품 공동 주문 계약서: {status}";
}
