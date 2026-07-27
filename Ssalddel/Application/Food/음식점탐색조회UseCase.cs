using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Restaurants;
using Ssalddel.Services.Orderer;
using 살뜰.Data;
using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Application.Food;

public interface I음식점탐색조회UseCase
{
    Task<Result<IReadOnlyList<음식점카테고리응답>>> 카테고리목록Async(
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<음식점탐색권역응답>>> 권역목록Async(
        CancellationToken cancellationToken);

    Task<Result<음식점공개목록응답>> 목록Async(
        음식점공개목록조회요청 request,
        CancellationToken cancellationToken);

    Task<Result<음식점공개상세응답>> 상세Async(
        long restaurantId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelUseCase(
    "공개 음식점과 메뉴 조회",
    Summary = "주문자가 직접 선택한 공개 행정권역 기준점과 반경으로 공개 음식점을 찾고 정확한 음식점의 공개 메뉴를 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
public sealed class 음식점탐색조회UseCase(
    SsalddelContext db,
    IRestaurantSearchPolicyStore policyStore) : I음식점탐색조회UseCase
{
    public async Task<Result<IReadOnlyList<음식점카테고리응답>>> 카테고리목록Async(
        CancellationToken cancellationToken)
    {
        var 공개분류별수 = await db.음식점공개프로필
            .AsNoTracking()
            .Where(item => item.공개여부 && !string.IsNullOrWhiteSpace(item.카테고리))
            .GroupBy(item => item.카테고리)
            .Select(group => new { 카테고리 = group.Key, 공개음식점수 = group.Count() })
            .ToDictionaryAsync(
                item => item.카테고리,
                item => item.공개음식점수,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        var items = 음식점카테고리Catalog.전체
            .Select(item => new 음식점카테고리응답
            {
                카테고리키 = item.카테고리키,
                표시명 = item.표시명,
                설명 = item.설명,
                대표메뉴안내 = item.대표메뉴안내,
                표시순서 = item.표시순서,
                공개음식점수 = 공개분류별수.GetValueOrDefault(item.카테고리키)
            })
            .ToArray();

        return Result.Ok<IReadOnlyList<음식점카테고리응답>>(items);
    }

    public Task<Result<IReadOnlyList<음식점탐색권역응답>>> 권역목록Async(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<음식점탐색권역응답> items = 국내행정구역배달권Catalog
            .전체조회()
            .OrderBy(item => item.시도명, StringComparer.Ordinal)
            .ThenBy(item => string.IsNullOrWhiteSpace(item.시군구명) ? 0 : 1)
            .ThenBy(item => item.시군구명, StringComparer.Ordinal)
            .Select(ToScope)
            .ToArray();
        return Task.FromResult(Result.Ok(items));
    }

    public async Task<Result<음식점공개목록응답>> 목록Async(
        음식점공개목록조회요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var scope = ResolveScope(request.배달권키);
        if (scope is null)
        {
            return Result.Fail<음식점공개목록응답>("조회할 공개 행정권역을 선택해 주세요.");
        }

        var policy = await policyStore.GetAsync(cancellationToken);
        var radius = (double)request.반경Km;
        if (radius < policy.MinRadiusKm || radius > policy.MaxRadiusKm)
        {
            return Result.Fail<음식점공개목록응답>(
                $"조회 반경은 {policy.MinRadiusKm:0.#}km부터 {policy.MaxRadiusKm:0.#}km 사이여야 합니다.");
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var latitudeDelta = (decimal)(radius / 110.574d);
        var longitudeDivisor = 111.320d * Math.Abs(Math.Cos((double)scope.대표위도 * Math.PI / 180d));
        var longitudeDelta = (decimal)(radius / Math.Max(longitudeDivisor, 0.001d));
        var minimumLatitude = scope.대표위도 - latitudeDelta;
        var maximumLatitude = scope.대표위도 + latitudeDelta;
        var minimumLongitude = scope.대표경도 - longitudeDelta;
        var maximumLongitude = scope.대표경도 + longitudeDelta;

        var query = db.음식점공개프로필
            .AsNoTracking()
            .Where(item => item.공개여부
                           && item.위도 >= minimumLatitude
                           && item.위도 <= maximumLatitude
                           && item.경도 >= minimumLongitude
                           && item.경도 <= maximumLongitude);

        if (request.주문가능만)
        {
            query = query.Where(item => item.주문가능여부);
        }

        if (!string.IsNullOrWhiteSpace(request.카테고리))
        {
            var category = 음식점카테고리Catalog.찾기(request.카테고리);
            if (category is null)
            {
                return Result.Fail<음식점공개목록응답>("지원하는 음식점 카테고리를 선택해 주세요.");
            }

            query = query.Where(item => item.카테고리 == category.카테고리키);
        }

        if (!string.IsNullOrWhiteSpace(request.검색어))
        {
            var search = request.검색어.Trim();
            query = query.Where(item =>
                item.상호명.Contains(search)
                || item.카테고리.Contains(search)
                || item.소개.Contains(search));
        }

        var candidates = await query
            .Select(item => new RestaurantCandidate
            {
                Id = item.Id,
                Name = item.상호명,
                Category = item.카테고리,
                Description = item.소개,
                PublicAddress = item.공개주소,
                ImageUrl = item.대표이미지Url,
                Latitude = item.위도,
                Longitude = item.경도,
                MinimumOrderAmount = item.최소주문금액,
                EstimatedCookingMinutes = item.예상조리분,
                IsOrderAvailable = item.주문가능여부,
                PublicMenuCount = item.메뉴목록.Count(menu => menu.공개여부),
                UpdatedAtUtc = item.UpdatedAtUtc
            })
            .ToArrayAsync(cancellationToken);

        var withinRadius = candidates
            .Select(item => new
            {
                Item = item,
                DistanceKm = DistanceKm(scope.대표위도, scope.대표경도, item.Latitude, item.Longitude)
            })
            .Where(item => item.DistanceKm <= request.반경Km)
            .OrderBy(item => item.DistanceKm)
            .ThenBy(item => item.Item.Name, StringComparer.Ordinal)
            .ToArray();

        var totalCount = withinRadius.Length;
        var items = withinRadius
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => ToSummary(item.Item, item.DistanceKm))
            .ToArray();

        return Result.Ok(new 음식점공개목록응답
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            배달권키 = scope.배달권키,
            적용반경Km = request.반경Km,
            거리기준안내 = DistanceBasis(scope)
        });
    }

    public async Task<Result<음식점공개상세응답>> 상세Async(
        long restaurantId,
        CancellationToken cancellationToken)
    {
        if (restaurantId <= 0)
        {
            return Result.Fail<음식점공개상세응답>("조회할 음식점 ID를 확인해 주세요.");
        }

        var restaurant = await db.음식점공개프로필
            .AsNoTracking()
            .Include(item => item.메뉴목록)
            .FirstOrDefaultAsync(item => item.Id == restaurantId && item.공개여부, cancellationToken);
        if (restaurant is null)
        {
            return NotFound<음식점공개상세응답>("공개된 음식점을 찾을 수 없습니다.");
        }

        var publicMenus = restaurant.메뉴목록
            .Where(item => item.공개여부)
            .OrderBy(item => item.표시순서)
            .ThenBy(item => item.Id)
            .ToArray();

        return Result.Ok(new 음식점공개상세응답
        {
            음식점 = new 음식점공개요약응답
            {
                Id = restaurant.Id,
                상호명 = restaurant.상호명,
                카테고리 = restaurant.카테고리,
                소개 = restaurant.소개,
                공개주소 = restaurant.공개주소,
                대표이미지Url = restaurant.대표이미지Url,
                거리Km = null,
                최소주문금액 = restaurant.최소주문금액,
                예상조리분 = restaurant.예상조리분,
                주문가능여부 = restaurant.주문가능여부,
                공개메뉴수 = publicMenus.Length,
                수정일시Utc = restaurant.UpdatedAtUtc
            },
            메뉴목록 = publicMenus.Select(item => new 음식점메뉴공개응답
            {
                Id = item.Id,
                메뉴명 = item.메뉴명,
                설명 = item.설명,
                판매가 = item.판매가,
                대표이미지Url = item.대표이미지Url,
                품절여부 = item.품절여부,
                표시순서 = item.표시순서
            }).ToArray()
        });
    }

    private static 기초배달권항목? ResolveScope(string? scopeKey)
        => string.IsNullOrWhiteSpace(scopeKey)
            ? null
            : 국내행정구역배달권Catalog.전체조회().FirstOrDefault(item =>
                string.Equals(item.배달권키, scopeKey.Trim(), StringComparison.Ordinal));

    private static 음식점탐색권역응답 ToScope(기초배달권항목 item)
        => new()
        {
            배달권키 = item.배달권키,
            시도명 = item.시도명,
            시군구명 = item.시군구명,
            표시명 = string.IsNullOrWhiteSpace(item.시군구명)
                ? item.시도명
                : $"{item.시도명} {item.시군구명}",
            기준지명 = item.대표건물명,
            거리기준안내 = DistanceBasis(item)
        };

    private static string DistanceBasis(기초배달권항목 item)
        => $"{item.대표건물명} 공개 기준점에서 계산한 직선거리입니다. 현재 위치나 주소는 자동 수집하지 않습니다.";

    private static 음식점공개요약응답 ToSummary(RestaurantCandidate item, decimal distanceKm)
        => new()
        {
            Id = item.Id,
            상호명 = item.Name,
            카테고리 = item.Category,
            소개 = item.Description,
            공개주소 = item.PublicAddress,
            대표이미지Url = item.ImageUrl,
            거리Km = distanceKm,
            최소주문금액 = item.MinimumOrderAmount,
            예상조리분 = item.EstimatedCookingMinutes,
            주문가능여부 = item.IsOrderAvailable,
            공개메뉴수 = item.PublicMenuCount,
            수정일시Utc = item.UpdatedAtUtc
        };

    private static decimal DistanceKm(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        const double earthRadiusKm = 6371.0088d;
        var lat1 = (double)latitude1 * Math.PI / 180d;
        var lat2 = (double)latitude2 * Math.PI / 180d;
        var deltaLatitude = ((double)latitude2 - (double)latitude1) * Math.PI / 180d;
        var deltaLongitude = ((double)longitude2 - (double)longitude1) * Math.PI / 180d;
        var haversine = Math.Pow(Math.Sin(deltaLatitude / 2d), 2d)
                        + Math.Cos(lat1) * Math.Cos(lat2)
                        * Math.Pow(Math.Sin(deltaLongitude / 2d), 2d);
        var normalizedHaversine = Math.Clamp(haversine, 0d, 1d);
        var distance = earthRadiusKm * 2d * Math.Atan2(
            Math.Sqrt(normalizedHaversine),
            Math.Sqrt(1d - normalizedHaversine));
        return Math.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
    }

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message)
            .WithMetadata("StatusCode", StatusCodes.Status404NotFound));

    private sealed class RestaurantCandidate
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string PublicAddress { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public decimal Latitude { get; init; }
        public decimal Longitude { get; init; }
        public decimal MinimumOrderAmount { get; init; }
        public int EstimatedCookingMinutes { get; init; }
        public bool IsOrderAvailable { get; init; }
        public int PublicMenuCount { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
    }
}
