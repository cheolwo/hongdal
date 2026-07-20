using System.Text.Json;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Domain.HsCodes;

namespace Ssalddel.Application.Customs;

/// <summary>
/// HS 코드 원장 값을 화주 조회 계약으로 바꾸는 표시·공개 정책입니다.
/// 조회 순서와 영속성은 UseCase가 담당하고 이 형식은 개인정보를 포함하지 않습니다.
/// </summary>
internal static class 화주HS코드검토투영정책
{
    internal static 화주HS코드검토항목응답 항목(
        HsCodeEntry entry,
        int officialCaseCount,
        int customsAgencyExperienceCount,
        int importAgencyExperienceCount)
    {
        var activeTags = entry.RiskTags
            .Where(tag => tag.IsActive)
            .OrderBy(tag => (int)tag.TagType)
            .ToArray();
        var risk = 위험도(activeTags, entry.BusinessCategory);

        return new 화주HS코드검토항목응답
        {
            ReviewId = entry.Id,
            Code = entry.Code,
            NormalizedCode = entry.NormalizedCode,
            KoreanName = entry.KoreanName,
            EnglishName = entry.EnglishName,
            Description = entry.Description,
            Level = (int)entry.Level,
            LevelLabel = 단계명(entry.Level),
            BusinessCategory = (int)entry.BusinessCategory,
            BusinessCategoryLabel = 업무분류명(entry.BusinessCategory),
            RiskLevelCode = risk.Code,
            RiskLevelLabel = risk.Label,
            BrokerReviewRecommended = activeTags.Any(tag =>
                tag.TagType == HsCodeRiskTagType.BrokerReviewRecommended),
            RiskTagLabels = activeTags
                .Select(tag => string.IsNullOrWhiteSpace(tag.Label)
                    ? 주의태그명(tag.TagType)
                    : tag.Label.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            OfficialCaseCount = officialCaseCount,
            CustomsAgencyExperienceCount = customsAgencyExperienceCount,
            ImportAgencyExperienceCount = importAgencyExperienceCount,
            Source = 출처(entry.CatalogVersion)
        };
    }

    internal static 화주HS코드주의태그응답 주의태그(HsCodeEntryRiskTag tag)
        => new()
        {
            TagType = (int)tag.TagType,
            Label = string.IsNullOrWhiteSpace(tag.Label) ? 주의태그명(tag.TagType) : tag.Label.Trim(),
            Reason = tag.Reason.Trim(),
            SourceLabel = tag.Source switch
            {
                HsCodeRiskTagSource.SystemRule => "시스템 규칙",
                HsCodeRiskTagSource.AdminOverride => "운영자 보정",
                HsCodeRiskTagSource.BrokerReview => "관세사 검토",
                _ => "확인 필요"
            }
        };

    internal static 화주HS코드공식분류사례응답 공식사례(HsCodeClassificationCase classificationCase)
        => new()
        {
            CaseId = classificationCase.Id,
            CountryCode = classificationCase.CountryCode.Trim(),
            SourceType = classificationCase.SourceType.Trim(),
            SourceReferenceNo = classificationCase.SourceReferenceNo.Trim(),
            SourceUrl = 안전한Http주소(classificationCase.SourceUrl),
            IssuingAuthority = classificationCase.IssuingAuthority.Trim(),
            DecidedAt = classificationCase.DecidedAt,
            ProductName = classificationCase.ProductName.Trim(),
            GoodsDescription = classificationCase.GoodsDescription.Trim(),
            DecisionReason = classificationCase.DecisionReason.Trim()
        };

    internal static 화주HS코드공개대행경험응답 공개대행경험(HsCodePlatformAgencyExperience experience)
        => new()
        {
            ExperienceId = experience.Id,
            AgencyType = experience.AgencyType.Trim(),
            AgencyTypeLabel = 대행유형명(experience.AgencyType),
            CountryRoute = experience.CountryRoute.Trim(),
            CaseStatus = experience.CaseStatus.Trim(),
            RiskLevel = experience.RiskLevel.Trim(),
            Summary = experience.Summary.Trim(),
            RequiredDocuments = 필요서류(experience.RequiredDocumentsJson),
            DisclosurePolicy = experience.DisclosurePolicy.Trim(),
            CompletedAtUtc = experience.CompletedAtUtc
        };

    internal static bool 동일코드(HsCodeEntry entry, string? hsCode)
        => string.Equals(
            정규화(hsCode),
            string.IsNullOrWhiteSpace(entry.NormalizedCode)
                ? 정규화(entry.Code)
                : 정규화(entry.NormalizedCode),
            StringComparison.Ordinal);

    internal static IReadOnlyList<string> 조회코드후보(HsCodeEntry entry)
        => new[] { entry.Code.Trim(), entry.NormalizedCode.Trim() }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    internal static bool 통관대행(string? agencyType)
    {
        var value = agencyType?.Trim() ?? string.Empty;
        return value.Contains("customs", StringComparison.OrdinalIgnoreCase)
               || value.Contains("통관", StringComparison.Ordinal)
               || value.Contains("관세", StringComparison.Ordinal);
    }

    internal static bool 수입대행(string? agencyType)
    {
        var value = agencyType?.Trim() ?? string.Empty;
        return value.Contains("import", StringComparison.OrdinalIgnoreCase)
               || value.Contains("수입", StringComparison.Ordinal);
    }

    private static 화주HS코드검토출처응답 출처(HsCodeCatalogVersion? catalog)
        => catalog is null
            ? new 화주HS코드검토출처응답()
            : new 화주HS코드검토출처응답
            {
                StandardCode = catalog.StandardCode.Trim(),
                CountryCode = catalog.CountryCode.Trim(),
                CodeDigits = catalog.CodeDigits,
                Revision = catalog.Revision.Trim(),
                SourceName = catalog.SourceName.Trim(),
                SourceUrl = 안전한Http주소(catalog.SourceUrl),
                EffectiveFrom = catalog.EffectiveFrom,
                EffectiveTo = catalog.EffectiveTo,
                ImportedAtUtc = catalog.ImportedAtUtc
            };

    private static (string Code, string Label) 위험도(
        IReadOnlyCollection<HsCodeEntryRiskTag> tags,
        HsCodeBusinessCategory category)
    {
        if (tags.Any(tag => tag.TagType is
                HsCodeRiskTagType.FoodQuarantine or
                HsCodeRiskTagType.SupplementOrPreparedFoodReview or
                HsCodeRiskTagType.Chemical or
                HsCodeRiskTagType.BatteryIncludedPossible))
        {
            return ("high", "사전 확인 필요");
        }

        if (tags.Count > 0 || category == HsCodeBusinessCategory.Unknown)
        {
            return ("review", "검토 권장");
        }

        return ("low", "기본 확인");
    }

    private static string 단계명(HsCodeLevel level)
        => level switch
        {
            HsCodeLevel.Chapter => "류(2단위)",
            HsCodeLevel.Heading => "호(4단위)",
            HsCodeLevel.Subheading => "소호(6단위)",
            HsCodeLevel.National => "국가 세번",
            _ => "분류 단위 확인 필요"
        };

    private static string 업무분류명(HsCodeBusinessCategory category)
        => category switch
        {
            HsCodeBusinessCategory.Food => "식품 관련",
            HsCodeBusinessCategory.GeneralCargo => "일반 화물",
            HsCodeBusinessCategory.Mixed => "복합 화물",
            _ => "미분류"
        };

    private static string 주의태그명(HsCodeRiskTagType tagType)
        => tagType switch
        {
            HsCodeRiskTagType.Food => "식품 관련",
            HsCodeRiskTagType.FoodQuarantine => "검역·식품신고 확인",
            HsCodeRiskTagType.SupplementOrPreparedFoodReview => "조제식품·보충제 검토",
            HsCodeRiskTagType.Textile => "섬유·의류",
            HsCodeRiskTagType.Chemical => "화학물질 확인",
            HsCodeRiskTagType.ElectricalCertification => "전기·인증 확인",
            HsCodeRiskTagType.BatteryIncludedPossible => "배터리 포함 가능",
            HsCodeRiskTagType.Furniture => "가구·생활용품",
            HsCodeRiskTagType.BrokerReviewRecommended => "관세사 검토 권장",
            _ => "추가 확인"
        };

    private static string 대행유형명(string? agencyType)
        => 통관대행(agencyType)
            ? "통관 대행"
            : 수입대행(agencyType)
                ? "수입 대행"
                : "공개 대행 경험";

    private static IReadOnlyList<string> 필요서류(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return (JsonSerializer.Deserialize<string[]>(json) ?? [])
                .Select(value => value?.Trim() ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Take(20)
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string 정규화(string? hsCode)
        => new((hsCode ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string 안전한Http주소(string? value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return string.Empty;
        }

        return uri.AbsoluteUri;
    }
}
