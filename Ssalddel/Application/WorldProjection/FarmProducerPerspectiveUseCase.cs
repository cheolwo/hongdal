using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.WorldProjection;
using 살뜰.Data;

namespace Ssalddel.Application.WorldProjection;

public interface IFarmProducerPerspectiveUseCase
{
    Task<Result<FarmProducerPerspectiveResponse>> QueryAsync(
        CancellationToken cancellationToken = default);
}

public sealed class FarmProducerPerspectiveUseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor)
    : IFarmProducerPerspectiveUseCase
{
    private const string ProducerRoleCode = "Producer";

    public async Task<Result<FarmProducerPerspectiveResponse>> QueryAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Fail<FarmProducerPerspectiveResponse>(
                new Error("로그인 사용자 인증 정보가 필요합니다.")
                    .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));
        }

        var farms = await db.농장.AsNoTracking()
            .Where(item => item.소유자UserId == userId)
            .OrderBy(item => item.StableId)
            .ToListAsync(cancellationToken);
        var farmIds = farms.Select(item => item.Id).ToList();
        var plots = await db.농장구획.AsNoTracking()
            .Where(item => farmIds.Contains(item.농장Id))
            .OrderBy(item => item.StableId)
            .ToListAsync(cancellationToken);
        var plotIds = plots.Select(item => item.Id).ToList();
        var cultivations = await db.재배작기.AsNoTracking()
            .Where(item => plotIds.Contains(item.농장구획Id))
            .OrderBy(item => item.StableId)
            .ToListAsync(cancellationToken);
        var sensors = await db.농업센서.AsNoTracking()
            .Where(item => plotIds.Contains(item.농장구획Id))
            .OrderBy(item => item.StableId)
            .ToListAsync(cancellationToken);
        var sensorIds = sensors.Select(item => item.Id).ToList();
        var observations = await db.농업센서관측.AsNoTracking()
            .Where(item => sensorIds.Contains(item.농업센서Id))
            .OrderByDescending(item => item.관측시각Utc)
            .ThenByDescending(item => item.Id)
            .ToListAsync(cancellationToken);
        var tasks = await db.농장작업.AsNoTracking()
            .Where(item => farmIds.Contains(item.농장Id))
            .OrderBy(item => item.StableId)
            .ToListAsync(cancellationToken);
        var latestObservationBySensor = observations
            .GroupBy(item => item.농업센서Id)
            .ToDictionary(group => group.Key, group => group.First());

        var revision = farms.Select(item => item.Revision)
            .Concat(plots.Select(item => item.Revision))
            .Concat(cultivations.Select(item => item.Revision))
            .Concat(sensors.Select(item => item.Revision))
            .Concat(tasks.Select(item => item.Revision))
            .DefaultIfEmpty(0L)
            .Max();
        var projectedFarms = farms.Select(farm => new FarmResponse(
            farm.StableId,
            farm.Revision,
            farm.농장명,
            farm.운영상태Code,
            plots.Where(plot => plot.농장Id == farm.Id)
                .Select(plot => new FarmPlotResponse(
                    plot.StableId,
                    plot.Revision,
                    plot.구획명,
                    plot.토양관리ProfileCode,
                    cultivations.Where(item => item.농장구획Id == plot.Id)
                        .Select(item => new FarmCultivationResponse(
                            item.StableId,
                            item.Revision,
                            item.작물명,
                            item.작물기준StableId,
                            item.작물기준SourceKey,
                            item.생육상태Code,
                            item.파종일,
                            item.예상수확일))
                        .ToArray(),
                    sensors.Where(sensor => sensor.농장구획Id == plot.Id)
                        .Select(sensor => new FarmSensorResponse(
                            sensor.StableId,
                            sensor.Revision,
                            sensor.센서유형Code,
                            sensor.상태Code,
                            MapObservation(latestObservationBySensor.GetValueOrDefault(sensor.Id))))
                        .ToArray()))
                .ToArray()))
            .ToArray();

        return Result.Ok(new FarmProducerPerspectiveResponse(
            "role-perspective:farm.producer",
            revision,
            ProducerRoleCode,
            "farm",
            RolePerspectiveViewerScopeCodes.AuthorizedParty,
            RolePerspectiveSourceTypeCodes.OperationalProjection,
            $"authorized-farm-producer:{revision}.{projectedFarms.Length}",
            DateTimeOffset.UtcNow,
            projectedFarms,
            tasks.Select(task => new NpcMovementResponse
            {
                StableId = "npc-movement:" + task.StableId,
                Revision = task.Revision,
                NpcStableId = task.NpcStableId,
                ActorRoleCode = ProducerRoleCode,
                WorldZoneCode = "farm",
                RouteCode = task.RouteCode,
                CurrentWaypointKey = task.CurrentWaypointKey,
                DestinationWaypointKey = task.DestinationWaypointKey,
                MovementStateCode = task.MovementStateCode,
                ArrivalActionCode = task.ArrivalActionCode,
                SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
                CanonicalTaskStableId = task.StableId,
                GeneratedAt = new DateTimeOffset(AsUtc(task.UpdatedAtUtc)),
            }).ToArray()));
    }

    private static FarmSensorObservationResponse? MapObservation(
        살뜰.도메인.농업.농업센서관측? source)
        => source is null
            ? null
            : new FarmSensorObservationResponse(
                source.관측값,
                source.단위Code,
                new DateTimeOffset(AsUtc(source.관측시각Utc)),
                source.최신성상태Code,
                source.판정상태Code,
                source.판정규칙Revision,
                source.근거카드Id,
                source.확신도Code,
                source.판정한계);

    private static DateTime AsUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
