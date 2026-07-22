namespace Ssalddel.Tests.Architecture;

public sealed class SsalddelActionSheetCompositionTests
{
    [Fact]
    public void ActionSheet_PreservesFigmaStructureAndAccessibleDialogContract()
    {
        var source = File.ReadAllText(Path.Combine(FindComponentDirectory(), "SsalddelActionSheet.razor"));

        Assert.Contains("role=\"dialog\"", source, StringComparison.Ordinal);
        Assert.Contains("aria-modal=\"true\"", source, StringComparison.Ordinal);
        Assert.Contains("@onkeydown=\"HandleKeyDownAsync\"", source, StringComparison.Ordinal);
        Assert.Contains("ssalddel-action-sheet__group", source, StringComparison.Ordinal);
        Assert.Contains("ssalddel-action-sheet__cancel", source, StringComparison.Ordinal);
        Assert.Contains("action.IsDisabled", source, StringComparison.Ordinal);
        Assert.Contains("SsalddelActionSheetActionStyle.Destructive", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ActionSheetCss_PreservesMeasuredMobileLayoutAndActionStates()
    {
        var css = File.ReadAllText(Path.Combine(FindComponentDirectory(), "SsalddelActionSheet.razor.css"));

        Assert.Contains("width: min(377px, 100%)", css, StringComparison.Ordinal);
        Assert.Contains("border-radius: 14px", css, StringComparison.Ordinal);
        Assert.Contains("min-height: 56px", css, StringComparison.Ordinal);
        Assert.Contains("gap: 8px", css, StringComparison.Ordinal);
        Assert.Contains("#007aff", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#ff3b30", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#aeaeb2", css, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindComponentDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "Ssalddel.Ui.Common",
                "Areas",
                "App",
                "Components",
                "Inputs");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Ssalddel action sheet component directory was not found.");
    }
}
