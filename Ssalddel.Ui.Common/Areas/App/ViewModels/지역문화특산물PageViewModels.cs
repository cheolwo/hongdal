using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "Figma 01A.07 지역 문화·특산물 목록의 국가 선택과 공개 탐색 상태를 관리",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.IndependentExecution,
    Boundary = "지역 선택은 공개 정보 탐색만 좁히며 주문·참여·수입 상태를 만들지 않습니다.")]
public sealed class 지역문화특산물목록PageViewModel : PageViewModelBase
{
    private string? selectedCountryCode;

    public string? SelectedCountryCode
    {
        get => selectedCountryCode;
        private set
        {
            if (SetProperty(ref selectedCountryCode, value))
            {
                OnPropertyChanged(nameof(VisibleRegions));
            }
        }
    }

    public IReadOnlyList<RegionalCultureSpecialty> VisibleRegions
        => RegionalCultureSpecialtyCatalog.ForCountry(SelectedCountryCode);

    public void SelectCountry(string? countryCode)
        => SelectedCountryCode = string.IsNullOrWhiteSpace(countryCode)
            ? null
            : countryCode.Trim();

    public bool IsCountrySelected(string? countryCode)
        => string.Equals(
            SelectedCountryCode,
            string.IsNullOrWhiteSpace(countryCode) ? null : countryCode.Trim(),
            StringComparison.OrdinalIgnoreCase);

    protected override Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "Figma 01A.07 지역 문화·특산물 상세 route와 현재 지역의 공개 탐색 문맥을 관리",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.IndependentExecution,
    Boundary = "문화 이미지와 지역 문맥은 원산지 증명이나 구매·수입 실행 근거로 사용하지 않습니다.")]
public sealed class 지역문화특산물상세PageViewModel : PageViewModelBase
{
    private string? regionKey;
    private RegionalCultureSpecialty? region;

    public string? RegionKey
    {
        get => regionKey;
        private set => SetProperty(ref regionKey, value);
    }

    public RegionalCultureSpecialty? Region
    {
        get => region;
        private set
        {
            if (SetProperty(ref region, value))
            {
                OnPropertyChanged(nameof(CultureConversationHref));
            }
        }
    }

    public string CultureConversationHref
        => Region is null
            ? CommunityPageRoutes.Boards
            : CommunityPageRoutes.BoardsFor(
                CommunityBoardCatalog.Food.DisplayName,
                CommunityBoardCatalog.Food.Key,
                workflowTag: CultureTransportContentCatalog.FoodCultureWorkflowTag,
                search: Region.RegionName,
                regionKey: Region.Key);

    public void Configure(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(RegionKey, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        RegionKey = normalized;
        Region = RegionalCultureSpecialtyCatalog.Find(normalized);
    }

    protected override Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}
