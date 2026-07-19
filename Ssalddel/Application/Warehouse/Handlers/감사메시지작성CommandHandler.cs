using FluentResults;
using MediatR;
using System.Text.Json;
using Ssalddel.Application.CommandProcessing;
using 살뜰.도메인.판매;
using 살뜰.도메인.설정;

namespace Ssalddel.Application.Warehouse;

public sealed class 감사메시지작성CommandHandler : IRequestHandler<감사메시지작성Command, Result<long>>
{
    private static readonly string[] 차단키워드 = ["환불", "환불요청", "클레임", "파손", "욕설", "광고"]; 

    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;

    public 감사메시지작성CommandHandler(
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
    }

    public async Task<Result<long>> Handle(감사메시지작성Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(
                _currentUserAccessor.UserId,
                _currentUserAccessor.Role,
                request.참여자Id,
                request.실행역할,
                out var 오류메시지))
        {
            return Result.Fail<long>(오류메시지 ?? "감사 메시지를 작성할 권한이 없습니다.");
        }

        if (string.IsNullOrWhiteSpace(request.메시지내용))
        {
            return Result.Fail<long>("감사 메시지 내용을 입력해주세요.");
        }

        if (ContainsBlockedKeyword(request.메시지내용))
        {
            return Result.Fail<long>("감사 메시지에 부적절하거나 민원성 키워드가 포함되어 있습니다. 고객문의 채널을 이용해주세요.");
        }

        if (!string.IsNullOrWhiteSpace(request.대상참여자Id))
        {
            var allowedParticipants = await GetAllowedParticipantsAsync(request.상품Id, cancellationToken);
            if (!allowedParticipants.Contains(request.대상참여자Id))
            {
                return Result.Fail<long>("해당 상품 여정의 처리 주체에게만 감사 메시지를 보낼 수 있습니다.");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new 감사메시지
        {
            상품Id = request.상품Id,
            주문Id = request.주문Id,
            통관절차Id = request.통관절차Id,
            발신자구분 = string.IsNullOrWhiteSpace(request.발신자구분) ? "익명구매자" : request.발신자구분.Trim(),
            발신참여자Id = request.발신참여자Id,
            대상역할 = request.대상역할.Trim(),
            대상참여자Id = request.대상참여자Id,
            대상표시명 = request.대상표시명.Trim(),
            메시지내용 = request.메시지내용.Trim(),
            공개가능여부 = request.공개가능여부,
            수신자에게전달여부 = false,
            검수상태 = 감사메시지검수상태.대기,
            작성일시 = now
        };

        _db.감사메시지.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _db.Command알림Outbox.Add(new Command알림Outbox
        {
            CommandName = nameof(감사메시지작성CommandHandler),
            EventName = "감사메시지작성됨",
            FeatureName = "Gratitude",
            Target = string.IsNullOrWhiteSpace(request.대상참여자Id) ? "Role" : "Participant",
            PayloadJson = JsonSerializer.Serialize(new
            {
                감사메시지Id = entity.Id,
                entity.상품Id,
                entity.주문Id,
                entity.통관절차Id,
                entity.대상역할,
                entity.대상참여자Id,
                entity.대상표시명,
                entity.메시지내용,
                entity.공개가능여부
            }),
            Status = "Pending",
            TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(entity.Id);
    }

    private static bool ContainsBlockedKeyword(string message)
    {
        foreach (var keyword in 차단키워드)
        {
            if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<HashSet<string>> GetAllowedParticipantsAsync(long 상품Id, CancellationToken cancellationToken)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);

        var productOwner = await _db.판매상품
            .AsNoTracking()
            .Where(x => x.Id == 상품Id)
            .Select(x => x.소유자UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(productOwner))
        {
            set.Add(productOwner);
        }

        var outbound = await _db.출고예정
            .AsNoTracking()
            .Where(x => x.판매상품Id == 상품Id)
            .Select(x => new { x.Id, x.입고요청Id, x.판매자UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(outbound?.판매자UserId))
        {
            set.Add(outbound.판매자UserId);
        }

        if (outbound?.입고요청Id is long inboundId)
        {
            var inboundSeller = await _db.입고요청
                .AsNoTracking()
                .Where(x => x.Id == inboundId)
                .Select(x => x.판매자UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(inboundSeller))
            {
                set.Add(inboundSeller);
            }
        }

        var outboundId = outbound?.Id;
        var outboundInboundId = outbound?.입고요청Id;

        var customsParticipant = await _db.통관절차
            .AsNoTracking()
            .Where(x => (outboundId != null && x.출고예정Id == outboundId)
                        || (outboundInboundId != null && x.입고요청Id == outboundInboundId))
            .Select(x => x.확정관세사참여자Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(customsParticipant))
        {
            set.Add(customsParticipant);
        }

        var logisticsParticipants = await _db.상품물류자산
            .AsNoTracking()
            .Where(x => x.상품Id == 상품Id)
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

        return set;
    }
}
