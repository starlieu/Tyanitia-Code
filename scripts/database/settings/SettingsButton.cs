using System;

public class SettingsButton
{
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public Action OnPressed { get; set; }
}
