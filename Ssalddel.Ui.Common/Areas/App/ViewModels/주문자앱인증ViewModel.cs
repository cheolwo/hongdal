using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>업무 종류와 무관하게 주문자 앱 세션의 복원·로그인·로그아웃 표시 상태만 관리합니다.</summary>
public sealed partial class 주문자앱인증ViewModel(
    I주문자앱인증Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(로그인됨))]
    [NotifyPropertyChangedFor(nameof(현재사용자표시))]
    public partial 주문자앱세션상태 세션 { get; private set; } = 주문자앱세션상태.익명;

    [ObservableProperty]
    public partial bool 초기화됨 { get; private set; }

    public bool 로그인됨 => 세션.로그인됨;
    public string 현재사용자표시 => 세션.사용자표시;

    public Task<bool> 복원Async(CancellationToken cancellationToken = default)
        => 인증실행Async(
            token => service.복원Async(token),
            "주문자 로그인 세션을 확인했습니다.",
            cancellationToken,
            markInitialized: true);

    public Task<bool> 로그인Async(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(유효성실패("아이디와 비밀번호를 입력해 주세요."));
        }

        return 인증실행Async(
            token => service.로그인Async(userNameOrEmail.Trim(), password, token),
            "주문자 계정으로 로그인했습니다.",
            cancellationToken,
            markInitialized: true);
    }

    public Task<bool> 로그아웃Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                await service.로그아웃Async(token);
                세션 = 주문자앱세션상태.익명;
                초기화됨 = true;
            },
            "로그아웃했습니다.",
            cancellationToken,
            ex => $"로그아웃을 완료하지 못했습니다. {ex.Message}");

    private Task<bool> 인증실행Async(
        Func<CancellationToken, Task<주문자앱인증결과>> action,
        string successMessage,
        CancellationToken cancellationToken,
        bool markInitialized)
        => 작업실행Async(
            async token =>
            {
                var result = await action(token);
                if (!result.성공)
                {
                    throw new InvalidOperationException(
                        result.오류메시지 ?? "주문자 로그인 상태를 확인하지 못했습니다.");
                }

                세션 = result.세션;
                if (markInitialized)
                {
                    초기화됨 = true;
                }
            },
            successMessage,
            cancellationToken,
            ex => string.IsNullOrWhiteSpace(ex.Message)
                ? "주문자 로그인 상태를 확인하지 못했습니다."
                : ex.Message);
}
