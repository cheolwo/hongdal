using 살뜰.Services.Options;

namespace Ssalddel.Application.CommandProcessing;

public sealed record Command후처리Context(
    string CommandName,
    object Request,
    object? Response,
    bool IsSuccess,
    string TraceId,
    DateTime OccurredAt,
    CommandProcessingRule Rule);
