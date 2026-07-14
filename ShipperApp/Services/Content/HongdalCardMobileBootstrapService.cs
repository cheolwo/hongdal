using Hongdal.Client.Infrastructure.Notifications;
using Hongdal.Contracts.Common.Notifications;
using Hongdal.Ui.Common.Areas.App.Services;

namespace ShipperApp.Services.Content;

public sealed class HongdalCardMobileBootstrapService
{
    private readonly IAuthSession _authSession;
    private readonly IHongdalMobilePushTokenProvider _pushTokenProvider;
    private readonly HongikHakdangCardDeliveryClient _cardClient;

    public HongdalCardMobileBootstrapService(
        IAuthSession authSession,
        IHongdalMobilePushTokenProvider pushTokenProvider,
        HongikHakdangCardDeliveryClient cardClient)
    {
        _authSession = authSession;
        _pushTokenProvider = pushTokenProvider;
        _cardClient = cardClient;
    }

    public async Task<bool> RegisterCurrentInstallationAsync(
        CancellationToken cancellationToken = default)
    {
        await _authSession.RestoreAsync(cancellationToken);
        if (!_authSession.IsLoggedIn)
        {
            return false;
        }

        var token = await _pushTokenProvider.GetCurrentAsync(cancellationToken);
        if (token is null)
        {
            return false;
        }

        await _cardClient.RegisterInstallationAsync(
            new HongdalMobilePushInstallationUpsertRequest(
                token.InstallationId,
                token.AppKey,
                token.Platform,
                token.PushToken,
                token.AppVersion,
                token.DeviceModel),
            cancellationToken);
        return true;
    }
}
