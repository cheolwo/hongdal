using System.Text.RegularExpressions;
using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.External.Naver;
using Ssalddel.Services.Food;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Services.Operations;

public interface IOperatingMarketDeliveryScopeService
{
    Task<OperatingMarketDeliveryScopePlan> ResolveAsync(
        OperatingMarketDeliveryScopeResolveRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 한국 주소를 상세주소가 없는 시·군·구/읍·면·동 단위 같이 주문 모집권으로 정규화합니다.
/// 도로명주소와 Kakao/Naver 지역 결과가 없을 때도 주소 구조만으로 후보를 만들되,
/// 어느 경로에서도 원문 상세주소는 결과나 자동집단 키에 포함하지 않습니다.
/// </summary>
public sealed class KoreaDeliveryScopeService : IOperatingMarketDeliveryScopeService
{
    private const int Level2MinimumParticipants = 3;
    private const int Level3MinimumParticipants = 5;

    private readonly IOperatingMarketDeployment _deployment;
    private readonly IOperatingMarketAddressLookupService _addressLookupService;
    private readonly I주문자집단배송권조회Service _scopeLookupService;
    private readonly IKakao좌표변환Service _kakaoGeocodingService;
    private readonly INaverMapsReverseGeocodingService _naverReverseGeocodingService;

    public KoreaDeliveryScopeService(
        IOperatingMarketDeployment deployment,
        IOperatingMarketAddressLookupService addressLookupService,
        I주문자집단배송권조회Service scopeLookupService,
        IKakao좌표변환Service kakaoGeocodingService,
        INaverMapsReverseGeocodingService naverReverseGeocodingService)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        ArgumentNullException.ThrowIfNull(addressLookupService);
        ArgumentNullException.ThrowIfNull(scopeLookupService);
        ArgumentNullException.ThrowIfNull(kakaoGeocodingService);
        ArgumentNullException.ThrowIfNull(naverReverseGeocodingService);

        _deployment = deployment;
        _addressLookupService = addressLookupService;
        _scopeLookupService = scopeLookupService;
        _kakaoGeocodingService = kakaoGeocodingService;
        _naverReverseGeocodingService = naverReverseGeocodingService;
    }

    public async Task<OperatingMarketDeliveryScopePlan> ResolveAsync(
        OperatingMarketDeliveryScopeResolveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestedMarketCode = string.IsNullOrWhiteSpace(request.MarketCode)
            ? _deployment.MarketCode
            : request.MarketCode;
        if (!OperatingMarketCodes.TryNormalize(requestedMarketCode, out var marketCode) ||
            marketCode != OperatingMarketCodes.Korea ||
            _deployment.MarketCode != OperatingMarketCodes.Korea)
        {
            return Failure("이 배포 환경에서는 한국 같이 주문 모집권만 계산할 수 있습니다.");
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            return Failure("같이 주문 모집권을 확인할 주소를 입력해 주세요.");
        }

        var address = request.Address.Trim();
        var officialAddress = await TryLookupOfficialAddressAsync(address, cancellationToken);
        var kakaoAddress = await TryLookupKakaoAddressAsync(
            officialAddress?.FormattedAddress ?? address,
            cancellationToken);
        var naverDistrict = await TryLookupNaverDistrictAsync(
            request,
            kakaoAddress,
            cancellationToken);

        var scopeResponse = _scopeLookupService.후보검색(new 주문자집단배송권조회요청
        {
            RoadAddress = officialAddress?.FormattedAddress ?? address,
            KakaoRegionLevel1 = FirstNotBlank(kakaoAddress?.Region1, naverDistrict?.SidoName),
            KakaoRegionLevel2 = FirstNotBlank(kakaoAddress?.Region2, naverDistrict?.SigunguName),
            KakaoRegionLevel3 = kakaoAddress?.Region3,
            PageSize = 5
        });
        if (!scopeResponse.Success || scopeResponse.Items.Count == 0)
        {
            return Failure(scopeResponse.ErrorMessage ?? "주소에서 같이 주문 모집권을 찾지 못했습니다.");
        }

        var level2 = scopeResponse.Items.FirstOrDefault(item => item.IsDefaultScope)
                     ?? scopeResponse.Items[0];
        var level2Key = BuildCanonicalScopeKey(
            "admin2",
            officialAddress?.AdministrativeAreaCode,
            level2.RoadAddressLevel1,
            level2.RoadAddressLevel2);
        var participantCount = request.ParticipantCount;
        var items = new List<OperatingMarketDeliveryScopeCandidate>
        {
            ToCandidate(
                level2,
                level2Key,
                OperatingDeliveryScopeTypeCodes.AdministrativeLevel2Recruitment,
                OperatingGeographicAreaTypeCodes.AdministrativeLevel2,
                GeographicCode(
                    officialAddress?.AdministrativeAreaCode,
                    level2.RoadAddressLevel1,
                    level2.RoadAddressLevel2,
                    useFullAdministrativeCode: false),
                parentScopeKey: null,
                isRecommended: true,
                isFineGrained: false,
                minimumParticipants: Level2MinimumParticipants,
                participantCount,
                OperatingDeliveryScopeLogisticsRoleCodes.UrbanHubConsolidation)
        };

        foreach (var scope in scopeResponse.Items.Where(item => !item.IsDefaultScope))
        {
            items.Add(ToCandidate(
                scope,
                BuildCanonicalScopeKey(
                    "admin3",
                    officialAddress?.AdministrativeAreaCode,
                    scope.RoadAddressLevel1,
                    scope.RoadAddressLevel2,
                    scope.RoadAddressLevel3),
                OperatingDeliveryScopeTypeCodes.AdministrativeLevel3Delivery,
                OperatingGeographicAreaTypeCodes.AdministrativeLevel3,
                GeographicCode(
                    officialAddress?.AdministrativeAreaCode,
                    scope.RoadAddressLevel1,
                    scope.RoadAddressLevel2,
                    useFullAdministrativeCode: true,
                    scope.RoadAddressLevel3),
                level2Key,
                isRecommended: false,
                isFineGrained: true,
                minimumParticipants: Level3MinimumParticipants,
                participantCount,
                OperatingDeliveryScopeLogisticsRoleCodes.LastMileStopConsolidation));
        }

        return new OperatingMarketDeliveryScopePlan
        {
            Success = true,
            MarketCode = OperatingMarketCodes.Korea,
            RecommendedScopeKey = level2Key,
            RecommendedDemandConsolidationScopeKey = level2Key,
            ProviderCode = ResolveProviderCode(officialAddress, kakaoAddress, naverDistrict),
            Items = items
        };
    }

    private async Task<OperatingMarketAddressCandidate?> TryLookupOfficialAddressAsync(
        string address,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _addressLookupService.SearchAsync(
                new OperatingMarketAddressSearchRequest
                {
                    MarketCode = OperatingMarketCodes.Korea,
                    Query = address,
                    Page = 1,
                    PageSize = 1
                },
                cancellationToken);
            return result.Success ? result.Items.FirstOrDefault() : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Kakao주소정보?> TryLookupKakaoAddressAsync(
        string address,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _kakaoGeocodingService.주소정보조회Async(address, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<NaverDistrictRegion?> TryLookupNaverDistrictAsync(
        OperatingMarketDeliveryScopeResolveRequest request,
        Kakao주소정보? kakaoAddress,
        CancellationToken cancellationToken)
    {
        var latitude = request.Latitude ?? kakaoAddress?.위도;
        var longitude = request.Longitude ?? kakaoAddress?.경도;
        if (latitude is null || longitude is null)
        {
            return null;
        }

        try
        {
            return await _naverReverseGeocodingService.ResolveDistrictAsync(
                latitude.Value,
                longitude.Value,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static OperatingMarketDeliveryScopeCandidate ToCandidate(
        주문자집단배송권후보항목 source,
        string scopeKey,
        string scopeTypeCode,
        string geographicAreaTypeCode,
        string geographicAreaCode,
        string? parentScopeKey,
        bool isRecommended,
        bool isFineGrained,
        int minimumParticipants,
        int? participantCount,
        string logisticsRoleCode)
        => new()
        {
            MarketCode = OperatingMarketCodes.Korea,
            ScopeKey = scopeKey,
            ScopeTypeCode = scopeTypeCode,
            LogisticsRoleCode = logisticsRoleCode,
            GeographicAreaTypeCode = geographicAreaTypeCode,
            GeographicAreaCode = geographicAreaCode,
            DisplayName = source.DisplayName,
            ParentScopeKey = parentScopeKey,
            IsRecommendedRecruitmentScope = isRecommended,
            IsRecommendedDemandConsolidationScope = isRecommended,
            IsFineGrained = isFineGrained,
            MinimumParticipantsForPublicDisplay = minimumParticipants,
            CanPublishForParticipantCount = participantCount >= minimumParticipants,
            SupportsLastMileBatching = true,
            RequiresLogisticsFeasibilityValidation = true,
            RequiresOperationalRouteValidation = true
        };

    private static string BuildCanonicalScopeKey(
        string level,
        string? administrativeAreaCode,
        params string?[] labels)
        => $"kr-{level}:{GeographicCode(administrativeAreaCode, labels, level == "admin3")}";

    private static string GeographicCode(
        string? administrativeAreaCode,
        string? level1,
        string? level2,
        bool useFullAdministrativeCode,
        string? level3 = null)
        => GeographicCode(
            administrativeAreaCode,
            [level1, level2, level3],
            useFullAdministrativeCode);

    private static string GeographicCode(
        string? administrativeAreaCode,
        IEnumerable<string?> labels,
        bool useFullAdministrativeCode)
    {
        var digits = Regex.Replace(administrativeAreaCode ?? string.Empty, "[^0-9]", string.Empty);
        if (digits.Length >= 5)
        {
            return useFullAdministrativeCode && digits.Length >= 10
                ? digits[..10]
                : digits[..5];
        }

        var labelKey = string.Join('-', labels
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Regex.Replace(
                value!.Trim().ToLowerInvariant(),
                @"[^0-9a-z가-힣]+",
                "-").Trim('-'))
            .Where(value => value.Length > 0));
        return labelKey.Length == 0 ? "unknown" : labelKey;
    }

    private static string ResolveProviderCode(
        OperatingMarketAddressCandidate? officialAddress,
        Kakao주소정보? kakaoAddress,
        NaverDistrictRegion? naverDistrict)
        => officialAddress is not null
            ? officialAddress.ProviderCode
            : kakaoAddress is not null
                ? "KakaoLocal"
                : naverDistrict is not null
                    ? OperatingMapProviderCodes.NaverMaps
                    : "AddressStructureFallback";

    private static string? FirstNotBlank(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static OperatingMarketDeliveryScopePlan Failure(string message)
        => new()
        {
            Success = false,
            MarketCode = OperatingMarketCodes.Korea,
            ErrorMessage = message
        };
}
