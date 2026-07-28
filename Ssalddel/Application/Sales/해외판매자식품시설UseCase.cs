using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Services.Community;

namespace Ssalddel.Application.Sales;

public interface I해외판매자식품시설UseCase
{
    Task<Result<해외판매자식품시설목록응답>> 목록Async(
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<Result<해외판매자식품시설응답>> 조회Async(
        string profileId,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<Result<해외판매자식품시설응답>> 저장Async(
        string profileId,
        해외판매자식품시설저장요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Application,
    "해외 판매자, 실제 식품 제조시설, 국내 수입자를 분리해 한국 수입식품 신고 전 준비 상태를 원장으로 관리합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(해외판매자식품시설응답),
    FlowOrder = 22,
    Boundary = "식약처 등록·수입 신고·검사·통관을 자동 실행하거나 적법성을 확정하지 않고 누락과 특수 정부 절차만 안내합니다.")]
public sealed partial class 해외판매자식품시설UseCase(I커뮤니티원장저장소 ledgerStore)
    : I해외판매자식품시설UseCase
{
    private const string 원장접두사 = "seller-food-facility:";
    private const string 등록정보BlockId = "foreign-food-facility-profile";
    private const string 등록정보JsonKey = "ProfileJson";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] OfficialSources =
    [
        "https://law.go.kr/LSW/lsInfoP.do?lsId=012247",
        "https://www.law.go.kr/LSW/lumLsLinkPop.do?chrClsCd=010202&lspttninfSeq=126970",
        "https://impfood.mfds.go.kr/CFAAA01F01/"
    ];

    [GeneratedRegex("^[A-Za-z0-9_-]{1,80}$", RegexOptions.CultureInvariant)]
    private static partial Regex ProfileIdPattern();

    public async Task<Result<해외판매자식품시설목록응답>> 목록Async(
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (!ActorValid(actorUserId, isAdministrator))
        {
            return Forbidden<해외판매자식품시설목록응답>("로그인한 판매자 정보가 필요합니다.");
        }

        var ledgers = await ledgerStore.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                원장템플릿Key = CommunityLedgerTemplateKeys.ForeignFoodFacilityProfile,
                접근UserId = isAdministrator ? null : actorUserId.Trim(),
                Limit = 100
            },
            cancellationToken);

        return Result.Ok(new 해외판매자식품시설목록응답
        {
            Items = ledgers
                .Where(x => isAdministrator || AccessAllowed(x, actorUserId, false))
                .OrderByDescending(x => x.수정시각Utc)
                .Select(ToResponse)
                .ToArray()
        });
    }

    public async Task<Result<해외판매자식품시설응답>> 조회Async(
        string profileId,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var profileValidation = ValidateProfileId(profileId);
        if (profileValidation.IsFailed)
        {
            return profileValidation.ToResult<해외판매자식품시설응답>();
        }

        var ledger = await ledgerStore.원장조회Async(LedgerId(profileValidation.Value), cancellationToken);
        if (ledger is null)
        {
            return NotFound<해외판매자식품시설응답>("해외 식품시설 준비 원장을 찾을 수 없습니다.");
        }

        return AccessAllowed(ledger, actorUserId, isAdministrator)
            ? Result.Ok(ToResponse(ledger))
            : Forbidden<해외판매자식품시설응답>("이 식품시설 준비 원장에 접근할 권한이 없습니다.");
    }

    public async Task<Result<해외판매자식품시설응답>> 저장Async(
        string profileId,
        해외판매자식품시설저장요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest<해외판매자식품시설응답>("등록 정보가 필요합니다.");
        }

        if (!ActorValid(actorUserId, isAdministrator))
        {
            return Forbidden<해외판매자식품시설응답>("로그인한 판매자 정보가 필요합니다.");
        }

        var profileValidation = ValidateProfileId(profileId);
        if (profileValidation.IsFailed)
        {
            return profileValidation.ToResult<해외판매자식품시설응답>();
        }

        var ledgerId = LedgerId(profileValidation.Value);
        var existing = await ledgerStore.원장조회Async(ledgerId, cancellationToken);
        if (existing is not null && !AccessAllowed(existing, actorUserId, isAdministrator))
        {
            return Forbidden<해외판매자식품시설응답>("다른 판매자의 식품시설 준비 원장은 변경할 수 없습니다.");
        }

        if (existing is null && request.기대Revision is > 0)
        {
            return Conflict<해외판매자식품시설응답>("새 원장의 기대 Revision은 비워 두거나 0이어야 합니다.");
        }

        if (existing is not null
            && request.기대Revision.HasValue
            && request.기대Revision.Value != existing.Revision)
        {
            return Conflict<해외판매자식품시설응답>(
                "식품시설 준비 원장이 먼저 변경되었습니다. 다시 불러온 뒤 저장하세요.");
        }

        var expectedRevision = request.기대Revision;
        var normalized = Normalize(request);
        normalized.기대Revision = null;
        var assessment = Assess(normalized, DateOnly.FromDateTime(DateTime.UtcNow));
        var ownerId = existing?.생성자UserId ?? actorUserId.Trim();
        var saved = await ledgerStore.원장저장Async(
            new 커뮤니티원장저장요청
            {
                원장Id = ledgerId,
                기대Revision = expectedRevision ?? existing?.Revision ?? 0,
                커뮤니티Id = "platform",
                원장템플릿Key = CommunityLedgerTemplateKeys.ForeignFoodFacilityProfile,
                제목 = string.IsNullOrWhiteSpace(normalized.시설명)
                    ? $"{normalized.판매자업체명} 한국 수입식품 준비"
                    : $"{normalized.시설명} 한국 수입식품 준비",
                상태 = assessment.Blockers.Count == 0 ? 커뮤니티원장상태.진행중 : 커뮤니티원장상태.초안,
                현재단계Key = assessment.Blockers.Count == 0 ? "human-review-ready" : "information-collection",
                대상OsCode = "TradeReadinessOS",
                대상OsName = "1.5 무역 준비 OS",
                생성자UserId = ownerId,
                생성자표시명 = existing?.생성자표시명 ?? ownerId,
                참여자목록 =
                [
                    new 커뮤니티원장참여자Dto
                    {
                        UserId = ownerId,
                        DisplayName = existing?.생성자표시명 ?? ownerId,
                        RoleLabel = "해외 판매자",
                        ParticipationState = "참여중"
                    }
                ],
                블록목록 =
                [
                    new 커뮤니티원장블록Dto
                    {
                        BlockId = 등록정보BlockId,
                        BlockType = CommunityLedgerBlockTypes.Evidence,
                        Title = "해외 판매자·실제 제조시설·국내 수입자",
                        State = assessment.Blockers.Count == 0 ? "사람검토대기" : "정보수집중",
                        Data = new Dictionary<string, string>
                        {
                            [등록정보JsonKey] = JsonSerializer.Serialize(normalized, JsonOptions),
                            ["ProcedureCode"] = assessment.ProcedureCode,
                            ["LegalBasisVersion"] = "2026-01-01",
                            ["ExternalSubmissionOccurred"] = "false"
                        }
                    }
                ],
                외부참조 = new Dictionary<string, string>
                {
                    ["MfdsForeignFacilityCode"] = normalized.기존식약처등록코드
                },
                확장속성 = new Dictionary<string, string>
                {
                    ["ExecutionMode"] = "Simulation",
                    ["DeclarationSubmissionAllowed"] = "false",
                    ["ExternalTransmissionAllowed"] = "false",
                    ["ProcedureCode"] = assessment.ProcedureCode
                }
            },
            actorUserId.Trim(),
            cancellationToken);

        return Result.Ok(ToResponse(saved));
    }

    internal static 해외판매자식품시설응답 ToResponse(커뮤니티원장Dto ledger)
    {
        var request = Deserialize(ledger);
        var assessment = Assess(request, DateOnly.FromDateTime(DateTime.UtcNow));
        return new 해외판매자식품시설응답
        {
            프로필Id = ledger.원장Id.StartsWith(원장접두사, StringComparison.OrdinalIgnoreCase)
                ? ledger.원장Id[원장접두사.Length..]
                : ledger.원장Id,
            Revision = ledger.Revision,
            상태 = ledger.상태,
            등록정보 = request,
            적용절차코드 = assessment.ProcedureCode,
            다음조치 = assessment.NextAction,
            시설등록준비완료여부 = assessment.FacilityReady,
            한국수입준비완료여부 = assessment.KoreanImportReady,
            차단사유목록 = assessment.Blockers,
            주의사항목록 = assessment.Warnings,
            외부신고발생여부 = false,
            실행모드 = "Simulation",
            공식근거Url목록 = OfficialSources
        };
    }

    private static Assessment Assess(해외판매자식품시설저장요청 request, DateOnly today)
    {
        var blockers = new List<string>();
        var warnings = new List<string>();
        Required(request.판매자업체명, "판매자·수출자 업체명이 필요합니다.", blockers);
        Required(request.판매자국가코드, "판매자 국가 코드가 필요합니다.", blockers);
        Required(request.판매자담당자명, "판매자 담당자명이 필요합니다.", blockers);
        Required(request.판매자이메일, "판매자 연락 이메일이 필요합니다.", blockers);
        Required(request.시설명, "실제로 제조·가공·포장하는 시설명이 필요합니다.", blockers);
        Required(request.시설대표자명, "시설 대표자명이 필요합니다.", blockers);
        Required(request.시설주소, "실제 시설 주소가 필요합니다.", blockers);
        Required(request.시설국가코드, "시설 국가 코드가 필요합니다.", blockers);
        Required(request.시설전화번호, "시설 전화번호가 필요합니다.", blockers);
        Required(request.시설이메일, "시설 이메일이 필요합니다.", blockers);

        if (request.생산품목코드목록.Count == 0)
        {
            blockers.Add("생산 품목을 하나 이상 선택해야 합니다.");
        }

        if (request.업종코드목록.Count == 0)
        {
            blockers.Add("시설 업종을 하나 이상 선택해야 합니다.");
        }

        if (!request.현지실사동의여부)
        {
            blockers.Add("식약처 현지실사 동의가 필요합니다.");
        }

        if (!request.정보진실성확인여부)
        {
            blockers.Add("등록 정보의 진실성 확인이 필요합니다.");
        }

        if (request.신청자유형 == 해외판매자식품시설신청자유형코드.국내수입자)
        {
            Required(request.국내수입업체명, "국내 수입자 신청에는 수입업체명이 필요합니다.", blockers);
            Required(request.국내수입업체주소, "국내 수입자 신청에는 수입업체 주소가 필요합니다.", blockers);
            Required(request.국내수입업체전화번호, "국내 수입자 신청에는 수입업체 전화번호가 필요합니다.", blockers);
            Required(request.국내수입업체이메일, "국내 수입자 신청에는 수입업체 이메일이 필요합니다.", blockers);
            Required(
                request.국내수입식품영업등록번호,
                "국내 수입자 신청에는 수입식품 영업등록 번호가 필요합니다.",
                blockers);
            if (!request.시설운영자동의여부)
            {
                blockers.Add("국내 수입자가 신청할 때는 해외 시설 운영자의 동의가 필요합니다.");
            }
        }

        var procedure = Procedure(request);
        var specialGovernmentRoute = procedure != 한국수입식품절차코드.해외제조업소등록;
        var evidenceTypes = request.증빙목록
            .Select(x => x.증빙유형)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (specialGovernmentRoute)
        {
            if (!evidenceTypes.Contains(해외판매자식품시설증빙유형코드.정부경로확인))
            {
                blockers.Add("이 품목은 수출국 정부를 통한 등록·확인 경로 증빙이 필요합니다.");
            }
        }
        else if (!evidenceTypes.Contains(해외판매자식품시설증빙유형코드.수출국허가등록증빙)
                 && !evidenceTypes.Contains(해외판매자식품시설증빙유형코드.등록확인서대체서식))
        {
            blockers.Add("수출국이 발급한 영업 허가·등록 증빙 또는 허용된 등록확인서 대체서식이 필요합니다.");
        }
        else if (!evidenceTypes.Contains(해외판매자식품시설증빙유형코드.수출국허가등록증빙)
                 && evidenceTypes.Contains(해외판매자식품시설증빙유형코드.등록확인서대체서식))
        {
            warnings.Add("등록확인서 대체서식은 수출국 발급 증빙을 제출할 수 없는 경우에 한해 인정 가능한지 사람 검토가 필요합니다.");
        }

        if (request.증빙목록.Any(x =>
                !string.Equals(x.언어코드, "ko", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.언어코드, "en", StringComparison.OrdinalIgnoreCase))
            && !evidenceTypes.Contains(해외판매자식품시설증빙유형코드.공식영문확인)
            && !evidenceTypes.Contains(해외판매자식품시설증빙유형코드.공증번역))
        {
            blockers.Add("한국어·영어가 아닌 증빙에는 공식 영문 확인 또는 공증 번역이 필요합니다.");
        }

        if (request.안전관리인증코드목록.Count > 0
            && !evidenceTypes.Contains(해외판매자식품시설증빙유형코드.식품안전인증))
        {
            blockers.Add("선택한 HACCP·ISO 22000·GMP·GFSI 인증 증빙이 필요합니다.");
        }

        if (request.식약처등록만료일 is { } expiry)
        {
            if (expiry < today)
            {
                blockers.Add("기존 해외제조업소 등록 유효기간이 만료되었습니다.");
            }
            else if (expiry <= today.AddDays(30))
            {
                warnings.Add("해외제조업소 등록 만료가 30일 이내입니다. 만료 7일 전까지 갱신 신청을 준비하세요.");
            }
        }

        if (request.판매자가시설운영자인가
            && (!string.Equals(request.판매자업체명, request.시설명, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(request.판매자국가코드, request.시설국가코드, StringComparison.OrdinalIgnoreCase)))
        {
            warnings.Add("판매자와 시설 운영자가 동일하다고 표시했지만 업체명 또는 국가가 다릅니다.");
        }

        var facilityReady = blockers.Count == 0;
        var importerReady = !string.IsNullOrWhiteSpace(request.국내수입업체명)
                            && !string.IsNullOrWhiteSpace(request.국내수입식품영업등록번호)
                            && request.국내수입업체확인여부;
        if (!importerReady)
        {
            warnings.Add("한국 내 수입·판매 영업등록을 보유한 수입자를 지정하고 확인해야 실제 수입 신고를 준비할 수 있습니다.");
        }

        return new Assessment(
            procedure,
            NextAction(procedure, facilityReady, importerReady),
            facilityReady,
            facilityReady && importerReady,
            blockers,
            warnings);
    }

    private static string Procedure(해외판매자식품시설저장요청 request)
    {
        if (request.생산품목코드목록.Contains(
                해외판매자식품시설품목코드.축산물,
                StringComparer.OrdinalIgnoreCase))
        {
            return 한국수입식품절차코드.축산물해외작업장수출국정부신청;
        }

        if (request.생산품목코드목록.Contains(
                해외판매자식품시설품목코드.동물성식품,
                StringComparer.OrdinalIgnoreCase))
        {
            return 한국수입식품절차코드.동물성식품수출국정부신청;
        }

        return request.수산물정부경로검토필요여부
            ? 한국수입식품절차코드.수산물정부경로검토
            : 한국수입식품절차코드.해외제조업소등록;
    }

    private static string NextAction(string procedure, bool facilityReady, bool importerReady)
    {
        if (!facilityReady)
        {
            return "차단 사유를 보완한 뒤 수입 전문가 또는 식약처 담당자의 사람 검토를 요청하세요.";
        }

        if (!importerReady)
        {
            return "한국 내 수입·판매 영업등록을 보유한 수입자를 지정하고 등록번호를 확인하세요.";
        }

        return procedure == 한국수입식품절차코드.해외제조업소등록
            ? "실제 수입 신고 전에 식약처 해외제조업소 등록 상태와 제품별 요건을 최종 확인하세요."
            : "수출국 정부 경로의 등록·승인 상태를 확인한 뒤 한국 수입 신고를 준비하세요.";
    }

    private static 해외판매자식품시설저장요청 Normalize(해외판매자식품시설저장요청 source)
    {
        source.판매자업체명 = source.판매자업체명.Trim();
        source.판매자국가코드 = source.판매자국가코드.Trim().ToUpperInvariant();
        source.판매자현지등록번호 = source.판매자현지등록번호.Trim();
        source.판매자담당자명 = source.판매자담당자명.Trim();
        source.판매자이메일 = source.판매자이메일.Trim();
        source.판매자전화번호 = source.판매자전화번호.Trim();
        source.시설명 = source.시설명.Trim();
        source.시설대표자명 = source.시설대표자명.Trim();
        source.시설주소 = source.시설주소.Trim();
        source.시설국가코드 = source.시설국가코드.Trim().ToUpperInvariant();
        source.시설전화번호 = source.시설전화번호.Trim();
        source.시설이메일 = source.시설이메일.Trim();
        source.기존식약처등록코드 = source.기존식약처등록코드.Trim();
        source.국내수입업체명 = source.국내수입업체명.Trim();
        source.국내수입식품영업등록번호 = source.국내수입식품영업등록번호.Trim();
        source.생산품목코드목록 = Distinct(source.생산품목코드목록);
        source.업종코드목록 = Distinct(source.업종코드목록);
        source.안전관리인증코드목록 = Distinct(source.안전관리인증코드목록);
        return source;
    }

    private static List<string> Distinct(IEnumerable<string> values)
        => values.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static 해외판매자식품시설저장요청 Deserialize(커뮤니티원장Dto ledger)
    {
        var json = ledger.블록목록
            .FirstOrDefault(x => string.Equals(x.BlockId, 등록정보BlockId, StringComparison.OrdinalIgnoreCase))
            ?.Data.GetValueOrDefault(등록정보JsonKey);
        return string.IsNullOrWhiteSpace(json)
            ? new 해외판매자식품시설저장요청()
            : JsonSerializer.Deserialize<해외판매자식품시설저장요청>(json, JsonOptions) ?? new();
    }

    private static void Required(string value, string message, ICollection<string> blockers)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            blockers.Add(message);
        }
    }

    private static bool ActorValid(string actorUserId, bool isAdministrator)
        => isAdministrator || !string.IsNullOrWhiteSpace(actorUserId);

    private static bool AccessAllowed(커뮤니티원장Dto ledger, string actorUserId, bool isAdministrator)
        => isAdministrator
           || (!string.IsNullOrWhiteSpace(actorUserId)
               && (string.Equals(ledger.생성자UserId, actorUserId.Trim(), StringComparison.Ordinal)
                   || ledger.참여자목록.Any(x =>
                       string.Equals(x.UserId, actorUserId.Trim(), StringComparison.Ordinal))));

    private static string LedgerId(string profileId) => $"{원장접두사}{profileId}";

    private static Result<string> ValidateProfileId(string profileId)
        => !string.IsNullOrWhiteSpace(profileId) && ProfileIdPattern().IsMatch(profileId.Trim())
            ? Result.Ok(profileId.Trim())
            : Result.Fail<string>(new Error("프로필 ID는 영문, 숫자, 밑줄, 하이픈으로 1~80자여야 합니다.")
                .WithMetadata("StatusCode", StatusCodes.Status400BadRequest));

    private static Result<T> BadRequest<T>(string message) => Failure<T>(message, StatusCodes.Status400BadRequest);
    private static Result<T> NotFound<T>(string message) => Failure<T>(message, StatusCodes.Status404NotFound);
    private static Result<T> Forbidden<T>(string message) => Failure<T>(message, StatusCodes.Status403Forbidden);
    private static Result<T> Conflict<T>(string message) => Failure<T>(message, StatusCodes.Status409Conflict);
    private static Result<T> Failure<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));

    private sealed record Assessment(
        string ProcedureCode,
        string NextAction,
        bool FacilityReady,
        bool KoreanImportReady,
        IReadOnlyList<string> Blockers,
        IReadOnlyList<string> Warnings);
}
