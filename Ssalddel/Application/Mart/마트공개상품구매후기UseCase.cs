using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Mart;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;

namespace Ssalddel.Application.Mart;

public interface I마트공개상품구매후기UseCase
{
    Task<Result<마트공개상품구매후기응답>> 작성Async(
        long productId,
        마트공개상품구매후기작성요청? request,
        CancellationToken cancellationToken);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.Application,
    "완료된 공동구매 원장 참여자가 공개 상품 구매후기를 기존 커뮤니티 글로 작성",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.ClosedLoop,
    Boundary = "공개 상품과 완료 원장을 서버에서 다시 확인하고 원장 접근 권한이 있는 로그인 사용자의 명시적 작성만 저장합니다.")]
[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelUseCase(
    "마트 공개 상품 구매후기 작성",
    Summary = "완료된 원장과 연결된 공개 상품에만 후기 작성을 허용하고 기존 완료 사례·후기 게시판에 원장 연결 글을 저장합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
public sealed class 마트공개상품구매후기UseCase(
    SsalddelContext db,
    I커뮤니티게시글발행UseCase communityPostPublishing) : I마트공개상품구매후기UseCase
{
    public async Task<Result<마트공개상품구매후기응답>> 작성Async(
        long productId,
        마트공개상품구매후기작성요청? request,
        CancellationToken cancellationToken)
    {
        if (productId <= 0 || request is null)
        {
            return Result.Fail<마트공개상품구매후기응답>("상품과 후기 내용을 확인해 주세요.");
        }

        var source = await (
                from publicProduct in db.마트공개상품.AsNoTracking()
                join salesProduct in db.판매상품.AsNoTracking()
                    on publicProduct.판매상품Id equals (long?)salesProduct.Id
                join inboundProduct in db.입고상품.AsNoTracking()
                    on salesProduct.입고상품Id equals inboundProduct.Id
                where publicProduct.Id == productId && publicProduct.공개여부
                select new
                {
                    publicProduct.상품명,
                    inboundProduct.커뮤니티원장Id,
                    inboundProduct.커뮤니티원장템플릿Key,
                    inboundProduct.커뮤니티원장상태
                })
            .FirstOrDefaultAsync(cancellationToken);
        if (source is null)
        {
            return 실패("공개된 상품과 구매 원장 연결을 찾을 수 없습니다.", StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrWhiteSpace(source.커뮤니티원장Id))
        {
            return 실패("완료 후기를 연결할 구매 원장이 아직 없습니다.", StatusCodes.Status409Conflict);
        }

        var ledgerId = source.커뮤니티원장Id.Trim();
        var completionPostExists = await db.PlatformCommunityPosts
            .AsNoTracking()
            .AnyAsync(post => !post.IsDeleted
                              && post.커뮤니티원장Id == ledgerId
                              && post.AuthorUserId == CommunityLedgerCompletionPublication.SystemAuthorKey
                              && post.PublicationStatusCode == PlatformCommunityPostPublicationStatusCodes.Published,
                cancellationToken);
        if (!completionPostExists
            && !string.Equals(
                source.커뮤니티원장상태,
                커뮤니티원장상태.완료,
                StringComparison.OrdinalIgnoreCase))
        {
            return 실패("구매 원장이 완료된 뒤 참여자가 후기를 작성할 수 있습니다.", StatusCodes.Status409Conflict);
        }

        var workflowTag = CommunityWorkClassificationCatalog
                              .FindByLedgerTemplate(source.커뮤니티원장템플릿Key)?.WorkflowTag
                          ?? "공동구매";
        var created = await communityPostPublishing.생성Async(new PlatformCommunityPostCreateRequest
        {
            AppKey = "platform",
            Category = CommunityLedgerCompletionPublication.Category,
            WorkflowTag = workflowTag,
            RoleTag = "구매 참여자",
            Title = request.제목,
            Body = request.본문,
            커뮤니티원장Id = ledgerId,
            Nickname = request.작성자표시명,
            Password = request.글비밀번호
        }, cancellationToken);
        if (created.IsFailed)
        {
            return Result.Fail<마트공개상품구매후기응답>(created.Errors);
        }

        return Result.Ok(new 마트공개상품구매후기응답
        {
            게시글Id = created.Value.Id,
            제목 = created.Value.Title,
            본문요약 = 요약(created.Value.Body, 280),
            작성자표시명 = created.Value.Nickname,
            추천수 = created.Value.RecommendationCount,
            댓글수 = created.Value.CommentCount,
            작성시각Utc = created.Value.PublishedAtUtc ?? created.Value.CreatedAtUtc
        });
    }

    private static Result<마트공개상품구매후기응답> 실패(string message, int statusCode)
        => Result.Fail<마트공개상품구매후기응답>(
            new Error(message).WithMetadata("StatusCode", statusCode));

    private static string 요약(string? value, int maxLength)
    {
        var normalized = string.Join(' ', (value ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= maxLength
            ? normalized
            : $"{normalized[..maxLength].TrimEnd()}…";
    }
}
