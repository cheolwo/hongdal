using FluentResults;
using MediatR;

namespace Hongdal.Application.Immigration;

public sealed class VisaSupportRequestCommandHandler : IRequestHandler<VisaSupportRequestCommand, Result<VisaSupportRequestResult>>
{
    private readonly IPublisher _publisher;

    public VisaSupportRequestCommandHandler(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<Result<VisaSupportRequestResult>> Handle(VisaSupportRequestCommand request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation.IsFailed)
        {
            return Result.Fail<VisaSupportRequestResult>(validation.Errors);
        }

        var requestedAtUtc = DateTime.UtcNow;
        var requestId = $"visa-{requestedAtUtc:yyyyMMddHHmmss}-{Guid.NewGuid():N}";

        await _publisher.Publish(
            new VisaSupportRequestedEvent(
                requestId,
                request.RequesterUserId.Trim(),
                request.ForeignPartnerName.Trim(),
                request.ForeignPartnerCountry.Trim(),
                NormalizeNullable(request.ForeignPartnerCompanyName),
                NormalizeNullable(request.ImporterUserId),
                NormalizeNullable(request.RelatedOrderReference),
                NormalizeNullable(request.DesiredVisaType),
                NormalizeNullable(request.SupportMemo),
                requestedAtUtc,
                Guid.NewGuid().ToString("N")),
            cancellationToken);

        return Result.Ok(new VisaSupportRequestResult(requestId, "AdministrativeAgentNotificationPending", requestedAtUtc));
    }

    private static Result Validate(VisaSupportRequestCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.RequesterUserId))
        {
            return Result.Fail("비자 행정지원 요청자 정보가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.ForeignPartnerName))
        {
            return Result.Fail("외국인 파트너 이름이 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.ForeignPartnerCountry))
        {
            return Result.Fail("외국인 파트너 국가 정보가 필요합니다.");
        }

        return Result.Ok();
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
