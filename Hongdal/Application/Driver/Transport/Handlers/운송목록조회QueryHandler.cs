using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송목록조회QueryHandler : IRequestHandler<운송목록조회Query, IReadOnlyList<기사운송요약응답>>
{
    private readonly HongdalContext _db;

    public 운송목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<기사운송요약응답>> Handle(운송목록조회Query request, CancellationToken cancellationToken)
    {
        var transports = await _db.배송_운송
            .AsNoTracking()
            .Where(x => x.기사_운송자 == request.기사Id)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var requestIds = transports.Select(x => x.운송번호).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
        var requestMap = requestIds.Length == 0
            ? new Dictionary<string, 운송증빙조건>(StringComparer.Ordinal)
            : await _db.화주운송의뢰
                .AsNoTracking()
                .Where(x => requestIds.Contains(x.의뢰Id))
                .Select(x => new { x.의뢰Id, x.결제수단, x.증빙방식, x.요청사항, x.정산메모 })
                .ToDictionaryAsync(
                    x => x.의뢰Id,
                    x => new 운송증빙조건(x.결제수단, x.증빙방식, x.요청사항, x.정산메모),
                    StringComparer.Ordinal,
                    cancellationToken);

        return transports.Select(x =>
        {
            if (!requestMap.TryGetValue(x.운송번호, out var shipperRequest))
            {
                shipperRequest = 운송증빙조건.Empty;
            }

            return new 기사운송요약응답
            {
                Id = x.Id,
                운송번호 = x.운송번호,
                상태 = x.상태,
                출발지 = x.출발지,
                도착지 = x.도착지,
                기사_운송자 = x.기사_운송자,
                출발_픽업 = x.출발_픽업,
                도착 = x.도착,
                운임 = x.운임,
                결제방식 = shipperRequest.결제수단,
                인수증필요 = IsReceiptRequired(shipperRequest.증빙방식, shipperRequest.결제수단),
                인수증서명필수 = IsReceiptSignatureRequired(shipperRequest.요청사항, shipperRequest.정산메모),
                UpdatedAt = x.UpdatedAt
            };
        }).ToArray();
    }

    private static bool IsReceiptRequired(string? evidenceMethod, string? paymentMethod)
        => string.Equals(evidenceMethod, "인수증", StringComparison.Ordinal)
           || (paymentMethod?.Contains("인수증", StringComparison.Ordinal) ?? false);

    private static bool IsReceiptSignatureRequired(string? requestText, string? settlementMemo)
        => ContainsSignatureRequired(requestText) || ContainsSignatureRequired(settlementMemo);

    private static bool ContainsSignatureRequired(string? value)
        => value?.Contains("서명필수", StringComparison.Ordinal) == true
           || value?.Contains("서명 필수", StringComparison.Ordinal) == true;

    private sealed record 운송증빙조건(string 결제수단, string 증빙방식, string 요청사항, string 정산메모)
    {
        public static readonly 운송증빙조건 Empty = new(string.Empty, string.Empty, string.Empty, string.Empty);
    }
}
