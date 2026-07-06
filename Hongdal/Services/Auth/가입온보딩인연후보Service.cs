using Hongdal.Application.CommandProcessing;
using 홍달.도메인.사용자;

namespace Hongdal.Services.Auth;

public interface I가입온보딩인연후보Service
{
    Task<IReadOnlyList<가입인연후보항목응답>> 후보조회Async(가입인연후보조회요청 request, CancellationToken cancellationToken = default);
}

public sealed class 가입온보딩인연후보Service : I가입온보딩인연후보Service
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 가입온보딩인연후보Service(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyList<가입인연후보항목응답>> 후보조회Async(가입인연후보조회요청 request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return [];
        }

        var orderReference = request.주문참조번호.Trim();
        if (string.IsNullOrWhiteSpace(orderReference))
        {
            return [];
        }

        var identityHints = BuildIdentityHints(currentUserId, request);
        var max = Math.Clamp(request.최대건수, 1, 50);

        var outboundCandidates = await _db.출고예정
            .AsNoTracking()
            .Where(x => x.주문참조번호 == orderReference)
            .Select(x => new
            {
                x.Id,
                x.주문Id,
                x.주문참조번호,
                x.주문자UserId,
                x.판매자UserId,
                x.출고창고Id,
                x.상품명
            })
            .ToArrayAsync(cancellationToken);

        var inboundCandidates = await _db.입고요청
            .AsNoTracking()
            .Where(x => x.주문참조번호 == orderReference || x.원주문참조번호 == orderReference)
            .Select(x => new
            {
                x.Id,
                x.주문Id,
                x.주문참조번호,
                x.원주문참조번호,
                x.주문자UserId,
                x.판매자UserId,
                x.창고Id,
                x.공급처명
            })
            .ToArrayAsync(cancellationToken);

        var warehouseIds = outboundCandidates
            .Select(x => x.출고창고Id)
            .Concat(inboundCandidates.Select(x => x.창고Id))
            .Distinct()
            .ToArray();

        var warehouseUsers = await _db.창고사용자
            .AsNoTracking()
            .Where(x => warehouseIds.Contains(x.창고Id) && x.IsPrimary)
            .Select(x => new { x.창고Id, x.UserId })
            .ToArrayAsync(cancellationToken);

        var customsParticipants = await _db.통관절차
            .AsNoTracking()
            .Where(x => x.주문참조번호 == orderReference && x.확정관세사참여자Id != null)
            .Select(x => new { x.Id, x.주문참조번호, x.확정관세사참여자Id })
            .ToArrayAsync(cancellationToken);

        var results = new List<가입인연후보항목응답>();

        foreach (var outbound in outboundCandidates)
        {
            var confidence = CalculateConfidence(outbound.주문자UserId, identityHints, hasOrderReferenceMatch: true);
            if (confidence <= 0)
            {
                continue;
            }

            AddCandidate(
                results,
                $"outbound:{outbound.Id}:{outbound.판매자UserId}",
                outbound.판매자UserId,
                홍달역할유형.판매자,
                outbound.주문Id,
                outbound.주문참조번호,
                "출고예정",
                $"{outbound.상품명} 출고 예정 기록과 가입자 단서가 일치합니다.",
                confidence,
                "가입 후 과거 주문 관련 판매자 연결",
                $"{outbound.주문참조번호} 주문과 관련해 판매자와 연결을 요청합니다.");

            foreach (var warehouseUser in warehouseUsers.Where(x => x.창고Id == outbound.출고창고Id))
            {
                AddCandidate(
                    results,
                    $"warehouse-outbound:{outbound.Id}:{warehouseUser.UserId}",
                    warehouseUser.UserId,
                    홍달역할유형.창고관리자,
                    outbound.주문Id,
                    outbound.주문참조번호,
                    "창고출고작업",
                    $"{outbound.상품명} 출고 작업 창고 담당자 후보입니다.",
                    Math.Min(0.9m, confidence),
                    "가입 후 과거 주문 관련 창고 담당자 연결",
                    $"{outbound.주문참조번호} 출고 작업과 관련해 창고 담당자와 연결을 요청합니다.");
            }
        }

        foreach (var inbound in inboundCandidates)
        {
            var confidence = CalculateConfidence(inbound.주문자UserId, identityHints, hasOrderReferenceMatch: true);
            if (confidence <= 0)
            {
                continue;
            }

            var inboundOrderReference = string.IsNullOrWhiteSpace(inbound.주문참조번호)
                ? inbound.원주문참조번호
                : inbound.주문참조번호;

            AddCandidate(
                results,
                $"inbound:{inbound.Id}:{inbound.판매자UserId}",
                inbound.판매자UserId,
                홍달역할유형.판매자,
                inbound.주문Id,
                inboundOrderReference,
                "입고요청",
                $"{inbound.공급처명} 입고 예정 기록과 가입자 단서가 일치합니다.",
                confidence,
                "가입 후 과거 입고 관련 판매자 연결",
                $"{orderReference} 입고/주문 기록과 관련해 판매자와 연결을 요청합니다.");

            foreach (var warehouseUser in warehouseUsers.Where(x => x.창고Id == inbound.창고Id))
            {
                AddCandidate(
                    results,
                    $"warehouse-inbound:{inbound.Id}:{warehouseUser.UserId}",
                    warehouseUser.UserId,
                    홍달역할유형.창고관리자,
                    inbound.주문Id,
                    inboundOrderReference,
                    "창고입고작업",
                    $"{inbound.공급처명} 입고 작업 창고 담당자 후보입니다.",
                    Math.Min(0.9m, confidence),
                    "가입 후 과거 입고 관련 창고 담당자 연결",
                    $"{orderReference} 입고 작업과 관련해 창고 담당자와 연결을 요청합니다.");
            }
        }

        foreach (var customs in customsParticipants)
        {
            var relatedOrderer = outboundCandidates.FirstOrDefault(x => x.주문참조번호 == customs.주문참조번호)?.주문자UserId
                ?? inboundCandidates.FirstOrDefault(x => x.주문참조번호 == customs.주문참조번호 || x.원주문참조번호 == customs.주문참조번호)?.주문자UserId
                ?? string.Empty;

            var confidence = CalculateConfidence(relatedOrderer, identityHints, hasOrderReferenceMatch: true);
            if (confidence <= 0 || string.IsNullOrWhiteSpace(customs.확정관세사참여자Id))
            {
                continue;
            }

            AddCandidate(
                results,
                $"customs:{customs.Id}:{customs.확정관세사참여자Id}",
                customs.확정관세사참여자Id,
                홍달역할유형.관세사,
                null,
                customs.주문참조번호,
                "통관절차",
                $"{customs.주문참조번호} 통관 절차에 연결된 관세사 후보입니다.",
                Math.Min(0.85m, confidence),
                "가입 후 과거 통관 관련 관세사 연결",
                $"{customs.주문참조번호} 통관 절차와 관련해 관세사와 연결을 요청합니다.");
        }

        return results
            .GroupBy(x => x.후보키, StringComparer.Ordinal)
            .Select(x => x.OrderByDescending(c => c.신뢰도).First())
            .OrderByDescending(x => x.신뢰도)
            .ThenBy(x => x.대상자참여자Id)
            .Take(max)
            .ToArray();
    }

    private static void AddCandidate(
        List<가입인연후보항목응답> results,
        string candidateKey,
        string participantId,
        홍달역할유형 role,
        long? orderId,
        string orderReference,
        string evidenceType,
        string evidenceDescription,
        decimal confidence,
        string purpose,
        string message)
    {
        if (string.IsNullOrWhiteSpace(participantId))
        {
            return;
        }

        results.Add(new 가입인연후보항목응답
        {
            후보키 = candidateKey,
            대상자참여자Id = participantId,
            대상표시명 = MaskParticipant(participantId),
            대상자역할 = role,
            주문Id = orderId,
            주문참조번호 = orderReference,
            연결근거유형 = evidenceType,
            연결근거설명 = evidenceDescription,
            신뢰도 = confidence,
            추천요청목적 = purpose,
            추천요청메시지 = message
        });
    }

    private static HashSet<string> BuildIdentityHints(string currentUserId, 가입인연후보조회요청 request)
    {
        var hints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Normalize(currentUserId)
        };

        AddHint(hints, request.표시이름);
        AddHint(hints, request.이메일);
        AddHint(hints, request.연락처);
        return hints;
    }

    private static void AddHint(HashSet<string> hints, string value)
    {
        var normalized = Normalize(value);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            hints.Add(normalized);
        }
    }

    private static decimal CalculateConfidence(string ordererIdentity, HashSet<string> identityHints, bool hasOrderReferenceMatch)
    {
        var normalizedOrderer = Normalize(ordererIdentity);
        if (string.IsNullOrWhiteSpace(normalizedOrderer))
        {
            return 0m;
        }

        if (identityHints.Contains(normalizedOrderer))
        {
            return hasOrderReferenceMatch ? 0.95m : 0.75m;
        }

        return 0m;
    }

    private static string Normalize(string value)
        => new((value ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

    private static string MaskParticipant(string participantId)
    {
        var value = participantId.Trim();
        if (value.Length <= 4)
        {
            return value.Length == 0 ? "연결 후보" : $"{value[0]}***";
        }

        return $"{value[..Math.Min(3, value.Length)]}***{value[^2..]}";
    }
}

public sealed class 가입인연후보조회요청
{
    public string 주문참조번호 { get; set; } = string.Empty;
    public string 표시이름 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public int 최대건수 { get; set; } = 20;
}

public sealed class 가입인연후보항목응답
{
    public string 후보키 { get; init; } = string.Empty;
    public string 대상자참여자Id { get; init; } = string.Empty;
    public string 대상표시명 { get; init; } = string.Empty;
    public 홍달역할유형 대상자역할 { get; init; }
    public long? 주문Id { get; init; }
    public string 주문참조번호 { get; init; } = string.Empty;
    public string 연결근거유형 { get; init; } = string.Empty;
    public string 연결근거설명 { get; init; } = string.Empty;
    public decimal 신뢰도 { get; init; }
    public string 추천요청목적 { get; init; } = string.Empty;
    public string 추천요청메시지 { get; init; } = string.Empty;
}
