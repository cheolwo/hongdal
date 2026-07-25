using System.Globalization;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "국내 농산물 경락가격 원천 선택과 공개 조회 요청 검증",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "경락·정산가격을 KAMIS 중도매·소매 조사값과 구분하며 거래 실행이나 출하자 식별정보를 제공하지 않습니다.")]
public sealed class 국내농산물경락가격조회Service : I국내농산물경락가격조회Service
{
    private readonly IReadOnlyDictionary<string, I국내농산물경락가격공급자> _providers;

    public 국내농산물경락가격조회Service(
        IEnumerable<I국내농산물경락가격공급자> providers)
    {
        _providers = providers.ToDictionary(
            provider => provider.SourceKey,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<국내농산물경락가격원천응답> GetSources()
        => _providers.Values
            .Select(provider => provider.GetSource())
            .OrderBy(source => source.DisplayName, StringComparer.Ordinal)
            .ToArray();

    public Task<국내농산물경락가격조회응답> 조회Async(
        국내농산물경락가격조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalized = Normalize(request);
        var validationError = Validate(normalized);
        if (validationError is not null)
        {
            return Task.FromResult(Fail(
                normalized,
                국내농산물경락가격조회상태Codes.잘못된요청,
                validationError));
        }

        if (!_providers.TryGetValue(normalized.SourceKey, out var provider))
        {
            return Task.FromResult(Fail(
                normalized,
                국내농산물경락가격조회상태Codes.지원하지않는출처,
                $"지원하지 않는 경락가격 원천입니다. SourceKey={normalized.SourceKey}"));
        }

        return provider.조회Async(normalized, cancellationToken);
    }

    private static 국내농산물경락가격조회요청 Normalize(
        국내농산물경락가격조회요청 request)
        => new()
        {
            SourceKey = string.IsNullOrWhiteSpace(request.SourceKey)
                ? 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement
                : request.SourceKey.Trim(),
            SettlementDate = request.SettlementDate.Trim(),
            WholesaleMarketCode = NormalizeOptional(request.WholesaleMarketCode),
            CorporationCode = NormalizeOptional(request.CorporationCode),
            ItemName = NormalizeOptional(request.ItemName),
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 1000)
        };

    private static string? Validate(국내농산물경락가격조회요청 request)
    {
        if (!DateOnly.TryParseExact(
                request.SettlementDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            return "SettlementDate는 yyyy-MM-dd 형식이어야 합니다.";
        }

        if (request.WholesaleMarketCode?.Length > 20
            || request.CorporationCode?.Length > 30
            || request.ItemName?.Length > 100)
        {
            return "시장·법인·품목 검색값의 허용 길이를 초과했습니다.";
        }

        return null;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static 국내농산물경락가격조회응답 Fail(
        국내농산물경락가격조회요청 request,
        string statusCode,
        string errorMessage)
        => new()
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            Query = request,
            Notices = DefaultNotices
        };

    internal static IReadOnlyList<string> DefaultNotices { get; } =
    [
        "경락가격은 실제 도매시장 경매·정산 단계의 가격이며 KAMIS 중도매·소매 조사값과 같은 값이 아닙니다.",
        "단위중량·포장·크기·등급이 다른 가격을 그대로 평균하거나 비교하지 마세요.",
        "출하자·생산자·중도매인 식별정보는 수집·저장·공개하지 않습니다."
    ];
}
