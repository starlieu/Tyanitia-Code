using Godot;

public class HardRockModifier : Modifier
{
    public override string Name => "HardRock";
    public override Color Color => new("ff7195");
    public override bool Rankable => true;
    public override double ScoreMultiplier => 1.08;
}
