using Godot;

public partial class MobileGameControls : Control
{
    [Export]
    private Game game;

    [Export]
    private Runner runner;

    private Button closeMap;
    private Button pause;

    public override void _Ready()
    {
        closeMap = GetNode<Button>("CloseMap");
        pause = GetNode<Button>("Pause");
        closeMap.Pressed += closeCurrentMap;
        pause.Pressed += () => game.Menu.ShowMenu();
        MobileUi.EnlargeTouchTargets(this);
        updateVisibility();
    }

    public override void _Process(double delta) => updateVisibility();

    private void updateVisibility()
    {
        Visible = MobileUi.IsMobile && runner.Playing && !runner.Attempt.IsReplay && !SettingsManager.Shown;
    }

    private void closeCurrentMap()
    {
        runner.Fail();
        runner.Stop(false);
        SceneManager.Load("res://scenes/main_menu.tscn");
    }
}
