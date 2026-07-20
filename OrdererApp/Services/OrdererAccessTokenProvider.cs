using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace OrdererApp.Services;

/// <summary>공용 보호 API 클라이언트에 현재 주문자 앱 access token만 제공합니다.</summary>
public sealed class OrdererAccessTokenProvider(
    ClientAuthSession session) : ISsalddelAccessTokenProvider
{
    public string? AccessToken => session.AccessToken;
}
