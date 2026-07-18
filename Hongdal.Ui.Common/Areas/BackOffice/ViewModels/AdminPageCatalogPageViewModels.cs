using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Ui.Common.Areas.BackOffice.ViewModels;

public sealed class AdminPageCatalogListViewModel : 조립ViewModelBase
{
    private IReadOnlyList<AdminManagedPageSnapshot> _items = [];
    private AdminPageCatalogSummary _summary = new(0, 0, 0, 0, 0);

    public IReadOnlyList<AdminManagedPageSnapshot> Items
    {
        get => _items;
        private set => SetProperty(ref _items, value);
    }

    public AdminPageCatalogSummary Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public void Replace(
        IReadOnlyList<AdminManagedPageSnapshot> items,
        AdminPageCatalogSummary summary)
    {
        Items = items;
        Summary = summary;
    }
}

public sealed class AdminPageCatalogDetailViewModel : 조립ViewModelBase
{
    private AdminManagedPageSnapshot? _selectedPage;
    private AdminPageReviewState _reviewState;
    private AdminPageNavigationState _navigationState;
    private bool _desktopVerified;
    private bool _mobileVerified;
    private string _adminNote = string.Empty;

    public AdminManagedPageSnapshot? SelectedPage
    {
        get => _selectedPage;
        private set
        {
            if (SetProperty(ref _selectedPage, value))
            {
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    public bool HasSelection => SelectedPage is not null;

    public AdminPageReviewState ReviewState
    {
        get => _reviewState;
        set => SetProperty(ref _reviewState, value);
    }

    public AdminPageNavigationState NavigationState
    {
        get => _navigationState;
        set => SetProperty(ref _navigationState, value);
    }

    public bool DesktopVerified
    {
        get => _desktopVerified;
        set => SetProperty(ref _desktopVerified, value);
    }

    public bool MobileVerified
    {
        get => _mobileVerified;
        set => SetProperty(ref _mobileVerified, value);
    }

    public string AdminNote
    {
        get => _adminNote;
        set => SetProperty(ref _adminNote, value);
    }

    public void Select(AdminManagedPageSnapshot page)
    {
        SelectedPage = page;
        ReviewState = page.ReviewState;
        NavigationState = page.NavigationState;
        DesktopVerified = page.DesktopVerified;
        MobileVerified = page.MobileVerified;
        AdminNote = page.AdminNote;
    }

    public void Clear()
        => SelectedPage = null;
}

public sealed class AdminPageCatalogPageViewModel : PageViewModelBase
{
    private readonly IAdminPageCatalogClient _client;
    private IReadOnlyList<AdminManagedPageSnapshot> _allPages = [];
    private IReadOnlyList<AdminPageCatalogOption> _appOptions = [];
    private IReadOnlyList<AdminPageCatalogOption> _areaOptions = [];
    private string _appFilter = string.Empty;
    private string _areaFilter = string.Empty;
    private string _reviewFilter = string.Empty;
    private string _executionFilter = string.Empty;
    private string _searchText = string.Empty;
    private bool _needsAttentionOnly;
    private bool _commandInProgress;
    private string? _message;
    private AdminPageCatalogMessageKind _messageKind = AdminPageCatalogMessageKind.Info;

    public AdminPageCatalogPageViewModel(
        IAdminPageCatalogClient client,
        AdminPageCatalogListViewModel list,
        AdminPageCatalogDetailViewModel detail)
    {
        _client = client;
        List = 하위ViewModel등록(list);
        Detail = 하위ViewModel등록(detail);
    }

    public AdminPageCatalogListViewModel List { get; }
    public AdminPageCatalogDetailViewModel Detail { get; }

    public IReadOnlyList<AdminPageCatalogOption> AppOptions
    {
        get => _appOptions;
        private set => SetProperty(ref _appOptions, value);
    }

    public IReadOnlyList<AdminPageCatalogOption> AreaOptions
    {
        get => _areaOptions;
        private set => SetProperty(ref _areaOptions, value);
    }

    public string AppFilter
    {
        get => _appFilter;
        set
        {
            if (SetProperty(ref _appFilter, value ?? string.Empty))
            {
                ApplyFilter();
            }
        }
    }

    public string AreaFilter
    {
        get => _areaFilter;
        set
        {
            if (SetProperty(ref _areaFilter, value ?? string.Empty))
            {
                ApplyFilter();
            }
        }
    }

    public string ReviewFilter
    {
        get => _reviewFilter;
        set
        {
            if (SetProperty(ref _reviewFilter, value ?? string.Empty))
            {
                ApplyFilter();
            }
        }
    }

    public string ExecutionFilter
    {
        get => _executionFilter;
        set
        {
            if (SetProperty(ref _executionFilter, value ?? string.Empty))
            {
                ApplyFilter();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
            {
                ApplyFilter();
            }
        }
    }

    public bool NeedsAttentionOnly
    {
        get => _needsAttentionOnly;
        set
        {
            if (SetProperty(ref _needsAttentionOnly, value))
            {
                ApplyFilter();
            }
        }
    }

    public bool CommandInProgress
    {
        get => _commandInProgress;
        private set => SetProperty(ref _commandInProgress, value);
    }

    public string? Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public AdminPageCatalogMessageKind MessageKind
    {
        get => _messageKind;
        private set => SetProperty(ref _messageKind, value);
    }

    public bool SelectPage(string pageKey)
    {
        var selected = _allPages.FirstOrDefault(page => page.PageKey == pageKey);
        if (selected is null)
        {
            return false;
        }

        Detail.Select(selected);
        Message = null;
        return true;
    }

    public void ResetFilters()
    {
        _appFilter = string.Empty;
        _areaFilter = string.Empty;
        _reviewFilter = string.Empty;
        _executionFilter = string.Empty;
        _searchText = string.Empty;
        _needsAttentionOnly = false;
        OnPropertyChanged(nameof(AppFilter));
        OnPropertyChanged(nameof(AreaFilter));
        OnPropertyChanged(nameof(ReviewFilter));
        OnPropertyChanged(nameof(ExecutionFilter));
        OnPropertyChanged(nameof(SearchText));
        OnPropertyChanged(nameof(NeedsAttentionOnly));
        ApplyFilter();
    }

    public async Task<bool> SaveSelectedAsync(
        bool canManage,
        string reviewer,
        CancellationToken cancellationToken = default)
    {
        if (!canManage)
        {
            SetMessage("서버관리자 로그인이 필요합니다.", AdminPageCatalogMessageKind.Warning);
            return false;
        }

        if (Detail.SelectedPage is not { } selected)
        {
            SetMessage("관리할 페이지를 먼저 선택해 주세요.", AdminPageCatalogMessageKind.Warning);
            return false;
        }

        if (Detail.AdminNote.Length > 500)
        {
            SetMessage("관리 메모는 500자 이내로 입력해 주세요.", AdminPageCatalogMessageKind.Warning);
            return false;
        }

        CommandInProgress = true;
        try
        {
            var updated = await _client.UpdatePageAsync(
                new AdminPageCatalogUpdateRequest(
                    selected.PageKey,
                    Detail.ReviewState,
                    Detail.NavigationState,
                    Detail.DesktopVerified,
                    Detail.MobileVerified,
                    Detail.AdminNote.Trim(),
                    string.IsNullOrWhiteSpace(reviewer) ? "서버관리자" : reviewer.Trim()),
                cancellationToken);

            _allPages = _allPages
                .Select(page => page.PageKey == updated.PageKey ? updated : page)
                .ToArray();
            Detail.Select(updated);
            ApplyFilter(updated.PageKey);
            SetMessage(
                $"{updated.Title} 페이지의 관리 메타데이터를 저장했습니다.",
                AdminPageCatalogMessageKind.Success);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            SetMessage(ex.Message, AdminPageCatalogMessageKind.Error);
            return false;
        }
        finally
        {
            CommandInProgress = false;
        }
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        var selectedKey = Detail.SelectedPage?.PageKey;
        _allPages = (await _client.GetPagesAsync(cancellationToken))
            .OrderBy(page => page.AppName, StringComparer.Ordinal)
            .ThenBy(page => page.AreaName, StringComparer.Ordinal)
            .ThenBy(page => page.Title, StringComparer.Ordinal)
            .ToArray();

        AppOptions = _allPages
            .GroupBy(page => page.AppKey, StringComparer.Ordinal)
            .Select(group => new AdminPageCatalogOption(group.Key, group.First().AppName))
            .OrderBy(option => option.Label, StringComparer.Ordinal)
            .ToArray();
        AreaOptions = _allPages
            .GroupBy(page => page.AreaKey, StringComparer.Ordinal)
            .Select(group => new AdminPageCatalogOption(group.Key, group.First().AreaName))
            .OrderBy(option => option.Label, StringComparer.Ordinal)
            .ToArray();

        Message = null;
        ApplyFilter(selectedKey);
    }

    private void ApplyFilter(string? preferredPageKey = null)
    {
        if (_allPages.Count == 0)
        {
            List.Replace([], CreateSummary([]));
            Detail.Clear();
            return;
        }

        IEnumerable<AdminManagedPageSnapshot> query = _allPages;
        if (!string.IsNullOrWhiteSpace(AppFilter))
        {
            query = query.Where(page => page.AppKey == AppFilter);
        }

        if (!string.IsNullOrWhiteSpace(AreaFilter))
        {
            query = query.Where(page => page.AreaKey == AreaFilter);
        }

        if (Enum.TryParse<AdminPageReviewState>(ReviewFilter, out var reviewState))
        {
            query = query.Where(page => page.ReviewState == reviewState);
        }

        if (Enum.TryParse<AdminPageExecutionMode>(ExecutionFilter, out var executionMode))
        {
            query = query.Where(page => page.ExecutionMode == executionMode);
        }

        if (NeedsAttentionOnly)
        {
            query = query.Where(page => page.NeedsAttention);
        }

        var search = SearchText.Trim();
        if (search.Length > 0)
        {
            query = query.Where(page =>
                page.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                || page.RouteTemplate.Contains(search, StringComparison.OrdinalIgnoreCase)
                || page.AppName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || page.AreaName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || page.OwnerRole.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.ToArray();
        List.Replace(filtered, CreateSummary(_allPages));

        var selectedKey = preferredPageKey ?? Detail.SelectedPage?.PageKey;
        var selected = filtered.FirstOrDefault(page => page.PageKey == selectedKey)
                       ?? filtered.FirstOrDefault();
        if (selected is null)
        {
            Detail.Clear();
        }
        else if (Detail.SelectedPage?.PageKey != selected.PageKey || preferredPageKey is not null)
        {
            Detail.Select(selected);
        }
    }

    private static AdminPageCatalogSummary CreateSummary(
        IReadOnlyList<AdminManagedPageSnapshot> pages)
        => new(
            pages.Count,
            pages.Count(page => page.NavigationState == AdminPageNavigationState.Primary),
            pages.Count(page => page.ExecutionMode == AdminPageExecutionMode.Simulation),
            pages.Count(page => page.NeedsAttention),
            pages.Count(page => page.ReviewState == AdminPageReviewState.Verified
                                && page.DesktopVerified
                                && page.MobileVerified));

    private void SetMessage(string message, AdminPageCatalogMessageKind kind)
    {
        MessageKind = kind;
        Message = message;
    }
}
