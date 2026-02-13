namespace BlazorApp01.Web.Services;

public sealed class ThemeService
{
    public string CurrentTheme { get; private set; } = "auto";
    
    public void SetTheme(string theme)
    {
        CurrentTheme = theme;
    }
}