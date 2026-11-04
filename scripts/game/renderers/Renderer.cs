using Godot;

// The generic is kinda useless right now but keeping it for future use
public abstract partial class Renderer : Node3D
{
    internal SettingsProfile Settings { get; private set; }
    internal SkinProfile Skin { get; private set; }

    public virtual void Setup(SettingsProfile settings, SkinProfile skin)
    {
        Settings = settings;
        Skin = skin;
    }

    public abstract void Process(double delta, Attempt attempt);
}
