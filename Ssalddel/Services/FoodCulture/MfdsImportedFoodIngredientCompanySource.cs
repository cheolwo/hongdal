using Microsoft.Extensions.Options;
using 살뜰.Services.External.Mfds;
using 살뜰.Services.Options;

namespace Ssalddel.Services.FoodCulture;

public sealed record OfficialFoodIngredientImportedCompanyRecord(
    string ImporterName,
    string ForeignManufacturerName,
    string ManufacturerCountryName,
    string ProductName,
    string ProductCategory,
    string RawIngredientText,
    string ProcessedDate,
    string ForeignManufacturerIdentifier,
    bool ForeignManufacturerRegistryMatched,
    bool RequiresAttention,
    string AttentionReason)
{
    public string ForeignManufacturerAreaName { get; init; } = string.Empty;

    public string ForeignManufacturerAddress { get; init; } = string.Empty;
}

public sealed record OfficialFoodIngredientImportedCompanySourceResult(
    IReadOnlyList<OfficialFoodIngredientImportedCompanyRecord> Records,
    bool RegistryLookupAttempted,
    bool RegistryLookupFailed);

public interface IOfficialFoodIngredientImportedCompanySource
{
    bool IsLabelSourceConfigured { get; }

    bool IsForeignFacilitySourceConfigured { get; }

    Task<OfficialFoodIngredientImportedCompanySourceResult> SearchAsync(
        string ingredientName,
        int take,
        CancellationToken cancellationToken = default);
}

public sealed class MfdsImportedFoodIngredientCompanySource
    : IOfficialFoodIngredientImportedCompanySource
{
    public const string LabelSourceKey = "mfds-imported-food-korean-label";

    public const string LabelDocumentationUrl =
        "https://www.data.go.kr/data/15110214/openapi.do";

    public const string ForeignFacilitySourceKey = "mfds-overseas-manufacturer";

    public const string ForeignFacilityDocumentationUrl =
        "https://www.data.go.kr/data/15073967/openapi.do";

    private readonly I수입식품한글표시사항조회Service _labelService;
    private readonly I해외제조업소조회Service _foreignFacilityService;
    private readonly 수입식품한글표시사항조회Options _labelOptions;
    private readonly 해외제조업소조회Options _foreignFacilityOptions;
    private readonly MfdsIngredientCompanyOptions _researchOptions;

    public MfdsImportedFoodIngredientCompanySource(
        I수입식품한글표시사항조회Service labelService,
        I해외제조업소조회Service foreignFacilityService,
        IOptions<수입식품한글표시사항조회Options> labelOptions,
        IOptions<해외제조업소조회Options> foreignFacilityOptions,
        IOptions<PublicDataOptions> publicDataOptions)
    {
        _labelService = labelService;
        _foreignFacilityService = foreignFacilityService;
        _labelOptions = labelOptions.Value;
        _foreignFacilityOptions = foreignFacilityOptions.Value;
        _researchOptions = publicDataOptions.Value.MfdsIngredientCompanies;
    }

    public bool IsLabelSourceConfigured
        => !string.IsNullOrWhiteSpace(_labelOptions.ServiceKey);

    public bool IsForeignFacilitySourceConfigured
        => !string.IsNullOrWhiteSpace(_foreignFacilityOptions.ServiceKey);

    public async Task<OfficialFoodIngredientImportedCompanySourceResult> SearchAsync(
        string ingredientName,
        int take,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ingredientName);
        if (!IsLabelSourceConfigured)
        {
            throw new InvalidOperationException(
                "수입식품한글표시사항조회:ServiceKey 또는 PublicData:DataGoKrServiceKey 설정이 필요합니다.");
        }

        var labelResponse = await _labelService.조회Async(
            new 수입식품한글표시사항조회요청DTO
            {
                페이지번호 = 1,
                한페이지결과수 = Math.Clamp(take * 2, 1, 100),
                데이터형식 = "json",
                원재료명 = ingredientName.Trim()
            },
            cancellationToken);
        EnsureSuccessfulResponse(
            labelResponse.결과코드,
            labelResponse.결과메시지,
            "수입식품 한글표시사항");
        var records = labelResponse.항목목록
            .Where(item => !string.IsNullOrWhiteSpace(item.수입업체명)
                           || !string.IsNullOrWhiteSpace(item.해외제조업소명))
            .Select(item => new OfficialFoodIngredientImportedCompanyRecord(
                Clean(item.수입업체명),
                Clean(item.해외제조업소명),
                FirstNonEmpty(item.제조국명, item.수출국명),
                FirstNonEmpty(item.한글제품명, item.영문제품명),
                Clean(item.품목명),
                Clean(item.원재료명),
                Clean(item.처리일자),
                string.Empty,
                false,
                false,
                string.Empty))
            .Take(Math.Clamp(take, 1, 100))
            .ToArray();

        if (!IsForeignFacilitySourceConfigured || records.Length == 0)
        {
            return new(records, false, false);
        }

        var registryLookupFailed = false;
        var matches = new Dictionary<string, 해외제조업소조회항목>(StringComparer.Ordinal);
        var lookupTargets = records
            .Where(record => !string.IsNullOrWhiteSpace(record.ForeignManufacturerName))
            .Select(record => new
            {
                record.ForeignManufacturerName,
                record.ManufacturerCountryName,
                Key = CompanyKey(record.ForeignManufacturerName, record.ManufacturerCountryName)
            })
            .DistinctBy(target => target.Key)
            .Take(Math.Clamp(_researchOptions.MaxForeignFacilityLookups, 0, 10))
            .ToArray();

        foreach (var target in lookupTargets)
        {
            try
            {
                var facilityResponse = await _foreignFacilityService.조회Async(
                    new 해외제조업소조회요청
                    {
                        페이지번호 = 1,
                        한페이지결과수 = 10,
                        데이터형식 = "json",
                        해외제조업소명 = target.ForeignManufacturerName,
                        국가명 = target.ManufacturerCountryName
                    },
                    cancellationToken);
                EnsureSuccessfulResponse(
                    facilityResponse.헤더?.결과코드,
                    facilityResponse.헤더?.결과메시지,
                    "수입식품 해외제조업소");
                var match = facilityResponse.본문?.아이템?.항목.FirstOrDefault(item =>
                    NamesEqual(item.해외제조업소명, target.ForeignManufacturerName)
                    && CountriesCompatible(item.국가명, target.ManufacturerCountryName));
                if (match is not null)
                {
                    matches[target.Key] = match;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                registryLookupFailed = true;
            }
        }

        var enriched = records.Select(record =>
        {
            if (!matches.TryGetValue(
                    CompanyKey(record.ForeignManufacturerName, record.ManufacturerCountryName),
                    out var match))
            {
                return record;
            }

            return record with
            {
                ForeignManufacturerIdentifier = Clean(match.해외제조업소코드),
                ForeignManufacturerRegistryMatched = true,
                ForeignManufacturerAreaName = Clean(match.지역명),
                ForeignManufacturerAddress = Clean(match.해외제조업소주소),
                RequiresAttention = match.주의필요여부,
                AttentionReason = Clean(match.주의사유)
            };
        }).ToArray();

        return new(enriched, lookupTargets.Length > 0, registryLookupFailed);
    }

    private static bool NamesEqual(string? left, string right)
        => Normalize(left) == Normalize(right);

    private static bool CountriesCompatible(string? left, string right)
        => string.IsNullOrWhiteSpace(left)
           || string.IsNullOrWhiteSpace(right)
           || Normalize(left) == Normalize(right);

    private static string CompanyKey(string name, string country)
        => $"{Normalize(name)}|{Normalize(country)}";

    private static string Normalize(string? value)
        => string.Concat((value ?? string.Empty)
            .Where(char.IsLetterOrDigit))
            .ToUpperInvariant();

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
           ?? string.Empty;

    private static string Clean(string? value)
        => value?.Trim() ?? string.Empty;

    private static void EnsureSuccessfulResponse(
        string? code,
        string? message,
        string sourceName)
    {
        if (string.IsNullOrWhiteSpace(code)
            || string.Equals(code, "00", StringComparison.OrdinalIgnoreCase)
            || string.Equals(code, "INFO-000", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new InvalidOperationException(
            $"{sourceName} 응답 오류입니다. Code={code}, Message={message}");
    }
}
