using Godot;

/// <summary>
/// Base class for tween objects
/// </summary>
public abstract class AnimationObject : ITimelineObject
{
    int ITimelineObject.Id => (int)ObjectType.Tween;

    public int Index { get; set; }

    public int Millisecond { get; set; }

    public Tween.EaseType EaseType { get; set; }

    public Tween.TransitionType TransitionType { get; set; }

    public int CompareTo(ITimelineObject other)
    {
        throw new System.NotImplementedException();
    }

    public bool DoProcess(Runner runner)
    {
        throw new System.NotImplementedException();
    }

    public void Process(Runner runner)
    {
        throw new System.NotImplementedException();
    }
}
