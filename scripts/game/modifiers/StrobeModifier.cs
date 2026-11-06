using Godot;

public class StrobeModifier : Modifier, IObjectRenderModifier<Note>
{
    public override string Name => "Strobe";

    public override Color Color => new(0xffe854ff);

    public override bool Rankable => true;

    public override double ScoreMultiplier => 1.035;

    public void ModifyRenderObject(Note note, Attempt attempt)
    {
        double at = attempt.Settings.ApproachTime;

        if (attempt.Progress / 1000 / attempt.Speed % at >= at / 2)
        {
            note.Opacity = 0;
        }
    }
}
