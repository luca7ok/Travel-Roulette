using MudBlazor;

namespace PoliHack18.Themes;


public static class AppTheme
{
    public static MudTheme Default => new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#9E7448",
            Secondary = "#743725",
            Background = "#9E7448",
            TextPrimary = "#27120F"
        },
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "260px"
        }
    };
}
