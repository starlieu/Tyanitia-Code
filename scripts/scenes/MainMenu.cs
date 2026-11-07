using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MainMenu : BaseScene
{
    public Panel CurrentMenu;
    public Panel LastMenu;

    public Panel HomeMenu;
    public Panel PlayMenu;
    public Panel TopBar;
    public JukeboxPanel Jukebox;
    public MapList MapList;
    public MapInfo MapInfo;
    public OnlineMapBrowser OnlineMapBrowser;
    public MapEditorPanel MapEditor;

    private Panel menuHolder;
    private Node topBarButtonsContainer;
    private HBoxContainer playTools;
    private Button mapBrowserButton;

    public override void _Ready()
    {
        base._Ready();

        menuHolder = GetNode<Panel>("Menus");

        HomeMenu = menuHolder.GetNode<Panel>("Home");
        PlayMenu = menuHolder.GetNode<Panel>("Play");
        LastMenu = HomeMenu;

        TopBar = GetNode<Panel>("TopBar");
        topBarButtonsContainer = TopBar.GetNode("MenuButtons");
        var extras = menuHolder.GetNode<Panel>("Extras");
        extras.Visible = false;
        HomeMenu.GetNode<Button>("Buttons/MarginContainer3/Extras").Visible = false;
        var editorButton = topBarButtonsContainer.GetNode<Button>("Extras");
        editorButton.Name = "MapEditor";
        editorButton.Text = "Map Editor";
        MapEditor = new MapEditorPanel { Name = "MapEditor", Visible = false };
        menuHolder.AddChild(MapEditor);
        MapEditor.AnchorLeft = 0;
        MapEditor.AnchorTop = 0;
        MapEditor.AnchorRight = 1;
        MapEditor.AnchorBottom = 1;
        MapEditor.OffsetLeft = 0;
        MapEditor.OffsetTop = 0;
        MapEditor.OffsetRight = 0;
        MapEditor.OffsetBottom = 0;
        editorButton.Pressed += () => Transition(MapEditor);
        mapBrowserButton = new Button
        {
            Name = "MapBrowser",
            Text = "Map Browser",
            Flat = true,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand,
            Theme = editorButton.Theme,
        };
        mapBrowserButton.AddThemeFontSizeOverride("font_size", 18);
        topBarButtonsContainer.AddChild(mapBrowserButton);
        mapBrowserButton.Pressed += () => { Transition(PlayMenu); OnlineMapBrowser.Open(); };
        Jukebox = GetNode<JukeboxPanel>("JukeboxPanel");
        MapList = PlayMenu.GetNode<MapList>("MapList");
        MapInfo = PlayMenu.GetNode<MapInfo>("MapInfo");

        FileDialog importDialog = PlayMenu.GetNode<Button>("SubTopBar/HBoxContainer/Import").GetNode<FileDialog>("ImportDialog");
        OnlineMapBrowser = new OnlineMapBrowser
        {
            Name = "OnlineMapBrowser",
            Visible = false,
            ZIndex = 10,
            ImportDialog = importDialog,
        };
        PlayMenu.AddChild(OnlineMapBrowser);
        OnlineMapBrowser.AnchorLeft = 0;
        OnlineMapBrowser.AnchorTop = 0;
        OnlineMapBrowser.AnchorRight = 1;
        OnlineMapBrowser.AnchorBottom = 1;
        OnlineMapBrowser.OffsetLeft = 0;
        OnlineMapBrowser.OffsetTop = 0;
        OnlineMapBrowser.OffsetRight = 0;
        OnlineMapBrowser.OffsetBottom = 0;
        var onlineMapsButton = new Button
        {
            Name = "OnlineMaps",
            Text = "Online Maps",
            TooltipText = "Browse and download maps",
            CustomMinimumSize = new Vector2(0, 36),
            Theme = ResourceLoader.Load<Theme>("res://themes/theme.tres"),
            MouseDefaultCursorShape = Control.CursorShape.PointingHand,
        };
        playTools = PlayMenu.GetNode<HBoxContainer>("SubTopBar/HBoxContainer");
        playTools.AddChild(onlineMapsButton);
        playTools.MoveChild(onlineMapsButton, 3);
        onlineMapsButton.Pressed += OnlineMapBrowser.Open;

        CurrentMenu = HomeMenu;

        Input.MouseMode = SettingsManager.Instance.Settings.UseCursorInMenus ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;

        List<Node> menuButtons = [];
        menuButtons.AddRange(topBarButtonsContainer.GetChildren());

        foreach (var container in HomeMenu.GetNode("Buttons").GetChildren())
        {
            menuButtons.Add(container.GetChild<Button>(0));
        }

        foreach (Button button in menuButtons.Cast<Button>())
        {
            if (button == editorButton || button == mapBrowserButton)
                continue;
            var menu = (Panel)menuHolder.FindChild(button.Name, false);

            if (menu != null)
            {
                button.Pressed += () => Transition(menu);
            }
        }

        // Scene-authored button sizes are retained.

        if (MobileUi.IsMobile)
        {
            TopBar.CustomMinimumSize = new Vector2(0, 88);
            TopBar.OffsetBottom = 88;
            TopBar.GetNode<Control>("Logo").CustomMinimumSize = new Vector2(64, 64);
            TopBar.GetNode<Control>("Version").AddThemeFontSizeOverride("normal_font_size", 22);
            TopBar.GetNode<Control>("Version").OffsetLeft = 96;

            MobileUi.EnlargeTouchTargets(HomeMenu.GetNode("Buttons"));
            MobileUi.EnlargeTouchTargets(topBarButtonsContainer);
            MobileUi.EnlargeTouchTargets(PlayMenu.GetNode("SubTopBar"));
            MobileUi.EnlargeTouchTargets(MapEditor);

            var subBar = PlayMenu.GetNode<Panel>("SubTopBar");
            subBar.OffsetTop = 88;
            subBar.OffsetBottom = 170;
            MapList.OffsetTop = 178;
            MapInfo.OffsetTop = 178;
            MapList.SquareButtonMinimumSize = 240;

        }

        // Keep the card at a readable size and scroll it when the window is too short.
        // The modifier row must never be hidden behind the leaderboard.
        var selectedScroll = new ScrollContainer
        {
            Name = "SelectedMapScroll",
            AnchorLeft = MapInfo.AnchorLeft,
            AnchorTop = MapInfo.AnchorTop,
            AnchorRight = MapInfo.AnchorRight,
            AnchorBottom = MapInfo.AnchorBottom,
            OffsetLeft = MapInfo.OffsetLeft,
            OffsetTop = MapInfo.OffsetTop,
            OffsetRight = MapInfo.OffsetRight,
            OffsetBottom = MapInfo.OffsetBottom,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
        };
        PlayMenu.AddChild(selectedScroll);
        MapInfo.Reparent(selectedScroll);
        MapInfo.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        MapInfo.OffsetLeft = 0;
        MapInfo.OffsetTop = 0;
        MapInfo.OffsetRight = 0;
        MapInfo.OffsetBottom = 0;
        MapInfo.CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 970 : 850);
        MapInfo.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        selectedScroll.GetVScrollBar().CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 48 : 24, 0);
        void updateMapInfoRatio()
        {
            float cardHeight = MobileUi.IsMobile ? 940 : 830;
            MapInfo.Ratio = Mathf.Clamp((selectedScroll.Size.X - selectedScroll.GetVScrollBar().CustomMinimumSize.X - 16) / cardHeight, 0.36f, 1.2f);
        }
        selectedScroll.Resized += updateMapInfoRatio;
        Callable.From(updateMapInfoRatio).CallDeferred();

        ApplyMenuIcons();
        SkinManager.Instance.Loaded += _ => ApplyMenuIcons();

        var playSubBar = PlayMenu.GetNode<Panel>("SubTopBar");
        var subBarScroll = new ScrollContainer
        {
            Name = "ToolsScroll",
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled,
            MouseFilter = Control.MouseFilterEnum.Pass,
        };
        playSubBar.AddChild(subBarScroll);
        subBarScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        playTools.Reparent(subBarScroll);
        playTools.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        playTools.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        playTools.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        subBarScroll.GetHScrollBar().CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 20 : 10);
    }

    private void ApplyMenuIcons()
    {
        void setIcon(Button button, string icon, int width = 30)
        {
            button.Icon = TyanitiaIcons.Get(icon);
            button.AddThemeConstantOverride("icon_max_width", width);
        }

        setIcon(HomeMenu.GetNode<Button>("Buttons/MarginContainer/Play"), "play", 32);
        setIcon(HomeMenu.GetNode<Button>("Buttons/MarginContainer2/Settings"), "settings", 32);
        setIcon(HomeMenu.GetNode<Button>("Buttons/MarginContainer5/Quit"), "quit", 32);
        setIcon(topBarButtonsContainer.GetNode<Button>("Home"), "home", 24);
        setIcon(topBarButtonsContainer.GetNode<Button>("Play"), "play", 24);
        setIcon(topBarButtonsContainer.GetNode<Button>("MapEditor"), "grid", 24);
        setIcon(mapBrowserButton, "search", 24);

        foreach (var (name, icon) in new[] { ("GitHub", "repository"), ("Discord", "community") })
        {
            if (TopBar.GetNodeOrNull<TextureButton>($"LinkButtons/{name}") is not TextureButton button)
                continue;
            button.TextureNormal = TyanitiaIcons.Get(icon);
            button.TextureHover = button.TextureNormal;
            button.TexturePressed = button.TextureNormal;
        }

        var subBar = playTools;
        foreach (var (node, icon) in new[]
        {
            ("Settings", "settings"), ("UserFolder", "folder"),
            ("Import", "import"), ("OnlineMaps", "search"), ("Random", "random"),
            ("Filters", "filter"), ("Sort", "sort"),
            ("Layout", "grid"),
        })
            if (subBar.GetNodeOrNull<Button>(node) is Button button)
                setIcon(button, icon, 30);

        if (subBar.GetNodeOrNull<TextureRect>("SearchAuthor/TextureRect") is TextureRect authorIcon)
            authorIcon.Texture = TyanitiaIcons.Get("author");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            switch (mouseButton.ButtonIndex)
            {
                case MouseButton.Xbutton1:
                    Transition(HomeMenu);
                    break;
                case MouseButton.Xbutton2:
                    Transition(LastMenu);
                    break;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed)
        {
            switch (key.Keycode)
            {
                case Key.Escape:
                    Transition(HomeMenu);
                    break;
            }
        }
    }

    public override void Load()
    {
        base.Load();

        if (OS.GetName() != "Android")
        {
            DisplayServer.WindowSetVsyncMode(
                SettingsManager.Instance.Settings.VSyncMenus ? DisplayServer.VSyncMode.Adaptive : DisplayServer.VSyncMode.Disabled
            );
        }

        // Apply any map selection that was deferred while menu was off-tree (e.g. import from another scene)
        MapInfo.ApplyPendingSelection();

        MapInfo.InfoContainer?.Refresh();
        SceneManager.Space?.UpdateState(false);

        var map = MapManager.Selected.Value;

        if (map != null)
        {
            SceneManager.Space?.UpdateMap(map);
        }

        SoundManager.RefreshMenuMusicPlayback();

        if (SettingsManager.Instance.Settings.AutoplayJukebox.Value && !SoundManager.Song.Playing && SoundManager.Map != null)
        {
            SoundManager.PlayJukebox(SoundManager.JukeboxIndex);
        }
    }

    public override void _Process(double delta)
    {
        if (Tyanitia.Quitting && SoundManager.Song.VolumeDb > float.NegativeInfinity)
        {
            SoundManager.Song.VolumeDb = Mathf.Lerp(SoundManager.Song.VolumeDb, -80f, (float)delta * 2);
        }
    }

    public void Transition(Panel menu, bool instant = false)
    {
        if (CurrentMenu == menu)
        {
            return;
        }

        if (CurrentMenu == PlayMenu)
            OnlineMapBrowser.Visible = false;
        if (CurrentMenu == MapEditor)
            MapEditor.StopPlayback();
        if (menu == MapEditor)
            MapEditor.RefreshMaps();

        LastMenu = CurrentMenu;
        CurrentMenu = menu;

        topBarButtonsContainer.GetNode<Button>(new(LastMenu.Name)).Disabled = false;
        topBarButtonsContainer.GetNode<Button>(new(CurrentMenu.Name)).Disabled = true;

        double tweenTime = instant ? 0 : 0.15;

        Tween outTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        outTween.TweenProperty(LastMenu, "modulate", Color.Color8(255, 255, 255, 0), tweenTime);
        outTween.TweenCallback(
            Callable.From(() =>
            {
                LastMenu.Visible = false;
            })
        );

        CurrentMenu.Visible = true;

        Tween inTween = CreateTween().SetTrans(Tween.TransitionType.Quad);
        inTween.TweenProperty(CurrentMenu, "modulate", Color.Color8(255, 255, 255), tweenTime);

        SoundManager.RefreshMenuMusicPlayback();
    }
}
