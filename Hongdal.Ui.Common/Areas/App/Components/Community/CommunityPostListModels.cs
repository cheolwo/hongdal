using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public sealed record CommunitySeedPost(
    string Title,
    string Body,
    string Category,
    string Meta,
    string Icon,
    Color Color,
    string Author,
    int RecommendationCount,
    int CommentCount,
    bool HasDiagramPreview);
