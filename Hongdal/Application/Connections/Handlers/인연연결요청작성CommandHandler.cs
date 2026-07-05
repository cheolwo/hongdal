using FluentResults;
using MediatR;
using System.Text.Json;
using Hongdal.Application.CommandProcessing;
using Hongdal.Application.Connections.Commands;
using 홍달.도메인.판매;
using 홍달.도메인.사용자;
using 홍달.도메인.설정;

namespace Hongdal.Application.Connections.Handlers;

public sealed class 인연연결요청작성CommandHandler : IRequestHandler<인연연결요청작성Command, Result<long>>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 인연연결요청작성CommandHandler(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<long>> Handle(인연연결요청작성Command request, CancellationToken cancellationToken)
    {
        if (!string.Equals(_currentUserAccessor.UserId, request.요청자참여자Id, StringComparison.Ordinal))
        {
            return Result.Fail<long>("현재 로그인 사용자와 요청자 참여자 정보가 일치하지 않습니다.");
        }

        if (string.IsNullOrWhiteSpace(request.요청목적))
        {
            return Result.Fail<long>("요청목적은 필수입니다.");
        }

        if (string.Equals(request.요청자참여자Id, request.대상자참여자Id, StringComparison.Ordinal))
        {
            return Result.Fail<long>("자기 자신에게는 인연 연결을 요청할 수 없습니다.");
        }

        var duplicated = await _db.인연연결요청
            .AnyAsync(x => x.요청자참여자Id == request.요청자참여자Id
                           && x.대상자참여자Id == request.대상자참여자Id
                           && x.상태 == 인연연결요청상태.대기, cancellationToken);

        if (duplicated)
        {
            return Result.Fail<long>("이미 대기 중인 인연 연결 요청이 있습니다.");
        }

        var allowedParticipants = await GetAllowedParticipantsAsync(request, cancellationToken);
        if (allowedParticipants.Count == 0)
        {
            return Result.Fail<long>("인연 연결 대상 검증을 위한 상품/주문/통관 문맥이 필요합니다.");
        }

        if (!allowedParticipants.Contains(request.대상자참여자Id))
        {
            return Result.Fail<long>("해당 상품 여정의 처리 주체에게만 인연 연결을 요청할 수 있습니다.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new 인연연결요청
        {
            요청자참여자Id = request.요청자참여자Id,
            요청자역할 = request.요청자역할,
            대상자참여자Id = request.대상자참여자Id,
            대상자역할 = request.대상자역할,
            감사메시지Id = request.감사메시지Id,
            주문Id = request.주문Id,
            통관절차Id = request.통관절차Id,
            요청목적 = request.요청목적.Trim(),
            요청메시지 = request.요청메시지.Trim(),
            상태 = 인연연결요청상태.대기,
            요청일시 = now
        };

        _db.인연연결요청.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _db.Command알림Outbox.Add(new Command알림Outbox
        {
            CommandName = nameof(인연연결요청작성CommandHandler),
            EventName = "인연연결요청생성됨",
            FeatureName = "Connection",
            Target = "Participant",
            PayloadJson = JsonSerializer.Serialize(new
            {
                인연연결요청Id = entity.Id,
                entity.요청자참여자Id,
                entity.요청자역할,
                entity.대상자참여자Id,
                entity.대상자역할,
                entity.요청목적,
                entity.요청메시지,
                entity.요청일시
            }),
            Status = "Pending",
            TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok(entity.Id);
    }

    private async Task<HashSet<string>> GetAllowedParticipantsAsync(인연연결요청작성Command request, CancellationToken cancellationToken)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);

        long? productId = null;

        if (request.감사메시지Id is long gratitudeId)
        {
            var gratitude = await _db.감사메시지
                .AsNoTracking()
                .Where(x => x.Id == gratitudeId)
                .Select(x => new { x.상품Id, x.대상참여자Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (gratitude is not null)
            {
                productId = gratitude.상품Id;
                if (!string.IsNullOrWhiteSpace(gratitude.대상참여자Id))
                {
                    set.Add(gratitude.대상참여자Id);
                }
            }
        }

        if (request.통관절차Id is long customsId)
        {
            var customs = await _db.통관절차
                .AsNoTracking()
                .Where(x => x.Id == customsId)
                .Select(x => new { x.확정관세사참여자Id, x.출고예정Id, x.입고요청Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (customs is not null)
            {
                if (!string.IsNullOrWhiteSpace(customs.확정관세사참여자Id))
                {
                    set.Add(customs.확정관세사참여자Id);
                }

                if (customs.출고예정Id is long outboundId)
                {
                    var outbound = await _db.출고예정
                        .AsNoTracking()
                        .Where(x => x.Id == outboundId)
                        .Select(x => new { x.판매상품Id, x.판매자UserId })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (outbound is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(outbound.판매자UserId))
                        {
                            set.Add(outbound.판매자UserId);
                        }

                        if (outbound.판매상품Id is long saleProductId)
                        {
                            productId ??= saleProductId;
                        }
                    }
                }
            }
        }

        if (request.주문Id is long orderId)
        {
            var outboundByOrder = await _db.출고예정
                .AsNoTracking()
                .Where(x => x.주문Id == orderId)
                .Select(x => new { x.판매상품Id, x.판매자UserId })
                .FirstOrDefaultAsync(cancellationToken);

            if (outboundByOrder is not null)
            {
                if (!string.IsNullOrWhiteSpace(outboundByOrder.판매자UserId))
                {
                    set.Add(outboundByOrder.판매자UserId);
                }

                if (outboundByOrder.판매상품Id is long saleProductId)
                {
                    productId ??= saleProductId;
                }
            }

            var inboundByOrder = await _db.입고요청
                .AsNoTracking()
                .Where(x => x.주문Id == orderId)
                .Select(x => x.판매자UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(inboundByOrder))
            {
                set.Add(inboundByOrder);
            }
        }

        if (productId is long pid)
        {
            var owner = await _db.판매상품
                .AsNoTracking()
                .Where(x => x.Id == pid)
                .Select(x => x.소유자UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(owner))
            {
                set.Add(owner);
            }

            var logisticsParticipants = await _db.상품물류자산
                .AsNoTracking()
                .Where(x => x.상품Id == pid)
                .Select(x => x.등록자Id)
                .Distinct()
                .ToArrayAsync(cancellationToken);

            foreach (var participant in logisticsParticipants)
            {
                if (!string.IsNullOrWhiteSpace(participant))
                {
                    set.Add(participant);
                }
            }
        }

        return set;
    }
}
