/// <summary>
/// Modifies the <see cref="Map"/> data
/// </summary>
public interface IMapModifier : IModifier
{
    /// <summary>
    /// Modifies the <see cref="Map"/> data, such as <see cref="ITimelineObject"/>
    /// </summary>
    void ModifyMap(Map map, Attempt attempt);
}
