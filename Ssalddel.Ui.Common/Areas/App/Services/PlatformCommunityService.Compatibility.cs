namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed class PlatformCommunityService : CommunityPlatformClient
{
    public PlatformCommunityService(
        HttpClient httpClient,
        SsalddelProtectedApiClient protectedApiClient)
        : base(httpClient, protectedApiClient)
    {
    }
}
