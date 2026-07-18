using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Content;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public sealed class CommunityInformationReviewPageViewModel : 조립ViewModelBase
{
    private readonly ICommunityInformationReviewClient _client;
    private IReadOnlyList<CommunityInformationSourceDto> _sources = [];
    private IReadOnlyList<CommunityInformationCandidateDto> _candidates = [];
    private IReadOnlyList<CommunityInformationSourceFailureDto> _failures = [];
    private CommunityInformationCandidateDto? _selectedCandidate;
    private CommunityInformationCandidateDto? _pendingDraftCandidate;
    private string _selectedSourceKey = string.Empty;
    private string _countryCode = string.Empty;
    private string _reviewState = string.Empty;
    private string _searchText = string.Empty;
    private bool _isLoading;
    private string? _errorMessage;
    private string? _statusMessage;
    private long? _publishedPostId;

    public CommunityInformationReviewPageViewModel(
        ICommunityInformationReviewClient client,
        CommunityPostComposerViewModel composer)
    {
        _client = client;
        Composer = 하위ViewModel등록(composer, 수명소유: true);
        Composer.Configure("platform", "운영자 정보 공유");
    }

    public CommunityPostComposerViewModel Composer { get; }

    public IReadOnlyList<CommunityInformationSourceDto> Sources
    {
        get => _sources;
        private set => SetProperty(ref _sources, value);
    }

    public IReadOnlyList<CommunityInformationCandidateDto> Candidates
    {
        get => _candidates;
        private set => SetProperty(ref _candidates, value);
    }

    public IReadOnlyList<CommunityInformationSourceFailureDto> Failures
    {
        get => _failures;
        private set => SetProperty(ref _failures, value);
    }

    public CommunityInformationCandidateDto? SelectedCandidate
    {
        get => _selectedCandidate;
        private set => SetProperty(ref _selectedCandidate, value);
    }

    public CommunityInformationCandidateDto? PendingDraftCandidate
    {
        get => _pendingDraftCandidate;
        private set
        {
            if (SetProperty(ref _pendingDraftCandidate, value))
            {
                OnPropertyChanged(nameof(HasDraftConflict));
            }
        }
    }

    public string SelectedSourceKey
    {
        get => _selectedSourceKey;
        set => SetProperty(ref _selectedSourceKey, value ?? string.Empty);
    }

    public string CountryCode
    {
        get => _countryCode;
        set => SetProperty(ref _countryCode, value ?? string.Empty);
    }

    public string ReviewState
    {
        get => _reviewState;
        set => SetProperty(ref _reviewState, value ?? string.Empty);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value ?? string.Empty);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public long? PublishedPostId
    {
        get => _publishedPostId;
        private set => SetProperty(ref _publishedPostId, value);
    }

    public bool HasDraftConflict
        => PendingDraftCandidate is not null;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Composer.LoadLocalDraftAsync(cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = null;
        try
        {
            if (Sources.Count == 0)
            {
                Sources = await _client.GetSourcesAsync(cancellationToken);
            }

            var response = await _client.GetCandidatesAsync(
                new CommunityInformationCollectionQuery
                {
                    SourceKey = NormalizeOptional(SelectedSourceKey),
                    CountryCode = NormalizeOptional(CountryCode),
                    ReviewState = NormalizeOptional(ReviewState),
                    SearchText = NormalizeOptional(SearchText),
                    Take = 100
                },
                cancellationToken);
            Sources = response.Sources.Count > 0 ? response.Sources : Sources;
            Candidates = response.Items;
            Failures = response.Failures;
            if (SelectedCandidate is not null)
            {
                SelectedCandidate = Candidates.FirstOrDefault(candidate =>
                    string.Equals(
                        candidate.CandidateKey,
                        SelectedCandidate.CandidateKey,
                        StringComparison.OrdinalIgnoreCase));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"자료 후보를 불러오지 못했습니다: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SelectCandidate(CommunityInformationCandidateDto candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        SelectedCandidate = candidate;
        StatusMessage = null;
    }

    public bool PrepareDraft(
        CommunityInformationCandidateDto candidate,
        string defaultNickname,
        bool replaceExisting = false)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        SelectCandidate(candidate);
        if (!replaceExisting
            && (Composer.Draft.HasContent || Composer.LocalDraftSavedAtUtc.HasValue))
        {
            PendingDraftCandidate = candidate;
            StatusMessage = "작성 중이거나 임시 저장된 초안이 있습니다.";
            return false;
        }

        ApplyCandidateToComposer(candidate, defaultNickname);
        return true;
    }

    public void ReplaceDraft(string defaultNickname)
    {
        if (PendingDraftCandidate is null)
        {
            return;
        }

        ApplyCandidateToComposer(PendingDraftCandidate, defaultNickname);
    }

    public void ContinueExistingDraft(string defaultNickname)
    {
        PendingDraftCandidate = null;
        if (string.IsNullOrWhiteSpace(Composer.Draft.Nickname))
        {
            Composer.Draft.Nickname = NormalizeNickname(defaultNickname);
        }

        Composer.Open();
        Composer.OpenSettings();
        StatusMessage = null;
    }

    public void ClearDraft()
    {
        PendingDraftCandidate = null;
        Composer.Reset();
        StatusMessage = "현재 화면의 초안을 비웠습니다.";
    }

    public void HandleComposerSaved(CommunityPostComposerSaveResult result)
    {
        if (!result.Succeeded || result.Post is null)
        {
            return;
        }

        PublishedPostId = result.Post.Id;
        PendingDraftCandidate = null;
        StatusMessage = $"커뮤니티 글 #{result.Post.Id:N0}을 등록했습니다.";
    }

    public static string BuildDraftBody(CommunityInformationCandidateDto candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(candidate.Summary))
        {
            lines.Add(candidate.Summary.Trim());
            lines.Add(string.Empty);
        }

        lines.Add($"자료 출처: {candidate.Provider}");
        if (candidate.ReferenceDate.HasValue)
        {
            lines.Add($"자료 기준일: {candidate.ReferenceDate:yyyy-MM-dd}");
        }
        else if (candidate.PublishedAtUtc.HasValue)
        {
            lines.Add($"원 게시일: {candidate.PublishedAtUtc:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(candidate.CurrencyCode)
            || !string.IsNullOrWhiteSpace(candidate.Unit))
        {
            lines.Add($"표시 기준: {string.Join(" · ", new[] { candidate.CurrencyCode, candidate.Unit }.Where(value => !string.IsNullOrWhiteSpace(value)))}");
        }

        lines.Add($"원문: {candidate.OriginalUrl}");
        lines.Add(string.Empty);
        lines.Add(candidate.SourceNotice.Trim());
        lines.Add($"확인할 점: {candidate.Limitations.Trim()}");
        lines.Add(string.Empty);
        lines.Add("이 자료를 보고 함께 확인하거나 나누고 싶은 생각을 적어 주세요.");
        return Limit(string.Join(Environment.NewLine, lines), 4000);
    }

    private void ApplyCandidateToComposer(
        CommunityInformationCandidateDto candidate,
        string defaultNickname)
    {
        Composer.Reset();
        Composer.Draft.Nickname = NormalizeNickname(defaultNickname);
        Composer.Draft.Category = ResolveCategory(candidate);
        Composer.Draft.WorkflowTag = ResolveWorkflowTag(candidate);
        Composer.Draft.RoleTag = "운영자 정보 공유";
        Composer.Draft.Title = Limit(BuildDraftTitle(candidate), 160);
        Composer.Draft.Body = BuildDraftBody(candidate);
        Composer.Draft.SharedLinkUrl = candidate.OriginalUrl;
        Composer.Draft.IsAuthorDisplayCountryPublic = false;
        Composer.Open();
        Composer.OpenSettings();
        PendingDraftCandidate = null;
        PublishedPostId = null;
        StatusMessage = "출처 정보를 포함한 글 초안을 만들었습니다.";
    }

    private static string ResolveCategory(CommunityInformationCandidateDto candidate)
    {
        if (candidate.TopicTags.Any(tag =>
                string.Equals(tag, "지식·성찰", StringComparison.OrdinalIgnoreCase)))
        {
            return CommunityBoardCatalog.Prajna.DisplayName;
        }

        if (candidate.TopicTags.Any(tag =>
                string.Equals(tag, "음식", StringComparison.OrdinalIgnoreCase)))
        {
            return CommunityBoardCatalog.Food.DisplayName;
        }

        return CommunityBoardCatalog.InformationPrices.DisplayName;
    }

    private static string ResolveWorkflowTag(CommunityInformationCandidateDto candidate)
        => candidate.SourceKey switch
        {
            CommunityInformationSourceKeys.KamisPriceObservations => "농수산물 가격 정보",
            CommunityInformationSourceKeys.YouTubeChannelVideos => "외부 공개 자료 공유",
            _ => "출처 기반 정보 공유"
        };

    private static string BuildDraftTitle(CommunityInformationCandidateDto candidate)
        => candidate.SourceType switch
        {
            CommunityInformationSourceTypes.Video => $"[영상 공유] {candidate.Title}",
            CommunityInformationSourceTypes.PublicData => $"[공공자료] {candidate.Title}",
            _ => $"[자료 공유] {candidate.Title}"
        };

    private static string NormalizeNickname(string value)
        => string.IsNullOrWhiteSpace(value) ? "홍달 운영자" : Limit(value.Trim(), 40);

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Limit(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
