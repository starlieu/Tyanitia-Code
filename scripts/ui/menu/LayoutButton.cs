using System;
using Godot;

public partial class LayoutButton : Button
{
    public override void _Pressed()
    {
        var layout = MapList.Instance.Layout == MapList.ListLayout.List ? MapList.ListLayout.Grid : MapList.ListLayout.List;

        Icon = TyanitiaIcons.Get(layout == MapList.ListLayout.List ? "list" : "grid");

        MapList.Instance.UpdateLayout(layout);
    }
}
