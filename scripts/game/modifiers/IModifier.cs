using Godot;

/// <summary>
/// Base interface for modifiers
/// </summary>
public interface IModifier
{
    /// <summary>
    /// Name of the <see cref="Modifier"/>
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Accent color of the <see cref="Modifier"/>
    /// </summary>
    Color Color { get; }
}
