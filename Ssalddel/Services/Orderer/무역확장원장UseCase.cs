using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.Orderer;

public interface I무역확장원장UseCase
{
    Task<Result<무역확장원장응답>> 개별수입생성Async(
        string 주문원장Id,
        개별수입원장생성요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<Result<무역확장원장응답>> 개별수출생성Async(
        string 주문원장Id,
        개별수출원장생성요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<Result<무역확장원장응답>> 공동수출생성Async(
        공동수출원장생성요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<Result<무역확장원장응답>> 조회Async(
        string 원장Id,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<Result<판매자수출원장목록응답>> 판매자수출목록조회Async(
        판매자수출원장목록조회요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Application,
    "개별수입·개별수출 확장을 원천 주문에 연결하고 공동수출 물류 집계 원장을 멱등하게 생성합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(무역확장원장응답),
    FlowOrder = 20,
    Boundary = "Simulation 원장만 만들며 계약·결제·신고·포워더 전송·운송 실행은 수행하지 않습니다.")]
public sealed class 무역확장원장UseCase : I무역확장원장UseCase
{
    private const int 최대멱등키길이 = 160;
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly I주문원장통합UseCase _주문원장통합;

    public 무역확장원장UseCase(
        I커뮤니티원장저장소 원장저장소,
        I주문원장통합UseCase 주문원장통합)
    {
        _원장저장소 = 원장저장소;
        _주문원장통합 = 주문원장통합;
    }

    public Task<Result<무역확장원장응답>> 개별수입생성Async(
        string 주문원장Id,
        개별수입원장생성요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
        => 개별확장생성Async(
            주문원장Id,
            request,
            CommunityLedgerTemplateKeys.IndividualImport,
            주문원장포함역할.개별수입,
            [
                블록("import-identity", "수입 주체·해외 판매자", new Dictionary<string, string>
                {
                    ["수입주체"] = request.수입주체.Trim(),
                    ["해외판매자"] = request.해외판매자.Trim(),
                    ["거래문맥"] = request.거래문맥.Trim()
                }),
                블록("import-readiness", "수입 준비·통관 검토", new Dictionary<string, string>
                {
                    ["Incoterms후보"] = request.Incoterms후보.Trim(),
                    ["통관검토메모"] = request.통관검토메모.Trim(),
                    ["실행상태"] = "검토대기"
                })
            ],
            actorUserId,
            isAdministrator,
            cancellationToken);

    public Task<Result<무역확장원장응답>> 개별수출생성Async(
        string 주문원장Id,
        개별수출원장생성요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
        => 개별확장생성Async(
            주문원장Id,
            request,
            CommunityLedgerTemplateKeys.IndividualExport,
            주문원장포함역할.개별수출,
            [
                블록("export-parties", "수출자·해외 구매자", new Dictionary<string, string>
                {
                    ["수출자"] = request.수출자.Trim(),
                    ["해외구매자"] = request.해외구매자.Trim(),
                    ["목적국가코드"] = request.목적국가코드.Trim(),
                    ["거래문맥"] = request.거래문맥.Trim()
                }),
                블록("export-readiness", "수출 요건·서류 검토", new Dictionary<string, string>
                {
                    ["Incoterms후보"] = request.Incoterms후보.Trim(),
                    ["규정검토메모"] = request.규정검토메모.Trim(),
                    ["신고상태"] = "미제출",
                    ["포워더전송상태"] = "미전송"
                })
            ],
            actorUserId,
            isAdministrator,
            cancellationToken);

    public async Task<Result<무역확장원장응답>> 공동수출생성Async(
        공동수출원장생성요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest("공동수출 원장 생성 요청이 필요합니다.");
        }

        var idempotency = 멱등키검증(request.요청멱등키);
        if (idempotency.IsFailed)
        {
            return idempotency.ToResult<무역확장원장응답>();
        }

        var ids = request.개별수출원장Ids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (ids.Length == 0)
        {
            return BadRequest("하나 이상의 개별수출 원장 ID가 필요합니다.");
        }

        var sources = new List<커뮤니티원장Dto>(ids.Length);
        foreach (var id in ids)
        {
            var source = await _원장저장소.원장조회Async(id, cancellationToken);
            if (source is null)
            {
                return NotFound($"개별수출 원장을 찾을 수 없습니다. 원장Id={id}");
            }

            if (!주문원장구성정책.개별수출인가(source.원장템플릿Key))
            {
                return BadRequest($"공동수출에는 개별수출 원장만 연결할 수 있습니다. 원장Id={id}");
            }

            if (!액세스가능(source, actorUserId, isAdministrator))
            {
                return Forbidden("접근할 수 없는 개별수출 원장이 포함되어 있습니다.");
            }

            sources.Add(source);
        }

        var communities = sources
            .Select(x => x.커뮤니티Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (communities.Length != 1)
        {
            return BadRequest("같은 커뮤니티에 속한 개별수출 원장만 공동수출로 집계할 수 있습니다.");
        }

        var ledgerId = 결정적원장Id("group-export", string.Join('|', ids), idempotency.Value);
        var existing = await _원장저장소.원장조회Async(ledgerId, cancellationToken);
        if (existing is not null)
        {
            return Result.Ok(응답(existing, sources, true));
        }

        var references = sources.Select((x, index) => new 커뮤니티포함원장참조Dto
        {
            원장Id = x.원장Id,
            원장템플릿Key = x.원장템플릿Key,
            역할 = 주문원장포함역할.개별수출,
            관계유형 = CommunityLedgerRelationTypes.Contains,
            필수여부 = true,
            표시순서 = index
        }).ToArray();
        var saved = await _원장저장소.원장저장Async(
            new 커뮤니티원장저장요청
            {
                원장Id = ledgerId,
                기대Revision = 0,
                커뮤니티Id = communities[0],
                원장템플릿Key = CommunityLedgerTemplateKeys.GroupExport,
                제목 = 제목(request.제목, "공동수출 물류 집계"),
                상태 = 커뮤니티원장상태.초안,
                현재단계Key = "collection-planning",
                대상OsCode = "TradeReadinessOS",
                대상OsName = "1.5 무역 준비 OS",
                생성자UserId = actorUserId.Trim(),
                생성자표시명 = actorUserId.Trim(),
                포함원장목록 = references,
                참여자목록 = [참여자(actorUserId, "집하 조정자")],
                블록목록 =
                [
                    블록("group-export-collection", "집하·합포장 계획", new Dictionary<string, string>
                    {
                        ["집하마감"] = request.집하마감.Trim(),
                        ["개별수출원장수"] = ids.Length.ToString(),
                        ["개별신고보존"] = "true"
                    }),
                    블록("group-export-handoff", "포워더 인계·공통비", new Dictionary<string, string>
                    {
                        ["포워더인계메모"] = request.포워더인계메모.Trim(),
                        ["공통비배부근거"] = request.공통비배부근거.Trim(),
                        ["외부전송상태"] = "미전송"
                    })
                ],
                외부참조 = new Dictionary<string, string>
                {
                    ["SourceIndividualExportLedgerIds"] = string.Join(',', ids)
                },
                확장속성 = 기본확장속성(idempotency.Value)
            },
            actorUserId.Trim(),
            cancellationToken);

        return Result.Ok(응답(saved, sources, false));
    }

    public async Task<Result<무역확장원장응답>> 조회Async(
        string 원장Id,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(원장Id))
        {
            return BadRequest("원장 ID가 필요합니다.");
        }

        var ledger = await _원장저장소.원장조회Async(원장Id.Trim(), cancellationToken);
        if (ledger is null)
        {
            return NotFound("무역 확장 원장을 찾을 수 없습니다.");
        }

        if (!주문원장구성정책.개별수입인가(ledger.원장템플릿Key)
            && !주문원장구성정책.개별수출인가(ledger.원장템플릿Key)
            && !주문원장구성정책.공동수출인가(ledger.원장템플릿Key))
        {
            return BadRequest("개별수입·개별수출·공동수출 원장만 조회할 수 있습니다.");
        }

        if (!액세스가능(ledger, actorUserId, isAdministrator))
        {
            return Forbidden("이 원장에 접근할 권한이 없습니다.");
        }

        var sources = new List<커뮤니티원장Dto>();
        foreach (var reference in ledger.포함원장목록.OrderBy(x => x.표시순서))
        {
            var source = await _원장저장소.원장조회Async(reference.원장Id, cancellationToken);
            if (source is not null)
            {
                sources.Add(source);
            }
        }

        if (sources.Count == 0
            && ledger.외부참조.TryGetValue("SourceOrderLedgerId", out var sourceOrderId))
        {
            var source = await _원장저장소.원장조회Async(sourceOrderId, cancellationToken);
            if (source is not null)
            {
                sources.Add(source);
            }
        }

        return Result.Ok(응답(ledger, sources, true));
    }

    public async Task<Result<판매자수출원장목록응답>> 판매자수출목록조회Async(
        판매자수출원장목록조회요청 request,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = actorUserId?.Trim();
        if (!isAdministrator && string.IsNullOrWhiteSpace(actor))
        {
            return Result.Fail<판매자수출원장목록응답>(
                new Error("로그인 사용자를 확인할 수 없습니다.")
                    .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));
        }

        var ledgers = await _원장저장소.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                원장템플릿Keys =
                [
                    CommunityLedgerTemplateKeys.IndividualExport,
                    CommunityLedgerTemplateKeys.GroupExport
                ],
                상태 = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim(),
                Limit = 200
            },
            cancellationToken);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        var accessible = new List<커뮤니티원장Dto>();
        foreach (var ledger in ledgers)
        {
            if (isAdministrator
                || 액세스가능(ledger, actor!, false)
                || await 판매자연결접근가능Async(ledger, actor!, cancellationToken))
            {
                accessible.Add(ledger);
            }
        }

        var filtered = accessible
            .Where(ledger => 주문원장구성정책.개별수출인가(ledger.원장템플릿Key)
                             || 주문원장구성정책.공동수출인가(ledger.원장템플릿Key))
            .Where(ledger => string.IsNullOrWhiteSpace(request.Status)
                             || string.Equals(ledger.상태, request.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(ledger => search is null
                             || ledger.원장Id.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || ledger.제목.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || ledger.상태.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(ledger => ledger.수정시각Utc)
            .ThenByDescending(ledger => ledger.생성시각Utc)
            .ToArray();
        var page = Math.Max(0, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return Result.Ok(new 판매자수출원장목록응답
        {
            Items = filtered
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(요약)
                .ToArray(),
            TotalCount = filtered.Length,
            Page = page,
            PageSize = pageSize,
            외부실행발생여부 = false,
            실행모드 = "Simulation"
        });
    }

    private async Task<bool> 판매자연결접근가능Async(
        커뮤니티원장Dto exportLedger,
        string actorUserId,
        CancellationToken cancellationToken)
    {
        if (주문원장구성정책.공동수출인가(exportLedger.원장템플릿Key))
        {
            foreach (var reference in exportLedger.포함원장목록.Where(reference =>
                         string.Equals(
                             reference.원장템플릿Key,
                             CommunityLedgerTemplateKeys.IndividualExport,
                             StringComparison.OrdinalIgnoreCase)))
            {
                var individualExport = await _원장저장소.원장조회Async(reference.원장Id, cancellationToken);
                if (individualExport is not null
                    && await 판매자연결접근가능Async(individualExport, actorUserId, cancellationToken))
                {
                    return true;
                }
            }

            return false;
        }

        if (!주문원장구성정책.개별수출인가(exportLedger.원장템플릿Key)
            || !exportLedger.외부참조.TryGetValue("SourceOrderLedgerId", out var sourceOrderLedgerId)
            || string.IsNullOrWhiteSpace(sourceOrderLedgerId))
        {
            return false;
        }

        var order = await _원장저장소.원장조회Async(sourceOrderLedgerId.Trim(), cancellationToken);
        if (order is null || !주문원장구성정책.주문루트인가(order.원장템플릿Key))
        {
            return false;
        }

        foreach (var saleReference in order.포함원장목록.Where(reference =>
                     string.Equals(reference.역할, 주문원장포함역할.판매, StringComparison.OrdinalIgnoreCase)))
        {
            var saleLedger = await _원장저장소.원장조회Async(saleReference.원장Id, cancellationToken);
            if (saleLedger is not null && 액세스가능(saleLedger, actorUserId, false))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<Result<무역확장원장응답>> 개별확장생성Async(
        string 주문원장Id,
        무역확장원장생성요청 request,
        string templateKey,
        string role,
        IReadOnlyList<커뮤니티원장블록Dto> blocks,
        string actorUserId,
        bool isAdministrator,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(주문원장Id))
        {
            return BadRequest("원천 개별주문 원장과 생성 요청이 필요합니다.");
        }

        var idempotency = 멱등키검증(request.요청멱등키);
        if (idempotency.IsFailed)
        {
            return idempotency.ToResult<무역확장원장응답>();
        }

        var order = await _원장저장소.원장조회Async(주문원장Id.Trim(), cancellationToken);
        if (order is null)
        {
            return NotFound("원천 개별주문 원장을 찾을 수 없습니다.");
        }

        if (!string.Equals(order.원장템플릿Key, CommunityLedgerTemplateKeys.Order, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("개별수입·개별수출 확장은 order 원장의 하위 원장으로만 만들 수 있습니다.");
        }

        if (!액세스가능(order, actorUserId, isAdministrator))
        {
            return Forbidden("원천 개별주문 원장에 접근할 권한이 없습니다.");
        }

        var ledgerId = 결정적원장Id(templateKey, order.원장Id, idempotency.Value);
        var existing = await _원장저장소.원장조회Async(ledgerId, cancellationToken);
        var alreadyProcessed = existing is not null;
        if (existing is not null
            && order.포함원장목록.Any(x =>
                string.Equals(x.원장Id, existing.원장Id, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Ok(응답(existing, [order], true));
        }

        if (existing is null
            && request.기대원천Revision.HasValue
            && request.기대원천Revision.Value != order.Revision)
        {
            return Conflict("원천 주문 원장이 먼저 변경되었습니다. 다시 조회한 뒤 재시도해야 합니다.");
        }

        var extension = existing ?? await _원장저장소.원장저장Async(
            new 커뮤니티원장저장요청
            {
                원장Id = ledgerId,
                기대Revision = 0,
                커뮤니티Id = order.커뮤니티Id,
                원장템플릿Key = templateKey,
                제목 = 제목(request.제목, templateKey == CommunityLedgerTemplateKeys.IndividualImport
                    ? "개별수입 준비"
                    : "개별수출 준비"),
                원함 = request.메모.Trim(),
                상태 = 커뮤니티원장상태.초안,
                현재단계Key = "readiness-review",
                대상OsCode = "TradeReadinessOS",
                대상OsName = "1.5 무역 준비 OS",
                생성자UserId = actorUserId.Trim(),
                생성자표시명 = actorUserId.Trim(),
                참여자목록 = [참여자(actorUserId, "주문 당사자")],
                블록목록 =
                [
                    블록("source-order", "원천 개별주문 참조", new Dictionary<string, string>
                    {
                        ["원천원장Id"] = order.원장Id,
                        ["원천Revision"] = order.Revision.ToString(),
                        ["원본복제여부"] = "false"
                    }),
                    .. blocks
                ],
                외부참조 = new Dictionary<string, string>
                {
                    ["SourceOrderLedgerId"] = order.원장Id
                },
                확장속성 = 기본확장속성(idempotency.Value)
            },
            actorUserId.Trim(),
            cancellationToken);

        var integrated = await _주문원장통합.하위원장연결Async(
            order.원장Id,
            new 주문하위원장연결요청
            {
                하위원장Id = extension.원장Id,
                역할 = role,
                필수여부 = false,
                기대Revision = existing is null ? request.기대원천Revision : null
            },
            actorUserId,
            cancellationToken);
        if (integrated.IsFailed)
        {
            return Result.Fail<무역확장원장응답>(integrated.Errors);
        }

        return Result.Ok(응답(extension, [integrated.Value.주문원장], alreadyProcessed));
    }

    private static 커뮤니티원장블록Dto 블록(
        string id,
        string title,
        IReadOnlyDictionary<string, string> data)
        => new()
        {
            BlockId = id,
            BlockType = CommunityLedgerBlockTypes.Generic,
            Title = title,
            State = "검토대기",
            Data = data
        };

    private static 커뮤니티원장참여자Dto 참여자(string actorUserId, string role)
        => new()
        {
            UserId = actorUserId.Trim(),
            DisplayName = actorUserId.Trim(),
            RoleLabel = role,
            ParticipationState = "참여중"
        };

    private static IReadOnlyDictionary<string, string> 기본확장속성(string idempotencyKey)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["IdempotencyKey"] = idempotencyKey,
            ["ExecutionMode"] = "Simulation",
            ["ContractExecutionAllowed"] = "false",
            ["PaymentExecutionAllowed"] = "false",
            ["DeclarationSubmissionAllowed"] = "false",
            ["ForwarderAutoSelectionAllowed"] = "false",
            ["ExternalTransmissionAllowed"] = "false"
        };

    private static 무역확장원장응답 응답(
        커뮤니티원장Dto ledger,
        IReadOnlyList<커뮤니티원장Dto> sources,
        bool alreadyProcessed)
        => new()
        {
            원장 = 요약(ledger),
            원천원장목록 = sources.Select(요약).ToArray(),
            이미처리됨 = alreadyProcessed,
            외부실행발생여부 = false,
            실행모드 = "Simulation"
        };

    private static 무역확장원장요약응답 요약(커뮤니티원장Dto ledger)
        => new()
        {
            원장Id = ledger.원장Id,
            Revision = ledger.Revision,
            원장템플릿Key = ledger.원장템플릿Key,
            제목 = ledger.제목,
            상태 = ledger.상태,
            원천원장Ids = ledger.포함원장목록.Select(x => x.원장Id).ToArray()
        };

    private static string 결정적원장Id(string kind, string source, string idempotencyKey)
    {
        var digest = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes($"{kind}|{source}|{idempotencyKey}")))
            .ToLowerInvariant();
        return $"{kind}:{digest[..24]}";
    }

    private static Result<string> 멱등키검증(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Fail<string>(
                new Error("Idempotency-Key가 필요합니다.")
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest));
        }

        var normalized = value.Trim();
        return normalized.Length <= 최대멱등키길이
            ? Result.Ok(normalized)
            : Result.Fail<string>(
                new Error($"Idempotency-Key는 {최대멱등키길이}자 이하여야 합니다.")
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest));
    }

    private static bool 액세스가능(
        커뮤니티원장Dto ledger,
        string actorUserId,
        bool isAdministrator)
    {
        if (isAdministrator)
        {
            return true;
        }

        var actor = actorUserId?.Trim();
        return !string.IsNullOrWhiteSpace(actor)
               && (string.Equals(ledger.생성자UserId, actor, StringComparison.Ordinal)
                   || ledger.참여자목록.Any(x => string.Equals(x.UserId, actor, StringComparison.Ordinal)));
    }

    private static string 제목(string? requested, string fallback)
        => string.IsNullOrWhiteSpace(requested) ? fallback : requested.Trim();

    private static Result<무역확장원장응답> BadRequest(string message)
        => Failure(message, StatusCodes.Status400BadRequest);

    private static Result<무역확장원장응답> NotFound(string message)
        => Failure(message, StatusCodes.Status404NotFound);

    private static Result<무역확장원장응답> Forbidden(string message)
        => Failure(message, StatusCodes.Status403Forbidden);

    private static Result<무역확장원장응답> Conflict(string message)
        => Failure(message, StatusCodes.Status409Conflict);

    private static Result<무역확장원장응답> Failure(string message, int statusCode)
        => Result.Fail<무역확장원장응답>(
            new Error(message).WithMetadata("StatusCode", statusCode));
}
