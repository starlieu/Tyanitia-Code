using System;

/// <summary>
/// Base interface for a map object
/// </summary>
public interface ITimelineObject : IComparable<ITimelineObject>
{
    /// <summary>
    /// Identity of the <see cref="ITimelineObject"/>
    /// <para>Each object type must have a unique ID</para>
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Index position of the <see cref="ITimelineObject"/>
    /// </summary>
    int Index { get; }

    /// <summary>
    /// Millisecond timing for the <see cref="ITimelineObject"/>
    /// </summary>
    int Millisecond { get; }

    /// <summary>
    /// Checks if the <see cref="ITimelineObject"/> should be processed by its associated <see cref="Runner"/>
    /// </summary>
    bool DoProcess(Runner runner);

    /// <summary>
    /// Compute game logic on the <see cref="ITimelineObject"/> from its associated <see cref="Runner"/>
    /// </summary>
    void Process(Runner runner);
}
