using System;
using Godot;

/// <summary>
/// Base class for modifiers
/// </summary>
public abstract class Modifier
{
    public abstract string Name { get; }

    public abstract Color Color { get; }

    /// <summary>
    /// Determines if the <see cref="Modifier"/> was used
    /// </summary>
    public virtual bool Active { get; set; } = false;

    /// <summary>
    /// Determines if the <see cref="Modifier"/> is rankable
    /// </summary>
    public virtual bool Rankable { get; } = false;

    /// <summary>
    /// Score multiplier for the <see cref="Modifier"/>
    /// </summary>
    public virtual double ScoreMultiplier { get; } = 1;

    /// <summary>
    /// Mods that are incompatible with the <see cref="Modifier"/>
    /// </summary>
    public virtual Type[] IncompatibleMods => [];

    /// <summary>
    /// Called when the <see cref="Modifier"/> is affecting gameplay
    /// </summary>
    public virtual void Activate(Attempt attempt)
    {
        Active = true;
    }
}
