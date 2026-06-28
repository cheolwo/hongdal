using FluentResults;
using Hongdal.Contracts.Shipper.Request;
using 홍달.Services.External.Google;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰생성CommandHandler : IRequestHandler<의뢰생성Command, Result<화주운송의뢰응답>>
{
    private static readonly string[] AllowedPaymentStatuses = 상태값.결제상태.허용값;

    private readonly HongdalContext _db;
    private readonly IGeocodingService _geocodingService;

    public 의뢰생성CommandHandler(HongdalContext db, IGeocodingService geocodingService)
    {
        _db = db;
        _geocodingService = geocodingService;
    }

    public async Task<Result<화주운송의뢰응답>> Handle(의뢰생성Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.화물종류))
        {
            return Result.Fail<화주운송의뢰응답>("화물종류 is required");
        }

        if (string.IsNullOrWhiteSpace(request.픽업도로명주소))
        {
            return Result.Fail<화주운송의뢰응답>("픽업 주소 도로명주소 is required");
        }

        if (string.IsNullOrWhiteSpace(request.픽업연락처전화번호))
        {
            return Result.Fail<화주운송의뢰응답>("픽업 연락처 전화번호 is required");
        }

        if (request.픽업시간창시작일시 >= request.픽업시간창종료일시)
        {
            return Result.Fail<화주운송의뢰응답>("pickup.window.startAt must be before endAt");
        }

        if (!string.IsNullOrWhiteSpace(request.클라이언트요청Id) && string.IsNullOrWhiteSpace(request.화주Id))
        {
            return Result.Fail<화주운송의뢰응답>("화주Id is required when clientRequestId is provided");
        }

        var clientRequestId = request.클라이언트요청Id?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(clientRequestId))
        {
            var duplicate = await _db.화주운송의뢰
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.화주Id == request.화주Id && r.클라이언트요청Id == clientRequestId, cancellationToken);
            if (duplicate != null)
            {
                return Result.Fail<화주운송의뢰응답>("동일한 클라이언트요청Id로 이미 생성된 의뢰가 있습니다.");
            }
        }

        var paymentStatus = string.IsNullOrWhiteSpace(request.결제상태) ? 상태값.결제상태.결제대기 : request.결제상태.Trim();
        if (!AllowedPaymentStatuses.Contains(paymentStatus))
        {
            return Result.Fail<화주운송의뢰응답>($"결제상태 must be one of: {string.Join(", ", AllowedPaymentStatuses)}");
        }

        var (pickupLat, pickupLng) = await ResolveCoordinatesAsync(request.픽업도로명주소, request.픽업상세주소, cancellationToken);
        var (dropoffLat, dropoffLng) = await ResolveCoordinatesAsync(request.하차도로명주소, request.하차상세주소, cancellationToken);

        var entity = new 화주운송의뢰
        {
            의뢰Id = Guid.NewGuid().ToString(),
            화주Id = request.화주Id ?? string.Empty,
            화물종류 = request.화물종류,
            화물설명 = request.화물설명 ?? string.Empty,
            화물수량 = request.화물수량,
            화물중량Kg = request.화물중량Kg,
            화물부피Cbm = request.화물부피Cbm,
            화물파손주의여부 = request.화물파손주의여부,
            화물온도조건 = request.화물온도조건 ?? "상온",
            운송방식 = request.운송방식 ?? "혼적",
            차량종류 = request.차량종류 ?? string.Empty,
            결제수단 = request.결제수단 ?? "카드",
            결제예정금액 = request.결제예정금액,
            픽업_도로명주소 = request.픽업도로명주소,
            픽업_상세주소 = request.픽업상세주소 ?? string.Empty,
            픽업_위도 = pickupLat,
            픽업_경도 = pickupLng,
            픽업_연락처_이름 = request.픽업연락처이름,
            픽업_연락처_전화번호 = request.픽업연락처전화번호,
            픽업_시간창_시작일시 = request.픽업시간창시작일시,
            픽업_시간창_종료일시 = request.픽업시간창종료일시,
            하차_도로명주소 = request.하차도로명주소,
            하차_상세주소 = request.하차상세주소 ?? string.Empty,
            하차_위도 = dropoffLat,
            하차_경도 = dropoffLng,
            하차_연락처_이름 = request.하차연락처이름,
            하차_연락처_전화번호 = request.하차연락처전화번호,
            하차_시간창_시작일시 = request.하차시간창시작일시,
            하차_시간창_종료일시 = request.하차시간창종료일시,
            서비스레벨 = request.서비스레벨 ?? string.Empty,
            요청사항 = request.요청사항 ?? string.Empty,
            대기료 = request.대기료,
            수작업비 = request.수작업비,
            할증 = request.할증,
            클라이언트요청Id = clientRequestId,
            상태 = 상태값.의뢰상태.생성됨,
            결제상태 = paymentStatus,
            배차상태 = 상태값.배차상태.미시작,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.AddAsync(entity, cancellationToken);
        await 화주운송의뢰매퍼.UpsertCargoRequirementAsync(_db, entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(화주운송의뢰매퍼.To응답(entity));
    }

    private async Task<(decimal? lat, decimal? lng)> ResolveCoordinatesAsync(string? roadAddress, string? detailAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roadAddress))
        {
            return (null, null);
        }

        var query = roadAddress;
        if (!string.IsNullOrWhiteSpace(detailAddress))
        {
            query += " " + detailAddress;
        }

        var location = await _geocodingService.GeocodeAsync(query);
        if (!location.HasValue)
        {
            return (null, null);
        }

        return (location.Value.lat, location.Value.lng);
    }
}
