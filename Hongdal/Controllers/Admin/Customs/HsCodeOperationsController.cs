using System.Security.Claims;
using System.Text.Json;
using Hongdal.Contracts.Admin.Customs;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Domain.HsCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Audit;

namespace Hongdal.Controllers.Admin.Customs;

[ApiController]
[Authorize(Policy = "HsCode운영자전용")]
[Route("api/v1/admin/hs-codes")]
public sealed class HsCodeOperationsController : ControllerBase
{
    private readonly HongdalContext _db;
    private readonly I사용자행위로그Service _activityLogService;

    public HsCodeOperationsController(HongdalContext db, I사용자행위로그Service activityLogService)
    {
        _db = db;
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<ActionResult<AdminHsCodeListResponse>> 목록(
        [FromQuery] string? query,
        [FromQuery] int? businessCategory,
        [FromQuery] int? tagType,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
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
            var category = (HsCodeBusinessCategory)businessCategory.Value;
            hsCodes = hsCodes.Where(x => x.BusinessCategory == category);
        }

        if (tagType.HasValue)
        {
            var tag = (HsCodeRiskTagType)tagType.Value;
            hsCodes = hsCodes.Where(x => x.RiskTags.Any(t => t.TagType == tag && (includeInactive || t.IsActive)));
        }

        var totalCount = await hsCodes.CountAsync(cancellationToken);
        var items = await hsCodes
            .OrderBy(x => x.NormalizedCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new AdminHsCodeListResponse
        {
            Items = items.Select(Map).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpPut("{entryId:long}/business-category")]
    public async Task<ActionResult<AdminHsCodeEntryResponse>> 대분류수정(
        long entryId,
        [FromBody] AdminHsCodeBusinessCategoryUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(HsCodeBusinessCategory), request.BusinessCategory))
        {
            return BadRequest("지원하지 않는 HS 코드 업무 분류입니다.");
        }

        var entry = await _db.HsCodeEntries
            .Include(x => x.RiskTags)
            .FirstOrDefaultAsync(x => x.Id == entryId, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        entry.BusinessCategory = (HsCodeBusinessCategory)request.BusinessCategory;
        entry.BusinessCategoryReason = string.IsNullOrWhiteSpace(request.Reason)
            ? DefaultCorrectionReason()
            : request.Reason.Trim();
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("HsCodeBusinessCategoryUpdated", new { entryId, entry.Code, request.BusinessCategory }, cancellationToken);

        return Ok(Map(entry));
    }

    [HttpPost("{entryId:long}/risk-tags")]
    public async Task<ActionResult<AdminHsCodeEntryResponse>> 태그저장(
        long entryId,
        [FromBody] AdminHsCodeRiskTagUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(HsCodeRiskTagType), request.TagType))
        {
            return BadRequest("지원하지 않는 HS 코드 주의 태그입니다.");
        }

        var entry = await _db.HsCodeEntries
            .Include(x => x.RiskTags)
            .FirstOrDefaultAsync(x => x.Id == entryId, cancellationToken);

        if (entry is null)
        {
            return NotFound();
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

        ApplyTagUpdate(tag, request, ResolveTagSource());
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("HsCodeRiskTagSaved", new { entryId, entry.Code, request.TagType, request.IsActive }, cancellationToken);

        return Ok(Map(entry));
    }

    [HttpPut("risk-tags/{tagId:long}")]
    public async Task<ActionResult<AdminHsCodeEntryResponse>> 태그수정(
        long tagId,
        [FromBody] AdminHsCodeRiskTagUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(HsCodeRiskTagType), request.TagType))
        {
            return BadRequest("지원하지 않는 HS 코드 주의 태그입니다.");
        }

        var tag = await _db.HsCodeEntryRiskTags
            .Include(x => x.HsCodeEntry)
            .FirstOrDefaultAsync(x => x.Id == tagId, cancellationToken);

        if (tag?.HsCodeEntry is null)
        {
            return NotFound();
        }

        ApplyTagUpdate(tag, request, ResolveTagSource());
        tag.HsCodeEntry.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var entry = await _db.HsCodeEntries
            .AsNoTracking()
            .Include(x => x.RiskTags)
            .FirstAsync(x => x.Id == tag.HsCodeEntryId, cancellationToken);

        await LogAsync("HsCodeRiskTagUpdated", new { tagId, tag.HsCodeEntryId, request.TagType, request.IsActive }, cancellationToken);

        return Ok(Map(entry));
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

    private HsCodeRiskTagSource ResolveTagSource()
    {
        return User.IsInRole(역할명.관세사) && !User.IsInRole(역할명.서버관리자)
            ? HsCodeRiskTagSource.BrokerReview
            : HsCodeRiskTagSource.AdminOverride;
    }

    private string DefaultCorrectionReason()
        => User.IsInRole(역할명.관세사) && !User.IsInRole(역할명.서버관리자)
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

    private Task LogAsync(string actionName, object metadata, CancellationToken cancellationToken)
    {
        return _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = App식별자.HongdalAdmin,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            UserName = User.Identity?.Name ?? string.Empty,
            RoleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            ActionType = "HsCodeOperations",
            ActionName = actionName,
            Route = Request.Path.Value ?? string.Empty,
            TraceId = HttpContext.TraceIdentifier,
            IsSuccess = true,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(metadata)
        }, cancellationToken);
    }
}
