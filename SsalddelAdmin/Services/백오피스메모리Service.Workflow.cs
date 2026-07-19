namespace SsalddelAdmin.Services;

public sealed partial class 백오피스메모리Service
{
    public Task<운송워크플로우관제상세응답?> 운송워크플로우관제상세조회Async(string requestId, CancellationToken cancellationToken = default)
    {
        var normalized = requestId.Trim();
        var request = _requests.FirstOrDefault(x => string.Equals(x.의뢰Id, normalized, StringComparison.OrdinalIgnoreCase));
        var driverId = _transports.FirstOrDefault(x => string.Equals(x.운송번호, normalized, StringComparison.OrdinalIgnoreCase))?.기사_운송자;
        var detail = 운송워크플로우관제상세Factory.Build(
            normalized,
            request,
            _payments.Where(x => string.Equals(x.의뢰Id, normalized, StringComparison.OrdinalIgnoreCase)).ToArray(),
            _dispatchWait.Where(x => string.Equals(x.의뢰Id, normalized, StringComparison.OrdinalIgnoreCase)).ToArray(),
            _transports.Where(x => string.Equals(x.운송번호, normalized, StringComparison.OrdinalIgnoreCase)).ToArray(),
            _transportEvents.Where(x => string.Equals(x.의뢰Id, normalized, StringComparison.OrdinalIgnoreCase)).ToArray(),
            _filePods.Where(x => string.Equals(x.RequestId, normalized, StringComparison.OrdinalIgnoreCase)).ToArray(),
            string.IsNullOrWhiteSpace(driverId)
                ? Array.Empty<기사월정산관리응답>()
                : _settlements.Where(x => string.Equals(x.기사Id, driverId, StringComparison.OrdinalIgnoreCase)).ToArray());

        return Task.FromResult(detail);
    }
}
