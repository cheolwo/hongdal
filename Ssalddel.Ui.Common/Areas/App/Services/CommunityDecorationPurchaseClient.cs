namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed record CommunityDecorationPurchaseResult(
    bool Success,
    string Message,
    string? OrderId = null);

/// <summary>
/// 꾸미기 checkout Screen이 플랫폼별 FakePG·보유권 adapter를 호출하는 경계입니다.
/// </summary>
public interface ICommunityDecorationPurchaseClient
{
    Task<CommunityDecorationPurchaseResult> ConfirmAsync(
        CommunityDecorationProduct product,
        string paymentMethod,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Web 시각 검증과 sample 환경에서만 사용하는 세션 한정 FakePG adapter입니다.
/// </summary>
public sealed class LocalSimulationCommunityDecorationPurchaseClient(
    PlatformCommunityDecorationStateService decorationState,
    ISsalddel현재사용자Context currentUserContext)
    : ICommunityDecorationPurchaseClient
{
    public Task<CommunityDecorationPurchaseResult> ConfirmAsync(
        CommunityDecorationProduct product,
        string paymentMethod,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        cancellationToken.ThrowIfCancellationRequested();

        if (!currentUserContext.현재사용자.인증됨)
        {
            return Task.FromResult(new CommunityDecorationPurchaseResult(
                false,
                "꾸미기 보유권을 기록하려면 먼저 로그인해 주세요."));
        }

        decorationState.Purchase(product);
        var orderId = $"FAKE-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..34];
        return Task.FromResult(new CommunityDecorationPurchaseResult(
            true,
            "개발용 구매·보유권을 이 브라우저 세션에만 기록했습니다.",
            orderId));
    }
}
