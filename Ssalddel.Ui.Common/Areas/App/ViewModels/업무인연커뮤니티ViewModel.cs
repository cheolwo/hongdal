using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed partial class 업무인연커뮤니티ViewModel(
    I업무인연커뮤니티Service service) : 업무작업ViewModelBase
{
    private readonly HashSet<Guid> _연결요청완료스냅샷 = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(업무인연목록))]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial WorkRelationshipSnapshotListResponse 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<WorkRelationshipSnapshotResponse> 업무인연목록 => 응답.Items;

    public bool 결과없음 => 초기화됨 && 업무인연목록.Count == 0;

    public bool 연결요청가능(WorkRelationshipSnapshotResponse item)
        => string.Equals(
               item.PrivacyLevel,
               WorkRelationshipPrivacyCodes.ConnectionRequestEligible,
               StringComparison.Ordinal)
           && !string.IsNullOrWhiteSpace(item.CounterpartyAnonymousLabel)
           && !_연결요청완료스냅샷.Contains(item.Id);

    public bool 연결요청완료(Guid snapshotId)
        => _연결요청완료스냅샷.Contains(snapshotId);

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                응답 = await service.내업무인연조회Async(50, token);
                초기화됨 = true;
            },
            "업무에서 만난 인연 기록을 확인했습니다.",
            cancellationToken,
            조회오류문구);

    public async Task<bool> 연결요청Async(
        WorkRelationshipSnapshotResponse item,
        string purpose,
        string message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!연결요청가능(item))
        {
            return 유효성실패("연결 요청이 허용된 업무 인연 기록을 선택해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(purpose))
        {
            return 유효성실패("인연을 이어가려는 목적을 입력해 주세요.");
        }

        var succeeded = await 작업실행Async(
            async token =>
            {
                await service.연결요청Async(
                    item.Id,
                    new WorkRelationshipConnectionRequestCreateRequest
                    {
                        Purpose = purpose.Trim(),
                        Message = message.Trim()
                    },
                    token);
                _연결요청완료스냅샷.Add(item.Id);
                OnPropertyChanged(nameof(업무인연목록));
            },
            "인연 연결 요청을 보냈습니다. 상대가 수락하기 전에는 연락처가 공개되지 않습니다.",
            cancellationToken,
            연결오류문구);

        return succeeded;
    }

    private static string 조회오류문구(Exception exception)
        => exception switch
        {
            SsalddelApiException { StatusCode: 401 } =>
                "로그인하면 02~05 업무 앱에서 이어진 내 인연 기록을 확인할 수 있습니다.",
            SsalddelApiException { StatusCode: 403 } =>
                "현재 계정으로는 업무 인연 기록을 조회할 수 없습니다.",
            _ => $"업무 인연 DB에 연결하지 못했습니다. {exception.Message}"
        };

    private static string 연결오류문구(Exception exception)
        => exception switch
        {
            SsalddelApiException { StatusCode: 401 } =>
                "인연 연결 요청을 보내려면 다시 로그인해 주세요.",
            SsalddelApiException { StatusCode: 403 } =>
                "현재 계정으로는 이 인연 연결 요청을 보낼 수 없습니다.",
            _ => $"인연 연결 요청을 보내지 못했습니다. {exception.Message}"
        };
}
