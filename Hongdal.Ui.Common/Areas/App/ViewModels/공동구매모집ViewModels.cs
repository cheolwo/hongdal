using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 공동구매 목록과 선택된 공동구매의 상세·의견을 함께 불러옵니다.
/// </summary>
public sealed class 공동구매목록ViewModel(
    I공동구매업무Service service,
    공동구매화면상태ViewModel 화면상태) : 공동구매작업ViewModelBase
{
    public async Task<bool> 목록조회Async(
        string? communityScope = null,
        CancellationToken cancellationToken = default)
        => await 작업실행Async(
            async token =>
            {
                var response = await service.목록조회Async(communityScope, token);
                var campaigns = response.Items
                    .Where(campaign => !CommunityVoteWorkflowClassifier.IsGroupImport(campaign))
                    .OrderByDescending(campaign => campaign.CreatedAtUtc)
                    .ToArray();
                var previousId = 화면상태.선택된공동구매Id;

                화면상태.목록적용(campaigns);
                if (campaigns.Length == 0)
                {
                    화면상태.선택해제();
                    return;
                }

                var target = campaigns.FirstOrDefault(campaign => campaign.Id == previousId)
                    ?? campaigns[0];
                await LoadSelectionAsync(target.Id, token);
            },
            "공동구매 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"공동구매 목록을 불러오지 못했습니다. 로그인 상태를 확인해 주세요. {ex.Message}");

    public async Task<bool> 선택Async(
        Guid voteId,
        CancellationToken cancellationToken = default)
    {
        if (voteId == Guid.Empty)
        {
            return 유효성실패("조회할 공동구매를 선택해 주세요.");
        }

        return await 작업실행Async(
            token => LoadSelectionAsync(voteId, token),
            "공동구매 상세 정보를 불러왔습니다.",
            cancellationToken,
            ex => $"공동구매 상세 정보를 불러오지 못했습니다. {ex.Message}");
    }

    private async Task LoadSelectionAsync(Guid voteId, CancellationToken cancellationToken)
    {
        var campaign = await service.상세조회Async(voteId, cancellationToken)
            ?? throw new InvalidOperationException("공동구매 상세 조회 응답이 비어 있습니다.");
        var comments = campaign.SourcePostId is long postId
            ? await service.의견조회Async(postId, cancellationToken)
            : [];

        화면상태.선택적용(campaign, comments);
    }
}

/// <summary>
/// 제안 게시글과 수요 투표를 하나의 사용자 작업으로 묶습니다.
/// 두 번째 API가 실패하면 생성된 게시글 번호를 남겨 운영자가 복구할 수 있습니다.
/// </summary>
public sealed partial class 공동구매제안ViewModel(
    I공동구매업무Service service,
    공동구매화면상태ViewModel 화면상태) : 공동구매작업ViewModelBase
{
    [ObservableProperty]
    public partial string 앱키 { get; set; } = "shipper";

    [ObservableProperty]
    public partial string 제목 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 설명 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 제안자표시명 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 게시글비밀번호 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 상품명 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 상품키 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 커뮤니티범위 { get; set; } = "platform";

    [ObservableProperty]
    public partial string 참여정책코드 { get; set; } = CommunityVoteParticipationPolicyCodes.Hybrid;

    [ObservableProperty]
    public partial string 수량단위 { get; set; } = "개";

    [ObservableProperty]
    public partial int 최소참여자수 { get; set; } = 3;

    [ObservableProperty]
    public partial int 최소총수량 { get; set; } = 10;

    [ObservableProperty]
    public partial int? 반경미터 { get; set; } = 3000;

    [ObservableProperty]
    public partial string 수령소명 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 수령소주소 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 온도코드 { get; set; } = "상온";

    [ObservableProperty]
    public partial string 물류방식 { get; set; } = "LCL";

    [ObservableProperty]
    public partial int 모집기간일수 { get; set; } = 7;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제안글만생성됨))]
    public partial PlatformCommunityPostResponse? 생성된게시글 { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제안글만생성됨))]
    public partial CommunityVoteResponse? 생성된공동구매 { get; private set; }

    public bool 제안글만생성됨 => 생성된게시글 is not null && 생성된공동구매 is null;
    public long? 복구할게시글Id => 제안글만생성됨 ? 생성된게시글?.Id : null;

    public async Task<bool> 등록Async(CancellationToken cancellationToken = default)
    {
        if (!입력검증())
        {
            return false;
        }

        생성된게시글 = null;
        생성된공동구매 = null;
        OnPropertyChanged(nameof(복구할게시글Id));

        return await 작업실행Async(
            async token =>
            {
                생성된게시글 = await service.제안글생성Async(제안글요청생성(), token)
                    ?? throw new InvalidOperationException("제안 글 생성 응답이 비어 있습니다.");
                OnPropertyChanged(nameof(복구할게시글Id));

                생성된공동구매 = await service.공동구매생성Async(
                    공동구매요청생성(생성된게시글.Id),
                    token)
                    ?? throw new InvalidOperationException("공동구매 수요 투표 생성 응답이 비어 있습니다.");

                화면상태.새공동구매적용(생성된공동구매);
                OnPropertyChanged(nameof(복구할게시글Id));
            },
            "제안 글과 공동구매 수요 투표를 만들었습니다.",
            cancellationToken,
            ex => 생성된게시글 is null
                ? $"공동구매 제안을 만들지 못했습니다. {ex.Message}"
                : $"제안 글은 저장됐지만 수요 투표 연결에 실패했습니다. 게시글 번호 {생성된게시글.Id}를 확인해 주세요. {ex.Message}");
    }

    private bool 입력검증()
    {
        if (string.IsNullOrWhiteSpace(제목)
            || string.IsNullOrWhiteSpace(상품명)
            || string.IsNullOrWhiteSpace(제안자표시명)
            || string.IsNullOrWhiteSpace(게시글비밀번호))
        {
            return 유효성실패("제안 제목, 상품명, 제안자 표시명과 게시글 비밀번호를 입력해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(수량단위)
            || 최소참여자수 <= 0
            || 최소총수량 <= 0
            || 모집기간일수 <= 0)
        {
            return 유효성실패("수량 단위, 최소 참여자·수량과 모집 기간을 올바르게 입력해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(수령소명) != string.IsNullOrWhiteSpace(수령소주소))
        {
            return 유효성실패("공동 수령소를 지정하려면 수령소 이름과 주소를 모두 입력해 주세요.");
        }

        return true;
    }

    private PlatformCommunityPostCreateRequest 제안글요청생성()
        => new()
        {
            AppKey = string.IsNullOrWhiteSpace(앱키) ? "platform" : 앱키.Trim(),
            Category = "공동구매",
            WorkflowTag = "공동 구매",
            RoleTag = "구매 참여자",
            Title = 제목.Trim(),
            Body = 제안글본문생성(),
            Nickname = 제안자표시명.Trim(),
            Password = 게시글비밀번호
        };

    private CommunityVoteCreateRequest 공동구매요청생성(long postId)
    {
        var scope = string.IsNullOrWhiteSpace(커뮤니티범위) ? "platform" : 커뮤니티범위.Trim();
        var pickupPoints = string.IsNullOrWhiteSpace(수령소명)
            ? Array.Empty<CommunityVotePickupPointRequest>()
            :
            [
                new CommunityVotePickupPointRequest
                {
                    PickupPointId = $"pickup-{Guid.NewGuid():N}"[..20],
                    Name = 수령소명.Trim(),
                    AddressSummary = 수령소주소.Trim(),
                    StorageTypeCode = CommunityVotePickupStorageTypeCodes.Ambient,
                    MinimumParticipantCount = 최소참여자수,
                    MinimumTotalQuantity = 최소총수량
                }
            ];

        return new CommunityVoteCreateRequest
        {
            CommunityScope = scope,
            Title = 제목.Trim(),
            Description = 설명.Trim(),
            SourcePostId = postId,
            StructuredOptions =
            [
                new CommunityVoteOptionCreateRequest
                {
                    Text = 상품명.Trim(),
                    ProductKey = string.IsNullOrWhiteSpace(상품키)
                        ? $"community-product:{postId}"
                        : 상품키.Trim(),
                    QuantityUnit = 수량단위.Trim(),
                    TemperatureCode = 온도코드.Trim(),
                    LogisticsMode = 물류방식.Trim()
                }
            ],
            ResolutionDocumentEnabled = true,
            SignatureRequired = true,
            ClosesAtUtc = DateTime.UtcNow.AddDays(모집기간일수),
            CreatedByDisplayName = 제안자표시명.Trim(),
            GroupPurchase = new CommunityGroupPurchaseVoteSettingsRequest
            {
                ParticipationPolicyCode = 참여정책코드,
                QuantityUnit = 수량단위.Trim(),
                ServiceAreaKey = scope,
                ServiceAreaLabel = scope,
                RadiusMeters = 반경미터,
                MinimumParticipantCount = 최소참여자수,
                MinimumTotalQuantity = 최소총수량,
                PickupPoints = pickupPoints
            }
        };
    }

    private string 제안글본문생성()
        => string.Join(
            Environment.NewLine,
            설명.Trim(),
            string.Empty,
            $"상품: {상품명.Trim()}",
            $"최소 참여: {최소참여자수}명",
            $"최소 수량: {최소총수량}{수량단위.Trim()}",
            $"참여 범위: {(string.IsNullOrWhiteSpace(커뮤니티범위) ? "platform" : 커뮤니티범위.Trim())}",
            string.IsNullOrWhiteSpace(수령소명)
                ? "공동 수령소: 지정하지 않음"
                : $"공동 수령소: {수령소명.Trim()} · {수령소주소.Trim()}");
}

public sealed partial class 공동구매수요참여ViewModel : 공동구매작업ViewModelBase, IDisposable
{
    private readonly I공동구매업무Service _service;
    private readonly 공동구매화면상태ViewModel _화면상태;
    private Guid? _입력대상Id;

    public 공동구매수요참여ViewModel(
        I공동구매업무Service service,
        공동구매화면상태ViewModel 화면상태)
    {
        _service = service;
        _화면상태 = 화면상태;
        _화면상태.PropertyChanged += 화면상태변경;
        선택기본값동기화();
    }

    [ObservableProperty]
    public partial string 참여자표시명 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 참여자키 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 상품선택지Id { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int 요청수량 { get; set; } = 1;

    [ObservableProperty]
    public partial string 참여방식코드 { get; set; } = CommunityVoteParticipationMethodCodes.CommunityMember;

    [ObservableProperty]
    public partial string? 수령소Id { get; set; }

    [ObservableProperty]
    public partial bool 인근수령소대체허용 { get; set; } = true;

    public async Task<bool> 참여Async(CancellationToken cancellationToken = default)
    {
        var campaign = _화면상태.선택된공동구매;
        if (campaign is null || string.IsNullOrWhiteSpace(상품선택지Id))
        {
            return 유효성실패("참여할 공동구매와 상품을 선택해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(참여자표시명) || 요청수량 <= 0)
        {
            return 유효성실패("참여자 표시명과 요청 수량을 올바르게 입력해 주세요.");
        }

        if (참여방식코드 == CommunityVoteParticipationMethodCodes.PickupPoint
            && string.IsNullOrWhiteSpace(수령소Id))
        {
            return 유효성실패("공동 수령소를 선택해 주세요.");
        }

        return await 작업실행Async(
            async token =>
            {
                var updated = await _service.수요참여Async(
                    campaign.Id,
                    new CommunityVoteCastRequest
                    {
                        VoterDisplayName = 참여자표시명.Trim(),
                        VoterKey = 참여자키.Trim(),
                        OptionIds = [상품선택지Id],
                        RequestedQuantity = 요청수량,
                        ParticipationMethodCode = 참여방식코드,
                        PickupPointId = 참여방식코드 == CommunityVoteParticipationMethodCodes.PickupPoint
                            ? 수령소Id
                            : null,
                        AllowNearbyPickupPointFallback = 인근수령소대체허용
                    },
                    token)
                    ?? throw new InvalidOperationException("공동구매 수요 참여 응답이 비어 있습니다.");

                _화면상태.공동구매갱신(updated);
            },
            "공동구매 수요 참여가 반영됐습니다.",
            cancellationToken);
    }

    public void Dispose()
    {
        _화면상태.PropertyChanged -= 화면상태변경;
        GC.SuppressFinalize(this);
    }

    private void 화면상태변경(object? sender, PropertyChangedEventArgs e)
        => 선택기본값동기화();

    private void 선택기본값동기화()
    {
        var campaign = _화면상태.선택된공동구매;
        if (campaign?.Id == _입력대상Id)
        {
            return;
        }

        _입력대상Id = campaign?.Id;
        상품선택지Id = campaign?.Options.FirstOrDefault()?.OptionId ?? string.Empty;
        수령소Id = campaign?.GroupPurchase?.PickupPoints.FirstOrDefault()?.PickupPointId;
    }
}

public sealed partial class 공동구매이의검토ViewModel : 공동구매작업ViewModelBase, IDisposable
{
    private readonly I공동구매업무Service _service;
    private readonly 공동구매화면상태ViewModel _화면상태;

    public 공동구매이의검토ViewModel(
        I공동구매업무Service service,
        공동구매화면상태ViewModel 화면상태)
    {
        _service = service;
        _화면상태 = 화면상태;
        _화면상태.PropertyChanged += 화면상태변경;
    }

    [ObservableProperty]
    public partial string 작성자표시명 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 게시글비밀번호 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 이의내용 { get; set; } = string.Empty;

    public IReadOnlyList<PlatformCommunityPostCommentResponse> 전체이의
        => _화면상태.의견목록.Where(이의여부).ToArray();

    public IReadOnlyList<PlatformCommunityPostCommentResponse> 현재단계이의
        => _화면상태.의견목록
            .Where(comment => comment.Body.StartsWith(
                $"[이의제기:{_화면상태.현재단계코드}]",
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

    public int 전체이의수 => 전체이의.Count;
    public int 현재단계이의수 => 현재단계이의.Count;

    public async Task<bool> 등록Async(CancellationToken cancellationToken = default)
    {
        var campaign = _화면상태.선택된공동구매;
        if (campaign?.SourcePostId is not long postId
            || string.IsNullOrWhiteSpace(작성자표시명)
            || string.IsNullOrWhiteSpace(게시글비밀번호)
            || string.IsNullOrWhiteSpace(이의내용))
        {
            return 유효성실패("표시명, 게시글 비밀번호와 이의 내용을 입력해 주세요.");
        }

        return await 작업실행Async(
            async token =>
            {
                var created = await _service.이의등록Async(
                    postId,
                    new PlatformCommunityPostCommentCreateRequest
                    {
                        Nickname = 작성자표시명.Trim(),
                        Password = 게시글비밀번호,
                        Body = $"[이의제기:{_화면상태.현재단계코드}] {이의내용.Trim()}"
                    },
                    token)
                    ?? throw new InvalidOperationException("이의제기 등록 응답이 비어 있습니다.");

                _화면상태.의견추가(created);
                이의내용 = string.Empty;
            },
            $"{공동구매절차카탈로그.찾기(_화면상태.현재단계코드)?.제목 ?? "현재"} 단계에 이의제기를 등록했습니다.",
            cancellationToken);
    }

    public static string 접두어제거(string body)
    {
        var end = body.IndexOf(']');
        return end >= 0 && end + 1 < body.Length ? body[(end + 1)..].Trim() : body;
    }

    public void Dispose()
    {
        _화면상태.PropertyChanged -= 화면상태변경;
        GC.SuppressFinalize(this);
    }

    private static bool 이의여부(PlatformCommunityPostCommentResponse comment)
        => comment.Body.StartsWith("[이의제기:", StringComparison.OrdinalIgnoreCase);

    private void 화면상태변경(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(전체이의));
        OnPropertyChanged(nameof(현재단계이의));
        OnPropertyChanged(nameof(전체이의수));
        OnPropertyChanged(nameof(현재단계이의수));
    }
}

public sealed class 공동구매모집기능ViewModel : 조립ViewModelBase
{
    public 공동구매모집기능ViewModel(
        공동구매목록ViewModel 목록,
        공동구매제안ViewModel 제안,
        공동구매수요참여ViewModel 수요참여,
        공동구매이의검토ViewModel 이의검토)
    {
        this.목록 = 하위ViewModel등록(목록);
        this.제안 = 하위ViewModel등록(제안);
        this.수요참여 = 하위ViewModel등록(수요참여);
        this.이의검토 = 하위ViewModel등록(이의검토);
    }

    public 공동구매목록ViewModel 목록 { get; }
    public 공동구매제안ViewModel 제안 { get; }
    public 공동구매수요참여ViewModel 수요참여 { get; }
    public 공동구매이의검토ViewModel 이의검토 { get; }

    public bool 처리중 => 목록.처리중 || 제안.처리중 || 수요참여.처리중 || 이의검토.처리중;
}
