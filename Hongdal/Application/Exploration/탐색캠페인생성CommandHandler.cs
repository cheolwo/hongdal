using FluentResults;
using Hongdal.Contracts.Common.Exploration;
using Hongdal.Application.CommandProcessing;
using 탐색캠페인응답Dto = Hongdal.Contracts.Common.Exploration.탐색캠페인응답;

namespace Hongdal.Application.Exploration;

public sealed class 탐색캠페인생성CommandHandler : IRequestHandler<탐색캠페인생성Command, Result<탐색캠페인응답Dto>>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 탐색캠페인생성CommandHandler(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<탐색캠페인응답Dto>> Handle(탐색캠페인생성Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserAccessor.UserId))
        {
            return Result.Fail<탐색캠페인응답Dto>("인증 사용자 정보를 찾을 수 없습니다.");
        }

        if (!string.Equals(request.요청.개시자역할, _currentUserAccessor.Role, StringComparison.Ordinal))
        {
            return Result.Fail<탐색캠페인응답Dto>("현재 로그인 역할과 탐색 개시자 역할이 일치하지 않습니다.");
        }

        var entity = new 탐색캠페인
        {
            개시자UserId = _currentUserAccessor.UserId,
            개시자역할 = request.요청.개시자역할,
            대상역할 = request.요청.대상역할,
            탐색유형 = request.요청.탐색유형,
            탐색명 = request.요청.탐색명,
            운행예정일 = request.요청.운행예정일,
            출발권역 = request.요청.출발권역,
            희망도착권역 = request.요청.희망도착권역,
            경유권역Json = request.요청.경유권역Json,
            차량종류 = request.요청.차량종류,
            최대적재중량Kg = request.요청.최대적재중량Kg,
            최대적재부피Cbm = request.요청.최대적재부피Cbm,
            모집대상수 = Math.Max(1, request.요청.모집대상수),
            메모 = request.요청.메모 ?? string.Empty,
            탐색상태 = 상태값.탐색캠페인상태.초안,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.탐색캠페인.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(new 탐색캠페인응답Dto
        {
            Id = entity.Id,
            개시자UserId = entity.개시자UserId,
            개시자역할 = entity.개시자역할,
            대상역할 = entity.대상역할,
            탐색유형 = entity.탐색유형,
            탐색명 = entity.탐색명,
            운행예정일 = entity.운행예정일,
            출발권역 = entity.출발권역,
            희망도착권역 = entity.희망도착권역,
            차량종류 = entity.차량종류,
            최대적재중량Kg = entity.최대적재중량Kg,
            최대적재부피Cbm = entity.최대적재부피Cbm,
            모집대상수 = entity.모집대상수,
            탐색상태 = entity.탐색상태,
            메모 = entity.메모,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        });
    }
}
