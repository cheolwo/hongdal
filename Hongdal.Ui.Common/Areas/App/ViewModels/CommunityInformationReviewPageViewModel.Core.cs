using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Content;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public sealed partial class CommunityInformationReviewPageViewModel : 조립ViewModelBase
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
    private CommunityAuthoringTool _activeTool = CommunityAuthoringTool.CollectedSources;

    public CommunityInformationReviewPageViewModel(
        ICommunityInformationReviewClient client,
        CommunityPostComposerViewModel composer,
        CommunityScheduledPostListViewModel scheduledPosts,
        CommunityAuthoringSocialResearchViewModel socialResearch,
        CommunityAuthoringDiagramViewModel diagram,
        CommunityAuthoringMutualBenefitViewModel mutualBenefit,
        CommunityAuthoringEvidenceChartViewModel evidenceChart,
        CommunityOperatorWritingPersonaViewModel writingPersona,
        CommunityVowVersionViewModel vowVersion)
    {
        _client = client;
        Composer = 하위ViewModel등록(composer, 수명소유: true);
        ScheduledPosts = 하위ViewModel등록(scheduledPosts, 수명소유: true);
        SocialResearch = 하위ViewModel등록(socialResearch, 수명소유: true);
        Diagram = 하위ViewModel등록(diagram, 수명소유: true);
        MutualBenefit = 하위ViewModel등록(mutualBenefit, 수명소유: true);
        EvidenceChart = 하위ViewModel등록(evidenceChart, 수명소유: true);
        WritingPersona = 하위ViewModel등록(writingPersona, 수명소유: true);
        VowVersion = 하위ViewModel등록(vowVersion, 수명소유: true);
        Composer.Configure("platform", "운영자 정보 공유", allowScheduledPublication: true);
    }

    public CommunityPostComposerViewModel Composer { get; }
    public CommunityScheduledPostListViewModel ScheduledPosts { get; }
    public CommunityAuthoringSocialResearchViewModel SocialResearch { get; }
    public CommunityAuthoringDiagramViewModel Diagram { get; }
    public CommunityAuthoringMutualBenefitViewModel MutualBenefit { get; }
    public CommunityAuthoringEvidenceChartViewModel EvidenceChart { get; }
    public CommunityOperatorWritingPersonaViewModel WritingPersona { get; }
    public CommunityVowVersionViewModel VowVersion { get; }

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

    public CommunityAuthoringTool ActiveTool
    {
        get => _activeTool;
        set => SetProperty(ref _activeTool, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Composer.LoadLocalDraftAsync(cancellationToken);
        VowVersion.RestoreFromWorkflowTag(Composer.Draft.WorkflowTag);
        await Task.WhenAll(
            RefreshAsync(cancellationToken),
            ScheduledPosts.RefreshAsync(cancellationToken),
            SocialResearch.InitializeAsync(cancellationToken));
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
}
