namespace Ssalddel.Ui.Common.Areas.App.Components.Inputs;

public sealed record SsalddelActionSheetAction(
    string Id,
    string Label,
    SsalddelActionSheetActionStyle Style = SsalddelActionSheetActionStyle.Standard,
    bool IsDisabled = false);

public enum SsalddelActionSheetActionStyle
{
    Standard,
    Destructive
}
