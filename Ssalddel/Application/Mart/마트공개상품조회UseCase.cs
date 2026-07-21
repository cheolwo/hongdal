using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Mart;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.도메인.마트;

namespace Ssalddel.Application.Mart;

public interface I마트공개상품조회UseCase
{
    Task<Result<마트공개상품목록응답>> 목록Async(
        마트공개상품목록조회요청 request,
        CancellationToken cancellationToken);

    Task<Result<마트공개상품상세응답>> 상세Async(
        long productId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelUseCase(
    "마트 공개 상품 조회",
    Summary = "주문자에게 공개된 상품과 별도 투영된 판매 가능 수량만 조회하며 내부 창고·소유자·계약 원장은 노출하지 않습니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
public sealed class 마트공개상품조회UseCase(SsalddelContext db) : I마트공개상품조회UseCase
{
    internal const string 재고기준안내문 =
        "표시 수량은 내부 창고 재고를 직접 공개한 값이 아니라 마지막 투영 시각 기준의 판매 가능 수량입니다.";

    public async Task<Result<마트공개상품목록응답>> 목록Async(
        마트공개상품목록조회요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = db.마트공개상품
            .AsNoTracking()
            .Where(item => item.공개여부);

        if (request.판매가능만)
        {
            query = query.Where(item => item.판매허용여부 && item.판매가능수량 > 0);
        }

        if (!string.IsNullOrWhiteSpace(request.검색어))
        {
            var search = request.검색어.Trim();
            query = query.Where(item =>
                item.상품명.Contains(search)
                || item.카테고리.Contains(search)
                || item.짧은설명.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(item => item.카테고리)
            .ThenBy(item => item.상품명)
            .ThenBy(item => item.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new 마트공개상품요약응답
            {
                Id = item.Id,
                상품명 = item.상품명,
                카테고리 = item.카테고리,
                짧은설명 = item.짧은설명,
                판매단위 = item.판매단위,
                판매가 = item.판매가,
                대표이미지Url = item.대표이미지Url,
                판매가능수량 = item.판매가능수량 > 0 ? item.판매가능수량 : 0,
                판매가능여부 = item.판매허용여부 && item.판매가능수량 > 0,
                재고기준시각Utc = item.재고기준시각Utc,
                수정일시Utc = item.UpdatedAtUtc
            })
            .ToArrayAsync(cancellationToken);

        return Result.Ok(new 마트공개상품목록응답
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            재고기준안내 = 재고기준안내문
        });
    }

    public async Task<Result<마트공개상품상세응답>> 상세Async(
        long productId,
        CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return Result.Fail<마트공개상품상세응답>("조회할 마트 상품 ID를 확인해 주세요.");
        }

        var item = await db.마트공개상품
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == productId && product.공개여부, cancellationToken);
        if (item is null)
        {
            return NotFound<마트공개상품상세응답>("공개된 마트 상품을 찾을 수 없습니다.");
        }

        return Result.Ok(new 마트공개상품상세응답
        {
            Id = item.Id,
            상품명 = item.상품명,
            카테고리 = item.카테고리,
            설명 = item.설명,
            판매단위 = item.판매단위,
            판매가 = item.판매가,
            대표이미지Url = item.대표이미지Url,
            판매가능수량 = Math.Max(0, item.판매가능수량),
            판매가능여부 = IsAvailable(item),
            재고기준시각Utc = item.재고기준시각Utc,
            수정일시Utc = item.UpdatedAtUtc,
            재고기준안내 = 재고기준안내문,
            구매근거 = await 구매근거Async(item.판매상품Id, cancellationToken)
        });
    }

    private async Task<마트공개상품구매근거응답> 구매근거Async(
        long? 판매상품Id,
        CancellationToken cancellationToken)
    {
        const string privacyNotice =
            "완료 여부와 공개 후기만 비식별 집계합니다. 참여자, 주소, 결제·계약, 개별 수량과 내부 재고 원문은 공개하지 않습니다.";
        if (판매상품Id is not > 0)
        {
            return new 마트공개상품구매근거응답
            {
                원장근거상태 = "연결된 구매 원장 없음",
                공개범위안내 = privacyNotice
            };
        }

        var source = await (
                from salesProduct in db.판매상품.AsNoTracking()
                join inboundProduct in db.입고상품.AsNoTracking()
                    on salesProduct.입고상품Id equals inboundProduct.Id
                where salesProduct.Id == 판매상품Id.Value
                select new
                {
                    inboundProduct.커뮤니티원장Id,
                    inboundProduct.커뮤니티원장상태,
                    inboundProduct.커뮤니티원장동기화시각Utc
                })
            .FirstOrDefaultAsync(cancellationToken);
        if (source is null || string.IsNullOrWhiteSpace(source.커뮤니티원장Id))
        {
            return new 마트공개상품구매근거응답
            {
                원장근거상태 = "연결된 구매 원장 없음",
                공개범위안내 = privacyNotice
            };
        }

        var ledgerId = source.커뮤니티원장Id.Trim();
        var now = DateTime.UtcNow;
        var publishedPosts = db.PlatformCommunityPosts
            .AsNoTracking()
            .Where(post => !post.IsDeleted
                           && !post.IsReportBoardPost
                           && post.커뮤니티원장Id == ledgerId
                           && post.PublicationStatusCode == PlatformCommunityPostPublicationStatusCodes.Published
                           && post.PublishedAtUtc.HasValue
                           && post.PublishedAtUtc <= now);
        var completedAtUtc = await publishedPosts
            .Where(post => post.AuthorUserId == CommunityLedgerCompletionPublication.SystemAuthorKey)
            .OrderByDescending(post => post.PublishedAtUtc)
            .Select(post => (DateTime?)(post.PublishedAtUtc ?? post.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
        var completed = string.Equals(
                            source.커뮤니티원장상태,
                            커뮤니티원장상태.완료,
                            StringComparison.OrdinalIgnoreCase)
                        || completedAtUtc.HasValue;
        if (!completed)
        {
            return new 마트공개상품구매근거응답
            {
                원장근거상태 = "구매 원장 완료 확인 전",
                근거기준시각Utc = source.커뮤니티원장동기화시각Utc,
                공개범위안내 = privacyNotice
            };
        }

        var reviewQuery = publishedPosts
            .Where(post => post.Category == CommunityLedgerCompletionPublication.Category
                           && post.AuthorUserId != CommunityLedgerCompletionPublication.SystemAuthorKey);
        var reviewCount = await reviewQuery.CountAsync(cancellationToken);
        var reviewRows = await reviewQuery
            .OrderByDescending(post => post.PublishedAtUtc)
            .ThenByDescending(post => post.Id)
            .Take(3)
            .Select(post => new
            {
                post.Id,
                post.Title,
                post.Body,
                post.Nickname,
                post.RecommendationCount,
                post.CommentCount,
                작성시각Utc = post.PublishedAtUtc ?? post.CreatedAtUtc
            })
            .ToArrayAsync(cancellationToken);
        var reviews = reviewRows
            .Select(row => new 마트공개상품구매후기응답
            {
                게시글Id = row.Id,
                제목 = row.Title,
                본문요약 = 요약(row.Body, 280),
                작성자표시명 = row.Nickname,
                추천수 = row.RecommendationCount,
                댓글수 = row.CommentCount,
                작성시각Utc = row.작성시각Utc
            })
            .ToArray();
        var latestReviewAtUtc = reviews.FirstOrDefault()?.작성시각Utc;

        return new 마트공개상품구매근거응답
        {
            완료원장확인여부 = true,
            원장근거상태 = "완료된 공동구매 원장 확인",
            공개후기수 = reviewCount,
            완료확인시각Utc = completedAtUtc ?? source.커뮤니티원장동기화시각Utc,
            근거기준시각Utc = 최근시각(
                source.커뮤니티원장동기화시각Utc,
                completedAtUtc,
                latestReviewAtUtc),
            후기작성가능여부 = true,
            구매후기목록 = reviews,
            공개범위안내 = privacyNotice
        };
    }

    private static string 요약(string? value, int maxLength)
    {
        var normalized = string.Join(' ', (value ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= maxLength
            ? normalized
            : $"{normalized[..maxLength].TrimEnd()}…";
    }

    private static DateTime? 최근시각(params DateTime?[] values)
        => values
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .DefaultIfEmpty()
            .Max() is var latest && latest != default
                ? latest
                : null;

    private static bool IsAvailable(마트공개상품 item)
        => item.판매허용여부 && item.판매가능수량 > 0;

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message)
            .WithMetadata("StatusCode", StatusCodes.Status404NotFound));
}
