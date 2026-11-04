using Godot;

public class HorizontalFlipModifier : Modifier, IMapModifier
{
    public override string Name => "HFlip";

    public override Color Color => new(0xffffffff);

    public override bool Rankable => true;

    public void ModifyMap(Map map, Attempt attempt)
    {
        foreach (var note in map.Notes)
        {
            note.X = -note.X;
        }
    }
}
