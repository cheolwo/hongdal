using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class 지역농수산MapPageViewModel(
    I농수산공공데이터Client dataClient,
    I홍익학당철학영상MapLayerClient hongikAcademyMapLayerClient) : PageViewModelBase
{
    private RegionalAgriculturalMapMarkerListResponse? _response;
    private MarineFishingAreaOceanTileResponse? _oceanTileResponse;
    private HongikAcademyContentMapLayerResponse? _hongikAcademyResponse;
    private RegionalAgriculturalMapMarkerDto? _selectedMarker;
    private MarineFishingAreaOceanTileDto? _selectedOceanTile;
    private string _contentLayerKey = RegionalAgriculturalMapContentLayerKeys.AgriculturalLivingInformation;
    private string _productName = string.Empty;
    private string _countryCode = RegionalAgriculturalMapCountryCodes.Korea;
    private string? _relationTypeCode;

    public string ContentLayerKey
    {
        get => _contentLayerKey;
        private set
        {
            if (SetProperty(ref _contentLayerKey, value))
            {
                OnPropertyChanged(nameof(IsAgriculturalLivingInformationLayer));
                OnPropertyChanged(nameof(IsMarineFishingAreaLayer));
                OnPropertyChanged(nameof(IsHongikAcademyLayer));
            }
        }
    }

    public bool IsAgriculturalLivingInformationLayer
        => ContentLayerKey == RegionalAgriculturalMapContentLayerKeys.AgriculturalLivingInformation;

    public bool IsHongikAcademyLayer
        => ContentLayerKey == RegionalAgriculturalMapContentLayerKeys.HongikAcademyPhilosophyVideo;

    public bool IsMarineFishingAreaLayer
        => ContentLayerKey == RegionalAgriculturalMapContentLayerKeys.MarineFishingAreas;

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

    public HongikAcademyContentMapLayerResponse? HongikAcademyResponse
    {
        get => _hongikAcademyResponse;
        private set
        {
            if (SetProperty(ref _hongikAcademyResponse, value))
            {
                OnPropertyChanged(nameof(Notices));
            }
        }
    }

    public MarineFishingAreaOceanTileResponse? OceanTileResponse
    {
        get => _oceanTileResponse;
        private set
        {
            if (SetProperty(ref _oceanTileResponse, value))
            {
                OnPropertyChanged(nameof(OceanTiles));
                OnPropertyChanged(nameof(Notices));
            }
        }
    }

    public IReadOnlyList<RegionalAgriculturalMapMarkerDto> Markers
        => Response?.Items ?? [];

    public IReadOnlyList<MarineFishingAreaOceanTileDto> OceanTiles
        => OceanTileResponse?.Items ?? [];

    public IReadOnlyList<string> Notices
        => IsHongikAcademyLayer
            ? HongikAcademyResponse?.Notices ?? []
            : IsMarineFishingAreaLayer
                ? OceanTileResponse?.Notices ?? []
                : Response?.Notices ?? [];

    public RegionalAgriculturalMapMarkerDto? SelectedMarker
    {
        get => _selectedMarker;
        private set => SetProperty(ref _selectedMarker, value);
    }

    public MarineFishingAreaOceanTileDto? SelectedOceanTile
    {
        get => _selectedOceanTile;
        private set => SetProperty(ref _selectedOceanTile, value);
    }

    public void 마커선택(RegionalAgriculturalMapMarkerDto marker)
        => SelectedMarker = marker;

    public void 바다Tile선택(MarineFishingAreaOceanTileDto tile)
        => SelectedOceanTile = tile;

    public void 초기국가설정(string? countryCode)
        => CountryCode = RegionalAgriculturalMapCountryCodes.NormalizeOrDefault(countryCode);

    public void 초기콘텐츠레이어설정(string? contentLayerKey)
        => ContentLayerKey = RegionalAgriculturalMapContentLayerKeys.NormalizeOrDefault(contentLayerKey);

    public async Task<bool> 콘텐츠레이어선택Async(
        string? contentLayerKey,
        CancellationToken cancellationToken = default)
    {
        var normalized = RegionalAgriculturalMapContentLayerKeys.NormalizeOrDefault(contentLayerKey);
        if (string.Equals(ContentLayerKey, normalized, StringComparison.Ordinal))
        {
            return true;
        }

        ContentLayerKey = normalized;
        ResetLayerState();
        return await 새로고침Async(cancellationToken);
    }

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
        if (IsHongikAcademyLayer)
        {
            await LoadHongikAcademyLayerAsync(cancellationToken);
            return;
        }

        if (IsMarineFishingAreaLayer)
        {
            await LoadMarineFishingAreaLayerAsync(cancellationToken);
            return;
        }

        await LoadAgriculturalLivingInformationLayerAsync(cancellationToken);
    }

    private async Task LoadHongikAcademyLayerAsync(CancellationToken cancellationToken)
    {
        Response = null;
        OceanTileResponse = null;
        HongikAcademyResponse = await hongikAcademyMapLayerClient.레이어조회Async(cancellationToken);
        SelectedMarker = null;
        SelectedOceanTile = null;
    }

    private async Task LoadMarineFishingAreaLayerAsync(CancellationToken cancellationToken)
    {
        var selectedOceanKey = SelectedOceanTile?.TileKey;
        Response = null;
        HongikAcademyResponse = null;
        OceanTileResponse = await dataClient.해양수산Map바다Tile조회Async(cancellationToken);
        SelectedMarker = null;
        SelectedOceanTile = OceanTiles.FirstOrDefault(tile => tile.TileKey == selectedOceanKey)
                            ?? OceanTiles.FirstOrDefault();
    }

    private async Task LoadAgriculturalLivingInformationLayerAsync(CancellationToken cancellationToken)
    {
        var selectedKey = SelectedMarker?.MarkerKey;
        OceanTileResponse = null;
        HongikAcademyResponse = null;
        SelectedOceanTile = null;
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

    private void ResetLayerState()
    {
        Response = null;
        OceanTileResponse = null;
        HongikAcademyResponse = null;
        SelectedMarker = null;
        SelectedOceanTile = null;
    }
}
