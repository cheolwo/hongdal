using Hongdal.Contracts.Common.Drivers;
using Microsoft.Extensions.Options;
using 홍달.Services.Dispatch.Queue;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.Services.Storage.Local;
using 홍달.도메인.공통;
using 홍달.도메인.기사;
using 홍달.도메인.운송;

namespace Hongdal.Tests.Services.Dispatch.Queue;

public sealed class 음식배달배차업무정책Tests
{
    [Fact]
    public async Task F드라이버_운행중_최신위치_후보만_선정한다()
    {
        var now = DateTime.UtcNow;
        var store = new FakeDriverStateStore(
        [
            Snapshot("food-driver", 기사앱식별자.FoodDeliveryDriverApp, 37.501m, 127.001m, now, 3m),
            Snapshot("cargo-driver", 기사앱식별자.CargoYongdalDriverApp, 37.5001m, 127.0001m, now, 50m),
            Snapshot("stale-food-driver", 기사앱식별자.FoodDeliveryDriverApp, 37.5001m, 127.0001m, now.AddMinutes(-11), 100m)
        ]);
        var policy = CreatePolicy(store, new FakeRejectedRequestStore());

        var candidate = await policy.다음후보선정Async(Queue());

        Assert.NotNull(candidate);
        Assert.Equal("food-driver", candidate!.DriverId);
        Assert.Contains("음식점까지", candidate.추천사유);
    }

    [Fact]
    public async Task 이미_거절한_F드라이버는_다음_후보에서_제외한다()
    {
        var now = DateTime.UtcNow;
        var store = new FakeDriverStateStore(
        [
            Snapshot("rejected-driver", 기사앱식별자.FoodDeliveryDriverApp, 37.5001m, 127.0001m, now, 100m),
            Snapshot("next-driver", 기사앱식별자.FoodDeliveryDriverApp, 37.502m, 127.002m, now, 0m)
        ]);
        var rejected = new FakeRejectedRequestStore();
        await rejected.RejectAsync("rejected-driver", "food-order-1");
        var policy = CreatePolicy(store, rejected);

        var candidate = await policy.다음후보선정Async(Queue());

        Assert.NotNull(candidate);
        Assert.Equal("next-driver", candidate!.DriverId);
    }

    private static 음식배달배차업무정책 CreatePolicy(
        I국내화물운송기사상태Store store,
        IDriverRejectedRequestStore rejected)
        => new(
            store,
            rejected,
            new StraightLineRouteService(),
            Options.Create(new 배차큐정책Options
            {
                기사후보검색반경Km = 5m,
                기사후보최대조회수 = 20
            }));

    private static 운송원장 Queue()
        => new()
        {
            의뢰Id = "food-order-1",
            배차업무유형 = 상태값.배차업무유형.음식배달,
            픽업_위도 = 37.5m,
            픽업_경도 = 127m
        };

    private static 국내화물운송기사상태Snapshot Snapshot(
        string driverId,
        string appKey,
        decimal latitude,
        decimal longitude,
        DateTime receivedAt,
        decimal aging)
        => new(
            driverId,
            null,
            상태값.기사운행상태.운행중,
            receivedAt.AddMinutes(-20),
            receivedAt.AddMinutes(-20),
            aging,
            latitude,
            longitude,
            5m,
            receivedAt,
            receivedAt,
            null,
            null,
            0,
            "immediate",
            "test",
            null,
            AppKey: appKey);

    private sealed class FakeDriverStateStore(
        IReadOnlyList<국내화물운송기사상태Snapshot> snapshots) : I국내화물운송기사상태Store
    {
        public Task UpsertAsync(국내화물운송기사상태Snapshot snapshot, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<국내화물운송기사상태Snapshot?> GetAsync(string driverId, CancellationToken cancellationToken = default)
            => Task.FromResult(snapshots.FirstOrDefault(x => x.DriverId == driverId));

        public Task<IReadOnlyList<국내화물운송기사상태Snapshot>> 위치반경조회Async(
            decimal latitude,
            decimal longitude,
            decimal radiusKm,
            int take,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<국내화물운송기사상태Snapshot>>(snapshots.Take(take).ToArray());

        public Task<IReadOnlyList<국내화물운송기사상태Snapshot>> 활성기사조회Async(
            int take,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<국내화물운송기사상태Snapshot>>(snapshots.Take(take).ToArray());

        public Task RemoveAsync(string driverId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeRejectedRequestStore : IDriverRejectedRequestStore
    {
        private readonly Dictionary<string, HashSet<string>> _driversByRequest = new(StringComparer.Ordinal);

        public Task RejectAsync(string driverId, string requestId, CancellationToken cancellationToken = default)
        {
            if (!_driversByRequest.TryGetValue(requestId, out var drivers))
            {
                drivers = new HashSet<string>(StringComparer.Ordinal);
                _driversByRequest[requestId] = drivers;
            }

            drivers.Add(driverId);
            return Task.CompletedTask;
        }

        public Task<bool> IsRejectedAsync(string driverId, string requestId, CancellationToken cancellationToken = default)
            => Task.FromResult(_driversByRequest.TryGetValue(requestId, out var drivers) && drivers.Contains(driverId));

        public Task<IReadOnlySet<string>> GetRejectedRequestIdsAsync(string driverId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlySet<string>>(_driversByRequest
                .Where(x => x.Value.Contains(driverId))
                .Select(x => x.Key)
                .ToHashSet(StringComparer.Ordinal));

        public Task<IReadOnlySet<string>> GetRejectedDriverIdsAsync(string requestId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlySet<string>>(
                _driversByRequest.TryGetValue(requestId, out var drivers)
                    ? drivers
                    : new HashSet<string>(StringComparer.Ordinal));
    }

    private sealed class StraightLineRouteService : I배차추천경로Service
    {
        public Task<배차경로좌표?> ResolveOriginLocationAsync(
            string driverId,
            용달기사? driver,
            DriverLocationSnapshot? currentLocation,
            배차추천검색조건? criteria)
            => Task.FromResult<배차경로좌표?>(null);

        public Task<배차경로좌표?> ResolveRouteAnchorLocationAsync(
            string driverId,
            용달기사? driver,
            DriverLocationSnapshot? currentLocation)
            => Task.FromResult<배차경로좌표?>(null);

        public Task<배차경로예상결과?> EstimateRouteAsync(배차경로좌표? origin, 배차경로좌표? destination)
            => Task.FromResult<배차경로예상결과?>(null);

        public Task<배차경로예상결과?> EstimateOrderedRouteAsync(
            배차경로좌표? origin,
            IReadOnlyList<배차경로좌표> orderedStops,
            CancellationToken cancellationToken = default)
            => Task.FromResult<배차경로예상결과?>(null);

        public Task<배차삽입경로예상결과?> EstimateInsertionDelayAsync(
            배차경로좌표? origin,
            배차경로좌표? routeAnchor,
            배차경로좌표? pickup,
            배차경로좌표? dropoff)
            => Task.FromResult<배차삽입경로예상결과?>(null);

        public decimal? CalculateDistanceKm(배차경로좌표 source, 배차경로좌표 target)
        {
            var latitudeKm = Math.Abs(source.Latitude - target.Latitude) * 111m;
            var longitudeKm = Math.Abs(source.Longitude - target.Longitude) * 88m;
            return latitudeKm + longitudeKm;
        }
    }
}
