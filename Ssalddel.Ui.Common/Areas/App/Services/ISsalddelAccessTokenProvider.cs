namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface ISsalddelAccessTokenProvider
{
    string? AccessToken { get; }
}

internal sealed class EmptySsalddelAccessTokenProvider : ISsalddelAccessTokenProvider
{
    public string? AccessToken => null;
}
