using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace OrdererApp.Services;

/// <summary>주문자 앱의 저장 세션 복원·갱신과 공용 인증 계약 연결만 담당합니다.</summary>
public sealed class OrdererSessionService(
    ClientAuthSession session,
    OrdererAuthApiService authApi) : I주문자앱인증Service
{
    public async Task<주문자앱인증결과> 복원Async(
        CancellationToken cancellationToken = default)
    {
        var state = await session.RestoreAsync(cancellationToken);
        if (state == ClientAuthSessionRestoreState.RefreshRequired)
        {
            var refresh = await authApi.갱신Async(
                session.UserId ?? string.Empty,
                session.RefreshToken ?? string.Empty,
                cancellationToken);
            if (!refresh.성공)
            {
                await session.ClearAsync(cancellationToken);
                return new 주문자앱인증결과(주문자앱세션상태.익명, refresh.오류메시지);
            }
        }

        return new 주문자앱인증결과(CurrentSession());
    }

    public async Task<주문자앱인증결과> 로그인Async(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = await authApi.로그인Async(userNameOrEmail, password, cancellationToken);
        return result.성공
            ? new 주문자앱인증결과(CurrentSession())
            : new 주문자앱인증결과(주문자앱세션상태.익명, result.오류메시지);
    }

    public Task 로그아웃Async(CancellationToken cancellationToken = default)
        => session.ClearAsync(cancellationToken);

    private 주문자앱세션상태 CurrentSession()
        => session.IsAuthenticated
            ? new 주문자앱세션상태(true, session.UserId, session.UserName)
            : 주문자앱세션상태.익명;
}
