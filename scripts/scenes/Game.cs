using System;
using System.Collections.Generic;
using Godot;

public partial class Game : BaseScene
{
    [Export]
    public Runner Runner;

    [Export]
    public PauseMenu Menu;

    [Export]
    public PlaytestOverlay PlaytestOverlay;

    [Export]
    public ReplayManager ReplayManager { get; private set; }

    [Export]
    public PlayerInputController PlayerInputController { get; private set; }

    [Export]
    public CursorManager CursorManager { get; private set; }

    [Signal]
    public delegate void StartTempPauseEventHandler(Attempt attempt);

    public static Game Instance;
    public static Attempt Attempt;
    public static bool StartQueued = false;
    private static double fadeoutToggleValue = -1;

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        ReplayManager ??= GetNode<ReplayManager>("ReplayManager");
        CursorManager ??= GetNode<CursorManager>("CursorManager");
        PlayerInputController ??= GetNode<PlayerInputController>("PlayerInputController");

        if (CursorManager == null)
            Logger.Error("No CursorManager found!");
        if (PlayerInputController == null)
            Logger.Error("No PlayerInputController found!");

        PlayerInputController.OnMouseMove += (relative, absolute) =>
        {
            if (OS.GetName() == "Android" || !Runner.Playing || Attempt.IsReplay || Attempt.Modifiers.Exists(mod => mod is AutoplayModifier))
                return;

            if (Attempt.Settings.AbsoluteInput || OS.GetName() == "Android")
            {
                // Take mouse position difference between center of the current window size
                // This is to make the mouse position the same as relative if it was locked, or confined
                Vector2 absolutePosition = absolute - (GetViewport().GetWindow().Size / 2);

                // Multiply by 0.582f to make it 1:1 to absolute scale on nightly
                CursorManager.UpdateCursor(absolutePosition * 0.582f);
            }
            else
            {
                CursorManager.UpdateCursor(relative);
            }
            Attempt.DistanceMM += relative.Length() / Attempt.Settings.Sensitivity / 57.5;
        };

        PlayerInputController.OnLeftMouseButton += isPressed => { };

        PlayerInputController.OnTogglePaused += () =>
        {
            if (Runner.ObjectIndicesStart[typeof(Note)] > 0 && Attempt.Progress < Attempt.Map.Notes[^1].Millisecond)
            {
                Attempt.Qualifies = false;
            }

            if (SettingsManager.Shown)
            {
                SettingsMenu.Instance.HideMenu();
            }
            else
            {
                if (Tyanitia.TempMode && !PlaytestOverlay.PlaytestInit)
                    return;
                Menu.ShowMenu(!Menu.Shown);
            }
        };

        PlayerInputController.OnToggleReplayViewerVisibility += () =>
        {
            if (Attempt.IsReplay)
            {
                ReplayManager.ShowReplayViewer(Attempt);
            }
        };

        PlayerInputController.OnToggleShowOrthonogalCamera += () =>
        {
            if (Attempt.IsReplay)
            {
                ReplayManager.ShowOrthonogalCamera(Attempt);
            }
        };

        PlayerInputController.OnPauseOrSkipPressed += () =>
        {
            if (Attempt.IsReplay)
            {
                ReplayManager.TogglePause();
            }
            else if (PlaytestOverlay.PlaytestInit == false && Tyanitia.TempMode)
            {
                PlaytestOverlay.UpdatePlaytestOverlay(false);
            }
            else
            {
                if (Lobby.Players.Count > 1)
                    return;
                Runner.Skip();

                // Space To Pause
                // if (!Attempt.CanSkip && Attempt.Settings.SpaceToPause)
                // {
                // 	EmitSignal(SignalName.StartTempPause, Attempt);
                // }
            }
        };

        // PlayerInputController.OnPauseOrSkipReleased += () =>
        // {

        // };
        PlayerInputController.OnToggleFade += () =>
        {
            double val;

            if (Attempt.Settings.FadeOut.Value > 0)
            {
                fadeoutToggleValue = Attempt.Settings.FadeOut.Value;
                val = 0;
            }
            else
            {
                val = fadeoutToggleValue == -1 ? 100 : fadeoutToggleValue;
            }

            Attempt.Settings.FadeOut.Value = val;
        };

        PlayerInputController.OnTogglePushback += () => Attempt.Settings.Pushback.Value = !Attempt.Settings.Pushback;
        PlayerInputController.OnRestartPressed += Restart;
    }

<<<<<<< HEAD
    public override void _UnhandledInput(InputEvent @event)
=======
    public override void _Input(InputEvent @event)
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    {
        if (OS.GetName() != "Android" || Runner == null || !Runner.Playing || Menu.Shown || Attempt.IsReplay)
        {
            mobileTouchIndex = -1;
            return;
        }

        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

        if (@event is InputEventScreenTouch touch)
        {
<<<<<<< HEAD
            if (touch.Pressed && touch.Position.X >= viewportSize.X * 0.36f)
=======
            if (touch.Pressed && mobileTouchIndex == -1 && touch.Position.X >= viewportSize.X * 0.36f)
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
            {
                mobileTouchIndex = touch.Index;
                lastMobileTouchPosition = touch.Position;
                GetViewport().SetInputAsHandled();
            }
            else if (!touch.Pressed && touch.Index == mobileTouchIndex)
            {
                mobileTouchIndex = -1;
                GetViewport().SetInputAsHandled();
            }
        }
        else if (@event is InputEventScreenDrag drag && drag.Index == mobileTouchIndex)
        {
            updateTouchCursor(drag.Position, viewportSize);
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            UpdateMobileCursorVertical(Mathf.Clamp((double)(Attempt.CursorPosition.Y / (float)Constants.BOUNDS.Y) * -0.5f + 0.5f + (mouseButton.ButtonIndex == MouseButton.WheelUp ? -0.05f : 0.05f), 0, 1));
        }
    }

    private int mobileTouchIndex = -1;
    private Vector2 lastMobileTouchPosition;

    private void updateTouchCursor(Vector2 touchPosition, Vector2 viewportSize)
    {
        float touchAreaStart = viewportSize.X * 0.36f;
        float touchAreaWidth = viewportSize.X - touchAreaStart;
<<<<<<< HEAD
=======
        if (touchAreaWidth <= 0 || viewportSize.Y <= 0)
            return;

>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
        Vector2 movement = touchPosition - lastMobileTouchPosition;
        lastMobileTouchPosition = touchPosition;
        float sensitivity = (float)(double)Attempt.Settings.Sensitivity / 0.3f;
        Vector2 delta = new(movement.X / touchAreaWidth * 2 * (float)Constants.BOUNDS.X,
            -movement.Y / viewportSize.Y * 2 * (float)Constants.BOUNDS.Y);
        Vector2 position = (Attempt.CursorPosition + delta * sensitivity).Clamp(-Constants.BOUNDS, Constants.BOUNDS);
        float distance = Attempt.CursorPosition.DistanceTo(position);

        CursorManager.UpdateTouchCursor(position);
        Attempt.DistanceMM += distance / Math.Max(0.01, (double)Attempt.Settings.Sensitivity) / 57.5;
    }

    public void UpdateMobileCursorVertical(double value)
    {
        if (OS.GetName() != "Android" || Runner == null || !Runner.Playing || Menu.Shown || Attempt.IsReplay)
            return;

        float normalizedY = 1f - (float)value * 2f;
        Vector2 position = new(Attempt.CursorPosition.X, normalizedY * (float)Constants.BOUNDS.Y);
        float distance = Attempt.CursorPosition.DistanceTo(position);
        CursorManager.UpdateTouchCursor(position);
        Attempt.DistanceMM += distance / Math.Max(0.01, (double)Attempt.Settings.Sensitivity) / 57.5;
    }

    public override void Load()
    {
        base.Load();
        GraphicsSettings.ApplyRenderPercentage(SettingsManager.Instance.Settings.RenderPercentage.Value);

        if (OS.GetName() != "Android")
        {
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
        }

        MenuCursor.Instance.UpdateVisible(false, false);
        SceneManager.Space.UpdateState(true);
        SceneManager.Space.UpdateMap(Attempt.Map);

        var focused = SceneManager.Root.GetViewport().GuiGetFocusOwner();
        focused?.ReleaseFocus();

        Input.MouseMode =
            OS.GetName() == "Android" || Attempt.Settings.AbsoluteInput || Attempt.IsReplay
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        Input.UseAccumulatedInput = false;

        Runner.Attempt = Attempt;
        ReplayManager.InitReplayLength();
        ReplayManager.ShowReplayViewer(Runner.Attempt, Runner.Attempt.IsReplay);
        ReplayManager.ShowOrthonogalCamera(Runner.Attempt, Runner.Attempt.IsReplay);

        Menu.HideMenu(true);

        if (Runner.Attempt.IsReplay)
        {
            ReplayManager.CurrentMode = ReplayManager.Mode.PLAYBACK;
        }
        else if (Runner.Attempt.Settings.RecordReplays && !Runner.Attempt.Modifiers.Exists(mod => mod is AutoplayModifier))
        {
            ReplayManager.NewReplay(Runner.Attempt);
            ReplayManager.CurrentMode = ReplayManager.Mode.RECORD;
        }
        else
        {
            ReplayManager.CurrentMode = ReplayManager.Mode.NONE;
        }

        Logger.Log($"Replay Mode: {ReplayManager.CurrentMode}");

        Runner.Play();
        StartQueued = false;

        if (!Attempt.IsReplay)
        {
            Stats.Instance.Attempts++;
            Attempt.Map.PlayCount++;
            MapManager.Update(Attempt.Map);
        }

        if (Tyanitia.TempMode && !PlaytestOverlay.PlaytestInit)
        {
            PlaytestOverlay.Attempt = Attempt;
            PlaytestOverlay.Runner = Runner;
            PlaytestOverlay.UpdatePlaytestOverlay(true);
        }
    }

    public static void Play(
        Map map,
        double speed,
        double startFrom,
        CameraMode cameraMode,
        List<Modifier> mods,
        string[] players = null,
        Replay[] replays = null
    )
    {
        if (StartQueued)
            return;

        StartQueued = true;

        var parsedMap = MapParser.Decode(map.FolderPath, Tyanitia.AudioFilePath);
        Attempt = new(parsedMap, speed, startFrom, cameraMode, mods, players, replays);

        SceneManager.Load("res://scenes/game.tscn");
    }

    public void Restart()
    {
        Runner.Fail();
        Runner.Stop(false);

        var oldAttempt = Attempt;
        var map = MapParser.Decode(oldAttempt.Map.FolderPath, Tyanitia.AudioFilePath);
        Attempt = new(
            map,
            oldAttempt.Speed,
            oldAttempt.StartFrom,
            oldAttempt.CameraMode,
            oldAttempt.Modifiers,
            oldAttempt.Players,
            oldAttempt.Replays
        );

        SceneManager.ReloadCurrentScene();
    }
}
