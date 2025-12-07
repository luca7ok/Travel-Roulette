using MudBlazor;

namespace PoliHack18.Themes;

public static class AppTheme
{
    public static MudTheme Default => new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#0F1210",
            Secondary = "#9c8c69",
            Background = "#6b786d",
            TextPrimary = "#27120F"
        },
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "260px",
        }
            
    };
}