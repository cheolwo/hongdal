using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Services.FoodCulture;

public interface IOfficialFoodIngredientCompanyResearchService
{
    Task<OfficialFoodIngredientCompanyResearchResponse> ResearchAsync(
        OfficialFoodIngredientCompanyQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class OfficialFoodIngredientCompanyResearchService(
    IOfficialFoodIngredientDomesticCompanySource domesticSource,
    IOfficialFoodIngredientImportedCompanySource importedSource,
    TimeProvider timeProvider,
    ILogger<OfficialFoodIngredientCompanyResearchService> logger)
    : IOfficialFoodIngredientCompanyResearchService
{
    private const string MfdsProvider = "식품의약품안전처·식품안전나라";

    public async Task<OfficialFoodIngredientCompanyResearchResponse> ResearchAsync(
        OfficialFoodIngredientCompanyQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var ingredientName = query.IngredientName?.Trim() ?? string.Empty;
        if (ingredientName.Length == 0
            || (ingredientName.Length < 2
                && string.IsNullOrWhiteSpace(query.IngredientKey)))
        {
            throw new ArgumentException(
                "한 글자 재료는 전산화된 재료 키와 함께 조회해야 합니다.",
                nameof(query));
        }

        var take = Math.Clamp(query.Take, 1, 100);
        var researchedAtUtc = timeProvider.GetUtcNow();
        var candidates = new List<OfficialFoodIngredientCompanyCandidateDto>();
        var sources = new List<OfficialFoodIngredientCompanySourceDto>();

        await ResearchDomesticAsync(
            ingredientName,
            take,
            researchedAtUtc,
            candidates,
            sources,
            cancellationToken);
        await ResearchImportedAsync(
            ingredientName,
            take,
            researchedAtUtc,
            candidates,
            sources,
            cancellationToken);

        var distinctCandidates = candidates
            .DistinctBy(candidate => candidate.CandidateKey)
            .OrderBy(candidate => candidate.RelationCode, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.OrganizationName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.RelatedProductName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var selectedCandidates = TakeBalanced(distinctCandidates, take);

        return new OfficialFoodIngredientCompanyResearchResponse(
            DetermineStatus(sources, selectedCandidates.Count),
            query.IngredientKey?.Trim() ?? string.Empty,
            ingredientName,
            researchedAtUtc,
            sources,
            selectedCandidates,
            [
                "표시된 업체는 공식 제품·표시 이력에서 재료 관계가 확인된 조사 후보이며 현재 재고, 공급능력, 판매 의사 또는 계약 권한을 보증하지 않습니다.",
                "대표자명, 전화번호, 상세 주소 등 개인 또는 직접 연락 정보는 공개 화면에 복제하지 않습니다.",
                "플랫폼은 업체를 자동 추천·선정·초대하지 않으며 실제 거래 전 당사자 동의와 최신 인허가·인증·수입중단 상태를 다시 확인해야 합니다.",
                "음식의 문화적 국가, 제품 제조국, 상품 원산지와 실제 선적 출발국은 서로 다른 정보로 관리합니다.",
                "중국 권역은 식약처 등록 해외제조업소의 소재 근거이며 원재료 재배지·어획지나 법정 원산지 표기를 대신하지 않습니다."
            ]);
    }

    private async Task ResearchDomesticAsync(
        string ingredientName,
        int take,
        DateTimeOffset researchedAtUtc,
        ICollection<OfficialFoodIngredientCompanyCandidateDto> candidates,
        ICollection<OfficialFoodIngredientCompanySourceDto> sources,
        CancellationToken cancellationToken)
    {
        if (!domesticSource.IsConfigured)
        {
            sources.Add(DomesticSource(
                OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured,
                "식품안전나라 인증키를 설정하면 원재료별 국내 품목제조보고 업체를 조회합니다."));
            return;
        }

        try
        {
            var records = await domesticSource.SearchAsync(
                ingredientName,
                take,
                cancellationToken);
            foreach (var record in records)
            {
                candidates.Add(new OfficialFoodIngredientCompanyCandidateDto(
                    CandidateKey(
                        OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer,
                        "KR",
                        record.OrganizationName,
                        record.ProductReportNumber,
                        record.ProductName),
                    record.OrganizationName,
                    "KR",
                    "대한민국",
                    OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer,
                    OfficialFoodIngredientCompanyEvidenceCodes.DomesticProductIngredientReport,
                    $"품목제조보고 원재료 목록에 '{ingredientName}' 관련 표기가 있는 제품 이력",
                    record.ProductName,
                    record.ProductCategory,
                    record.LicenseNumber,
                    OfficialFoodIngredientCompanyVerificationStatusCodes.OfficialProductReport,
                    false,
                    string.Empty,
                    MfdsIngredientProductCompanySource.SourceKey,
                    "식품(첨가물) 품목제조보고(원재료)",
                    MfdsIngredientProductCompanySource.DocumentationUrl,
                    researchedAtUtc,
                    true,
                    false,
                    false)
                {
                    RawIngredientText = record.RawIngredientText,
                    EvidenceDate = record.ReportDate,
                    EvidenceLastChangedDate = record.ChangedDate,
                    EvidenceSequence = record.RawIngredientOrder,
                    EvidenceRecordIdentifier = record.ProductReportNumber
                });
            }

            sources.Add(DomesticSource(
                OfficialFoodIngredientCompanySourceStatusCodes.Available,
                $"원재료 기준 공식 제품 이력 {records.Count:N0}건을 확인했습니다."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to query MFDS domestic ingredient company evidence for {IngredientName}",
                ingredientName);
            sources.Add(DomesticSource(
                OfficialFoodIngredientCompanySourceStatusCodes.Failed,
                "국내 품목제조보고 원천을 현재 조회하지 못했습니다. 잠시 뒤 다시 확인해 주세요."));
        }
    }

    private async Task ResearchImportedAsync(
        string ingredientName,
        int take,
        DateTimeOffset researchedAtUtc,
        ICollection<OfficialFoodIngredientCompanyCandidateDto> candidates,
        ICollection<OfficialFoodIngredientCompanySourceDto> sources,
        CancellationToken cancellationToken)
    {
        if (!importedSource.IsLabelSourceConfigured)
        {
            sources.Add(ImportedLabelSource(
                OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured,
                "공공데이터포털 서비스키를 설정하면 원재료별 수입업체·해외 제조업소 이력을 조회합니다."));
            sources.Add(ForeignFacilitySource(
                importedSource.IsForeignFacilitySourceConfigured
                    ? OfficialFoodIngredientCompanySourceStatusCodes.SupportingSource
                    : OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured,
                importedSource.IsForeignFacilitySourceConfigured
                    ? "해외 제조업소 명부는 제품 표시 이력이 생긴 뒤 보조 검증에 사용합니다."
                    : "공공데이터포털 서비스키를 설정하면 해외 제조업소 코드와 주의 상태를 보조 확인합니다."));
            return;
        }

        try
        {
            var result = await importedSource.SearchAsync(
                ingredientName,
                take,
                cancellationToken);
            foreach (var record in result.Records)
            {
                AddImportedCandidates(record, ingredientName, researchedAtUtc, candidates);
            }

            sources.Add(ImportedLabelSource(
                OfficialFoodIngredientCompanySourceStatusCodes.Available,
                $"수입식품 표시 이력 {result.Records.Count:N0}건에서 업체 관계를 확인했습니다."));
            sources.Add(ForeignFacilitySource(
                importedSource.IsForeignFacilitySourceConfigured
                    ? ForeignFacilityStatus(result)
                    : OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured,
                importedSource.IsForeignFacilitySourceConfigured
                    ? ForeignFacilityMessage(result)
                    : "공공데이터포털 서비스키를 설정하면 해외 제조업소 코드와 주의 상태를 보조 확인합니다."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to query MFDS imported ingredient company evidence for {IngredientName}",
                ingredientName);
            sources.Add(ImportedLabelSource(
                OfficialFoodIngredientCompanySourceStatusCodes.Failed,
                "수입식품 한글표시사항 원천을 현재 조회하지 못했습니다. 잠시 뒤 다시 확인해 주세요."));
            sources.Add(ForeignFacilitySource(
                importedSource.IsForeignFacilitySourceConfigured
                    ? OfficialFoodIngredientCompanySourceStatusCodes.SupportingSource
                    : OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured,
                "직접 원재료 근거를 찾지 못해 해외 제조업소 보조 대조를 수행하지 않았습니다."));
        }
    }

    private static void AddImportedCandidates(
        OfficialFoodIngredientImportedCompanyRecord record,
        string ingredientName,
        DateTimeOffset researchedAtUtc,
        ICollection<OfficialFoodIngredientCompanyCandidateDto> candidates)
    {
        if (!string.IsNullOrWhiteSpace(record.ImporterName))
        {
            candidates.Add(new OfficialFoodIngredientCompanyCandidateDto(
                CandidateKey(
                    OfficialFoodIngredientCompanyRelationCodes.DomesticImporter,
                    "KR",
                    record.ImporterName,
                    string.Empty,
                    record.ProductName),
                record.ImporterName,
                "KR",
                "대한민국",
                OfficialFoodIngredientCompanyRelationCodes.DomesticImporter,
                OfficialFoodIngredientCompanyEvidenceCodes.ImportedProductIngredientLabel,
                $"수입식품 한글표시사항 원재료명에 '{ingredientName}' 관련 표기가 있는 제품 이력",
                record.ProductName,
                record.ProductCategory,
                string.Empty,
                OfficialFoodIngredientCompanyVerificationStatusCodes.ImportedLabelEvidenceOnly,
                false,
                string.Empty,
                MfdsImportedFoodIngredientCompanySource.LabelSourceKey,
                "수입식품 제품별 한글표시사항",
                MfdsImportedFoodIngredientCompanySource.LabelDocumentationUrl,
                researchedAtUtc,
                true,
                false,
                false)
            {
                RawIngredientText = record.RawIngredientText,
                EvidenceDate = record.ProcessedDate
            });
        }

        if (string.IsNullOrWhiteSpace(record.ForeignManufacturerName))
        {
            return;
        }

        var manufacturerRegion =
            ChinaImportedFoodManufacturerRegionClassifier.Classify(
                record.ManufacturerCountryName,
                record.ForeignManufacturerAreaName,
                record.ForeignManufacturerAddress)
            ?? UnitedStatesImportedFoodManufacturerRegionClassifier.Classify(
                record.ManufacturerCountryName,
                record.ForeignManufacturerAreaName,
                record.ForeignManufacturerAddress)
            ?? JapanImportedFoodManufacturerPrefectureClassifier.Classify(
                record.ManufacturerCountryName,
                record.ForeignManufacturerAreaName,
                record.ForeignManufacturerAddress);
        candidates.Add(new OfficialFoodIngredientCompanyCandidateDto(
            CandidateKey(
                OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer,
                CountryCode(record.ManufacturerCountryName),
                record.ForeignManufacturerName,
                record.ForeignManufacturerIdentifier,
                record.ProductName),
            record.ForeignManufacturerName,
            CountryCode(record.ManufacturerCountryName),
            record.ManufacturerCountryName,
            OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer,
            OfficialFoodIngredientCompanyEvidenceCodes.ImportedProductIngredientLabel,
            $"수입식품 한글표시사항 원재료명에 '{ingredientName}' 관련 표기가 있는 해외 제조 제품 이력",
            record.ProductName,
            record.ProductCategory,
            record.ForeignManufacturerIdentifier,
            record.ForeignManufacturerRegistryMatched
                ? OfficialFoodIngredientCompanyVerificationStatusCodes.OverseasFacilityMatched
                : OfficialFoodIngredientCompanyVerificationStatusCodes.ImportedLabelEvidenceOnly,
            record.RequiresAttention,
            record.AttentionReason,
            MfdsImportedFoodIngredientCompanySource.LabelSourceKey,
            "수입식품 제품별 한글표시사항",
            MfdsImportedFoodIngredientCompanySource.LabelDocumentationUrl,
            researchedAtUtc,
            true,
            false,
            false)
        {
            RawIngredientText = record.RawIngredientText,
            EvidenceDate = record.ProcessedDate,
            ManufacturerRegionCode = manufacturerRegion?.RegionCode ?? string.Empty,
            ManufacturerRegionName = manufacturerRegion?.RegionName ?? string.Empty,
            ManufacturerRegionScope = manufacturerRegion?.RegionScope ?? string.Empty,
            ManufacturerRegionClassificationMethod =
                manufacturerRegion?.ClassificationMethodCode ?? string.Empty,
            ManufacturerRegionEvidence = manufacturerRegion?.Evidence ?? string.Empty,
            ManufacturerRegionConfidence = manufacturerRegion?.Confidence ?? 0m
        });
    }

    private static OfficialFoodIngredientCompanySourceDto DomesticSource(
        string statusCode,
        string message)
        => new(
            MfdsIngredientProductCompanySource.SourceKey,
            MfdsProvider,
            "식품(첨가물) 품목제조보고(원재료)",
            "대한민국",
            MfdsIngredientProductCompanySource.DocumentationUrl,
            statusCode,
            message,
            true,
            false,
            true);

    private static OfficialFoodIngredientCompanySourceDto ImportedLabelSource(
        string statusCode,
        string message)
        => new(
            MfdsImportedFoodIngredientCompanySource.LabelSourceKey,
            "식품의약품안전처",
            "수입식품 제품별 한글표시사항",
            "대한민국으로 수입된 국내외 제품",
            MfdsImportedFoodIngredientCompanySource.LabelDocumentationUrl,
            statusCode,
            message,
            true,
            false,
            true);

    private static OfficialFoodIngredientCompanySourceDto ForeignFacilitySource(
        string statusCode,
        string message)
        => new(
            MfdsImportedFoodIngredientCompanySource.ForeignFacilitySourceKey,
            "식품의약품안전처",
            "수입식품 해외제조업소 정보",
            "대한민국 수입식품 등록 해외 시설",
            MfdsImportedFoodIngredientCompanySource.ForeignFacilityDocumentationUrl,
            statusCode,
            message,
            false,
            true,
            true);

    private static string ForeignFacilityStatus(
        OfficialFoodIngredientImportedCompanySourceResult result)
        => result.RegistryLookupFailed
            ? OfficialFoodIngredientCompanySourceStatusCodes.Failed
            : result.RegistryLookupAttempted
                ? OfficialFoodIngredientCompanySourceStatusCodes.Available
                : OfficialFoodIngredientCompanySourceStatusCodes.SupportingSource;

    private static string ForeignFacilityMessage(
        OfficialFoodIngredientImportedCompanySourceResult result)
        => result.RegistryLookupFailed
            ? "일부 해외 제조업소 보조 대조에 실패했습니다. 후보별 최신 상태를 공식 원천에서 다시 확인해야 합니다."
            : result.RegistryLookupAttempted
                ? "해외 제조업소명과 국가가 정확히 일치한 경우에만 시설 코드와 주의 상태를 보조 연결했습니다."
                : "대조할 해외 제조업소 후보가 없거나 보조 원천이 설정되지 않아 시설 대조를 수행하지 않았습니다.";

    private static string DetermineStatus(
        IReadOnlyCollection<OfficialFoodIngredientCompanySourceDto> sources,
        int candidateCount)
    {
        var directSources = sources.Where(source => source.ProvidesDirectIngredientEvidence).ToArray();
        var unavailableCount = directSources.Count(source =>
            source.StatusCode != OfficialFoodIngredientCompanySourceStatusCodes.Available);
        if (candidateCount > 0)
        {
            return unavailableCount > 0
                ? OfficialFoodIngredientCompanyResearchStatusCodes.Partial
                : OfficialFoodIngredientCompanyResearchStatusCodes.Available;
        }

        if (directSources.All(source =>
                source.StatusCode == OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured))
        {
            return OfficialFoodIngredientCompanyResearchStatusCodes.NotConfigured;
        }

        return unavailableCount > 0
            ? OfficialFoodIngredientCompanyResearchStatusCodes.Partial
            : OfficialFoodIngredientCompanyResearchStatusCodes.NoResults;
    }

    private static IReadOnlyList<OfficialFoodIngredientCompanyCandidateDto> TakeBalanced(
        IReadOnlyList<OfficialFoodIngredientCompanyCandidateDto> candidates,
        int take)
    {
        var relationOrder = new[]
        {
            OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer,
            OfficialFoodIngredientCompanyRelationCodes.DomesticImporter,
            OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer
        };
        var queues = relationOrder
            .Select(relationCode => new Queue<OfficialFoodIngredientCompanyCandidateDto>(
                candidates.Where(candidate => candidate.RelationCode == relationCode)))
            .ToArray();
        var selected = new List<OfficialFoodIngredientCompanyCandidateDto>(take);
        while (selected.Count < take && queues.Any(queue => queue.Count > 0))
        {
            foreach (var queue in queues)
            {
                if (queue.Count > 0 && selected.Count < take)
                {
                    selected.Add(queue.Dequeue());
                }
            }
        }

        return selected;
    }

    private static string CandidateKey(params string[] parts)
    {
        var normalized = string.Join('|', parts.Select(Normalize));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return $"organization-candidate:{Convert.ToHexString(hash)[..24].ToLowerInvariant()}";
    }

    private static string Normalize(string? value)
        => string.Concat((value ?? string.Empty)
            .Where(char.IsLetterOrDigit))
            .ToUpperInvariant();

    private static string CountryCode(string countryName)
        => countryName.Trim() switch
        {
            "대한민국" or "한국" => "KR",
            "미국" => "US",
            "일본" => "JP",
            "영국" => "GB",
            "캐나다" => "CA",
            "프랑스" => "FR",
            "중국" => "CN",
            "호주" => "AU",
            "뉴질랜드" => "NZ",
            "이탈리아" => "IT",
            "스페인" => "ES",
            "독일" => "DE",
            "베트남" => "VN",
            "태국" => "TH",
            "인도" => "IN",
            _ => string.Empty
        };
}
