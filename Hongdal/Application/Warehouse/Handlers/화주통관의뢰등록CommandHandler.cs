using System.Diagnostics;
using FluentResults;
using MediatR;
using 홍달.도메인.통관;

namespace Hongdal.Application.Warehouse;

public sealed class 화주통관의뢰등록CommandHandler : IRequestHandler<화주통관의뢰등록Command, Result<화주통관의뢰등록결과>>
{
    private const long 미지정창고Id = 0;

    private readonly HongdalContext _db;
    private readonly IPublisher _publisher;

    public 화주통관의뢰등록CommandHandler(HongdalContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<Result<화주통관의뢰등록결과>> Handle(화주통관의뢰등록Command request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation.IsFailed)
        {
            return Result.Fail<화주통관의뢰등록결과>(validation.Errors);
        }

        if (!string.IsNullOrWhiteSpace(request.대상관세사참여자Id))
        {
            var brokerExists = await _db.관세사프로필.AnyAsync(x =>
                x.참여자Id == request.대상관세사참여자Id &&
                x.관리자승인여부 &&
                x.수임가능여부,
                cancellationToken);

            if (!brokerExists)
            {
                return Result.Fail<화주통관의뢰등록결과>("대상 관세사가 수임 가능한 상태가 아닙니다.");
            }
        }

        var now = DateTime.UtcNow;
        var 절차 = new 통관절차
        {
            주문Id = request.주문Id,
            주문참조번호 = request.주문참조번호?.Trim() ?? string.Empty,
            출고창고Id = request.출고창고Id ?? 미지정창고Id,
            입고창고Id = request.입고창고Id ?? 미지정창고Id,
            물류거래방향 = request.물류거래방향,
            대표상품명 = request.대표상품명.Trim(),
            상태 = string.IsNullOrWhiteSpace(request.대상관세사참여자Id)
                ? 통관절차상태.관세사검토대기
                : 통관절차상태.수임요청,
            확정관세사참여자Id = null,
            메모 = BuildMemo(request),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.통관절차.Add(절차);
        await _db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.대상관세사참여자Id))
        {
            _db.통관수임.Add(new 통관수임
            {
                통관절차Id = 절차.Id,
                관세사참여자Id = request.대상관세사참여자Id,
                상태 = 통관수임상태.수임요청,
                요청시각 = DateTimeOffset.UtcNow,
                메모 = request.요청메모,
                CreatedAt = now,
                UpdatedAt = now
            });

            await _db.SaveChangesAsync(cancellationToken);
        }

        await _publisher.Publish(
            new 화주통관의뢰등록됨Event(
                절차.Id,
                request.화주UserId.Trim(),
                request.의뢰유형.Trim(),
                request.물류거래방향,
                request.대상관세사참여자Id,
                절차.대표상품명,
                now,
                Activity.Current?.TraceId.ToString() ?? string.Empty),
            cancellationToken);

        return Result.Ok(new 화주통관의뢰등록결과(절차.Id, request.의뢰유형.Trim(), 절차.상태));
    }

    private static Result Validate(화주통관의뢰등록Command request)
    {
        if (string.IsNullOrWhiteSpace(request.화주UserId))
        {
            return Result.Fail("화주 사용자 정보가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.의뢰유형))
        {
            return Result.Fail("통관 의뢰유형이 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.대표상품명))
        {
            return Result.Fail("대표상품명이 필요합니다.");
        }

        if (request.물류거래방향 == 물류거래방향.국내)
        {
            return Result.Fail("국내 거래에는 통관 의뢰를 등록할 수 없습니다.");
        }

        return Result.Ok();
    }

    private static string BuildMemo(화주통관의뢰등록Command request)
    {
        var parts = new List<string>
        {
            $"의뢰유형={request.의뢰유형.Trim()}",
            $"화주UserId={request.화주UserId.Trim()}"
        };

        if (!string.IsNullOrWhiteSpace(request.요청메모))
        {
            parts.Add($"요청메모={request.요청메모.Trim()}");
        }

        return string.Join("; ", parts);
    }
}
