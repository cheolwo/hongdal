using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송문제신고CommandHandler : IRequestHandler<운송문제신고Command, 기사운송요약응답>
{
    private readonly HongdalContext _db;

    public 운송문제신고CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사운송요약응답> Handle(운송문제신고Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.배송_운송
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사_운송자 == request.기사Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException("운송을 찾을 수 없습니다.");
        }

        var issueText = string.IsNullOrWhiteSpace(request.사유) ? "문제 신고" : request.사유.Trim();
        var memo = string.IsNullOrWhiteSpace(request.메모) ? issueText : $"{issueText}: {request.메모!.Trim()}";
        entity.메모 = string.IsNullOrWhiteSpace(entity.메모) ? memo : $"{entity.메모}\n{memo}";
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new 기사운송요약응답
        {
            Id = entity.Id,
            운송번호 = entity.운송번호,
            상태 = entity.상태,
            출발지 = entity.출발지,
            도착지 = entity.도착지,
            기사_운송자 = entity.기사_운송자,
            출발_픽업 = entity.출발_픽업,
            도착 = entity.도착,
            운임 = entity.운임,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
