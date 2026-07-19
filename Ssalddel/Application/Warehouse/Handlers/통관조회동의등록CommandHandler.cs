using FluentResults;
using MediatR;
using Ssalddel.Application.CommandProcessing;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.통관;
using 살뜰.도메인.사용자;

namespace Ssalddel.Application.Warehouse;

public sealed class 통관조회동의등록CommandHandler : IRequestHandler<통관조회동의등록Command, Result<Unit>>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;
    private readonly IPersonalDataEncryptionService _암호화Service;
    private readonly I개인통관부호검증Service _검증Service;
    private readonly IPublisher _publisher;

    public 통관조회동의등록CommandHandler(
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사,
        IPersonalDataEncryptionService 암호화Service,
        I개인통관부호검증Service 검증Service,
        IPublisher publisher)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
        _암호화Service = 암호화Service;
        _검증Service = 검증Service;
        _publisher = publisher;
    }

    public async Task<Result<Unit>> Handle(통관조회동의등록Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(
                _currentUserAccessor.UserId,
                _currentUserAccessor.Role,
                request.참여자Id,
                request.실행역할,
                out var 오류메시지))
        {
            return Result.Fail<Unit>(오류메시지 ?? "통관 조회 동의 등록 권한이 없습니다.");
        }

        if (request.실행역할 != 살뜰역할유형.주문자)
        {
            return Result.Fail<Unit>("주문자 역할에서만 통관 조회 동의를 등록할 수 있습니다.");
        }

        var 절차 = await _db.통관절차
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.통관절차Id, cancellationToken);

        if (절차 is null)
        {
            return Result.Fail<Unit>("통관절차를 찾을 수 없습니다.");
        }

        if (절차.주문Id != request.주문Id)
        {
            return Result.Fail<Unit>("주문과 통관절차가 일치하지 않습니다.");
        }

        var 검증결과 = await _검증Service.검증Async(
            new 개인통관부호검증Request
            {
                개인통관고유부호 = request.개인통관고유부호,
                이름 = request.수취인이름,
                휴대폰번호 = request.휴대폰번호,
                우편번호 = request.우편번호
            },
            cancellationToken);

        if (!검증결과.성공여부)
        {
            return Result.Fail<Unit>($"개인통관고유부호 검증 실패: {검증결과.메시지}");
        }

        var 연동 = await _db.통관조회연동
            .FirstOrDefaultAsync(x => x.주문Id == request.주문Id && x.사용자Id == request.사용자Id, cancellationToken);

        var now = DateTime.UtcNow;

        if (연동 is null)
        {
            연동 = new 통관조회연동
            {
                주문Id = request.주문Id,
                사용자Id = request.사용자Id,
                통관절차Id = request.통관절차Id,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.통관조회연동.Add(연동);
        }

        연동.통관절차Id = request.통관절차Id;
        연동.개인통관고유부호암호문 = _암호화Service.Protect(request.개인통관고유부호);
        연동.사용자조회동의여부 = true;
        연동.동의시각 = DateTimeOffset.UtcNow;
        연동.연동상태 = 통관연동상태.조회대기;
        연동.마지막오류 = null;
        연동.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new 통관조회동의등록됨Event(
                request.주문Id,
                request.통관절차Id,
                request.사용자Id,
                now,
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty),
            cancellationToken);

        return Result.Ok(Unit.Value);
    }
}
