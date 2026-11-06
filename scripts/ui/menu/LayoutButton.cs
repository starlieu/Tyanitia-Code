using System;
using Godot;

public partial class LayoutButton : Button
{
    public override void _Pressed()
    {
        var layout = MapList.Instance.Layout == MapList.ListLayout.List ? MapList.ListLayout.Grid : MapList.ListLayout.List;

<<<<<<< HEAD
        Icon = TyanitiaIcons.Get(layout == MapList.ListLayout.List ? "list" : "grid");
=======
        Icon = layout == MapList.ListLayout.List ? SkinManager.Instance.Skin.LayoutListButtonImage : SkinManager.Instance.Skin.LayoutGridButtonImage;
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

        MapList.Instance.UpdateLayout(layout);
    }
}
