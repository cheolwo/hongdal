using MediatR;
using 살뜰.도메인.창고;
using 살뜰.도메인.통관;

namespace Ssalddel.Application.Warehouse;

public sealed class 국제거래통관절차생성EventHandler : INotificationHandler<판매자상품출고됨Event>
{
    private readonly SsalddelContext _db;
    private readonly IPublisher _publisher;
    private readonly ILogger<국제거래통관절차생성EventHandler> _logger;

    public 국제거래통관절차생성EventHandler(
        SsalddelContext db,
        IPublisher publisher,
        ILogger<국제거래통관절차생성EventHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(판매자상품출고됨Event notification, CancellationToken cancellationToken)
    {
        if (notification.출고예정Ids.Count == 0)
        {
            return;
        }

        var 출고목록 = await _db.출고예정
            .Where(x => notification.출고예정Ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var 출고 in 출고목록)
        {
            if (출고.입고요청Id is null)
            {
                continue;
            }

            var 입고 = await _db.입고요청
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == 출고.입고요청Id.Value, cancellationToken);
            if (입고 is null)
            {
                continue;
            }

            var 출고창고 = await _db.창고.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 출고.출고창고Id, cancellationToken);
            var 입고창고 = await _db.창고.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 입고.창고Id, cancellationToken);
            if (출고창고 is null || 입고창고 is null)
            {
                continue;
            }

            var 물류거래방향 = 판정거래방향(출고창고.국가코드, 입고창고.국가코드);
            if (물류거래방향 == 물류거래방향.국내)
            {
                continue;
            }

            var alreadyExists = await _db.통관절차.AnyAsync(x =>
                x.출고예정Id == 출고.Id &&
                x.입고요청Id == 입고.Id &&
                x.상태 != 통관절차상태.완료,
                cancellationToken);

            if (alreadyExists)
            {
                continue;
            }

            var now = DateTime.UtcNow;
            var 절차 = new 통관절차
            {
                주문Id = 출고.주문Id,
                주문참조번호 = 출고.주문참조번호,
                출고예정Id = 출고.Id,
                입고요청Id = 입고.Id,
                출고창고Id = 출고.출고창고Id,
                입고창고Id = 입고.창고Id,
                물류거래방향 = 물류거래방향,
                대표상품명 = 출고.상품명,
                상태 = 통관절차상태.관세사검토대기,
                메모 = "국제거래 감지로 생성된 통관절차",
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.통관절차.Add(절차);
            await _db.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(
                new 통관절차생성됨Event(
                    절차.Id,
                    절차.주문Id,
                    절차.주문참조번호,
                    절차.물류거래방향,
                    절차.출고창고Id,
                    절차.입고창고Id,
                    절차.대표상품명,
                    now,
                    notification.TraceId),
                cancellationToken);

            _logger.LogInformation(
                "통관절차 생성 완료: 통관절차Id={통관절차Id}, 주문참조번호={주문참조번호}, 방향={방향}",
                절차.Id,
                절차.주문참조번호,
                절차.물류거래방향);
        }
    }

    private static 물류거래방향 판정거래방향(string? 출고국가코드, string? 입고국가코드)
    {
        var from = NormalizeCountryCode(출고국가코드);
        var to = NormalizeCountryCode(입고국가코드);

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            return 물류거래방향.국내;
        }

        if (string.Equals(to, "KR", StringComparison.OrdinalIgnoreCase))
        {
            return 물류거래방향.수입;
        }

        return 물류거래방향.수출;
    }

    private static string NormalizeCountryCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "KR";
        }

        return value.Trim().ToUpperInvariant();
    }
}
