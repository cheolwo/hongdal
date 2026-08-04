using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Services.Privacy;

namespace Ssalddel.Services.Community;

public interface I지도신청가원장UseCase
{
    Task<IReadOnlyList<지도신청가원장Response>> 내마커원장조회Async(
        string markerId,
        string? ledgerId,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 생성Async(
        지도신청가원장생성Request request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 신청제출반영Async(
        string ledgerId,
        지도신청실원장전환Request request,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 동의철회반영Async(
        string ledgerId,
        지도신청동의철회반영Request request,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response?> 운영원본조회Async(
        string workCode,
        string operationalSourceType,
        string operationalSourceId,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 운영신청취소반영Async(
        string ledgerId,
        지도신청운영취소반영Request request,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 운송취소검토요청Async(
        string ledgerId,
        지도신청운송취소검토요청Request request,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<지도신청가원장Response>> 관리자운송취소검토목록Async(
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 관리자운송취소검토확인Async(
        string ledgerId,
        지도신청운송취소검토처리Request request,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 관리자운송취소검토결과반영Async(
        string ledgerId,
        지도신청운송취소검토처리Request request,
        string actorUserId,
        CancellationToken cancellationToken = default);
}

public sealed class 지도신청가원장UseCase(
    I신청개인정보동의증적Service consentService,
    I커뮤니티원장저장소 ledgerStore) : I지도신청가원장UseCase
{
    private static readonly IReadOnlyList<string> 지도신청원장템플릿Keys =
    [
        CommunityLedgerTemplateKeys.WarehouseInbound,
        CommunityLedgerTemplateKeys.CargoTransport,
        CommunityLedgerTemplateKeys.Order
    ];

    public async Task<IReadOnlyList<지도신청가원장Response>> 내마커원장조회Async(
        string markerId,
        string? ledgerId,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = Require(actorUserId, "로그인한 신청자 정보가 필요합니다.");
        var normalizedMarkerId = RequireIdentifier(markerId, 160, "지도 마커 ID가 올바르지 않습니다.");
        var normalizedLedgerId = string.IsNullOrWhiteSpace(ledgerId)
            ? null
            : RequireIdentifier(ledgerId, 200, "원장 ID가 올바르지 않습니다.");
        var candidates = await ledgerStore.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                원장템플릿Keys = 지도신청원장템플릿Keys,
                접근UserId = actor,
                외부참조조건 = new Dictionary<string, string>
                {
                    [지도신청가원장정책.지도MarkerIdKey] = normalizedMarkerId
                },
                Limit = 20
            },
            cancellationToken);

        return candidates
            .Where(candidate => string.Equals(candidate.생성자UserId, actor, StringComparison.Ordinal)
                                && (normalizedLedgerId is null
                                    || string.Equals(candidate.원장Id, normalizedLedgerId, StringComparison.Ordinal))
                                && 지도신청원장템플릿Keys.Contains(candidate.원장템플릿Key, StringComparer.Ordinal)
                                && candidate.외부참조.TryGetValue(지도신청가원장정책.지도MarkerIdKey, out var savedMarkerId)
                                && string.Equals(savedMarkerId, normalizedMarkerId, StringComparison.Ordinal))
            .Select(candidate => ToResponse(
                candidate,
                CommunityLedgerTemplateCatalog.Find(candidate.원장템플릿Key).DisplayName,
                reused: true))
            .ToArray();
    }

    public async Task<지도신청가원장Response> 생성Async(
        지도신청가원장생성Request request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = Require(actorUserId, "로그인한 신청자 정보가 필요합니다.");
        var displayName = string.IsNullOrWhiteSpace(actorDisplayName) ? "신청자" : actorDisplayName.Trim();
        var workCode = Require(request.업무Code, "신청 업무 코드가 필요합니다.");
        var sourceCode = Require(request.신청출처Code, "신청 출처 코드가 필요합니다.");
        if (!string.Equals(sourceCode, 신청개인정보출처Codes.커뮤니티지도, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("지도에서 시작한 신청만 가원장으로 접수할 수 있습니다.");
        }

        await consentService.유효한동의요구Async(
            request.신청개인정보동의증적Id,
            workCode,
            sourceCode,
            actor,
            cancellationToken);

        var templateKey = 지도신청가원장정책.원장템플릿Key(workCode);
        var template = CommunityLedgerTemplateCatalog.Find(templateKey);
        var ledgerId = BuildLedgerId(workCode, request.신청개인정보동의증적Id);
        var existing = await ledgerStore.원장조회Async(ledgerId, cancellationToken);
        if (existing is not null)
        {
            EnsureSameApplication(existing, actor, templateKey, request.신청개인정보동의증적Id);
            return ToResponse(existing, template.DisplayName, reused: true);
        }

        var markerName = Clean(request.MarkerName);
        var title = string.IsNullOrWhiteSpace(markerName)
            ? $"[가원장] {template.DisplayName} 신청"
            : $"[가원장] {markerName} · {template.DisplayName} 신청";
        var saved = await ledgerStore.원장저장Async(
            new 커뮤니티원장저장요청
            {
                원장Id = ledgerId,
                기대Revision = 0,
                커뮤니티Id = "platform",
                원장템플릿Key = templateKey,
                제목 = title,
                원함 = "지도에서 확인한 업체 또는 정보 지점을 기준으로 신청 조건을 검토하고 싶습니다.",
                상태 = 커뮤니티원장상태.초안,
                현재단계Key = 지도신청가원장정책.신청접수단계,
                대상OsCode = template.TargetOperatingSystemCode,
                대상OsName = template.TargetOperatingSystemName,
                생성자UserId = actor,
                생성자표시명 = displayName,
                참여자목록 =
                [
                    new 커뮤니티원장참여자Dto
                    {
                        UserId = actor,
                        DisplayName = displayName,
                        RoleLabel = 지도신청가원장정책.신청자역할(workCode),
                        ParticipationState = "가원장 발의"
                    }
                ],
                블록목록 =
                [
                    new 커뮤니티원장블록Dto
                    {
                        BlockId = 지도신청가원장정책.신청접수BlockId,
                        BlockType = CommunityLedgerBlockTypes.Order,
                        Title = "지도 신청 접수",
                        State = "신청서 작성중",
                        Data = new Dictionary<string, string>
                        {
                            ["WorkCode"] = workCode,
                            ["SourceCode"] = sourceCode,
                            ["MarkerId"] = Clean(request.MarkerId),
                            ["MarkerName"] = markerName,
                            ["LayerCode"] = Clean(request.LayerCode),
                            ["CountryCode"] = Clean(request.CountryCode),
                            ["ExternalExecutionOccurred"] = bool.FalseString
                        }
                    }
                ],
                외부참조 = new Dictionary<string, string>
                {
                    ["ApplicationPrivacyConsentEvidenceId"] = request.신청개인정보동의증적Id.ToString("D"),
                    ["ApplicationSourceCode"] = sourceCode,
                    [지도신청가원장정책.지도MarkerIdKey] = Clean(request.MarkerId)
                },
                확장속성 = new Dictionary<string, string>
                {
                    [CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey] = CommunityPostProvisionalLedgerPolicy.LedgerMaturityCode,
                    [CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = CommunityPostProvisionalLedgerPolicy.NonBindingEffectCode,
                    [지도신청가원장정책.신청업무CodeKey] = workCode,
                    ["OperationalHandoffAllowed"] = bool.FalseString,
                    ["ExternalExecutionOccurred"] = bool.FalseString
                }
            },
            actor,
            cancellationToken);

        return ToResponse(saved, template.DisplayName, reused: false);
    }

    public async Task<지도신청가원장Response> 신청제출반영Async(
        string ledgerId,
        지도신청실원장전환Request request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = Require(actorUserId, "로그인한 신청자 정보가 필요합니다.");
        var workCode = Require(request.업무Code, "신청 업무 코드가 필요합니다.");
        var sourceCode = Require(request.신청출처Code, "신청 출처 코드가 필요합니다.");
        var operationalSourceType = Require(request.운영원본종류, "연결할 신청 원본 종류가 필요합니다.");
        var operationalSourceId = Require(request.운영원본Id, "연결할 신청 원본 ID가 필요합니다.");
        if (!string.Equals(
                operationalSourceType,
                지도신청가원장정책.운영원본종류(workCode),
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("신청 업무와 운영 원본 종류가 일치하지 않습니다.");
        }

        await consentService.유효한동의요구Async(
            request.신청개인정보동의증적Id,
            workCode,
            sourceCode,
            actor,
            cancellationToken);

        var ledger = await RequireOwnedLedgerAsync(
            ledgerId,
            actor,
            지도신청가원장정책.원장템플릿Key(workCode),
            request.신청개인정보동의증적Id,
            cancellationToken);
        var template = CommunityLedgerTemplateCatalog.Find(ledger.원장템플릿Key);
        if (ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본IdKey, out var savedSourceId))
        {
            var sameSource = string.Equals(savedSourceId, operationalSourceId, StringComparison.Ordinal)
                && ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본종류Key, out var savedSourceType)
                && string.Equals(savedSourceType, operationalSourceType, StringComparison.Ordinal);
            if (!sameSource)
            {
                throw new InvalidOperationException("이미 다른 운영 신청 원본과 연결된 원장입니다.");
            }

            return ToResponse(ledger, template.DisplayName, reused: true);
        }

        var externalReferences = Copy(ledger.외부참조);
        externalReferences[지도신청가원장정책.운영원본종류Key] = operationalSourceType;
        externalReferences[지도신청가원장정책.운영원본IdKey] = operationalSourceId;
        var attributes = Copy(ledger.확장속성);
        attributes[CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey] = 지도신청가원장정책.실원장성숙도Code;
        attributes[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = 지도신청가원장정책.신청제출효과Code;
        attributes["OperationalHandoffAllowed"] = bool.TrueString;
        attributes["ExternalExecutionOccurred"] = bool.TrueString;
        attributes[지도신청가원장정책.개인정보동의철회Key] = bool.FalseString;

        var saved = await ledgerStore.원장저장Async(
            CopyForSave(
                ledger,
                커뮤니티원장상태.진행중,
                지도신청가원장정책.신청제출단계,
                UpdateIntakeBlock(ledger.블록목록, "신청 제출됨", operationalSourceType, operationalSourceId),
                externalReferences,
                attributes),
            actor,
            cancellationToken);
        return ToResponse(saved, template.DisplayName, reused: false);
    }

    public async Task<지도신청가원장Response> 동의철회반영Async(
        string ledgerId,
        지도신청동의철회반영Request request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = Require(actorUserId, "로그인한 신청자 정보가 필요합니다.");
        var evidence = await consentService.내증적조회Async(
                           request.신청개인정보동의증적Id,
                           actor,
                           cancellationToken)
                       ?? throw new KeyNotFoundException("개인정보 동의 증적을 찾을 수 없습니다.");
        if (!string.Equals(evidence.상태Code, 신청개인정보동의상태Codes.철회, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("철회된 개인정보 동의 증적만 원장에 반영할 수 있습니다.");
        }
        if (!string.Equals(evidence.출처Code, 신청개인정보출처Codes.커뮤니티지도, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("지도 신청에서 기록된 개인정보 동의만 이 원장에 반영할 수 있습니다.");
        }

        var templateKey = 지도신청가원장정책.원장템플릿Key(evidence.업무Code);
        var ledger = await RequireOwnedLedgerAsync(
            ledgerId,
            actor,
            templateKey,
            request.신청개인정보동의증적Id,
            cancellationToken);
        var template = CommunityLedgerTemplateCatalog.Find(templateKey);
        if (ledger.확장속성.TryGetValue(지도신청가원장정책.개인정보동의철회Key, out var withdrawn)
            && bool.TryParse(withdrawn, out var alreadyWithdrawn)
            && alreadyWithdrawn)
        {
            return ToResponse(ledger, template.DisplayName, reused: true);
        }

        var attributes = Copy(ledger.확장속성);
        attributes[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = CommunityPostProvisionalLedgerPolicy.NonBindingEffectCode;
        attributes["OperationalHandoffAllowed"] = bool.FalseString;
        attributes[지도신청가원장정책.개인정보동의철회Key] = bool.TrueString;
        attributes["OperationalApplicationAutomaticallyCancelled"] = bool.FalseString;

        var saved = await ledgerStore.원장저장Async(
            CopyForSave(
                ledger,
                커뮤니티원장상태.보류,
                지도신청가원장정책.동의철회확인단계,
                UpdateIntakeBlock(ledger.블록목록, "동의 철회 · 사람 확인 필요", null, null),
                Copy(ledger.외부참조),
                attributes),
            actor,
            cancellationToken);
        return ToResponse(saved, template.DisplayName, reused: false);
    }

    public async Task<지도신청가원장Response?> 운영원본조회Async(
        string workCode,
        string operationalSourceType,
        string operationalSourceId,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = Require(actorUserId, "로그인한 신청자 정보가 필요합니다.");
        var normalizedWorkCode = Require(workCode, "신청 업무 코드가 필요합니다.");
        var normalizedSourceType = Require(operationalSourceType, "운영 원본 종류가 필요합니다.");
        var normalizedSourceId = Require(operationalSourceId, "운영 원본 ID가 필요합니다.");
        if (!string.Equals(
                normalizedSourceType,
                지도신청가원장정책.운영원본종류(normalizedWorkCode),
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("신청 업무와 운영 원본 종류가 일치하지 않습니다.");
        }

        var templateKey = 지도신청가원장정책.원장템플릿Key(normalizedWorkCode);
        var candidates = await ledgerStore.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                원장템플릿Key = templateKey,
                접근UserId = actor,
                외부참조조건 = new Dictionary<string, string>
                {
                    [지도신청가원장정책.운영원본종류Key] = normalizedSourceType,
                    [지도신청가원장정책.운영원본IdKey] = normalizedSourceId
                },
                Limit = 10
            },
            cancellationToken);
        var ledger = candidates.FirstOrDefault(candidate =>
            string.Equals(candidate.생성자UserId, actor, StringComparison.Ordinal)
            && candidate.외부참조.TryGetValue(지도신청가원장정책.운영원본종류Key, out var sourceType)
            && string.Equals(sourceType, normalizedSourceType, StringComparison.Ordinal)
            && candidate.외부참조.TryGetValue(지도신청가원장정책.운영원본IdKey, out var sourceId)
            && string.Equals(sourceId, normalizedSourceId, StringComparison.Ordinal));
        return ledger is null
            ? null
            : ToResponse(ledger, CommunityLedgerTemplateCatalog.Find(templateKey).DisplayName, reused: true);
    }

    public async Task<지도신청가원장Response> 운영신청취소반영Async(
        string ledgerId,
        지도신청운영취소반영Request request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = Require(actorUserId, "로그인한 신청자 정보가 필요합니다.");
        var sourceType = Require(request.운영원본종류, "취소한 운영 원본 종류가 필요합니다.");
        var sourceId = Require(request.운영원본Id, "취소한 운영 원본 ID가 필요합니다.");
        var ledger = await ledgerStore.원장조회Async(Require(ledgerId, "원장 ID가 필요합니다."), cancellationToken)
                     ?? throw new KeyNotFoundException("지도 신청 원장을 찾을 수 없습니다.");
        if (!string.Equals(ledger.생성자UserId, actor, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("본인의 지도 신청 원장만 취소 상태로 변경할 수 있습니다.");
        }
        if (!ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본종류Key, out var savedSourceType)
            || !string.Equals(savedSourceType, sourceType, StringComparison.Ordinal)
            || !ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본IdKey, out var savedSourceId)
            || !string.Equals(savedSourceId, sourceId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("원장에 연결된 운영 신청과 취소 결과가 일치하지 않습니다.");
        }
        var workCode = ledger.확장속성.GetValueOrDefault(지도신청가원장정책.신청업무CodeKey, string.Empty);
        if (!string.Equals(sourceType, 지도신청가원장정책.운영원본종류(workCode), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("원장의 신청 업무와 운영 원본 종류가 일치하지 않습니다.");
        }

        var template = CommunityLedgerTemplateCatalog.Find(ledger.원장템플릿Key);
        if (ReadBool(ledger.확장속성, "OperationalApplicationCancelled"))
        {
            return ToResponse(ledger, template.DisplayName, reused: true);
        }

        var attributes = Copy(ledger.확장속성);
        attributes[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = 지도신청가원장정책.신청취소효과Code;
        attributes["OperationalHandoffAllowed"] = bool.FalseString;
        attributes["OperationalApplicationCancelled"] = bool.TrueString;
        attributes["OperationalApplicationAutomaticallyCancelled"] = bool.FalseString;
        attributes["CancellationMode"] = "UserExplicit";
        var saved = await ledgerStore.원장저장Async(
            CopyForSave(
                ledger,
                커뮤니티원장상태.닫힘,
                지도신청가원장정책.운영신청취소단계,
                UpdateIntakeBlock(ledger.블록목록, "운영 신청 취소됨", sourceType, sourceId),
                Copy(ledger.외부참조),
                attributes),
            actor,
            cancellationToken);
        return ToResponse(saved, template.DisplayName, reused: false);
    }

    public async Task<지도신청가원장Response> 운송취소검토요청Async(
        string ledgerId,
        지도신청운송취소검토요청Request request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = Require(actorUserId, "로그인한 신청자 정보가 필요합니다.");
        var sourceId = Require(request.운영원본Id, "검토 요청할 운송 의뢰 ID가 필요합니다.");
        var reason = Require(request.사유, "운송 취소 검토 사유를 입력해 주세요.");
        if (reason.Length > 300)
        {
            throw new InvalidOperationException("운송 취소 검토 사유는 300자 이하여야 합니다.");
        }

        var ledger = await ledgerStore.원장조회Async(Require(ledgerId, "원장 ID가 필요합니다."), cancellationToken)
                     ?? throw new KeyNotFoundException("지도 신청 원장을 찾을 수 없습니다.");
        if (!string.Equals(ledger.생성자UserId, actor, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("본인의 운송 신청만 취소 검토를 요청할 수 있습니다.");
        }
        if (!string.Equals(ledger.원장템플릿Key, CommunityLedgerTemplateKeys.CargoTransport, StringComparison.Ordinal)
            || !ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본종류Key, out var sourceType)
            || !string.Equals(sourceType, 지도신청가원장정책.운영원본종류(신청개인정보업무Codes.운송대행), StringComparison.Ordinal)
            || !ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본IdKey, out var savedSourceId)
            || !string.Equals(savedSourceId, sourceId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("원장에 연결된 운송 의뢰와 검토 요청이 일치하지 않습니다.");
        }
        if (ReadBool(ledger.확장속성, "OperationalApplicationCancelled"))
        {
            throw new InvalidOperationException("이미 취소된 운송 의뢰입니다.");
        }
        if (ledger.확장속성.TryGetValue(지도신청가원장정책.운송취소검토상태Key, out var reviewState)
            && string.Equals(reviewState, 지도신청가원장정책.운송취소검토요청됨Code, StringComparison.Ordinal))
        {
            var savedReason = ledger.확장속성.GetValueOrDefault(지도신청가원장정책.운송취소검토사유Key, string.Empty);
            if (!string.Equals(savedReason, reason, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("이미 다른 사유로 운송 취소 검토를 요청했습니다.");
            }
            return ToResponse(ledger, CommunityLedgerTemplateCatalog.Find(ledger.원장템플릿Key).DisplayName, reused: true);
        }

        var attributes = Copy(ledger.확장속성);
        attributes[지도신청가원장정책.운송취소검토상태Key] = 지도신청가원장정책.운송취소검토요청됨Code;
        attributes[지도신청가원장정책.운송취소검토사유Key] = reason;
        attributes[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = CommunityPostProvisionalLedgerPolicy.NonBindingEffectCode;
        attributes["OperationalHandoffAllowed"] = bool.FalseString;
        attributes["OperationalApplicationAutomaticallyCancelled"] = bool.FalseString;
        var saved = await ledgerStore.원장저장Async(
            CopyForSave(
                ledger,
                커뮤니티원장상태.보류,
                지도신청가원장정책.운송취소검토단계,
                UpdateIntakeBlock(ledger.블록목록, "운송 취소 관리자 검토대기", sourceType, sourceId),
                Copy(ledger.외부참조),
                attributes),
            actor,
            cancellationToken);
        return ToResponse(saved, CommunityLedgerTemplateCatalog.Find(saved.원장템플릿Key).DisplayName, reused: false);
    }

    public async Task<IReadOnlyList<지도신청가원장Response>> 관리자운송취소검토목록Async(
        CancellationToken cancellationToken = default)
    {
        var ledgers = await ledgerStore.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                원장템플릿Key = CommunityLedgerTemplateKeys.CargoTransport,
                상태 = 커뮤니티원장상태.보류,
                Limit = 200
            },
            cancellationToken);
        var templateName = CommunityLedgerTemplateCatalog.Find(CommunityLedgerTemplateKeys.CargoTransport).DisplayName;
        return ledgers
            .Where(ledger => string.Equals(ledger.원장템플릿Key, CommunityLedgerTemplateKeys.CargoTransport, StringComparison.Ordinal)
                             && string.Equals(ledger.상태, 커뮤니티원장상태.보류, StringComparison.Ordinal)
                             && ledger.확장속성.TryGetValue(지도신청가원장정책.운송취소검토상태Key, out var state)
                             && string.Equals(state, 지도신청가원장정책.운송취소검토요청됨Code, StringComparison.Ordinal))
            .Select(ledger => ToResponse(ledger, templateName, reused: true))
            .ToArray();
    }

    public async Task<지도신청가원장Response> 관리자운송취소검토확인Async(
        string ledgerId,
        지도신청운송취소검토처리Request request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        Require(actorUserId, "검토 관리자 정보가 필요합니다.");
        var (ledger, _, _) = await RequireTransportCancellationReviewAsync(
            ledgerId,
            request,
            cancellationToken,
            allowCompletedDecision: true);
        if (ledger.확장속성.TryGetValue(지도신청가원장정책.운송취소검토상태Key, out var reviewState)
            && !string.Equals(reviewState, 지도신청가원장정책.운송취소검토요청됨Code, StringComparison.Ordinal))
        {
            var expectedDecision = request.승인
                ? 지도신청가원장정책.운송취소검토승인Code
                : 지도신청가원장정책.운송취소검토거절Code;
            var savedReason = ledger.확장속성.GetValueOrDefault(
                지도신청가원장정책.운송취소검토결과사유Key,
                string.Empty);
            if (!string.Equals(reviewState, expectedDecision, StringComparison.Ordinal)
                || !string.Equals(savedReason, request.검토사유.Trim(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("이미 다른 관리자 검토 결과가 기록되었습니다.");
            }
        }
        return ToResponse(
            ledger,
            CommunityLedgerTemplateCatalog.Find(CommunityLedgerTemplateKeys.CargoTransport).DisplayName,
            reused: true);
    }

    public async Task<지도신청가원장Response> 관리자운송취소검토결과반영Async(
        string ledgerId,
        지도신청운송취소검토처리Request request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = Require(actorUserId, "검토 관리자 정보가 필요합니다.");
        var (ledger, sourceType, sourceId) = await RequireTransportCancellationReviewAsync(
            ledgerId,
            request,
            cancellationToken,
            allowCompletedDecision: true);
        var reason = request.검토사유.Trim();
        var expectedDecision = request.승인
            ? 지도신청가원장정책.운송취소검토승인Code
            : 지도신청가원장정책.운송취소검토거절Code;
        if (ledger.확장속성.TryGetValue(지도신청가원장정책.운송취소검토상태Key, out var currentDecision)
            && string.Equals(currentDecision, expectedDecision, StringComparison.Ordinal)
            && string.Equals(
                ledger.확장속성.GetValueOrDefault(지도신청가원장정책.운송취소검토결과사유Key, string.Empty),
                reason,
                StringComparison.Ordinal))
        {
            return ToResponse(
                ledger,
                CommunityLedgerTemplateCatalog.Find(CommunityLedgerTemplateKeys.CargoTransport).DisplayName,
                reused: true);
        }

        var attributes = Copy(ledger.확장속성);
        attributes[지도신청가원장정책.운송취소검토상태Key] = request.승인
            ? 지도신청가원장정책.운송취소검토승인Code
            : 지도신청가원장정책.운송취소검토거절Code;
        attributes[지도신청가원장정책.운송취소검토결과사유Key] = reason;
        attributes["OperationalApplicationAutomaticallyCancelled"] = bool.FalseString;
        if (request.승인)
        {
            attributes["OperationalApplicationCancelled"] = bool.TrueString;
            attributes["CancellationMode"] = "AdministratorReviewed";
            attributes[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = 지도신청가원장정책.신청취소효과Code;
            attributes["OperationalHandoffAllowed"] = bool.FalseString;
        }
        else
        {
            attributes["OperationalApplicationCancelled"] = bool.FalseString;
            attributes[CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = 지도신청가원장정책.신청제출효과Code;
            attributes["OperationalHandoffAllowed"] = bool.TrueString;
        }

        var saved = await ledgerStore.원장저장Async(
            CopyForSave(
                ledger,
                request.승인 ? 커뮤니티원장상태.닫힘 : 커뮤니티원장상태.진행중,
                request.승인 ? 지도신청가원장정책.운영신청취소단계 : 지도신청가원장정책.신청제출단계,
                UpdateIntakeBlock(
                    ledger.블록목록,
                    request.승인 ? "관리자 검토 후 운송 취소됨" : "운송 취소 검토 거절 · 신청 유지",
                    sourceType,
                    sourceId),
                Copy(ledger.외부참조),
                attributes),
            actor,
            cancellationToken);
        return ToResponse(saved, CommunityLedgerTemplateCatalog.Find(saved.원장템플릿Key).DisplayName, reused: false);
    }

    private async Task<(커뮤니티원장Dto Ledger, string SourceType, string SourceId)> RequireTransportCancellationReviewAsync(
        string ledgerId,
        지도신청운송취소검토처리Request request,
        CancellationToken cancellationToken,
        bool allowCompletedDecision = false)
    {
        ArgumentNullException.ThrowIfNull(request);
        var sourceId = Require(request.확인운영원본Id, "확인용 운송 의뢰 ID가 필요합니다.");
        var reason = Require(request.검토사유, "관리자 검토 사유가 필요합니다.");
        if (reason.Length > 300)
        {
            throw new InvalidOperationException("관리자 검토 사유는 300자 이하여야 합니다.");
        }

        var ledger = await ledgerStore.원장조회Async(Require(ledgerId, "원장 ID가 필요합니다."), cancellationToken)
                     ?? throw new KeyNotFoundException("지도 신청 원장을 찾을 수 없습니다.");
        if (!ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본종류Key, out var sourceType)
            || !string.Equals(sourceType, 지도신청가원장정책.운영원본종류(신청개인정보업무Codes.운송대행), StringComparison.Ordinal)
            || !ledger.외부참조.TryGetValue(지도신청가원장정책.운영원본IdKey, out var savedSourceId)
            || !string.Equals(savedSourceId, sourceId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("원장에 연결된 운송 의뢰와 관리자 확인 ID가 일치하지 않습니다.");
        }

        if (!ledger.확장속성.TryGetValue(지도신청가원장정책.운송취소검토상태Key, out var reviewState)
            || (!string.Equals(reviewState, 지도신청가원장정책.운송취소검토요청됨Code, StringComparison.Ordinal)
                && !(allowCompletedDecision
                     && (string.Equals(reviewState, 지도신청가원장정책.운송취소검토승인Code, StringComparison.Ordinal)
                         || string.Equals(reviewState, 지도신청가원장정책.운송취소검토거절Code, StringComparison.Ordinal)))))
        {
            throw new InvalidOperationException("관리자 검토대기 상태의 운송 취소 요청이 아닙니다.");
        }

        return (ledger, sourceType, sourceId);
    }

    private static void EnsureSameApplication(
        커뮤니티원장Dto ledger,
        string actor,
        string templateKey,
        Guid evidenceId)
    {
        if (!string.Equals(ledger.생성자UserId, actor, StringComparison.Ordinal)
            || !string.Equals(ledger.원장템플릿Key, templateKey, StringComparison.Ordinal)
            || !ledger.외부참조.TryGetValue("ApplicationPrivacyConsentEvidenceId", out var savedEvidenceId)
            || !string.Equals(savedEvidenceId, evidenceId.ToString("D"), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("같은 지도 신청 식별자를 다른 신청에 사용할 수 없습니다.");
        }
    }

    private async Task<커뮤니티원장Dto> RequireOwnedLedgerAsync(
        string ledgerId,
        string actor,
        string templateKey,
        Guid evidenceId,
        CancellationToken cancellationToken)
    {
        var ledger = await ledgerStore.원장조회Async(Require(ledgerId, "원장 ID가 필요합니다."), cancellationToken)
                     ?? throw new KeyNotFoundException("지도 신청 원장을 찾을 수 없습니다.");
        EnsureSameApplication(ledger, actor, templateKey, evidenceId);
        return ledger;
    }

    private static 커뮤니티원장저장요청 CopyForSave(
        커뮤니티원장Dto ledger,
        string state,
        string currentStep,
        IReadOnlyList<커뮤니티원장블록Dto> blocks,
        IReadOnlyDictionary<string, string> externalReferences,
        IReadOnlyDictionary<string, string> attributes)
        => new()
        {
            원장Id = ledger.원장Id,
            기대Revision = ledger.Revision,
            커뮤니티Id = ledger.커뮤니티Id,
            원장템플릿Key = ledger.원장템플릿Key,
            제목 = ledger.제목,
            원함 = ledger.원함,
            상태 = state,
            현재단계Key = currentStep,
            대상OsCode = ledger.대상OsCode,
            대상OsName = ledger.대상OsName,
            생성자UserId = ledger.생성자UserId,
            생성자표시명 = ledger.생성자표시명,
            블록목록 = blocks,
            참여자목록 = ledger.참여자목록,
            포함원장목록 = ledger.포함원장목록,
            다이어그램스냅샷 = ledger.다이어그램스냅샷,
            외부참조 = externalReferences,
            확장속성 = attributes
        };

    private static IReadOnlyList<커뮤니티원장블록Dto> UpdateIntakeBlock(
        IReadOnlyList<커뮤니티원장블록Dto> blocks,
        string state,
        string? operationalSourceType,
        string? operationalSourceId)
        => blocks.Select(block =>
        {
            if (!string.Equals(block.BlockId, 지도신청가원장정책.신청접수BlockId, StringComparison.Ordinal))
            {
                return block;
            }

            var data = Copy(block.Data);
            data["ExternalExecutionOccurred"] = string.IsNullOrWhiteSpace(operationalSourceId)
                ? data.GetValueOrDefault("ExternalExecutionOccurred", bool.FalseString)
                : bool.TrueString;
            if (!string.IsNullOrWhiteSpace(operationalSourceType))
            {
                data[지도신청가원장정책.운영원본종류Key] = operationalSourceType;
            }
            if (!string.IsNullOrWhiteSpace(operationalSourceId))
            {
                data[지도신청가원장정책.운영원본IdKey] = operationalSourceId;
            }
            return new 커뮤니티원장블록Dto
            {
                BlockId = block.BlockId,
                BlockType = block.BlockType,
                Title = block.Title,
                State = state,
                담당자목록 = block.담당자목록,
                Data = data
            };
        }).ToArray();

    private static Dictionary<string, string> Copy(IReadOnlyDictionary<string, string> source)
        => new(source, StringComparer.Ordinal);

    private static 지도신청가원장Response ToResponse(
        커뮤니티원장Dto ledger,
        string templateName,
        bool reused)
        => new()
        {
            원장Id = ledger.원장Id,
            MapMarkerId = ledger.외부참조.GetValueOrDefault(지도신청가원장정책.지도MarkerIdKey, string.Empty),
            업무Code = 지도신청가원장정책.업무Code(ledger.원장템플릿Key),
            신청개인정보동의증적Id = ReadEvidenceId(ledger),
            Revision = ledger.Revision,
            원장템플릿Key = ledger.원장템플릿Key,
            원장템플릿명 = templateName,
            상태 = ledger.상태,
            현재단계Key = ledger.현재단계Key ?? string.Empty,
            기존가원장재사용 = reused,
            외부실행발생 = ReadBool(ledger.확장속성, "ExternalExecutionOccurred"),
            실원장전환됨 = ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey,
                out var maturity) && string.Equals(maturity, 지도신청가원장정책.실원장성숙도Code, StringComparison.Ordinal),
            동의철회보류 = ReadBool(ledger.확장속성, 지도신청가원장정책.개인정보동의철회Key),
            운영신청자동취소됨 = ReadBool(ledger.확장속성, "OperationalApplicationAutomaticallyCancelled"),
            운영신청취소됨 = ReadBool(ledger.확장속성, "OperationalApplicationCancelled"),
            운송취소검토상태Code = ledger.확장속성.GetValueOrDefault(지도신청가원장정책.운송취소검토상태Key, string.Empty),
            운송취소검토사유 = ledger.확장속성.GetValueOrDefault(지도신청가원장정책.운송취소검토사유Key, string.Empty),
            운송취소검토결과사유 = ledger.확장속성.GetValueOrDefault(지도신청가원장정책.운송취소검토결과사유Key, string.Empty),
            운영원본종류 = ledger.외부참조.GetValueOrDefault(지도신청가원장정책.운영원본종류Key, string.Empty),
            운영원본Id = ledger.외부참조.GetValueOrDefault(지도신청가원장정책.운영원본IdKey, string.Empty)
        };

    private static bool ReadBool(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) && parsed;

    private static Guid ReadEvidenceId(커뮤니티원장Dto ledger)
        => ledger.외부참조.TryGetValue("ApplicationPrivacyConsentEvidenceId", out var value)
           && Guid.TryParse(value, out var evidenceId)
            ? evidenceId
            : Guid.Empty;

    private static string BuildLedgerId(string workCode, Guid evidenceId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{workCode}:{evidenceId:D}"));
        return $"map-application:{Convert.ToHexString(hash).ToLowerInvariant()[..24]}";
    }

    private static string Require(string? value, string message)
        => !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new InvalidOperationException(message);

    private static string RequireIdentifier(string? value, int maximumLength, string message)
    {
        var normalized = Require(value, message);
        if (normalized.Length > maximumLength || normalized.Any(char.IsControl))
        {
            throw new InvalidOperationException(message);
        }

        return normalized;
    }

    private static string Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}
