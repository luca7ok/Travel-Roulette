using MudBlazor;

namespace PoliHack18.Themes;


public static class AppTheme
{
    public static MudTheme Default => new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#8B5CF6",
            Secondary = "#C4B5FD",
            Background = "#374151",
            TextSecondary = "#D1D5DB"
        },
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "260px"
        }
    };
}
