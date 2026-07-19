namespace Ssalddel.Contracts.Common.Privacy;

public static class PersonalDataFieldKey
{
    public const string DisplayName = "display-name";
    public const string PhoneNumber = "phone-number";
    public const string Email = "email";
    public const string RoadAddressLevel2 = "road-address-level-2";
    public const string DetailedAddress = "detailed-address";
    public const string OrdererGroupScope = "orderer-group-scope";
    public const string BankAccountNumber = "bank-account-number";
    public const string PaymentMethod = "payment-method";
    public const string LocationCoordinate = "location-coordinate";
    public const string IpAddress = "ip-address";
    public const string WorkSchedule = "work-schedule";
    public const string DeliveryCompletionPhoto = "delivery-completion-photo";
    public const string ContractDocument = "contract-document";
    public const string ElectronicSignatureEvidence = "electronic-signature-evidence";
    public const string CustomsClearanceReference = "customs-clearance-reference";
}

public static class PersonalDataFieldCategoryCode
{
    public const string Identifier = "Identifier";
    public const string Contact = "Contact";
    public const string Address = "Address";
    public const string Payment = "Payment";
    public const string Location = "Location";
    public const string Workforce = "Workforce";
    public const string Evidence = "Evidence";
    public const string Contract = "Contract";
    public const string Customs = "Customs";
}

public static class PersonalDataSensitivityCode
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Restricted = "Restricted";
}

public static class PersonalDataStorageProtectionCode
{
    public const string ClassifiedOnly = "ClassifiedOnly";
    public const string EncryptAtRest = "EncryptAtRest";
    public const string HashForEvidence = "HashForEvidence";
}

public static class PersonalDataProtectionActionCode
{
    public const string PurposeLimitedCollection = "PurposeLimitedCollection";
    public const string ConsentOrNotice = "ConsentOrNotice";
    public const string MaskByDefault = "MaskByDefault";
    public const string EncryptAtRest = "EncryptAtRest";
    public const string EncryptInTransit = "EncryptInTransit";
    public const string HashForEvidence = "HashForEvidence";
    public const string RoleBasedAccess = "RoleBasedAccess";
    public const string AuditOnAccess = "AuditOnAccess";
    public const string RetentionRuleRequired = "RetentionRuleRequired";
    public const string ThirdPartyOrOutsourcingReview = "ThirdPartyOrOutsourcingReview";
}

public sealed record PersonalDataFieldProtectionRule(
    string FieldKey,
    string DisplayName,
    string CategoryCode,
    string SensitivityCode,
    string StorageProtectionCode,
    IReadOnlyList<string> RequiredActionCodes,
    string DefaultMaskingHint,
    string RetentionHint,
    string SharingHint);

public sealed record PersonalDataFieldProtectionPlan(
    IReadOnlyList<PersonalDataFieldProtectionRule> Rules,
    IReadOnlyList<string> UnknownFieldKeys,
    IReadOnlyList<string> RequiredActionCodes,
    bool HasUnknownFields,
    string Summary,
    bool RequiresTransportEncryption,
    bool RequiresAtRestEncryption,
    bool RequiresEvidenceHash);

public static class PersonalDataFieldProtectionCatalog
{
    private static readonly PersonalDataFieldProtectionRule[] DefaultRules =
    [
        Rule(
            PersonalDataFieldKey.DisplayName,
            "표시 이름",
            PersonalDataFieldCategoryCode.Identifier,
            PersonalDataSensitivityCode.Low,
            PersonalDataStorageProtectionCode.ClassifiedOnly,
            [PersonalDataProtectionActionCode.PurposeLimitedCollection, PersonalDataProtectionActionCode.RoleBasedAccess],
            "커뮤니티 닉네임 또는 역할명 우선 표시",
            "회원 탈퇴 또는 관계 기록 보존 기간까지",
            "외부 공개 시 실명 대신 표시명 사용"),
        Rule(
            PersonalDataFieldKey.PhoneNumber,
            "연락처",
            PersonalDataFieldCategoryCode.Contact,
            PersonalDataSensitivityCode.High,
            PersonalDataStorageProtectionCode.EncryptAtRest,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptAtRest,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "010-****-1234",
            "운송/정산/분쟁 보존 기간 이후 파기",
            "기사, 화주, 창고에는 업무 진행에 필요한 시점과 범위만 제공"),
        Rule(
            PersonalDataFieldKey.Email,
            "이메일",
            PersonalDataFieldCategoryCode.Contact,
            PersonalDataSensitivityCode.Medium,
            PersonalDataStorageProtectionCode.ClassifiedOnly,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired
            ],
            "u***@example.com",
            "회원 탈퇴 또는 계약/정산 보존 기간까지",
            "알림/인증/계약 고지 목적 외 제공 금지"),
        Rule(
            PersonalDataFieldKey.RoadAddressLevel2,
            "도로명주소 2단계",
            PersonalDataFieldCategoryCode.Address,
            PersonalDataSensitivityCode.Medium,
            PersonalDataStorageProtectionCode.ClassifiedOnly,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired
            ],
            "시/군/구 단위만 표시",
            "집단 후보 판정 목적 종료 또는 사용자가 소속 해제할 때까지",
            "공동 주문 모집권 표시에는 상세주소를 포함하지 않음"),
        Rule(
            PersonalDataFieldKey.DetailedAddress,
            "상세 주소",
            PersonalDataFieldCategoryCode.Address,
            PersonalDataSensitivityCode.Restricted,
            PersonalDataStorageProtectionCode.EncryptAtRest,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptAtRest,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "경기 수원시 ***동",
            "운송/분배 완료와 분쟁 보존 기간 이후 파기",
            "상차/하차/분배 직전의 담당자에게만 단계적 제공"),
        Rule(
            PersonalDataFieldKey.OrdererGroupScope,
            "주문자 집단 범위",
            PersonalDataFieldCategoryCode.Address,
            PersonalDataSensitivityCode.High,
            PersonalDataStorageProtectionCode.ClassifiedOnly,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired
            ],
            "지역 2단계 또는 승인된 집단명만 표시",
            "집단 소속 해제 또는 공동 주문 종료 이후 파기/익명화",
            "다른 주문자에게는 상세 거주 단서가 아닌 집단 표시명만 공개"),
        Rule(
            PersonalDataFieldKey.BankAccountNumber,
            "계좌번호",
            PersonalDataFieldCategoryCode.Payment,
            PersonalDataSensitivityCode.Restricted,
            PersonalDataStorageProtectionCode.EncryptAtRest,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptAtRest,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "국민 ****1234",
            "정산 완료와 법정 증빙 보존 기간 이후 파기",
            "정산/환불 처리자와 결제대행 범위로 제한"),
        Rule(
            PersonalDataFieldKey.PaymentMethod,
            "결제수단",
            PersonalDataFieldCategoryCode.Payment,
            PersonalDataSensitivityCode.High,
            PersonalDataStorageProtectionCode.ClassifiedOnly,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "카드/계좌/현금 등 수단만 표시",
            "정산 완료와 법정 증빙 보존 기간까지",
            "결제대행/정산 처리 목적의 제공 범위 기록"),
        Rule(
            PersonalDataFieldKey.LocationCoordinate,
            "위치 좌표",
            PersonalDataFieldCategoryCode.Location,
            PersonalDataSensitivityCode.Restricted,
            PersonalDataStorageProtectionCode.EncryptAtRest,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptAtRest,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired
            ],
            "지도 표시용 근사 위치 또는 업무 지점명",
            "운행/배차 목적 종료 후 최소 보존",
            "실시간 위치는 배차/운행 담당 범위로 제한"),
        Rule(
            PersonalDataFieldKey.IpAddress,
            "접속 IP",
            PersonalDataFieldCategoryCode.Identifier,
            PersonalDataSensitivityCode.High,
            PersonalDataStorageProtectionCode.HashForEvidence,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.HashForEvidence,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired
            ],
            "관리자 화면에는 대역 또는 마지막 옥텟 마스킹",
            "보안 로그 보존 정책에 따라 보관 후 파기",
            "보안 감사와 작업장 접근 검증 목적"),
        Rule(
            PersonalDataFieldKey.WorkSchedule,
            "근무 일정",
            PersonalDataFieldCategoryCode.Workforce,
            PersonalDataSensitivityCode.High,
            PersonalDataStorageProtectionCode.ClassifiedOnly,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired
            ],
            "근무 가능 여부와 시간대만 표시",
            "근로계약/급여/분쟁 보존 기간까지",
            "HR 담당자와 작업장 권한 검증 범위로 제한"),
        Rule(
            PersonalDataFieldKey.DeliveryCompletionPhoto,
            "상차/하차 완료 사진",
            PersonalDataFieldCategoryCode.Evidence,
            PersonalDataSensitivityCode.High,
            PersonalDataStorageProtectionCode.EncryptAtRest,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptAtRest,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "썸네일과 촬영 시각만 기본 표시",
            "정산/분쟁 보존 기간 이후 파기",
            "화주, 기사, 운영자, 분쟁 처리자에게 필요한 범위만 제공"),
        Rule(
            PersonalDataFieldKey.ContractDocument,
            "계약 문서",
            PersonalDataFieldCategoryCode.Contract,
            PersonalDataSensitivityCode.Restricted,
            PersonalDataStorageProtectionCode.EncryptAtRest,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptAtRest,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "문서번호, 상태, 당사자 역할만 목록 표시",
            "계약 종료와 법정/분쟁 보존 기간 이후 파기",
            "서명 당사자, 운영자, 법무/분쟁 처리자 범위로 제한"),
        Rule(
            PersonalDataFieldKey.ElectronicSignatureEvidence,
            "전자서명 증적",
            PersonalDataFieldCategoryCode.Contract,
            PersonalDataSensitivityCode.Restricted,
            PersonalDataStorageProtectionCode.EncryptAtRest,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptAtRest,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "서명 완료 여부, 시각, 방법, 증적 해시만 기본 표시",
            "계약 종료와 법정/분쟁 보존 기간 이후 파기",
            "서명 당사자, 운영자, 법무/분쟁 처리자 범위로 제한"),
        Rule(
            PersonalDataFieldKey.CustomsClearanceReference,
            "통관 참조 정보",
            PersonalDataFieldCategoryCode.Customs,
            PersonalDataSensitivityCode.High,
            PersonalDataStorageProtectionCode.ClassifiedOnly,
            [
                PersonalDataProtectionActionCode.PurposeLimitedCollection,
                PersonalDataProtectionActionCode.ConsentOrNotice,
                PersonalDataProtectionActionCode.MaskByDefault,
                PersonalDataProtectionActionCode.EncryptInTransit,
                PersonalDataProtectionActionCode.RoleBasedAccess,
                PersonalDataProtectionActionCode.AuditOnAccess,
                PersonalDataProtectionActionCode.RetentionRuleRequired,
                PersonalDataProtectionActionCode.ThirdPartyOrOutsourcingReview
            ],
            "HS 코드와 진행 상태 중심 표시",
            "통관/수입/분쟁 보존 기간 이후 파기 또는 익명화",
            "관세사, 화주, 운영자에게 검토 목적 범위로 제한")
    ];

    public static IReadOnlyList<PersonalDataFieldProtectionRule> All()
        => DefaultRules;

    public static PersonalDataFieldProtectionRule? Find(string? fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            return null;
        }

        return DefaultRules.FirstOrDefault(x =>
            string.Equals(x.FieldKey, fieldKey.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static PersonalDataFieldProtectionPlan PlanFor(IEnumerable<string> fieldKeys)
    {
        ArgumentNullException.ThrowIfNull(fieldKeys);

        var normalizedKeys = fieldKeys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var rules = new List<PersonalDataFieldProtectionRule>();
        var unknown = new List<string>();

        foreach (var fieldKey in normalizedKeys)
        {
            var rule = Find(fieldKey);
            if (rule is null)
            {
                unknown.Add(fieldKey);
                continue;
            }

            rules.Add(rule);
        }

        var actionCodes = rules
            .SelectMany(x => x.RequiredActionCodes)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var requiresTransportEncryption = actionCodes.Contains(
            PersonalDataProtectionActionCode.EncryptInTransit,
            StringComparer.OrdinalIgnoreCase);
        var requiresAtRestEncryption = rules.Any(x =>
            string.Equals(x.StorageProtectionCode, PersonalDataStorageProtectionCode.EncryptAtRest, StringComparison.OrdinalIgnoreCase));
        var requiresEvidenceHash = rules.Any(x =>
            string.Equals(x.StorageProtectionCode, PersonalDataStorageProtectionCode.HashForEvidence, StringComparison.OrdinalIgnoreCase));

        return new PersonalDataFieldProtectionPlan(
            rules,
            unknown,
            actionCodes,
            unknown.Count > 0,
            BuildSummary(rules.Count, unknown.Count, actionCodes.Length),
            requiresTransportEncryption,
            requiresAtRestEncryption,
            requiresEvidenceHash);
    }

    private static PersonalDataFieldProtectionRule Rule(
        string fieldKey,
        string displayName,
        string categoryCode,
        string sensitivityCode,
        string storageProtectionCode,
        IReadOnlyList<string> requiredActionCodes,
        string defaultMaskingHint,
        string retentionHint,
        string sharingHint)
        => new(
            fieldKey,
            displayName,
            categoryCode,
            sensitivityCode,
            storageProtectionCode,
            requiredActionCodes,
            defaultMaskingHint,
            retentionHint,
            sharingHint);

    private static string BuildSummary(int ruleCount, int unknownCount, int actionCount)
    {
        if (unknownCount > 0)
        {
            return $"알 수 없는 개인정보 필드 {unknownCount}개 보완 필요";
        }

        return $"개인정보 필드 {ruleCount}개, 보호 조치 {actionCount}개 확인";
    }
}
