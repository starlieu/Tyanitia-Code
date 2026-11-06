using Godot;

public class NoFailModifier : Modifier, IFailModifier
{
    public override string Name => "NoFail";

    public override Color Color => new(0x20e12cff);

    public bool IsFail => false;

    public bool CheckFailCondition(bool _hit, double _health) => false;
}
