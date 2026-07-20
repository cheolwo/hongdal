using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace WarehouseManagerApp.Services;

/// <summary>공용 보호 API 클라이언트에 현재 창고 앱 세션의 access token만 제공합니다.</summary>
public sealed class WarehouseAccessTokenProvider(ClientAuthSession session)
    : ISsalddelAccessTokenProvider
{
    public string? AccessToken => session.AccessToken;
}
