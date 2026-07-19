using MediatR;

namespace Ssalddel.Application.Immigration;

public sealed record VisaSupportRequestedEvent(
    string RequestId,
    string RequesterUserId,
    string ForeignPartnerName,
    string ForeignPartnerCountry,
    string? ForeignPartnerCompanyName,
    string? ImporterUserId,
    string? RelatedOrderReference,
    string? DesiredVisaType,
    string? SupportMemo,
    DateTime RequestedAtUtc,
    string TraceId) : INotification;
