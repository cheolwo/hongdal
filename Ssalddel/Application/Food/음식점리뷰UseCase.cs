using System.Text.Json;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Food;
using Ssalddel.Contracts.Restaurants;
using 살뜰.Data;
using 살뜰.도메인.음식;

namespace Ssalddel.Application.Food;

public interface I음식점리뷰UseCase
{
    Task<Result<음식점리뷰목록응답>> 목록Async(
        long 음식점Id,
        CancellationToken cancellationToken);

    Task<Result<음식점리뷰요약응답>> 등록Async(
        long 음식점Id,
        음식점리뷰등록요청 request,
        string 주문자UserId,
        CancellationToken cancellationToken);
}

public sealed class 음식점리뷰UseCase(SsalddelContext db) : I음식점리뷰UseCase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<음식점리뷰목록응답>> 목록Async(
        long 음식점Id,
        CancellationToken cancellationToken)
    {
        if (음식점Id <= 0)
        {
            return BadRequest<음식점리뷰목록응답>("조회할 음식점 ID를 확인해 주세요.");
        }

        var now = DateTime.UtcNow;
        var reviews = await db.음식점리뷰
            .AsNoTracking()
            .Where(item =>
                item.음식점Id == 음식점Id
                && item.현재노출여부
                && (!item.게시종료일시Utc.HasValue || item.게시종료일시Utc > now))
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(200)
            .ToArrayAsync(cancellationToken);

        return Result.Ok(new 음식점리뷰목록응답
        {
            음식점Id = 음식점Id,
            Items = reviews.Select(ToPublicResponse).ToArray()
        });
    }

    public async Task<Result<음식점리뷰요약응답>> 등록Async(
        long 음식점Id,
        음식점리뷰등록요청 request,
        string 주문자UserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = Clean(주문자UserId);
        var orderNo = Clean(request.주문번호);
        var content = Clean(request.내용);
        if (음식점Id <= 0 || userId is null || orderNo is null)
        {
            return BadRequest<음식점리뷰요약응답>(
                "음식점, 로그인 사용자와 확인된 음식 주문번호가 필요합니다.");
        }

        if (request.별점 is < 1 or > 5)
        {
            return BadRequest<음식점리뷰요약응답>("별점은 1점부터 5점 사이여야 합니다.");
        }

        if (content is null || content.Length > 2000)
        {
            return BadRequest<음식점리뷰요약응답>("리뷰 내용은 1자 이상 2,000자 이하로 입력해 주세요.");
        }

        var order = await db.음식주문
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.주문번호 == orderNo, cancellationToken);
        var normalizedOrderStatus = 음식주문상태코드.Normalize(order?.상태);
        if (order is null
            || order.음식점Id != 음식점Id
            || !string.Equals(order.주문자UserId, userId, StringComparison.Ordinal)
            || normalizedOrderStatus is not (
                음식주문상태코드.전달완료 or
                음식주문상태코드.수령확인))
        {
            return NotFound<음식점리뷰요약응답>(
                "리뷰를 작성할 수 있는 전달 완료 음식 주문을 찾지 못했습니다.");
        }

        if (await db.음식점리뷰
                .AsNoTracking()
                .AnyAsync(item => item.주문번호 == orderNo, cancellationToken))
        {
            return Conflict<음식점리뷰요약응답>("이 음식 주문에는 이미 리뷰가 등록되어 있습니다.");
        }

        var photoUrls = request.사진Urls
            .Select(Clean)
            .Where(item => item is not null)
            .Select(item => item!)
            .Distinct(StringComparer.Ordinal)
            .Take(5)
            .ToArray();
        var recentRatings = await db.음식점리뷰
            .AsNoTracking()
            .Where(item => item.음식점Id == 음식점Id)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Select(item => item.별점)
            .Take(2)
            .ToArrayAsync(cancellationToken);
        var lowRatingStreak = request.별점 <= 2
                              && recentRatings.Length == 2
                              && recentRatings.All(rating => rating <= 2);
        var policy = await db.음식운영정책
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken)
            ?? new 음식운영정책();
        var now = DateTime.UtcNow;
        var review = new 음식점리뷰
        {
            음식점Id = 음식점Id,
            주문자UserId = userId,
            주문번호 = orderNo,
            별점 = request.별점,
            내용 = content,
            사진UrlsJson = JsonSerializer.Serialize(photoUrls, JsonOptions),
            같은음식점기준저평점3회연속여부 = lowRatingStreak,
            사장노출허용여부 = request.별점 >= 3,
            관리자검토필요여부 = request.별점 <= 2,
            관리자게시강제여부 = false,
            현재노출여부 = true,
            게시종료일시Utc = request.별점 <= 2
                ? now.AddDays(policy.기본저평점게시일수)
                : null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        db.음식점리뷰.Add(review);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok(ToPublicResponse(review));
    }

    private static 음식점리뷰요약응답 ToPublicResponse(음식점리뷰 review)
    {
        var photoUrls = DeserializePhotoUrls(review.사진UrlsJson);
        return new()
        {
            Id = review.Id,
            음식점Id = review.음식점Id,
            주문자UserId = "인증 주문자",
            주문번호 = null,
            별점 = review.별점,
            내용 = review.내용,
            사진Urls = photoUrls,
            사진포함여부 = photoUrls.Count > 0,
            사장노출허용여부 = review.사장노출허용여부,
            관리자검토필요여부 = review.관리자검토필요여부,
            관리자게시강제여부 = review.관리자게시강제여부,
            현재노출여부 = review.현재노출여부,
            CreatedAt = review.CreatedAtUtc,
            게시종료일시Utc = review.게시종료일시Utc
        };
    }

    internal static IReadOnlyList<string> DeserializePhotoUrls(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static Result<T> BadRequest<T>(string message)
        => Fail<T>(message, 400);

    private static Result<T> NotFound<T>(string message)
        => Fail<T>(message, 404);

    private static Result<T> Conflict<T>(string message)
        => Fail<T>(message, 409);

    private static Result<T> Fail<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
