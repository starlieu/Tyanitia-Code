using Godot;

public static class GraphicsSettings
{
    public static void ApplyRenderPercentage(int percentage)
    {
        if (Engine.GetMainLoop() is not SceneTree tree)
            return;
        applyScale(tree.Root, Mathf.Clamp(percentage, 25, 100) / 100f);
    }

    private static void applyScale(Node node, float scale)
    {
        if (node is Viewport viewport)
            viewport.Scaling3DScale = scale;
        foreach (Node child in node.GetChildren())
            applyScale(child, scale);
    }

    private const string renderer_setting = "renderer/rendering_method";

    public static string GetCurrentRendererLabel() => toLabel(RenderingServer.GetCurrentRenderingMethod());

    public static bool SaveRenderer(string label)
    {
        string method = toMethod(label);
        bool needsRestart = method != RenderingServer.GetCurrentRenderingMethod();

        string configPath = OS.HasFeature("editor")
            ? ProjectSettings.GlobalizePath("res://override.cfg")
            : OS.GetExecutablePath().GetBaseDir().PathJoin("override.cfg");

        ConfigFile config = new();
        Error loadError = config.Load(configPath);
        if (loadError != Error.Ok && loadError != Error.FileNotFound)
        {
            GD.PushError($"Could not read renderer settings at {configPath}: {loadError}");
            _ = ToastNotification.Notify("Could not save the renderer setting. Check file permissions.", 2);
            return false;
        }

        config.SetValue("rendering", renderer_setting, method);
        Error saveError = config.Save(configPath);
        if (saveError != Error.Ok)
        {
            GD.PushError($"Could not save renderer settings at {configPath}: {saveError}");
            _ = ToastNotification.Notify("Could not save the renderer setting. Check file permissions.", 2);
            return false;
        }

        return needsRestart;
    }

    private static string toMethod(string label) =>
        label switch
        {
            "Forward+" => "forward_plus",
            "Mobile" => "mobile",
            "Compatibility" => "gl_compatibility",
            _ => RenderingServer.GetCurrentRenderingMethod(),
        };

    private static string toLabel(string method) =>
        method switch
        {
            "forward_plus" => "Forward+",
            "mobile" => "Mobile",
            "gl_compatibility" => "Compatibility",
            _ => "Forward+",
        };
}
