using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class 지역농수산MapPageViewModel(
    I농수산공공데이터Client dataClient) : PageViewModelBase
{
    private RegionalAgriculturalMapMarkerListResponse? _response;
    private RegionalAgriculturalMapMarkerDto? _selectedMarker;
    private string _productName = string.Empty;
    private string _countryCode = RegionalAgriculturalMapCountryCodes.Korea;
    private string? _relationTypeCode;

    public string CountryCode
    {
        get => _countryCode;
        private set
        {
            if (SetProperty(ref _countryCode, value))
            {
                OnPropertyChanged(nameof(CountryName));
                OnPropertyChanged(nameof(CountryDataSourceName));
            }
        }
    }

    public string CountryName
        => CountryCode == RegionalAgriculturalMapCountryCodes.UnitedStates
            ? "미국"
            : "대한민국";

    public string CountryDataSourceName
        => CountryCode == RegionalAgriculturalMapCountryCodes.UnitedStates
            ? "USDA AMS"
            : "KAMIS·MAFRA";

    public string? RelationTypeCode
    {
        get => _relationTypeCode;
        private set => SetProperty(ref _relationTypeCode, value);
    }

    public string ProductName
    {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    public RegionalAgriculturalMapMarkerListResponse? Response
    {
        get => _response;
        private set
        {
            if (SetProperty(ref _response, value))
            {
                OnPropertyChanged(nameof(Markers));
                OnPropertyChanged(nameof(Notices));
            }
        }
    }

    public IReadOnlyList<RegionalAgriculturalMapMarkerDto> Markers
        => Response?.Items ?? [];

    public IReadOnlyList<string> Notices
        => Response?.Notices ?? [];

    public RegionalAgriculturalMapMarkerDto? SelectedMarker
    {
        get => _selectedMarker;
        private set => SetProperty(ref _selectedMarker, value);
    }

    public void 마커선택(RegionalAgriculturalMapMarkerDto marker)
        => SelectedMarker = marker;

    public void 초기국가설정(string? countryCode)
        => CountryCode = RegionalAgriculturalMapCountryCodes.NormalizeOrDefault(countryCode);

    public async Task<bool> 국가선택Async(
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        var normalized = RegionalAgriculturalMapCountryCodes.NormalizeOrDefault(countryCode);
        if (string.Equals(CountryCode, normalized, StringComparison.Ordinal))
        {
            return true;
        }

        CountryCode = normalized;
        SelectedMarker = null;
        return await 새로고침Async(cancellationToken);
    }

    public async Task<bool> 관계선택Async(
        string? relationTypeCode,
        CancellationToken cancellationToken = default)
    {
        var normalized = RegionalAgriculturalMapRelationTypeCodes.All.Contains(
            relationTypeCode,
            StringComparer.Ordinal)
            ? relationTypeCode
            : null;
        if (string.Equals(RelationTypeCode, normalized, StringComparison.Ordinal))
        {
            return true;
        }

        RelationTypeCode = normalized;
        SelectedMarker = null;
        return await 새로고침Async(cancellationToken);
    }

    public Task<bool> 검색Async(CancellationToken cancellationToken = default)
        => 새로고침Async(cancellationToken);

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        var selectedKey = SelectedMarker?.MarkerKey;
        Response = await dataClient.지역MapMarker조회Async(
            new RegionalAgriculturalMapMarkerQuery
            {
                CountryCode = CountryCode,
                RelationTypeCode = RelationTypeCode,
                ProductName = string.IsNullOrWhiteSpace(ProductName) ? null : ProductName.Trim(),
                MaxItems = 200
            },
            cancellationToken);
        SelectedMarker = Markers.FirstOrDefault(marker => marker.MarkerKey == selectedKey)
                         ?? Markers.FirstOrDefault();
    }
}
