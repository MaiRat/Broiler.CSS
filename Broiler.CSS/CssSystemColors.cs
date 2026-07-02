namespace Broiler.CSS;

public static class CssSystemColors
{
    public static bool TryResolve(string colorName, out CssColor color)
    {
        if (string.IsNullOrWhiteSpace(colorName))
        {
            color = default;
            return false;
        }

        switch (colorName.Trim().ToLowerInvariant())
        {
            case "field":
                color = new CssColor(255, 255, 255);
                return true;
            case "fieldtext":
                color = new CssColor(0, 0, 0);
                return true;
            default:
                color = default;
                return false;
        }
    }
}
