using Godot;

public static class MobileUi
{
    public static bool IsMobile => OS.GetName() == "Android";

    public static void EnlargeTouchTargets(Node root)
    {
        if (!IsMobile)
            return;

        foreach (Node child in root.GetChildren())
        {
            if (child is BaseButton button)
            {
                Vector2 minimum = button.CustomMinimumSize;
                minimum.Y = Mathf.Max(minimum.Y, 84);
                if (button is TextureButton)
                    minimum.X = Mathf.Max(minimum.X, 80);
                button.CustomMinimumSize = minimum;

                int currentFontSize = button.HasThemeFontSize("font_size") ? button.GetThemeFontSize("font_size") : 20;
                button.AddThemeFontSizeOverride("font_size", Mathf.Max(30, Mathf.CeilToInt(currentFontSize * 1.5f)));
            }
            else if (child is Range range)
            {
                Vector2 minimum = range.CustomMinimumSize;
                minimum.Y = Mathf.Max(minimum.Y, 64);
                range.CustomMinimumSize = minimum;
            }
            else if (child is LineEdit lineEdit)
            {
                Vector2 minimum = lineEdit.CustomMinimumSize;
                minimum.Y = Mathf.Max(minimum.Y, 72);
                lineEdit.CustomMinimumSize = minimum;
            }

            EnlargeTouchTargets(child);
        }
    }
}
