using System.Text.Json;
using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Admin.Customs;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Domain.HsCodes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Audit;

namespace Hongdal.Application.Customs;

public interface IHS코드운영UseCase
{
    Task<Result<AdminHsCodeListResponse>> 목록Async(
        string? query,
        int? businessCategory,
        int? tagType,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<AdminHsCodeEntryResponse>> 대분류수정Async(
        long entryId,
        AdminHsCodeBusinessCategoryUpdateRequest? request,
        HS코드운영자Context context,
        CancellationToken cancellationToken);

    Task<Result<AdminHsCodeEntryResponse>> 태그저장Async(
        long entryId,
        AdminHsCodeRiskTagUpdateRequest? request,
        HS코드운영자Context context,
        CancellationToken cancellationToken);

    Task<Result<AdminHsCodeEntryResponse>> 태그수정Async(
        long tagId,
        AdminHsCodeRiskTagUpdateRequest? request,
        HS코드운영자Context context,
        CancellationToken cancellationToken);
}

public sealed record HS코드운영자Context(
    bool IsBrokerReviewer,
    string UserId,
    string UserName,
    string RoleName,
    string Route,
    string TraceId,
    string ClientIp,
    string UserAgent);

[HongdalApiWorkflow(HongdalWorkflow.CustomsAndTradeData)]
[HongdalUseCase("HS 코드 운영", Summary = "운영자와 관세사가 HS 코드 업무 분류와 통관 주의 태그를 조회하고 보정합니다.")]
[HongdalUseCaseActor(HongdalActor.CustomsBroker)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class HS코드운영UseCase : IHS코드운영UseCase
{
    private readonly HongdalContext _db;
    private readonly I사용자행위로그Service _activityLogService;

    public HS코드운영UseCase(HongdalContext db, I사용자행위로그Service activityLogService)
    {
        _db = db;
        _activityLogService = activityLogService;
    }

    public async Task<Result<AdminHsCodeListResponse>> 목록Async(
        string? query,
        int? businessCategory,
        int? tagType,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var hsCodes = _db.HsCodeEntries
            .AsNoTracking()
            .Include(x => x.RiskTags)
            .AsQueryable();

        if (!includeInactive)
        {
            hsCodes = hsCodes.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            var normalizedTerm = new string(term.Where(char.IsDigit).ToArray());

            hsCodes = hsCodes.Where(x =>
                x.Code.Contains(term) ||
                x.NormalizedCode.Contains(term) ||
                x.KoreanName.Contains(term) ||
                x.EnglishName.Contains(term) ||
                (!string.IsNullOrWhiteSpace(normalizedTerm) && x.NormalizedCode.Contains(normalizedTerm)));
        }

        if (businessCategory.HasValue)
        {
            if (!Enum.IsDefined(typeof(HsCodeBusinessCategory), businessCategory.Value))
            {
                return BadRequest<AdminHsCodeListResponse>("지원하지 않는 HS 코드 업무 분류입니다.");
            }

            var category = (HsCodeBusinessCategory)businessCategory.Value;
            hsCodes = hsCodes.Where(x => x.BusinessCategory == category);
        }

        if (tagType.HasValue)
        {
            if (!Enum.IsDefined(typeof(HsCodeRiskTagType), tagType.Value))
            {
                return BadRequest<AdminHsCodeListResponse>("지원하지 않는 HS 코드 주의 태그입니다.");
            }

            var tag = (HsCodeRiskTagType)tagType.Value;
            hsCodes = hsCodes.Where(x => x.RiskTags.Any(t => t.TagType == tag && (includeInactive || t.IsActive)));
        }

        var totalCount = await hsCodes.CountAsync(cancellationToken);
        var items = await hsCodes
            .OrderBy(x => x.NormalizedCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result.Ok(new AdminHsCodeListResponse
        {
            Items = items.Select(Map).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<AdminHsCodeEntryResponse>> 대분류수정Async(
        long entryId,
        AdminHsCodeBusinessCategoryUpdateRequest? request,
        HS코드운영자Context context,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<AdminHsCodeEntryResponse>("request body is required");
        }

        if (!Enum.IsDefined(typeof(HsCodeBusinessCategory), request.BusinessCategory))
        {
            return BadRequest<AdminHsCodeEntryResponse>("지원하지 않는 HS 코드 업무 분류입니다.");
        }

        var entry = await _db.HsCodeEntries
            .Include(x => x.RiskTags)
            .FirstOrDefaultAsync(x => x.Id == entryId, cancellationToken);

        if (entry is null)
        {
            return NotFound<AdminHsCodeEntryResponse>("HS 코드 항목을 찾을 수 없습니다.");
        }

        entry.BusinessCategory = (HsCodeBusinessCategory)request.BusinessCategory;
        entry.BusinessCategoryReason = string.IsNullOrWhiteSpace(request.Reason)
            ? DefaultCorrectionReason(context)
            : request.Reason.Trim();
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("HsCodeBusinessCategoryUpdated", new { entryId, entry.Code, request.BusinessCategory }, context, cancellationToken);

        return Result.Ok(Map(entry));
    }

    public async Task<Result<AdminHsCodeEntryResponse>> 태그저장Async(
        long entryId,
        AdminHsCodeRiskTagUpdateRequest? request,
        HS코드운영자Context context,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<AdminHsCodeEntryResponse>("request body is required");
        }

        if (!Enum.IsDefined(typeof(HsCodeRiskTagType), request.TagType))
        {
            return BadRequest<AdminHsCodeEntryResponse>("지원하지 않는 HS 코드 주의 태그입니다.");
        }

        var entry = await _db.HsCodeEntries
            .Include(x => x.RiskTags)
            .FirstOrDefaultAsync(x => x.Id == entryId, cancellationToken);

        if (entry is null)
        {
            return NotFound<AdminHsCodeEntryResponse>("HS 코드 항목을 찾을 수 없습니다.");
        }

        var tagType = (HsCodeRiskTagType)request.TagType;
        var tag = entry.RiskTags.FirstOrDefault(x => x.TagType == tagType);
        if (tag is null)
        {
            tag = new HsCodeEntryRiskTag
            {
                HsCodeEntryId = entry.Id,
                TagType = tagType,
                Source = HsCodeRiskTagSource.AdminOverride,
                CreatedAtUtc = DateTime.UtcNow
            };
            entry.RiskTags.Add(tag);
        }

        ApplyTagUpdate(tag, request, ResolveTagSource(context));
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("HsCodeRiskTagSaved", new { entryId, entry.Code, request.TagType, request.IsActive }, context, cancellationToken);

        return Result.Ok(Map(entry));
    }

    public async Task<Result<AdminHsCodeEntryResponse>> 태그수정Async(
        long tagId,
        AdminHsCodeRiskTagUpdateRequest? request,
        HS코드운영자Context context,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<AdminHsCodeEntryResponse>("request body is required");
        }

        if (!Enum.IsDefined(typeof(HsCodeRiskTagType), request.TagType))
        {
            return BadRequest<AdminHsCodeEntryResponse>("지원하지 않는 HS 코드 주의 태그입니다.");
        }

        var tag = await _db.HsCodeEntryRiskTags
            .Include(x => x.HsCodeEntry)
            .FirstOrDefaultAsync(x => x.Id == tagId, cancellationToken);

        if (tag?.HsCodeEntry is null)
        {
            return NotFound<AdminHsCodeEntryResponse>("HS 코드 주의 태그를 찾을 수 없습니다.");
        }

        ApplyTagUpdate(tag, request, ResolveTagSource(context));
        tag.HsCodeEntry.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var entry = await _db.HsCodeEntries
            .AsNoTracking()
            .Include(x => x.RiskTags)
            .FirstAsync(x => x.Id == tag.HsCodeEntryId, cancellationToken);

        await LogAsync("HsCodeRiskTagUpdated", new { tagId, tag.HsCodeEntryId, request.TagType, request.IsActive }, context, cancellationToken);

        return Result.Ok(Map(entry));
    }

    private static void ApplyTagUpdate(
        HsCodeEntryRiskTag tag,
        AdminHsCodeRiskTagUpdateRequest request,
        HsCodeRiskTagSource source)
    {
        tag.TagType = (HsCodeRiskTagType)request.TagType;
        tag.Label = string.IsNullOrWhiteSpace(request.Label)
            ? RiskTagLabel(tag.TagType)
            : request.Label.Trim();
        tag.Reason = string.IsNullOrWhiteSpace(request.Reason)
            ? DefaultCorrectionReason(source)
            : request.Reason.Trim();
        tag.Source = source;
        tag.IsActive = request.IsActive;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static HsCodeRiskTagSource ResolveTagSource(HS코드운영자Context context)
        => context.IsBrokerReviewer
            ? HsCodeRiskTagSource.BrokerReview
            : HsCodeRiskTagSource.AdminOverride;

    private static string DefaultCorrectionReason(HS코드운영자Context context)
        => context.IsBrokerReviewer
            ? "관세사 검토 보정"
            : "관리자 수동 보정";

    private static string DefaultCorrectionReason(HsCodeRiskTagSource source)
        => source == HsCodeRiskTagSource.BrokerReview
            ? "관세사 검토 보정"
            : "관리자 수동 보정";

    private static AdminHsCodeEntryResponse Map(HsCodeEntry entry)
    {
        return new AdminHsCodeEntryResponse
        {
            Id = entry.Id,
            Code = entry.Code,
            NormalizedCode = entry.NormalizedCode,
            KoreanName = entry.KoreanName,
            EnglishName = entry.EnglishName,
            BusinessCategory = (int)entry.BusinessCategory,
            BusinessCategoryLabel = BusinessCategoryLabel(entry.BusinessCategory),
            BusinessCategoryReason = entry.BusinessCategoryReason,
            IsActive = entry.IsActive,
            RiskTags = entry.RiskTags
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => (int)x.TagType)
                .Select(x => new AdminHsCodeRiskTagResponse
                {
                    Id = x.Id,
                    TagType = (int)x.TagType,
                    TagTypeLabel = RiskTagLabel(x.TagType),
                    Label = x.Label,
                    Reason = x.Reason,
                    Source = (int)x.Source,
                    SourceLabel = SourceLabel(x.Source),
                    IsActive = x.IsActive
                })
                .ToArray()
        };
    }

    private static string BusinessCategoryLabel(HsCodeBusinessCategory category)
        => category switch
        {
            HsCodeBusinessCategory.Food => "식품 관련",
            HsCodeBusinessCategory.GeneralCargo => "일반 화물",
            HsCodeBusinessCategory.Mixed => "복합",
            _ => "미분류"
        };

    private static string RiskTagLabel(HsCodeRiskTagType tagType)
        => tagType switch
        {
            HsCodeRiskTagType.Food => "식품 관련",
            HsCodeRiskTagType.FoodQuarantine => "검역/식품신고 확인",
            HsCodeRiskTagType.SupplementOrPreparedFoodReview => "조제식품/보충제 검토",
            HsCodeRiskTagType.Textile => "섬유/의류",
            HsCodeRiskTagType.Chemical => "화학물질 확인",
            HsCodeRiskTagType.ElectricalCertification => "전기/인증 확인",
            HsCodeRiskTagType.BatteryIncludedPossible => "배터리 포함 가능",
            HsCodeRiskTagType.Furniture => "가구/생활용품",
            HsCodeRiskTagType.BrokerReviewRecommended => "관세사 검토 권장",
            _ => tagType.ToString()
        };

    private static string SourceLabel(HsCodeRiskTagSource source)
        => source switch
        {
            HsCodeRiskTagSource.SystemRule => "시스템 규칙",
            HsCodeRiskTagSource.AdminOverride => "관리자 보정",
            HsCodeRiskTagSource.BrokerReview => "관세사 검토",
            _ => source.ToString()
        };

    private Task LogAsync(string actionName, object metadata, HS코드운영자Context context, CancellationToken cancellationToken)
    {
        return _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = App식별자.HongdalAdmin,
            UserId = context.UserId,
            UserName = context.UserName,
            RoleName = context.RoleName,
            ActionType = "HsCodeOperations",
            ActionName = actionName,
            Route = context.Route,
            TraceId = context.TraceId,
            IsSuccess = true,
            ClientIp = context.ClientIp,
            UserAgent = context.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(metadata)
        }, cancellationToken);
    }

    private static Result<T> BadRequest<T>(string message) => Result.Fail<T>(message);

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));
}
