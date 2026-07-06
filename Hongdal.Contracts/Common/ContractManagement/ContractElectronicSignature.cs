using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Contracts.Common.ContractManagement;

public static class ContractSignatureMethodCode
{
    public const string PlatformClickSign = "PlatformClickSign";
    public const string ExternalProvider = "ExternalProvider";
    public const string CertificateBased = "CertificateBased";
    public const string ManualAdminRecorded = "ManualAdminRecorded";
}

public static class ContractSignatureStatusCode
{
    public const string Draft = "Draft";
    public const string WaitingForSignature = "WaitingForSignature";
    public const string PartiallySigned = "PartiallySigned";
    public const string Signed = "Signed";
    public const string Expired = "Expired";
    public const string InvalidEvidence = "InvalidEvidence";
}

public sealed record ContractSignatureRequest(
    string PartyId,
    string RoleCode,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "계약 전자서명 요청 대상 표시",
        IsContractData = true,
        ProtectionNote = "서명 요청 목록에는 표시명과 역할 중심으로 노출")]
    string SignerDisplayName,
    bool IsRequiredSigner = true,
    DateTimeOffset? RequestedAtUtc = null);

public sealed record ContractSignatureEvidence(
    string PartyId,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "계약 전자서명자 식별",
        IsContractData = true,
        ProtectionNote = "서명자 표시는 계약 당사자와 운영자 범위로 제한")]
    string SignerDisplayName,
    string SignatureMethodCode,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.ContractDocument,
        "전자서명 대상 문서 해시",
        IsPersonalData = false,
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem,
        ProtectionNote = "서명 당시의 계약 문서 해시와 현재 문서 해시가 일치해야 함")]
    string SignedDocumentHash,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.ElectronicSignatureEvidence,
        "전자서명 동의문 해시",
        IsContractData = true,
        ProtectionNote = "원문 동의문은 계약 문서에 보관하고 증적에는 해시를 남김")]
    string ConsentTextHash,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.ElectronicSignatureEvidence,
        "전자서명 증적 해시",
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ProtectionSafeguards,
        ProtectionNote = "서명 이벤트 원본 증적은 외부 저장소 또는 보호 저장소에 두고 해시로 검증")]
    string SignatureEvidenceHash,
    DateTimeOffset SignedAtUtc,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.IpAddress,
        "전자서명 접속 IP 해시",
        DomainCode = IsmsPDomainCode.ProtectionSafeguards,
        ProtectionNote = "원본 IP 대신 해시 또는 마스킹 값을 기본 보관")]
    string? ClientIpHash = null);

public sealed record ContractElectronicSignatureBundle(
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.ContractDocument,
        "전자서명 대상 계약 문서번호",
        IsPersonalData = false,
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem)]
    string ContractNumber,
    [property: IsmsPProtectedData(
        PersonalDataFieldKey.ContractDocument,
        "전자서명 대상 계약 문서 해시",
        IsPersonalData = false,
        IsContractData = true,
        DomainCode = IsmsPDomainCode.ManagementSystem)]
    string DocumentHash,
    IReadOnlyList<ContractSignatureRequest> SignatureRequests,
    IReadOnlyList<ContractSignatureEvidence> Evidences,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc = null);

public sealed record ContractElectronicSignaturePlan(
    ContractElectronicSignatureBundle Bundle,
    string StatusCode,
    int RequiredSignerCount,
    int SignedRequiredSignerCount,
    IReadOnlyList<string> MissingRequiredPartyIds,
    IReadOnlyList<string> InvalidEvidencePartyIds,
    bool IsFullySigned,
    string Summary);

public static class ContractElectronicSignaturePlanner
{
    public static ContractElectronicSignatureBundle CreateBundle(
        string contractNumber,
        string documentHash,
        IEnumerable<ContractSignatureRequest> signatureRequests,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentHash);
        ArgumentNullException.ThrowIfNull(signatureRequests);

        var requests = signatureRequests.ToArray();
        if (requests.Length == 0)
        {
            throw new ArgumentException("At least one signature request is required.", nameof(signatureRequests));
        }

        ValidateRequests(requests);

        return new ContractElectronicSignatureBundle(
            contractNumber.Trim(),
            documentHash.Trim(),
            requests,
            [],
            createdAtUtc,
            expiresAtUtc);
    }

    public static ContractElectronicSignatureBundle CreateBundleFromParties(
        string contractNumber,
        string documentHash,
        IEnumerable<ImportFoodGroupPurchaseContractParty> parties,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(parties);

        var requests = parties
            .Where(x => x.IsRequiredSigner)
            .Select(x => new ContractSignatureRequest(
                x.PartyId,
                x.RoleCode,
                x.DisplayName,
                x.IsRequiredSigner,
                createdAtUtc))
            .ToArray();

        return CreateBundle(contractNumber, documentHash, requests, createdAtUtc, expiresAtUtc);
    }

    public static ContractElectronicSignatureBundle AddEvidence(
        ContractElectronicSignatureBundle bundle,
        ContractSignatureEvidence evidence)
    {
        ValidateBundle(bundle);
        ValidateEvidence(evidence);

        var evidences = bundle.Evidences
            .Where(x => !string.Equals(x.PartyId, evidence.PartyId, StringComparison.OrdinalIgnoreCase))
            .Append(evidence)
            .OrderBy(x => x.SignedAtUtc)
            .ToArray();

        return bundle with { Evidences = evidences };
    }

    public static ContractElectronicSignaturePlan Plan(
        ContractElectronicSignatureBundle bundle,
        DateTimeOffset nowUtc)
    {
        ValidateBundle(bundle);

        var requiredRequests = bundle.SignatureRequests
            .Where(x => x.IsRequiredSigner)
            .ToArray();
        var invalidEvidencePartyIds = ResolveInvalidEvidencePartyIds(bundle).ToArray();
        var signedRequiredPartyIds = bundle.Evidences
            .Where(x => !invalidEvidencePartyIds.Contains(x.PartyId, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.PartyId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var missingRequiredPartyIds = requiredRequests
            .Where(x => !signedRequiredPartyIds.Contains(x.PartyId, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.PartyId)
            .ToArray();
        var isExpired = bundle.ExpiresAtUtc is not null && nowUtc > bundle.ExpiresAtUtc.Value;
        var isFullySigned = !isExpired &&
            invalidEvidencePartyIds.Length == 0 &&
            missingRequiredPartyIds.Length == 0;
        var status = ResolveStatus(
            bundle,
            isExpired,
            invalidEvidencePartyIds.Length,
            missingRequiredPartyIds.Length,
            signedRequiredPartyIds.Length);

        return new ContractElectronicSignaturePlan(
            bundle,
            status,
            requiredRequests.Length,
            signedRequiredPartyIds.Length,
            missingRequiredPartyIds,
            invalidEvidencePartyIds,
            isFullySigned,
            BuildSummary(bundle, status, signedRequiredPartyIds.Length, requiredRequests.Length));
    }

    private static IEnumerable<string> ResolveInvalidEvidencePartyIds(ContractElectronicSignatureBundle bundle)
    {
        var requestPartyIds = bundle.SignatureRequests
            .Select(x => x.PartyId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var evidence in bundle.Evidences)
        {
            if (!requestPartyIds.Contains(evidence.PartyId) ||
                !string.Equals(evidence.SignedDocumentHash, bundle.DocumentHash, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(evidence.SignatureMethodCode) ||
                string.IsNullOrWhiteSpace(evidence.ConsentTextHash) ||
                string.IsNullOrWhiteSpace(evidence.SignatureEvidenceHash))
            {
                yield return evidence.PartyId;
            }
        }
    }

    private static string ResolveStatus(
        ContractElectronicSignatureBundle bundle,
        bool isExpired,
        int invalidEvidenceCount,
        int missingRequiredCount,
        int signedRequiredCount)
    {
        if (isExpired)
        {
            return ContractSignatureStatusCode.Expired;
        }

        if (invalidEvidenceCount > 0)
        {
            return ContractSignatureStatusCode.InvalidEvidence;
        }

        if (missingRequiredCount == 0)
        {
            return ContractSignatureStatusCode.Signed;
        }

        if (signedRequiredCount > 0)
        {
            return ContractSignatureStatusCode.PartiallySigned;
        }

        return bundle.SignatureRequests.Count == 0
            ? ContractSignatureStatusCode.Draft
            : ContractSignatureStatusCode.WaitingForSignature;
    }

    private static void ValidateBundle(ContractElectronicSignatureBundle bundle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bundle.ContractNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(bundle.DocumentHash);

        if (bundle.SignatureRequests.Count == 0)
        {
            throw new ArgumentException("At least one signature request is required.", nameof(bundle));
        }

        ValidateRequests(bundle.SignatureRequests);

        foreach (var evidence in bundle.Evidences)
        {
            ValidateEvidence(evidence);
        }
    }

    private static void ValidateRequests(IReadOnlyList<ContractSignatureRequest> requests)
    {
        foreach (var request in requests)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.PartyId);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.RoleCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.SignerDisplayName);
        }

        var duplicatePartyId = requests
            .GroupBy(x => x.PartyId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicatePartyId is not null)
        {
            throw new ArgumentException($"Duplicate signer party id: {duplicatePartyId.Key}", nameof(requests));
        }
    }

    private static void ValidateEvidence(ContractSignatureEvidence evidence)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(evidence.PartyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidence.SignerDisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidence.SignatureMethodCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidence.SignedDocumentHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidence.ConsentTextHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidence.SignatureEvidenceHash);
    }

    private static string BuildSummary(
        ContractElectronicSignatureBundle bundle,
        string status,
        int signedRequiredCount,
        int requiredSignerCount)
        => $"{bundle.ContractNumber} 전자서명: {status} ({signedRequiredCount}/{requiredSignerCount})";
}
