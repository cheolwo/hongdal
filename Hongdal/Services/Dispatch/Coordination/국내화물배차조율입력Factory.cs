using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 홍달.도메인.공통;
using 홍달.도메인.배차;
using 홍달.도메인.기사;
using 홍달.도메인.차량;
using 홍달.도메인.화물;
using 홍달.도메인.화주;
using 홍달.Services.Dispatch.Queue;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.Services.Storage.Local;

namespace 홍달.Services.Dispatch.Coordination;

public interface I국내화물배차조율입력Factory
{
    Task<국내화물배차조율입력> 생성Async(
        국내화물배차조율입력요청 request,
        CancellationToken cancellationToken = default);
}

public sealed partial class 국내화물배차조율입력Factory : I국내화물배차조율입력Factory
{
    private readonly HongdalContext _db;
    private readonly I국내화물운송기사상태Store _기사상태Store;
    private readonly IDriverRejectedRequestStore _거절Store;
    private readonly I배차추천경로Service _경로Service;
    private readonly I차량화물적합성Service _적합성Service;
    private readonly I배차추천판정Service _판정Service;
    private readonly I배차추천평가Service _평가Service;
    private readonly I기사운송일정구성Service _기사운송일정구성Service;
    private readonly I운송일정삽입평가Service _운송일정삽입평가Service;
    private readonly 배차큐정책Options _options;

    public 국내화물배차조율입력Factory(
        HongdalContext db,
        I국내화물운송기사상태Store 기사상태Store,
        IDriverRejectedRequestStore 거절Store,
        I배차추천경로Service 경로Service,
        I차량화물적합성Service 적합성Service,
        I배차추천판정Service 판정Service,
        I배차추천평가Service 평가Service,
        I기사운송일정구성Service 기사운송일정구성Service,
        I운송일정삽입평가Service 운송일정삽입평가Service,
        IOptions<배차큐정책Options> options)
    {
        _db = db;
        _기사상태Store = 기사상태Store;
        _거절Store = 거절Store;
        _경로Service = 경로Service;
        _적합성Service = 적합성Service;
        _판정Service = 판정Service;
        _평가Service = 평가Service;
        _기사운송일정구성Service = 기사운송일정구성Service;
        _운송일정삽입평가Service = 운송일정삽입평가Service;
        _options = options.Value;
    }

    public async Task<국내화물배차조율입력> 생성Async(
        국내화물배차조율입력요청 request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var queues = await LoadQueuesAsync(request, now, cancellationToken);
        var requestMap = await LoadRequestMapAsync(queues.Select(x => x.의뢰Id), cancellationToken);
        var cargoMap = await LoadCargoMapAsync(queues.Select(x => x.의뢰Id), cancellationToken);
        var driverStates = await LoadDriverStatesAsync(request, cancellationToken);
        var driverMap = await LoadDriverMapAsync(driverStates, request, cancellationToken);
        var currentShiftMap = await LoadCurrentShiftMapAsync(driverMap.Keys, now, cancellationToken);
        var vehicleSpecMap = await LoadVehicleSpecMapAsync(driverMap.Values, cancellationToken);
        var acceptedTransportCounts = await LoadAcceptedTransportCountsAsync(driverMap.Keys, cancellationToken);
        var maxAcceptedTransportCount = Math.Max(1, request.기사당최대추천건수);
        var candidateQueues = FilterCandidateQueues(queues, requestMap, now);
        var candidateDriverStates = FilterCandidateDriverStates(
            driverStates,
            driverMap,
            acceptedTransportCounts,
            maxAcceptedTransportCount);

        var requestInputs = candidateQueues
            .Where(x => requestMap.ContainsKey(x.의뢰Id))
            .Select(x => ToRequestInput(x, requestMap[x.의뢰Id]))
            .ToArray();
        var driverInputs = candidateDriverStates
            .Where(x => driverMap.ContainsKey(x.DriverId))
            .Select(x => ToDriverInput(x, driverMap[x.DriverId], GetAcceptedTransportCount(acceptedTransportCounts, x.DriverId)))
            .ToArray();

        var evaluations = await BuildEvaluationsAsync(
            candidateQueues,
            requestMap,
            cargoMap,
            candidateDriverStates,
            driverMap,
            currentShiftMap,
            vehicleSpecMap,
            acceptedTransportCounts,
            maxAcceptedTransportCount,
            now,
            cancellationToken);

        return new 국내화물배차조율입력(
            now,
            Math.Max(1, request.기사당최대추천건수),
            requestInputs,
            driverInputs,
            evaluations);
    }
}
