using System;
using Godot;

public partial class FPSCounter : Label
{
    public uint Frames = 0;

    private double time = 0;

    public override void _Ready()
    {
        var settings = SettingsManager.Instance.Settings;
        if (LabelSettings != null)
        {
            LabelSettings = (LabelSettings)LabelSettings.Duplicate();
            LabelSettings.FontSize *= 3;
        }
        else
            AddThemeFontSizeOverride("font_size", GetThemeFontSize("font_size") * 3);
        MouseFilter = MouseFilterEnum.Ignore;
        if (MobileUi.IsMobile)
            OffsetLeft = 28;

        Visible = settings.DisplayFPS.Value;

        settings.DisplayFPS.Updated += value =>
        {
            Visible = (bool)value;
        };
    }

    public override void _Process(double delta)
    {
        Frames++;
        time += delta;

        if (time >= 1)
        {
            Text = $"{Frames} FPS";

            time--;
            Frames = 0;
        }
    }
}
