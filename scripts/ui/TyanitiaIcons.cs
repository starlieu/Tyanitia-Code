using Godot;

/// <summary>Shared vector icons for menu and map-selection controls.</summary>
public static class TyanitiaIcons
{
    public static Texture2D Get(string name) => ResourceLoader.Load<Texture2D>($"res://textures/ui/refresh/{name}.svg");

    public static Texture2D ForMod(string name) => name switch
    {
        "NoFail" => Get("nofail"),
        "HardRock" => Get("hardrock"),
        "Autoplay" => Get("autoplay"),
        "Ghost" => Get("ghost"),
        "Strobe" => Get("strobe"),
        "Chaos" => Get("chaos"),
        "Vortex" => Get("vortex"),
        "Earthquake" => Get("earthquake"),
        "HFlip" => Get("hflip"),
        "VFlip" => Get("vflip"),
        _ => Get("settings"),
    };
}
