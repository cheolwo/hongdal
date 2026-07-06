using MudBlazor;

namespace Hongdal.Ui.Common.Areas.BackOffice.Theme;

public static class BackOfficeTheme
{
    public static MudTheme Create()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = Colors.Blue.Default,
                Secondary = Colors.Green.Default,
                AppbarBackground = Colors.Blue.Darken2
            }
        };
    }
}
