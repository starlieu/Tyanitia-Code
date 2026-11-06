using Godot;

public partial class MobileGameControls : Control
{
    [Export]
    private Game game;

    [Export]
    private Runner runner;

    private Button closeMap;
    private Button pause;
<<<<<<< HEAD
=======
    private Button giveUp;
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

    public override void _Ready()
    {
        closeMap = GetNode<Button>("CloseMap");
        pause = GetNode<Button>("Pause");
<<<<<<< HEAD
        closeMap.Pressed += closeCurrentMap;
        pause.Pressed += () => game.Menu.ShowMenu();
=======
        giveUp = GetNode<Button>("GiveUp");
        closeMap.Pressed += closeCurrentMap;
        pause.Pressed += () => game.Menu.ShowMenu();
        giveUp.Pressed += runner.GiveUp;
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
        MobileUi.EnlargeTouchTargets(this);
        updateVisibility();
    }

    public override void _Process(double delta) => updateVisibility();

    private void updateVisibility()
    {
<<<<<<< HEAD
        Visible = MobileUi.IsMobile && runner.Playing && !runner.Attempt.IsReplay && !SettingsManager.Shown;
=======
        bool shouldBeVisible = MobileUi.IsMobile && runner.Playing && !runner.Attempt.IsReplay && !SettingsManager.Shown;
        if (Visible == shouldBeVisible)
            return;

        Visible = shouldBeVisible;
        if (!Visible)
            return;

        Color fadedStart = new(1, 1, 1, 0.5f);
        Color fadedEnd = new(1, 1, 1, 0);
        pause.Modulate = fadedStart;
        giveUp.Modulate = fadedStart;

        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut).SetParallel();
        tween.TweenProperty(pause, "modulate", fadedEnd, 1).SetDelay(1);
        tween.TweenProperty(giveUp, "modulate", fadedEnd, 1).SetDelay(1);
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    }

    private void closeCurrentMap()
    {
        runner.Fail();
        runner.Stop(false);
        SceneManager.Load("res://scenes/main_menu.tscn");
    }
}
