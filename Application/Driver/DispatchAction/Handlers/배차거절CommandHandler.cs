namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차거절CommandHandler : IRequestHandler<배차거절Command, FluentResults.Result<배차거절결과>>
{
    private readonly IDriverRejectedRequestStore _rejectedRequestStore;
    private readonly ILogger<배차거절CommandHandler> _logger;

    public 배차거절CommandHandler(IDriverRejectedRequestStore rejectedRequestStore, ILogger<배차거절CommandHandler> logger)
    {
        _rejectedRequestStore = rejectedRequestStore;
        _logger = logger;
    }

    public async Task<FluentResults.Result<배차거절결과>> Handle(배차거절Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.기사Id))
        {
            return FluentResults.Result.Fail<배차거절결과>("기사 인증 정보가 없습니다.");
        }

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return FluentResults.Result.Fail<배차거절결과>("의뢰Id는 필수입니다.");
        }

        await _rejectedRequestStore.RejectAsync(request.기사Id, request.RequestId, cancellationToken);
        var now = DateTime.UtcNow;
        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} RequestId={RequestId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DispatchRejected",
            request.기사Id,
            request.RequestId,
            "Success",
            "Driver rejected recommendation",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            now);

        return FluentResults.Result.Ok(new 배차거절결과(request.RequestId, "거절되었습니다."));
    }
}
