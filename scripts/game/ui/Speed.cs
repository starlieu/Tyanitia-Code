using Godot;

public partial class Speed : UIComponent
{
    private Label3D label;

    public override void Init()
    {
        label = GetNode<Label3D>("Label");

        label.Modulate = Color.Color8(255, 255, 255, (byte)(Runner.Speed == 1 ? 0 : 100));
        label.Text = $"{Runner.Speed:F2}x";
    }
}
