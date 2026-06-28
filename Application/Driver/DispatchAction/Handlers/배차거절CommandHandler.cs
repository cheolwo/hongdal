using MediatR;
using Microsoft.Extensions.Logging;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차거절CommandHandler : IRequestHandler<배차거절Command, 배차거절결과>
{
    private readonly IDriverRejectedRequestStore _rejectedRequestStore;
    private readonly ILogger<배차거절CommandHandler> _logger;

    public 배차거절CommandHandler(IDriverRejectedRequestStore rejectedRequestStore, ILogger<배차거절CommandHandler> logger)
    {
        _rejectedRequestStore = rejectedRequestStore;
        _logger = logger;
    }

    public async Task<배차거절결과> Handle(배차거절Command request, CancellationToken cancellationToken)
    {
        await _rejectedRequestStore.RejectAsync(request.기사Id, request.RequestId, cancellationToken);
        _logger.LogInformation(
            "Action={Action} DriverId={DriverId} RequestId={RequestId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DispatchRejected",
            request.기사Id,
            request.RequestId,
            "Success",
            "Driver rejected recommendation",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            DateTime.UtcNow);

        return new 배차거절결과(request.RequestId, "거절되었습니다.");
    }
}
