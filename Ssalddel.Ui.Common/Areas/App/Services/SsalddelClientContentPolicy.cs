namespace Ssalddel.Ui.Common.Areas.App.Services;

public static class SsalddelClientContentPolicy
{
    private const string HongikHakdangChannelName = "홍익학당";

    public static bool IsVisibleToGeneralClient(CommunityDecorationProduct product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return !string.Equals(
            product.ScriptureSource?.ChannelName,
            HongikHakdangChannelName,
            StringComparison.OrdinalIgnoreCase);
    }
}
