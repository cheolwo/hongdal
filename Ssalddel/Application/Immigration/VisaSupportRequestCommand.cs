using FluentResults;
using MediatR;

namespace Ssalddel.Application.Immigration;

public sealed record VisaSupportRequestCommand(
    string RequesterUserId,
    string ForeignPartnerName,
    string ForeignPartnerCountry,
    string? ForeignPartnerCompanyName,
    string? ImporterUserId,
    string? RelatedOrderReference,
    string? DesiredVisaType,
    string? SupportMemo) : IRequest<Result<VisaSupportRequestResult>>;

public sealed record VisaSupportRequestResult(
    string RequestId,
    string Status,
    DateTime RequestedAtUtc);
