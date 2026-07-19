using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Models;

public sealed record PlatformHomeQuickAction(
    string Title,
    string Description,
    string Href,
    string Icon,
    Color Color);
