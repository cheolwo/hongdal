using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Domain.HsCodes;
using 살뜰.Data;

namespace Ssalddel.Application.Customs;

public interface I화주HS코드검토조회UseCase
{
    Task<Result<화주HS코드검토목록응답>> 목록Async(
        string? query,
        int? businessCategory,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<화주HS코드검토상세응답>> 상세Async(
        long reviewId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.CustomsAndTradeData)]
[SsalddelUseCase(
    "화주 HS 코드 검토 조회",
    Summary = "화주와 판매자가 활성 HS 코드의 출처, 공개 공식 사례와 공개 동의된 대행 경험을 확인합니다.")]
[SsalddelUseCaseActor(SsalddelActor.ShipperOrSeller)]
[SsalddelUseCaseActor(SsalddelActor.CustomsBroker, SsalddelUseCaseActorRole.Supporting)]
public sealed class 화주HS코드검토조회UseCase(SsalddelContext db) : I화주HS코드검토조회UseCase
{
    public async Task<Result<화주HS코드검토목록응답>> 목록Async(
        string? query,
        int? businessCategory,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 50);

        var hsCodes = db.HsCodeEntries
            .AsNoTracking()
            .Include(entry => entry.CatalogVersion)
            .Include(entry => entry.RiskTags)
            .Where(entry => entry.IsActive &&
                            entry.CatalogVersion != null &&
                            entry.CatalogVersion.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            var normalizedTerm = 숫자만(term);
            hsCodes = hsCodes.Where(entry =>
                entry.Code.Contains(term) ||
                entry.NormalizedCode.Contains(term) ||
                entry.KoreanName.Contains(term) ||
                entry.EnglishName.Contains(term) ||
                entry.Description.Contains(term) ||
                entry.SearchKeywords.Contains(term) ||
                (!string.IsNullOrWhiteSpace(normalizedTerm) && entry.NormalizedCode.Contains(normalizedTerm)));
        }

        if (businessCategory.HasValue)
        {
            if (!Enum.IsDefined(typeof(HsCodeBusinessCategory), businessCategory.Value))
            {
                return Result.Fail<화주HS코드검토목록응답>("지원하지 않는 HS 코드 업무 분류입니다.");
            }

            var category = (HsCodeBusinessCategory)businessCategory.Value;
            hsCodes = hsCodes.Where(entry => entry.BusinessCategory == category);
        }

        var totalCount = await hsCodes.CountAsync(cancellationToken);
        var entries = await hsCodes
            .OrderBy(entry => entry.NormalizedCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return Result.Ok(new 화주HS코드검토목록응답
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        var entryIds = entries.Select(entry => entry.Id).ToList();
        var codeCandidates = entries
            .SelectMany(화주HS코드검토투영정책.조회코드후보)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var officialCases = await db.HsCodeClassificationCases
            .AsNoTracking()
            .Where(item => item.IsPublicOfficialCase &&
                ((item.HsCodeEntryId.HasValue && entryIds.Contains(item.HsCodeEntryId.Value)) ||
                 (!item.HsCodeEntryId.HasValue && codeCandidates.Contains(item.HsCode))))
            .Select(item => new { item.HsCodeEntryId, item.HsCode })
            .ToListAsync(cancellationToken);

        var publicAgencyExperiences = await db.HsCodePlatformAgencyExperiences
            .AsNoTracking()
            .Where(item => item.ContributorConsented &&
                           !item.IsPaidDetail &&
                           codeCandidates.Contains(item.HsCode))
            .Select(item => new { item.HsCode, item.AgencyType })
            .ToListAsync(cancellationToken);

        return Result.Ok(new 화주HS코드검토목록응답
        {
            Items = entries.Select(entry =>
            {
                var matchingExperiences = publicAgencyExperiences
                    .Where(item => 화주HS코드검토투영정책.동일코드(entry, item.HsCode))
                    .ToArray();
                return 화주HS코드검토투영정책.항목(
                    entry,
                    officialCases.Count(item =>
                        item.HsCodeEntryId.HasValue
                            ? item.HsCodeEntryId == entry.Id
                            : 화주HS코드검토투영정책.동일코드(entry, item.HsCode)),
                    matchingExperiences.Count(item => 화주HS코드검토투영정책.통관대행(item.AgencyType)),
                    matchingExperiences.Count(item => 화주HS코드검토투영정책.수입대행(item.AgencyType)));
            }).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<화주HS코드검토상세응답>> 상세Async(
        long reviewId,
        CancellationToken cancellationToken)
    {
        if (reviewId <= 0)
        {
            return Result.Fail<화주HS코드검토상세응답>("조회할 HS 코드 검토 ID를 확인해 주세요.");
        }

        var entry = await db.HsCodeEntries
            .AsNoTracking()
            .Include(item => item.CatalogVersion)
            .Include(item => item.RiskTags)
            .FirstOrDefaultAsync(item =>
                item.Id == reviewId &&
                item.IsActive &&
                item.CatalogVersion != null &&
                item.CatalogVersion.IsActive,
                cancellationToken);
        if (entry is null)
        {
            return NotFound<화주HS코드검토상세응답>("활성 HS 코드 검토 항목을 찾을 수 없습니다.");
        }

        var codeCandidates = 화주HS코드검토투영정책.조회코드후보(entry).ToList();
        var officialCaseQuery = db.HsCodeClassificationCases
            .AsNoTracking()
            .Where(item => item.IsPublicOfficialCase &&
                ((item.HsCodeEntryId.HasValue && item.HsCodeEntryId == entry.Id) ||
                 (!item.HsCodeEntryId.HasValue && codeCandidates.Contains(item.HsCode))));
        var publicAgencyQuery = db.HsCodePlatformAgencyExperiences
            .AsNoTracking()
            .Where(item => item.ContributorConsented &&
                           !item.IsPaidDetail &&
                           codeCandidates.Contains(item.HsCode));

        var officialCaseCount = await officialCaseQuery.CountAsync(cancellationToken);
        var agencyTypes = await publicAgencyQuery
            .Select(item => item.AgencyType)
            .ToListAsync(cancellationToken);
        var officialCases = await officialCaseQuery
            .OrderByDescending(item => item.DecidedAt ?? item.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);
        var publicAgencyExperiences = await publicAgencyQuery
            .OrderByDescending(item => item.CompletedAtUtc ?? item.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        var mappedExperiences = publicAgencyExperiences
            .Select(화주HS코드검토투영정책.공개대행경험)
            .ToArray();

        return Result.Ok(new 화주HS코드검토상세응답
        {
            Item = 화주HS코드검토투영정책.항목(
                entry,
                officialCaseCount,
                agencyTypes.Count(화주HS코드검토투영정책.통관대행),
                agencyTypes.Count(화주HS코드검토투영정책.수입대행)),
            RiskTags = entry.RiskTags
                .Where(tag => tag.IsActive)
                .OrderBy(tag => (int)tag.TagType)
                .Select(화주HS코드검토투영정책.주의태그)
                .ToArray(),
            OfficialCases = officialCases
                .Select(화주HS코드검토투영정책.공식사례)
                .ToArray(),
            AgencyExperiences = mappedExperiences,
            RequiredDocuments = mappedExperiences
                .SelectMany(item => item.RequiredDocuments)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray()
        });
    }

    private static string 숫자만(string value)
        => new(value.Where(char.IsDigit).ToArray());

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message)
            .WithMetadata("StatusCode", StatusCodes.Status404NotFound));
}
