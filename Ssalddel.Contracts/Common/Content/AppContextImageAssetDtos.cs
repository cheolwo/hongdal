namespace Ssalddel.Contracts.Common.Content;

public static class AppContextImageAssetRoutes
{
    public const string Base = "api/v1/common/content/app-context-images";

    public static string ForPack(string packId)
        => $"{Base}/{Uri.EscapeDataString(packId)}";
}

public sealed record AppContextImageAssetDto(
    string SceneKey,
    string AppPackId,
    int SceneNumber,
    string Title,
    string AltText,
    string ImageUrl,
    string AspectRatio,
    string QualityStatus,
    IReadOnlyList<string> RouteRefs);

public sealed record AppContextImageAssetListResponse(
    string AppPackId,
    int Count,
    IReadOnlyList<AppContextImageAssetDto> Items);
