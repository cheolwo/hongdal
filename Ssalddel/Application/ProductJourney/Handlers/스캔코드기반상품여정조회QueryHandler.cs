using MediatR;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.ProductJourney.Queries;

namespace Ssalddel.Application.ProductJourney.Handlers;

public sealed class 스캔코드기반상품여정조회QueryHandler : IRequestHandler<스캔코드기반상품여정조회Query, 상품여정조회응답?>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 스캔코드기반상품여정조회QueryHandler(SsalddelContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<상품여정조회응답?> Handle(스캔코드기반상품여정조회Query request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserAccessor.UserId))
        {
            return null;
        }

        var code = request.코드값?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var map = await _db.상품식별코드맵
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.코드값 == code && x.활성여부, cancellationToken);

        if (map is null)
        {
            return null;
        }

        var 상품 = await _db.판매상품
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == map.상품Id, cancellationToken);

        if (상품 is null)
        {
            return null;
        }

        var 출고 = await _db.출고예정
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.판매상품Id == 상품.Id, cancellationToken);

        var 입고 = 출고?.입고요청Id is long 입고요청Id
            ? await _db.입고요청.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 입고요청Id, cancellationToken)
            : null;

        var 주문자UserId = 입고?.주문자UserId ?? 출고?.주문자UserId;
        if (string.IsNullOrWhiteSpace(주문자UserId) || !string.Equals(주문자UserId, _currentUserAccessor.UserId, StringComparison.Ordinal))
        {
            return null;
        }

        var 통관 = await _db.통관절차
            .AsNoTracking()
            .FirstOrDefaultAsync(x => (출고 != null && x.출고예정Id == 출고.Id) || (입고 != null && x.입고요청Id == 입고.Id), cancellationToken);

        var 자산목록 = await _db.상품물류자산
            .AsNoTracking()
            .Where(x => x.상품Id == 상품.Id)
            .OrderByDescending(x => x.등록시각)
            .Take(100)
            .ToListAsync(cancellationToken);

        var 출품 = await _db.채널출품
            .AsNoTracking()
            .Where(x => x.판매상품Id == 상품.Id)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var 단계목록 = new List<상품여정단계응답>();

        단계목록.Add(new 상품여정단계응답
        {
            단계코드 = "SALE",
            단계명 = "판매상품 준비",
            상태 = 상품.상태,
            시각 = 상품.UpdatedAt,
            처리주체목록 = [
                new 처리주체응답
                {
                    참여자Id = 상품.소유자UserId,
                    역할 = "판매자",
                    표시명 = 상품.소유자UserId,
                    감사가능 = true,
                    친구요청가능 = true
                }
            ]
        });

        if (입고 is not null)
        {
            단계목록.Add(new 상품여정단계응답
            {
                단계코드 = "INBOUND",
                단계명 = "입고 요청",
                상태 = 입고.상태,
                시각 = 입고.UpdatedAt,
                처리주체목록 = [
                    new 처리주체응답
                    {
                        참여자Id = 입고.판매자UserId,
                        역할 = "판매자",
                        표시명 = 입고.판매자UserId,
                        감사가능 = true,
                        친구요청가능 = true
                    }
                ]
            });
        }

        if (출고 is not null)
        {
            단계목록.Add(new 상품여정단계응답
            {
                단계코드 = "OUTBOUND",
                단계명 = "출고 처리",
                상태 = 출고.상태,
                시각 = 출고.출고처리일시 ?? 출고.UpdatedAt,
                처리주체목록 = [
                    new 처리주체응답
                    {
                        참여자Id = 출고.판매자UserId,
                        역할 = "판매자",
                        표시명 = 출고.판매자UserId,
                        감사가능 = true,
                        친구요청가능 = true
                    }
                ]
            });
        }

        if (통관 is not null)
        {
            var 처리주체 = string.IsNullOrWhiteSpace(통관.확정관세사참여자Id)
                ? Array.Empty<처리주체응답>()
                : new[]
                {
                    new 처리주체응답
                    {
                        참여자Id = 통관.확정관세사참여자Id!,
                        역할 = "관세사",
                        표시명 = 통관.확정관세사참여자Id!,
                        감사가능 = true,
                        친구요청가능 = true
                    }
                };

            단계목록.Add(new 상품여정단계응답
            {
                단계코드 = "CUSTOMS",
                단계명 = "통관 처리",
                상태 = 통관.상태.ToString(),
                시각 = 통관.UpdatedAt,
                처리주체목록 = 처리주체
            });
        }

        if (자산목록.Count > 0)
        {
            단계목록.Add(new 상품여정단계응답
            {
                단계코드 = "LOGISTICS_ASSET",
                단계명 = "물류 증빙 등록",
                상태 = $"자산 {자산목록.Count}건",
                시각 = 자산목록.Max(x => x.등록시각),
                처리주체목록 = 자산목록
                    .Where(x => !string.IsNullOrWhiteSpace(x.등록자Id))
                    .GroupBy(x => x.등록자Id)
                    .Select(g => new 처리주체응답
                    {
                        참여자Id = g.Key,
                        역할 = "물류처리자",
                        표시명 = g.Key,
                        감사가능 = true,
                        친구요청가능 = true
                    })
                    .ToArray()
            });
        }

        if (출품 is not null)
        {
            단계목록.Add(new 상품여정단계응답
            {
                단계코드 = "MARKET",
                단계명 = "판매 채널 게시",
                상태 = $"{출품.출품상태}/{출품.동기화상태}",
                시각 = 출품.UpdatedAt,
                처리주체목록 = Array.Empty<처리주체응답>()
            });
        }

        return new 상품여정조회응답
        {
            코드값 = code,
            상품Id = 상품.Id,
            상품명 = 상품.대표상품명,
            주문Id = 출고?.주문Id ?? 입고?.주문Id,
            단계목록 = 단계목록
                .OrderBy(x => x.시각 ?? DateTimeOffset.MinValue)
                .ToArray()
        };
    }
}
