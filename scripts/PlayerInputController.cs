using System;
using Godot;

public partial class PlayerInputController : Node
{
    // I'm not sure if it's planned that the player can change keybindings
    // So these bindings are hardcoded (for now) -thom

    /// <summary>
    /// Fired when the mouse moves.
    /// <para>
    /// First parameter is relative movement (delta),
    /// second parameter is absolute screen position.
    /// </para>
    /// </summary>
    public event Action<Vector2, Vector2> OnMouseMove;

    public event Action OnTogglePaused;
    public event Action OnRestartPressed;
    public event Action OnToggleReplayViewerVisibility;
    public event Action OnToggleShowOrthonogalCamera;
    public event Action OnPauseOrSkipPressed;
    public event Action OnPauseOrSkipReleased;
    public event Action OnToggleFade;
    public event Action OnTogglePushback;
    public event Action<bool> OnLeftMouseButton;
    public bool IsEnabled { get; private set; } = true;
    private bool restartKeyHeld;
    private bool restartTriggered;
    private double restartHoldDuration;

    private const double RestartHoldThreshold = 0.9;

    public override void _Ready()
    {
        Input.UseAccumulatedInput = false;
    }

    public override void _Process(double delta)
    {
        if (!IsEnabled || !restartKeyHeld || restartTriggered)
            return;

        restartHoldDuration += delta;
        if (restartHoldDuration >= RestartHoldThreshold)
        {
            restartTriggered = true;
            OnRestartPressed?.Invoke();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsEnabled)
            return;

        handleInput(@event);
    }

    private void handleInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion:
            case InputEventMouseButton { ButtonIndex: MouseButton.Left }:
                if (OS.GetName() == "Android" && @event is InputEventMouseMotion)
                    break;
                handleMouseInput(@event);
                break;
            case InputEventKey { PhysicalKeycode: Key.Escape }:
            case InputEventKey { PhysicalKeycode: Key.F2 }:
            case InputEventKey { PhysicalKeycode: Key.F3 }:
            case InputEventKey { PhysicalKeycode: Key.Space }:
            case InputEventKey { PhysicalKeycode: Key.S }:
            case InputEventKey { PhysicalKeycode: Key.R }:
            case InputEventKey { PhysicalKeycode: Key.F }:
            case InputEventKey { PhysicalKeycode: Key.P }:
            case InputEventKey { PhysicalKeycode: Key.Quoteleft }:
                handleKeyboardInput(@event);
                break;
        }
    }

    private void handleMouseInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
            OnMouseMove?.Invoke(motion.Relative, motion.Position);

        if (@event is InputEventMouseButton mouseButton)
        {
            if (!mouseButton.Pressed || mouseButton.DoubleClick)
                return;

            OnLeftMouseButton?.Invoke(mouseButton.Pressed);
        }
    }

    private void handleKeyboardInput(InputEvent @event)
    {
        var key = (InputEventKey)@event;

        if (key.Echo)
            return;

        if (key.PhysicalKeycode == Key.R)
        {
            if (key.Pressed)
            {
                restartKeyHeld = true;
                restartTriggered = false;
                restartHoldDuration = 0;
            }
            else
            {
                restartKeyHeld = false;
                restartTriggered = false;
                restartHoldDuration = 0;
            }
            return;
        }

        if (!key.Pressed)
            return;

        switch (key)
        {
            case { PhysicalKeycode: Key.Escape }:
            case { PhysicalKeycode: Key.Space }:
                OnTogglePaused?.Invoke();
                break;
            case { PhysicalKeycode: Key.S }:
                OnPauseOrSkipPressed?.Invoke();
                break;
            case { PhysicalKeycode: Key.F2 }:
                OnToggleReplayViewerVisibility?.Invoke();
                break;
            case { PhysicalKeycode: Key.F3 }:
                OnToggleShowOrthonogalCamera?.Invoke();
                break;
            case { PhysicalKeycode: Key.F }:
                OnToggleFade?.Invoke();
                break;
            case { PhysicalKeycode: Key.P }:
                OnTogglePushback?.Invoke();
                break;
        }
    }

    public void ToggleState() => IsEnabled = !IsEnabled;
}
