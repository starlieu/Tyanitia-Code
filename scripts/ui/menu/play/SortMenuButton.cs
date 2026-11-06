using System;
using System.Linq;
using Godot;

public partial class SortMenuButton : Button, ISkinnable
{
    [Export]
    private Control panel;

    [Export]
    private Button order;

    [Export]
    private Label orderLabel;

    [Export]
    private VBoxContainer buttons;

    private Button previousButton;

    public override void _Ready()
    {
        Toggled += toggle;
        order.Pressed += toggleOrder;
        SkinManager.Instance.Loaded += UpdateSkin;

        previousButton = buttons.GetNode<Button>("Alphabetical");

        foreach (Button button in buttons.GetChildren().Cast<Button>())
        {
            button.Pressed += () => selectSort(button);
        }

        UpdateSkin(SkinManager.Instance.Skin);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            Rect2 rect = new(panel.GlobalPosition, panel.Size);
            Rect2 rect2 = new(GlobalPosition, Size);

            if (!rect.HasPoint(mouseButton.Position) && !rect2.HasPoint(mouseButton.Position))
            {
                ButtonPressed = false;
            }
        }
    }

    public void UpdateSkin(SkinProfile skin)
    {
<<<<<<< HEAD
        order.Icon = TyanitiaIcons.Get(MapList.Instance.Ascending.Value ? "sort_ascend" : "sort");
=======
        order.Icon = MapList.Instance.Ascending.Value ? skin.SortAscendButtonImage : skin.SortButtonImage;
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
        Icon = order.Icon;
    }

    private void toggle(bool toggled)
    {
        panel.Visible = toggled;
    }

    private void selectSort(Button button)
    {
        if (Enum.TryParse<MapList.SortType>(button.Name, true, out var result) && MapList.Instance.Sorting.Value != result)
        {
            previousButton?.Disabled = false;
            button.Disabled = true;
            previousButton = button;

            MapList.Instance.Sorting.Value = result;
        }
    }

    private void toggleOrder()
    {
<<<<<<< HEAD
        MapList.Instance.Ascending.Value = !MapList.Instance.Ascending.Value;
        orderLabel.Text = MapList.Instance.Ascending.Value ? "Ascending" : "Descending";
        order.Icon = TyanitiaIcons.Get(MapList.Instance.Ascending.Value ? "sort_ascend" : "sort");
=======
        var skin = SkinManager.Instance.Skin;

        MapList.Instance.Ascending.Value = !MapList.Instance.Ascending.Value;
        orderLabel.Text = MapList.Instance.Ascending.Value ? "Ascending" : "Descending";
        order.Icon = MapList.Instance.Ascending.Value ? skin.SortAscendButtonImage : skin.SortButtonImage;
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

        Icon = order.Icon;
    }
}
