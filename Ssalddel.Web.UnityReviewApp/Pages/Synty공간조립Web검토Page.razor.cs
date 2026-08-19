namespace Ssalddel.Web.UnityReviewApp.Pages;

public partial class Synty공간조립Web검토Page : IDisposable
{
    private bool IsServerAdministrator => AuthSession.IsServerAdministrator;

    protected override void OnInitialized()
    {
        AuthSession.Changed += HandleAuthChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await AuthSession.EnsureRestoredAsync();
        await Workspace.InitializeAsync(AuthSession.IsLoggedIn && IsServerAdministrator);
        await InvokeAsync(StateHasChanged);
    }

    private void HandleAuthChanged()
        => _ = InvokeAsync(async () =>
        {
            if (AuthSession.IsLoggedIn && IsServerAdministrator && !Workspace.Loaded)
            {
                await Workspace.LoadInboxAsync();
            }
            StateHasChanged();
        });

    public void Dispose()
    {
        AuthSession.Changed -= HandleAuthChanged;
    }
}
