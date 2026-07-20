using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>현재 사용자의 역할 지원 선택지와 영속 지원 이력 조회만 담당합니다.</summary>
public sealed partial class 인사역할지원목록ViewModel(
    I인사역할지원Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(지원목록))]
    [NotifyPropertyChangedFor(nameof(역할선택지))]
    [NotifyPropertyChangedFor(nameof(활성지원수))]
    [NotifyPropertyChangedFor(nameof(지원없음))]
    public partial HrRoleApplicationPageResponse 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(지원없음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<HrRoleApplicationResponse> 지원목록 => 응답.Applications;
    public IReadOnlyList<HrRoleApplicationOptionResponse> 역할선택지 => 응답.Options;
    public int 활성지원수 => 지원목록.Count(item => item.CanWithdraw);
    public bool 지원없음 => 초기화됨 && 지원목록.Count == 0;

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                응답 = await service.내지원목록Async(token);
                초기화됨 = true;
            },
            "내 역할 지원 이력을 불러왔습니다.",
            cancellationToken,
            인사역할지원오류.메시지);

    public void 결과초기화()
    {
        응답 = new HrRoleApplicationPageResponse();
        초기화됨 = false;
        작업상태초기화();
    }
}

/// <summary>역할 선택과 세 가지 명시적 확인을 검증해 지원 Command 한 번만 전송합니다.</summary>
public sealed partial class 인사역할지원작성ViewModel(
    I인사역할지원Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial string 선택역할코드 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial bool 자발적지원확인 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial bool 역할고용비보장확인 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(제출가능))]
    public partial bool 검토정보이용동의 { get; set; }

    [ObservableProperty]
    public partial HrRoleApplicationResponse? 제출결과 { get; private set; }

    private Guid _요청Id = Guid.NewGuid();

    public bool 제출가능
        => !처리중
           && HrRoleApplicationCatalog.Find(선택역할코드) is not null
           && 자발적지원확인
           && 역할고용비보장확인
           && 검토정보이용동의;

    public async Task<bool> 제출Async(CancellationToken cancellationToken = default)
    {
        if (HrRoleApplicationCatalog.Find(선택역할코드) is null)
        {
            return 유효성실패("지원할 역할을 선택해 주세요.");
        }

        if (!자발적지원확인 || !역할고용비보장확인 || !검토정보이용동의)
        {
            return 유효성실패("역할 지원 안내의 세 가지 항목을 모두 확인해 주세요.");
        }

        var succeeded = await 작업실행Async(
            async token => 제출결과 = await service.제출Async(new HrRoleApplicationSubmitRequest
            {
                SubmissionRequestId = _요청Id,
                RoleCode = 선택역할코드,
                ConfirmedVoluntaryApplication = 자발적지원확인,
                ConfirmedNoRoleOrEmploymentGuarantee = 역할고용비보장확인,
                ConfirmedReviewDataUse = 검토정보이용동의,
                ConsentVersion = HrRoleApplicationConsent.CurrentVersion
            }, token),
            "역할 지원을 저장했습니다. 역할 배정이나 고용·계약은 아직 이루어지지 않았습니다.",
            cancellationToken,
            인사역할지원오류.메시지);

        if (succeeded)
        {
            초안초기화();
        }

        return succeeded;
    }

    public void 결과초기화()
    {
        제출결과 = null;
        초안초기화();
        작업상태초기화();
    }

    private void 초안초기화()
    {
        선택역할코드 = string.Empty;
        자발적지원확인 = false;
        역할고용비보장확인 = false;
        검토정보이용동의 = false;
        _요청Id = Guid.NewGuid();
    }
}

/// <summary>본인의 활성 역할 지원 철회 Command만 담당합니다.</summary>
public sealed partial class 인사역할지원철회ViewModel(
    I인사역할지원Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial Guid? 처리중지원Id { get; private set; }

    [ObservableProperty]
    public partial HrRoleApplicationResponse? 철회결과 { get; private set; }

    public async Task<bool> 철회Async(Guid applicationId, CancellationToken cancellationToken = default)
    {
        if (applicationId == Guid.Empty)
        {
            return 유효성실패("철회할 역할 지원을 확인해 주세요.");
        }

        처리중지원Id = applicationId;
        try
        {
            return await 작업실행Async(
                async token => 철회결과 = await service.철회Async(applicationId, token),
                "역할 지원을 철회했습니다. 해당 지원은 활성 검토에서 제외됩니다.",
                cancellationToken,
                인사역할지원오류.메시지);
        }
        finally
        {
            처리중지원Id = null;
        }
    }

    public void 결과초기화()
    {
        처리중지원Id = null;
        철회결과 = null;
        작업상태초기화();
    }
}

/// <summary>조회·지원·철회 책임을 조립하고 Command 성공 뒤 서버 원장을 다시 조회합니다.</summary>
public sealed class 인사역할지원PageViewModel : 조립ViewModelBase
{
    public 인사역할지원PageViewModel(
        인사역할지원목록ViewModel list,
        인사역할지원작성ViewModel composer,
        인사역할지원철회ViewModel withdrawal)
    {
        목록 = 하위ViewModel등록(list);
        작성 = 하위ViewModel등록(composer);
        철회 = 하위ViewModel등록(withdrawal);
    }

    public 인사역할지원목록ViewModel 목록 { get; }
    public 인사역할지원작성ViewModel 작성 { get; }
    public 인사역할지원철회ViewModel 철회 { get; }
    public bool 처리중 => 목록.처리중 || 작성.처리중 || 철회.처리중;

    public Task<bool> 초기화Async(CancellationToken cancellationToken = default)
        => 목록.조회Async(cancellationToken);

    public async Task<bool> 제출후재조회Async(CancellationToken cancellationToken = default)
        => await 작성.제출Async(cancellationToken)
           && await 목록.조회Async(cancellationToken);

    public async Task<bool> 철회후재조회Async(Guid applicationId, CancellationToken cancellationToken = default)
        => await 철회.철회Async(applicationId, cancellationToken)
           && await 목록.조회Async(cancellationToken);

    public void 결과초기화()
    {
        목록.결과초기화();
        작성.결과초기화();
        철회.결과초기화();
    }
}

internal static class 인사역할지원오류
{
    internal static string 메시지(Exception exception)
        => exception is SsalddelApiException { StatusCode: 401 }
            ? "로그인 세션이 만료되었습니다. 다시 로그인해 주세요."
            : exception is SsalddelApiException { StatusCode: 403 }
                ? "역할 지원 기능이 아직 비활성 상태이거나 이 계정에서 사용할 수 없습니다."
                : exception is SsalddelApiException { StatusCode: 409 }
                    ? "같은 지원 요청이 이미 처리되었습니다. 목록을 새로고침해 주세요."
                    : $"역할 지원 원장을 처리하지 못했습니다. {exception.Message}";
}
