namespace Hongdal.Ui.Common.Areas.App.Services;

public interface IHongdalAccessTokenProvider
{
    string? AccessToken { get; }
}

internal sealed class EmptyHongdalAccessTokenProvider : IHongdalAccessTokenProvider
{
    public string? AccessToken => null;
}
