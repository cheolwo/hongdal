namespace Hongdal.Contracts.Common.Privacy;

public static class IsmsPDomainCode
{
    public const string ManagementSystem = "1";
    public const string ProtectionSafeguards = "2";
    public const string PrivacyLifecycle = "3";
}

public static class IsmsPReadinessControlCode
{
    public const string GovernanceOwner = "M-01";
    public const string ContractTermsReview = "C-01";
    public const string PurposeAndLegalBasis = "P-01";
    public const string DataMinimization = "P-02";
    public const string RetentionAndDestruction = "P-03";
    public const string ConsentOrNotice = "P-04";
    public const string PersonalDataFieldCatalog = "P-05";
    public const string RoleBasedAccessControl = "S-01";
    public const string MaskingOrEncryption = "S-02";
    public const string AuditLog = "S-03";
    public const string ThirdPartyOrOutsourcingReview = "S-04";
    public const string IncidentResponseOwner = "S-05";
    public const string SecureDevelopmentReview = "S-06";
    public const string BackupOrRecoveryPlan = "S-07";
}

public sealed record PersonalDataContractFeatureProfile(
    string FeatureName,
    string Owner,
    bool ProcessesPersonalData,
    bool ProcessesContractData,
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
    bool HasSecureDevelopmentReview = false,
    bool HasContractTermsReview = false,
    IReadOnlyList<string>? PersonalDataFieldKeys = null);

public sealed record IsmsPReadinessItem(
    string Code,
    string DomainCode,
    string Title,
    string EvidenceHint,
    bool IsRequired,
    bool IsSatisfied);

public sealed record IsmsPReadinessPlan(
    PersonalDataContractFeatureProfile Profile,
    IReadOnlyList<IsmsPReadinessItem> Items,
    int RequiredCount,
    int SatisfiedRequiredCount,
    IReadOnlyList<string> MissingRequiredCodes,
    bool IsReadyForInternalReview,
    string Summary,
    PersonalDataFieldProtectionPlan? FieldProtectionPlan = null);

public static class IsmsPReadinessPlanner
{
    public static IsmsPReadinessPlan Plan(PersonalDataContractFeatureProfile profile)
    {
        Validate(profile);

        var fieldProtectionPlan = ResolveFieldProtectionPlan(profile);
        var items = new List<IsmsPReadinessItem>
        {
            Required(
                IsmsPReadinessControlCode.GovernanceOwner,
                IsmsPDomainCode.ManagementSystem,
                "관리 책임자와 위험 검토 범위",
                "기능 책임자, 개인정보/계약 데이터 범위, 운영 리스크 검토 기록",
                true),
            Required(
                IsmsPReadinessControlCode.SecureDevelopmentReview,
                IsmsPDomainCode.ProtectionSafeguards,
                "개발 보안 검토",
                "입력 검증, 권한 체크, 로그/오류 노출, 의존성 검토 기록",
                profile.HasSecureDevelopmentReview)
        };

        if (profile.ProcessesPersonalData)
        {
            items.AddRange([
                Required(
                    IsmsPReadinessControlCode.PurposeAndLegalBasis,
                    IsmsPDomainCode.PrivacyLifecycle,
                    "개인정보 처리 목적과 법적 근거",
                    "수집 목적, 처리 근거, 필수/선택 항목 구분",
                    profile.HasPurposeAndLegalBasis),
                Required(
                    IsmsPReadinessControlCode.DataMinimization,
                    IsmsPDomainCode.PrivacyLifecycle,
                    "개인정보 최소 수집",
                    "주소, 연락처, 계좌, 거주/통관 정보의 최소 필드 정의",
                    profile.HasDataMinimization),
                Required(
                    IsmsPReadinessControlCode.RetentionAndDestruction,
                    IsmsPDomainCode.PrivacyLifecycle,
                    "보유 기간과 파기 기준",
                    "업무 완료, 정산 완료, 분쟁 보존 기간, 자동/수동 파기 증적",
                    profile.HasRetentionAndDestructionRule),
                Required(
                    IsmsPReadinessControlCode.ConsentOrNotice,
                    IsmsPDomainCode.PrivacyLifecycle,
                    "고지 또는 동의",
                    "정보주체 고지문, 선택 동의, 제3자 제공/위탁 안내",
                    profile.HasConsentOrNotice),
                Required(
                    IsmsPReadinessControlCode.PersonalDataFieldCatalog,
                    IsmsPDomainCode.PrivacyLifecycle,
                    "개인정보 필드 보호 카탈로그",
                    "기능에서 다루는 개인정보 필드와 필드별 마스킹/암호화/보유/제공 기준",
                    fieldProtectionPlan is not null &&
                        fieldProtectionPlan.Rules.Count > 0 &&
                        !fieldProtectionPlan.HasUnknownFields),
                Required(
                    IsmsPReadinessControlCode.MaskingOrEncryption,
                    IsmsPDomainCode.ProtectionSafeguards,
                    "마스킹 또는 암호화",
                    "연락처, 주소, 계좌, 수령자 정보의 화면 마스킹과 저장/전송 암호화",
                    profile.HasMaskingOrEncryption),
                Required(
                    IsmsPReadinessControlCode.ThirdPartyOrOutsourcingReview,
                    IsmsPDomainCode.ProtectionSafeguards,
                    "제3자 제공과 위탁 검토",
                    "기사, 화주, 창고, 관세사, 외부 API, 클라우드 저장소 제공/위탁 범위",
                    profile.HasThirdPartyOrOutsourcingReview)
            ]);
        }

        if (profile.ProcessesContractData)
        {
            items.Add(
                Required(
                    IsmsPReadinessControlCode.ContractTermsReview,
                    IsmsPDomainCode.ManagementSystem,
                    "계약 조항 검토",
                    "당사자, 지급, 환불, 취소, 분쟁, 개인정보/위탁 조항 검토 기록",
                    profile.HasContractTermsReview));
        }

        if (profile.ProcessesPersonalData || profile.ProcessesContractData)
        {
            items.AddRange([
                Required(
                    IsmsPReadinessControlCode.RoleBasedAccessControl,
                    IsmsPDomainCode.ProtectionSafeguards,
                    "역할 기반 접근권한",
                    "화주, 기사, 주문자, 창고, 관세사, 운영자별 조회/수정 범위",
                    profile.HasRoleBasedAccessControl),
                Required(
                    IsmsPReadinessControlCode.AuditLog,
                    IsmsPDomainCode.ProtectionSafeguards,
                    "감사 로그",
                    "조회, 수정, 다운로드, 계약 상태 변경, 지급 상태 변경 로그",
                    profile.HasAuditLog),
                Required(
                    IsmsPReadinessControlCode.IncidentResponseOwner,
                    IsmsPDomainCode.ProtectionSafeguards,
                    "침해사고 대응 담당",
                    "개인정보 유출, 계약 오입력, 결제/정산 사고 대응 담당과 처리 절차",
                    profile.HasIncidentResponseOwner),
                Required(
                    IsmsPReadinessControlCode.BackupOrRecoveryPlan,
                    IsmsPDomainCode.ProtectionSafeguards,
                    "백업과 복구 기준",
                    "계약, 정산, 증빙, 개인정보 데이터 백업 주기와 복구 점검 기록",
                    profile.HasBackupOrRecoveryPlan)
            ]);
        }

        var requiredItems = items.Where(x => x.IsRequired).ToArray();
        var satisfiedRequiredCount = requiredItems.Count(x => x.IsSatisfied);
        var missingRequiredCodes = requiredItems
            .Where(x => !x.IsSatisfied)
            .Select(x => x.Code)
            .ToArray();
        var isReady = missingRequiredCodes.Length == 0;

        return new IsmsPReadinessPlan(
            profile,
            items,
            requiredItems.Length,
            satisfiedRequiredCount,
            missingRequiredCodes,
            isReady,
            BuildSummary(profile, requiredItems.Length, satisfiedRequiredCount, isReady),
            fieldProtectionPlan);
    }

    private static PersonalDataFieldProtectionPlan? ResolveFieldProtectionPlan(
        PersonalDataContractFeatureProfile profile)
    {
        if (!profile.ProcessesPersonalData)
        {
            return null;
        }

        return profile.PersonalDataFieldKeys is null
            ? null
            : PersonalDataFieldProtectionCatalog.PlanFor(profile.PersonalDataFieldKeys);
    }

    private static IsmsPReadinessItem Required(
        string code,
        string domainCode,
        string title,
        string evidenceHint,
        bool isSatisfied)
        => new(code, domainCode, title, evidenceHint, true, isSatisfied);

    private static void Validate(PersonalDataContractFeatureProfile profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.FeatureName);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.Owner);

        if (!profile.ProcessesPersonalData && !profile.ProcessesContractData)
        {
            throw new ArgumentException("At least one of personal data or contract data must be processed.", nameof(profile));
        }
    }

    private static string BuildSummary(
        PersonalDataContractFeatureProfile profile,
        int requiredCount,
        int satisfiedRequiredCount,
        bool isReady)
    {
        var status = isReady ? "내부 검토 준비 완료" : "보완 필요";
        return $"{profile.FeatureName}: {status} ({satisfiedRequiredCount}/{requiredCount})";
    }
}
