using System;
using System.Globalization;
using System.IO;
<<<<<<< HEAD
using System.Linq;
=======
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
using Godot;

public partial class MapInfoContainer : Panel, ISkinnable
{
    /// <summary>
    /// Parsed map reference
    /// </summary>
    public Map Map;

    public Leaderboard Leaderboard = new();

    private readonly PackedScene leaderboardScoreTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/score_panel.tscn");

    // Info & main buttons

    [ExportCategory("Info")]
    [Export]
    private Control info;

    [Export]
    private ColorRect dim;

    [Export]
    private TextureRect coverBackground;

    [Export]
    private TextureRect cover;

    [Export]
    private Control infoSubholder;

    [Export]
    private RichTextLabel mainLabel;
    private string mainLabelFormat;

    [Export]
    private RichTextLabel extraLabel;
    private string extraLabelFormat;

    [Export]
    private LinkPopupButton artistLink;
    private string artistLinkFormat;

    [Export]
    private Button exportButton;

    [Export]
    private Button favoriteButton;

    [Export]
    private Button videoButton;

    [Export]
    private FileDialog videoDialog;

    [Export]
    private Button copyButton;

    [Export]
    private FileDialog copyDialog;

    [Export]
    private Button deleteButton;

    // Actions panel

    [ExportCategory("Actions")]
    [Export]
    private Control actions;

    [Export]
    private FlatPreview preview;

    // [Export]
    // private Panel previewHolder;

    // [Export]
    // private Panel modesHolder;

    // [Export]
    // private Panel modifiersHolder;

    [Export]
    private Control speedHolder;

    [Export]
    private HBoxContainer speedPresets;

    [Export]
    private Control playHolder;

    [Export]
    private Button startButton;

    // Leaderboard

    [ExportCategory("Leaderboards")]
    [Export]
    private Control leaderboard;

    [Export]
    private ScrollContainer lbScrollContainer;

    [Export]
    private VBoxContainer lbContainer;

    [Export]
    private Button lbExpand;

    [Export]
    private Button lbHide;

    // Misc

    private ShaderMaterial outlineMaterial;
<<<<<<< HEAD
    private Button compactFavorite;
    private Label compactTime;
    private HSlider compactSeek;
    private TextureRect selectionCover;
    private Control selectionCoverStage;
    private Label selectionTitle;
    private Label selectionSubtitle;
    private Label selectionStats;
=======
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

    public override void _Ready()
    {
        mainLabelFormat = mainLabel.Text;
        extraLabelFormat = extraLabel.Text;
        artistLinkFormat = artistLink.Text;
        outlineMaterial = info.GetNode<Panel>("Outline").Material as ShaderMaterial;

        exportButton.Pressed += () =>
        {
            string exportPath = $"{Constants.USER_FOLDER}/export/";
            string exportFilePath = Path.Combine(exportPath, $"{Map.Name}.phxm");

            _ = ToastNotification.Notify($"Exporting to {exportFilePath}", 1);
            MapParser.ExportEncode(Map);

            _ = ToastNotification.Notify($"Done! Opening export...", 0);

            OS.ShellShowInFileManager(exportFilePath);
        };

<<<<<<< HEAD
        favoriteButton.Pressed += ToggleFavorite;
=======
        favoriteButton.Pressed += () =>
        {
            Map.Favorite = !Map.Favorite;
            MapManager.Update(Map);

            var skin = SkinManager.Instance.Skin;

            favoriteButton.TooltipText = Map.Favorite ? "Unfavorite" : "Favorite";
            favoriteButton.Icon = Map.Favorite ? skin.UnfavoriteButtonImage : skin.FavoriteButtonImage;
        };
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

        //videoButton.Pressed += () =>
        //{
        //    videoDialog.Popup();
        //};

        //videoDialog.FileSelected += (file) =>
        //{
        //    MapManager.InsertVideo(Map, file);
        //};

        // copyButton.Pressed += () =>
        // {
        //     copyDialog.Popup();
        // };

        // copyDialog.FileSelected += (path) =>
        // {
        //     File.Copy(Map.FolderPath, path);
        //     _ = ToastNotification.Notify($"Copied to {path}");
        // };

        deleteButton.Pressed += () =>
        {
            MapManager.Delete(Map);
        };

        void updateOffset()
        {
            infoSubholder.OffsetLeft = coverBackground.Size.X + 8;
        }

        coverBackground.Connect("resized", Callable.From(updateOffset));

        Modulate = Color.Color8(255, 255, 255, 0);

        // Speed setup

        var speedSlider = speedHolder.GetNode<HSlider>("HSlider");
        var speedEdit = speedHolder.GetNode<LineEdit>("LineEdit");

        void displaySpeed(double speed)
        {
            if (!IsInstanceValid(speedSlider))
            {
                return;
            }

            speed *= 100;

            double rounded = Math.Round(speed * 10) / 10;

            if (Math.Abs(rounded - speed) <= Mathf.Epsilon)
            {
                speed = rounded;
            }

            speedSlider.SetValueNoSignal(speed);
            speedEdit.Text = speed.ToString(CultureInfo.InvariantCulture);
        }

        displaySpeed(Lobby.Speed);

        Lobby.Instance.SpeedChanged += displaySpeed;

        void applySpeed()
        {
            if (!double.TryParse(speedEdit.Text, CultureInfo.InvariantCulture, out double value))
            {
                value = 100;
            }

            value = Math.Clamp(value, 25, 1000) / 100;

            Lobby.SetSpeed(value);

            if (SoundManager.Map?.Name == Map.Name && SoundManager.Song.Playing)
            {
                SoundManager.Song.PitchScale = (float)Lobby.Speed;
            }
        }

        speedEdit.FocusExited += applySpeed;
        speedEdit.TextSubmitted += (_) =>
        {
            applySpeed();
        };
        speedSlider.ValueChanged += (value) =>
        {
            speedEdit.Text = value.ToString(CultureInfo.InvariantCulture);
            applySpeed();
        };

        // StartFrom setup

        var startFromSlider = playHolder.GetNode<HSlider>("HSlider");
        var startFromEdit = playHolder.GetNode<LineEdit>("LineEdit");

        void displayStartFrom(double startFrom)
        {
            if (!IsInstanceValid(startFromSlider))
            {
                return;
            }

            int length = Map != null ? Map.Length : 1;

            startFromSlider.SetValueNoSignal(startFrom / length);
            startFromEdit.Text = Util.String.FormatTime(startFrom / 1000);
            startButton.Text = $"START{(startFrom > 0 ? $" ({startFromEdit.Text})" : "")}";
        }

        Lobby.Instance.StartFromChanged += displayStartFrom;

        void applyStartFrom(string input = null, bool seek = true)
        {
            input ??= startFromEdit.Text == "" ? startFromEdit.PlaceholderText : startFromEdit.Text;

            double value = 0;
            string[] split = input.Split(":");

            split.Reverse();

            if (split.Length > 1 && split[1].IsValidFloat())
            {
                value += 60 * split[1].ToFloat();
            }

            if (double.TryParse(split[0], CultureInfo.InvariantCulture, out double inputValue))
            {
                if (inputValue < 1)
                {
                    inputValue *= Map.Length / 1000;
                }

                value += inputValue;
            }

            value = Math.Clamp(value * 1000, 0, Map.Length);

            Lobby.SetStartFrom(value);

            if (SoundManager.Map?.Name != Map.Name)
            {
                SoundManager.StartMapSelectionPlayback(Map);
            }

            if (seek && SoundManager.Song.Playing)
            {
                preview.Seek(Lobby.StartFrom);
                SoundManager.Song.Seek((float)Lobby.StartFrom / 1000);
            }
        }

        startFromEdit.FocusExited += () =>
        {
            applyStartFrom();
        };
        startFromEdit.TextSubmitted += (_) =>
        {
            applyStartFrom();
        };
        startFromSlider.ValueChanged += value =>
        {
            applyStartFrom((Math.Round(value * Map.Length) / 1000).ToString("F2", CultureInfo.InvariantCulture), false);
        };
        startFromSlider.DragEnded += changed =>
        {
            if (changed)
            {
                applyStartFrom();
            }
        };

        //

        startButton.Pressed += () =>
        {
            Game.Play(Map, Lobby.Speed, Lobby.StartFrom, Lobby.CameraMode, Lobby.Modifiers);
        };

        // Leaderboard

        var lbExpandHover = lbExpand.GetNode<Panel>("Hover");

        void tweenExpandHover(bool show)
        {
            CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Quart)
                .TweenProperty(lbExpandHover, "modulate", Color.Color8(255, 255, 255, (byte)(show ? 255 : 0)), 0.25);
        }

        lbExpand.MouseEntered += () =>
        {
            tweenExpandHover(true);
        };
        lbExpand.MouseExited += () =>
        {
            tweenExpandHover(false);
        };
        lbExpand.Pressed += () =>
        {
            toggleLeaderboard(true);
        };
        lbHide.Pressed += () =>
        {
            toggleLeaderboard(false);
        };

        //

        SkinManager.Instance.Loaded += UpdateSkin;

        UpdateSkin();
<<<<<<< HEAD
        BuildSelectionCard();
        BuildCompactActions();
=======
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    }

    public override void _Process(double delta)
    {
        outlineMaterial?.SetShaderParameter("cursor_position", GetViewport().GetMousePosition());
<<<<<<< HEAD
        if (selectionCoverStage != null && IsVisibleInTree())
            selectionCoverStage.Rotation = Mathf.Sin(Time.GetTicksMsec() * 0.00045f) * 0.048f;
=======
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            switch (mouseButton.ButtonIndex)
            {
                case MouseButton.Right:
                    toggleLeaderboard(false);
                    break;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            switch (mouseButton.ButtonIndex)
            {
                case MouseButton.Left:
                    toggleLeaderboard(false);
                    break;
            }
        }
    }

    public void Setup(Map map)
    {
        if (map == null)
            return;

        if (Name == map.Name)
        {
            return;
        }

        Map = map;
        Name = map.Name;

        // Transition

        Position = Vector2.Zero;
        Scale = Vector2.One;

        info.OffsetLeft -= 64;
        info.OffsetRight -= 64;
        actions.OffsetLeft -= 80;
        actions.OffsetRight -= 80;
        leaderboard.OffsetLeft -= 96;
        leaderboard.OffsetRight -= 96;

        Tween inTween = CreateTween().SetEase(Tween.EaseType.Out).SetParallel();
        inTween.SetTrans(Tween.TransitionType.Quint).TweenProperty(info, "offset_left", 0, 0.5);
        inTween.TweenProperty(info, "offset_right", 0, 0.5);
        inTween.SetTrans(Tween.TransitionType.Quart).TweenProperty(actions, "offset_left", 0, 0.6);
        inTween.TweenProperty(actions, "offset_right", 0, 0.6);
        inTween.SetTrans(Tween.TransitionType.Cubic).TweenProperty(leaderboard, "offset_left", 0, 0.7);
        inTween.TweenProperty(leaderboard, "offset_right", 0, 0.7);

        OffsetRight = 0;
        Position += Vector2.Left * 64;

        // Info

        var difficultyColor = Constants.DIFFICULTY_COLORS[Math.Clamp(map.Difficulty, 0, Constants.DIFFICULTY_COLORS.Length - 1)];

        mainLabel.Text = string.Format(
            CultureInfo.CurrentCulture,
            mainLabelFormat,
            Util.String.SanitizeBBCode(map.PrettyTitle),
            difficultyColor.ToHtml(),
            Util.String.SanitizeBBCode(map.DifficultyName),
            Util.String.SanitizeBBCode(map.PrettyMappers)
        );

        extraLabel.Text = string.Format(
            CultureInfo.CurrentCulture,
            extraLabelFormat,
            Util.String.FormatTime(map.Length / 1000),
            map.Notes.Length,
            Util.String.SanitizeBBCode(map.Name)
        );

        coverBackground.SelfModulate = difficultyColor;
        cover.Texture = map.Cover;
<<<<<<< HEAD
        if (selectionCover != null)
        {
            selectionCover.Texture = map.Cover;
            selectionTitle.Text = map.PrettyTitle;
            selectionSubtitle.Text = $"{map.DifficultyName}  ·  Mapped by {map.PrettyMappers}";
            selectionStats.Text = $"{map.Notes.Length:N0} NOTES     •     {Util.String.FormatTime(map.Length / 1000)} DURATION";
        }
        favoriteButton.TooltipText = map.Favorite ? "Unfavorite" : "Favorite";
        favoriteButton.Icon = map.Favorite ? SkinManager.Instance.Skin.UnfavoriteButtonImage : SkinManager.Instance.Skin.FavoriteButtonImage;
        UpdateCompactFavorite();
        if (compactTime != null)
            compactTime.Text = $"0:00  /  {Util.String.FormatTime(map.Length / 1000)}";
        if (compactSeek != null)
            compactSeek.SetValueNoSignal(0);
=======
        favoriteButton.TooltipText = map.Favorite ? "Unfavorite" : "Favorite";
        favoriteButton.Icon = map.Favorite ? SkinManager.Instance.Skin.UnfavoriteButtonImage : SkinManager.Instance.Skin.FavoriteButtonImage;
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

        artistLink.Visible = map.ArtistLink != "";
        artistLink.Text = string.Format(CultureInfo.CurrentCulture, artistLinkFormat, map.ArtistPlatform);

        artistLink.UpdateLink(map.ArtistLink);

        // Actions

        Lobby.SetStartFrom(0);

        LoadLeaderboard(map);

        preview.Setup(map, true);
    }

    public void Refresh()
    {
        Setup(Map);
        LoadLeaderboard(Map);
    }

    public void LoadLeaderboard(Map map)
    {
        // Leaderboard

        Leaderboard = new();

        if (File.Exists($"{Constants.USER_FOLDER}/pbs/{map.Name}"))
        {
            Leaderboard = new(map.Name, $"{Constants.USER_FOLDER}/pbs/{map.Name}");
        }

        leaderboard.Visible = Leaderboard.Valid && Leaderboard.ScoreCount > 0;

        foreach (Node child in lbContainer.GetChildren())
        {
            lbContainer.RemoveChild(child);
            child.QueueFree();
        }

        for (int i = 0; i < Math.Min(8, Leaderboard.ScoreCount); i++)
        {
            ScorePanel panel = leaderboardScoreTemplate.Instantiate<ScorePanel>();

            lbContainer.AddChild(panel);
            panel.Setup(Leaderboard.Scores[i]);
            panel.GetNode<ColorRect>("Background").Color = Color.Color8(255, 255, 255, (byte)(i % 2 * 8));

            panel.Button.Pressed += () =>
            {
                toggleLeaderboard(false);
            };
        }
    }

    public Tween Transition(bool show)
    {
        Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic).SetParallel();
        float time = show ? 0.4f : 0.3f;

        PivotOffset = Size / 2;

        tween.TweenProperty(this, "modulate", Color.Color8(255, 255, 255, (byte)(show ? 255 : 0)), time);
        tween.TweenProperty(this, "position", show ? Vector2.Zero : Vector2.Down * 24, time);
        tween.TweenProperty(this, "scale", Vector2.One * (show ? 1f : 0.9f), time);
        tween.Chain();

        return tween;
    }

    public void UpdateSkin(SkinProfile skin = null)
    {
        skin ??= SkinManager.Instance.Skin;

        coverBackground.Texture = skin.MapInfoCoverBackgroundImage;
        speedPresets.GetNode("MinusMinus").GetNode<SpeedPresetButton>("SpeedPresetButton").Icon = skin.SpeedPresetMinusMinusButtonImage;
        speedPresets.GetNode("Minus").GetNode<SpeedPresetButton>("SpeedPresetButton").Icon = skin.SpeedPresetMinusButtonImage;
        speedPresets.GetNode("Middle").GetNode<SpeedPresetButton>("SpeedPresetButton").Icon = skin.SpeedPresetMiddleButtonImage;
        speedPresets.GetNode("Plus").GetNode<SpeedPresetButton>("SpeedPresetButton").Icon = skin.SpeedPresetPlusButtonImage;
        speedPresets.GetNode("PlusPlus").GetNode<SpeedPresetButton>("SpeedPresetButton").Icon = skin.SpeedPresetPlusPlusButtonImage;
    }

<<<<<<< HEAD
    private void UpdateCompactFavorite()
    {
        if (compactFavorite == null || Map == null)
            return;
        compactFavorite.Text = "";
        compactFavorite.Icon = TyanitiaIcons.Get(Map.Favorite ? "bookmark_filled" : "bookmark");
        compactFavorite.TooltipText = Map.Favorite ? "Unpin map" : "Pin map to top";
        compactFavorite.Modulate = Map.Favorite ? new Color("ff2eb8") : Colors.White;
    }

    private void BuildSelectionCard()
    {
        info.GetNode<ScrollContainer>("ScrollContainer").Visible = false;
        info.AnchorBottom = MobileUi.IsMobile ? 0.56f : 0.58f;
        actions.OffsetBottom = -74;
        leaderboard.OffsetTop = -68;
        var card = new VBoxContainer { Name = "SelectedMapCard" };
        card.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        card.OffsetLeft = 28;
        card.OffsetTop = 12;
        card.OffsetRight = -28;
        card.OffsetBottom = -10;
        card.AddThemeConstantOverride("separation", 8);
        info.AddChild(card);
        var eyebrow = new Label { Text = "SELECTED MAP", HorizontalAlignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2(0, 22) };
        eyebrow.AddThemeFontSizeOverride("font_size", 15);
        eyebrow.AddThemeColorOverride("font_color", new Color("76e9df"));
        card.AddChild(eyebrow);
        selectionCoverStage = new Control { CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 310 : 278), SizeFlagsVertical = Control.SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
        card.AddChild(selectionCoverStage);
        selectionCoverStage.Resized += () => selectionCoverStage.PivotOffset = selectionCoverStage.Size / 2;
        var coverShadow = new Panel();
        coverShadow.SetAnchorsPreset(Control.LayoutPreset.Center);
        coverShadow.OffsetLeft = -145;
        coverShadow.OffsetTop = -145;
        coverShadow.OffsetRight = 145;
        coverShadow.OffsetBottom = 145;
        coverShadow.MouseFilter = MouseFilterEnum.Ignore;
        coverShadow.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("09141c"), BorderColor = new Color("f7f7ff"),
            BorderWidthLeft = 3, BorderWidthTop = 3, BorderWidthRight = 3, BorderWidthBottom = 3,
            CornerRadiusTopLeft = 14, CornerRadiusTopRight = 14, CornerRadiusBottomLeft = 14, CornerRadiusBottomRight = 14,
            ShadowColor = new Color(0.85f, 0.92f, 1f, 0.58f), ShadowSize = 20
        });
        selectionCoverStage.AddChild(coverShadow);
        selectionCover = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        selectionCoverStage.AddChild(selectionCover);
        selectionCover.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        selectionCover.OffsetLeft = 5;
        selectionCover.OffsetTop = 5;
        selectionCover.OffsetRight = -5;
        selectionCover.OffsetBottom = -5;
        var coverOutline = new Panel { MouseFilter = MouseFilterEnum.Ignore };
        selectionCoverStage.AddChild(coverOutline);
        coverOutline.SetAnchorsPreset(Control.LayoutPreset.Center);
        coverOutline.OffsetLeft = -145;
        coverOutline.OffsetTop = -145;
        coverOutline.OffsetRight = 145;
        coverOutline.OffsetBottom = 145;
        coverOutline.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            DrawCenter = false,
            BorderColor = new Color("f7f8ff"),
            BorderWidthLeft = 4, BorderWidthTop = 4, BorderWidthRight = 4, BorderWidthBottom = 4,
            CornerRadiusTopLeft = 14, CornerRadiusTopRight = 14, CornerRadiusBottomLeft = 14, CornerRadiusBottomRight = 14,
            ShadowColor = new Color(0.92f, 0.94f, 1f, 0.8f), ShadowSize = 14
        });
        selectionCoverStage.Resized += () =>
        {
            float halfSide = Mathf.Min(156, Mathf.Min(selectionCoverStage.Size.X - 12, selectionCoverStage.Size.Y - 12) / 2);
            foreach (Panel frame in new[] { coverShadow, coverOutline })
            {
                frame.OffsetLeft = -halfSide;
                frame.OffsetRight = halfSide;
                frame.OffsetTop = -halfSide;
                frame.OffsetBottom = halfSide;
            }
        };
        selectionTitle = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(0, 60) };
        selectionTitle.AddThemeFontSizeOverride("font_size", 28);
        card.AddChild(selectionTitle);
        selectionSubtitle = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        selectionSubtitle.AddThemeFontSizeOverride("font_size", 19);
        selectionSubtitle.AddThemeColorOverride("font_color", new Color("e4b9d9"));
        card.AddChild(selectionSubtitle);
        selectionStats = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        selectionStats.AddThemeFontSizeOverride("font_size", 18);
        selectionStats.AddThemeColorOverride("font_color", new Color("9be7e2"));
        card.AddChild(selectionStats);
        info.GetNode<Panel>("Background").AddThemeStyleboxOverride("panel", MakePanelStyle());
        actions.AnchorTop = MobileUi.IsMobile ? 0.56f : 0.58f;
    }

    private void ToggleFavorite()
    {
        if (Map == null) return;
        Map.Favorite = !Map.Favorite;
        MapManager.Update(Map);
        var skin = SkinManager.Instance.Skin;
        favoriteButton.TooltipText = Map.Favorite ? "Unfavorite" : "Favorite";
        favoriteButton.Icon = Map.Favorite ? skin.UnfavoriteButtonImage : skin.FavoriteButtonImage;
        UpdateCompactFavorite();
    }

    private void BuildCompactActions()
    {
        // Preserve the original controls for map playback and keyboard shortcuts,
        // while presenting the selected map's primary actions in one place.
        actions.GetNode<ScrollContainer>("ScrollContainer").Visible = false;
        var panel = new PanelContainer { Name = "SelectionActions" };
        panel.AddThemeStyleboxOverride("panel", MakePanelStyle());
        panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        panel.OffsetLeft = 12;
        panel.OffsetTop = 5;
        panel.OffsetRight = -12;
        panel.OffsetBottom = -5;
        actions.AddChild(panel);

        var stack = new VBoxContainer();
        stack.AddThemeConstantOverride("separation", 6);
        panel.AddChild(stack);

        var topRow = new HBoxContainer();
        topRow.AddThemeConstantOverride("separation", 12);
        stack.AddChild(topRow);

        var miniPreviewFrame = new Panel
        {
            Name = "MiniMapPreview",
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 128 : 106, MobileUi.IsMobile ? 128 : 106),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        miniPreviewFrame.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("111b28"), BorderColor = new Color("78c9c8"),
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12, CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
        });
        topRow.AddChild(miniPreviewFrame);
        preview.Reparent(miniPreviewFrame);
        preview.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        preview.OffsetLeft = 8;
        preview.OffsetTop = 8;
        preview.OffsetRight = -8;
        preview.OffsetBottom = -8;
        preview.MouseFilter = MouseFilterEnum.Ignore;
        preview.AddThemeStyleboxOverride("panel", new StyleBoxEmpty());

        var controlColumn = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        controlColumn.AddThemeConstantOverride("separation", 4);
        topRow.AddChild(controlColumn);

        var startLabel = new Label { Text = "START POSITION", CustomMinimumSize = new Vector2(0, 18) };
        startLabel.AddThemeFontSizeOverride("font_size", 15);
        startLabel.AddThemeColorOverride("font_color", new Color("9be7e2"));
        controlColumn.AddChild(startLabel);

        var seekRow = new HBoxContainer();
        seekRow.AddThemeConstantOverride("separation", 10);
        controlColumn.AddChild(seekRow);
        compactTime = new Label { Text = "0:00", CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 112 : 100, MobileUi.IsMobile ? 60 : 34), VerticalAlignment = VerticalAlignment.Center };
        seekRow.AddChild(compactTime);
        compactSeek = new HSlider { MinValue = 0, MaxValue = 1, Step = 0.001, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(80, MobileUi.IsMobile ? 60 : 34) };
        seekRow.AddChild(compactSeek);
        bool draggingSeek = false;
        var seekCommitTimer = new Timer { OneShot = true, WaitTime = 0.12 };
        AddChild(seekCommitTimer);
        void commitSeek()
        {
            if (Map == null) return;
            if (SoundManager.Map?.Name != Map.Name)
                SoundManager.StartMapSelectionPlayback(Map);
            preview.Seek(Lobby.StartFrom);
            if (SoundManager.Song?.Playing == true)
                SoundManager.Song.Seek((float)Lobby.StartFrom / 1000);
        }
        seekCommitTimer.Timeout += commitSeek;
        compactSeek.DragStarted += () => { draggingSeek = true; seekCommitTimer.Stop(); };
        compactSeek.DragEnded += changed =>
        {
            draggingSeek = false;
            if (changed) commitSeek();
        };
        compactSeek.ValueChanged += value =>
        {
            if (Map == null) return;
            Lobby.SetStartFrom(value * Map.Length);
            compactTime.Text = $"{Util.String.FormatTime(Lobby.StartFrom / 1000)}  /  {Util.String.FormatTime(Map.Length / 1000)}";
            if (!draggingSeek) seekCommitTimer.Start();
        };
        Lobby.Instance.StartFromChanged += startFrom =>
        {
            if (Map != null && IsInstanceValid(compactSeek))
                compactSeek.SetValueNoSignal(Map.Length > 0 ? startFrom / Map.Length : 0);
        };

        var actionRow = new HBoxContainer();
        actionRow.AddThemeConstantOverride("separation", 10);
        controlColumn.AddChild(actionRow);
        var bot = new Button { Icon = TyanitiaIcons.Get("autoplay"), TooltipText = "Watch autoplay", CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 76 : 52, MobileUi.IsMobile ? 72 : 54), IconAlignment = HorizontalAlignment.Center, ExpandIcon = true };
        StyleActionButton(bot, new Color("00e0cc"));
        bot.AddThemeFontSizeOverride("font_size", 30);
        bot.Pressed += () =>
        {
            if (Map == null) return;
            var mods = Lobby.Modifiers.Where(m => m is not AutoplayModifier).ToList();
            mods.Add(new AutoplayModifier());
            Game.Play(Map, Lobby.Speed, Lobby.StartFrom, Lobby.CameraMode, mods);
        };
        actionRow.AddChild(bot);
        var play = new Button { Text = "PLAY", Icon = TyanitiaIcons.Get("play"), TooltipText = "Start playing this map", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 140 : 100, MobileUi.IsMobile ? 72 : 54) };
        play.AddThemeConstantOverride("icon_max_width", 30);
        StyleActionButton(play, new Color("00e0cc"));
        play.AddThemeFontSizeOverride("font_size", MobileUi.IsMobile ? 26 : 22);
        play.Pressed += () => { if (Map != null) Game.Play(Map, Lobby.Speed, Lobby.StartFrom, Lobby.CameraMode, Lobby.Modifiers); };
        actionRow.AddChild(play);
        compactFavorite = new Button { Icon = TyanitiaIcons.Get("bookmark"), CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 76 : 52, MobileUi.IsMobile ? 72 : 54), IconAlignment = HorizontalAlignment.Center, ExpandIcon = true };
        StyleActionButton(compactFavorite, new Color("ff2eb8"));
        compactFavorite.AddThemeFontSizeOverride("font_size", 34);
        compactFavorite.Pressed += ToggleFavorite;
        actionRow.AddChild(compactFavorite);

        var modsLabel = new Label { Text = "SPEED  ·  CAMERA  ·  MODS", CustomMinimumSize = new Vector2(0, 18) };
        modsLabel.AddThemeFontSizeOverride("font_size", 15);
        modsLabel.AddThemeColorOverride("font_color", new Color("e8b7dc"));
        stack.AddChild(modsLabel);
        var modRow = new HBoxContainer();
        modRow.AddThemeConstantOverride("separation", 8);
        stack.AddChild(modRow);
        var speed = new OptionButton { CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 128 : 92, MobileUi.IsMobile ? 72 : 54), TooltipText = "Map speed" };
        speed.Alignment = HorizontalAlignment.Center;
        StyleActionButton(speed, new Color("00e0cc"));
        var speeds = new[] { 0.75, 0.8, 0.87, 1.0, 1.15, 1.25, 1.35, 1.45 };
        foreach (double value in speeds) speed.AddItem($"{value:0.##}×");
        speed.Selected = Array.FindIndex(speeds, x => Math.Abs(x - Lobby.Speed) < 0.0001);
        speed.ItemSelected += index =>
        {
            Lobby.SetSpeed(speeds[(int)index]);
            if (Map != null && SoundManager.Map?.Name == Map.Name && SoundManager.Song.Playing)
                SoundManager.Song.PitchScale = (float)Lobby.Speed;
        };
        Lobby.Instance.SpeedChanged += value =>
        {
            int index = Array.FindIndex(speeds, x => Math.Abs(x - value) < 0.0001);
            speed.Selected = index;
        };
        modRow.AddChild(speed);

        var cameraMode = new OptionButton { CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 128 : 100, MobileUi.IsMobile ? 72 : 54), TooltipText = "Camera: Lock or Spin" };
        cameraMode.Alignment = HorizontalAlignment.Center;
        StyleActionButton(cameraMode, new Color("b0b9ff"));
        foreach (var mode in Tyanitia.CameraModes)
            cameraMode.AddItem(mode.Name);
        void syncCameraMode(string modeName)
        {
            int index = Array.FindIndex(Tyanitia.CameraModes, mode => mode.Name == modeName);
            if (index >= 0) cameraMode.Selected = index;
        }
        syncCameraMode(Lobby.CameraMode.Name);
        cameraMode.ItemSelected += index => Lobby.SetCameraMode(Tyanitia.CameraModes[(int)index]);
        Lobby.Instance.CameraModeChanged += syncCameraMode;
        modRow.AddChild(cameraMode);

        var modPopup = new PopupPanel { Name = "ModifierPicker" };
        AddChild(modPopup);
        var popupMargin = new MarginContainer();
        popupMargin.AddThemeConstantOverride("margin_left", 16);
        popupMargin.AddThemeConstantOverride("margin_top", 16);
        popupMargin.AddThemeConstantOverride("margin_right", 16);
        popupMargin.AddThemeConstantOverride("margin_bottom", 16);
        modPopup.AddChild(popupMargin);
        popupMargin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        var popupStack = new VBoxContainer();
        popupStack.AddThemeConstantOverride("separation", 10);
        popupMargin.AddChild(popupStack);
        var popupTitle = new Label { Text = "MODIFIERS", HorizontalAlignment = HorizontalAlignment.Center };
        popupTitle.AddThemeFontSizeOverride("font_size", MobileUi.IsMobile ? 26 : 20);
        popupStack.AddChild(popupTitle);
        var modButtons = new GridContainer { Columns = 3 };
        modButtons.AddThemeConstantOverride("h_separation", 8);
        modButtons.AddThemeConstantOverride("v_separation", 8);
        popupStack.AddChild(modButtons);
        var modsButton = new Button { Text = "MODS", Icon = TyanitiaIcons.Get("filter"), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(100, MobileUi.IsMobile ? 72 : 54), TooltipText = "Choose modifiers" };
        modsButton.AddThemeConstantOverride("icon_max_width", 28);
        StyleActionButton(modsButton, new Color("e99dd7"));
        modsButton.Pressed += () => modPopup.PopupCentered(new Vector2I(MobileUi.IsMobile ? 520 : 460, MobileUi.IsMobile ? 350 : 300));
        modRow.AddChild(modsButton);

        var orderedMods = Tyanitia.Modifiers.OrderBy(m => m is NoFailModifier ? 0 : m is HardRockModifier ? 1 : m is GhostModifier ? 2 : 3);
        var buttons = new System.Collections.Generic.List<(Modifier mod, Button button)>();
        foreach (var mod in orderedMods)
        {
            if (mod is AutoplayModifier) continue;
            var button = new Button { Text = mod.Name, Icon = TyanitiaIcons.ForMod(mod.Name), TooltipText = mod.Name + " · tap to toggle", ToggleMode = true, CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 146 : 130, MobileUi.IsMobile ? 72 : 60) };
            button.AddThemeConstantOverride("icon_max_width", MobileUi.IsMobile ? 36 : 30);
            StyleActionButton(button, mod.Color);
            button.Pressed += () => Lobby.SetModifier(mod, !Lobby.Modifiers.Any(m => m.Name == mod.Name));
            modButtons.AddChild(button);
            buttons.Add((mod, button));
        }
        void updateMods(string[] active)
        {
            foreach (var (mod, button) in buttons)
            {
                button.ButtonPressed = active.Contains(mod.Name);
                button.Modulate = active.Contains(mod.Name) ? mod.Color : Colors.White;
            }
            modsButton.Text = active.Length == 0 ? "MODS" : $"MODS ({active.Length})";
        }
        Lobby.Instance.ModifiersChanged += updateMods;
        updateMods(Lobby.Modifiers.Select(m => m.Name).ToArray());
        UpdateCompactFavorite();
    }

    private static StyleBoxFlat MakePanelStyle() => new()
    {
        BgColor = new Color("0a111b"),
        BorderColor = new Color("328c8f"),
        BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
        CornerRadiusTopLeft = 22, CornerRadiusTopRight = 22,
        CornerRadiusBottomLeft = 22, CornerRadiusBottomRight = 22,
        ContentMarginLeft = 18, ContentMarginTop = 14, ContentMarginRight = 18, ContentMarginBottom = 14,
        ShadowColor = new Color(0, 0, 0, 0.3f), ShadowSize = 12
    };

    private static void StyleActionButton(Button button, Color accent)
    {
        StyleBoxFlat normal = new()
        {
            BgColor = new Color("172330"), BorderColor = accent.Darkened(0.35f),
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 16, CornerRadiusTopRight = 16,
            CornerRadiusBottomLeft = 16, CornerRadiusBottomRight = 16
        };
        StyleBoxFlat hover = (StyleBoxFlat)normal.Duplicate();
        hover.BgColor = new Color("294050");
        hover.BorderColor = accent;
        StyleBoxFlat pressed = (StyleBoxFlat)hover.Duplicate();
        pressed.BgColor = new Color("2e4250");
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("hover_pressed", pressed);
    }

=======
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    private void toggleLeaderboard(bool show)
    {
        lbExpand.Visible = !show;
        lbHide.Visible = show;
        lbScrollContainer.VerticalScrollMode = show ? ScrollContainer.ScrollMode.Auto : ScrollContainer.ScrollMode.ShowNever;

        foreach (Node node in lbContainer.GetChildren())
        {
            if (node is ScorePanel panel)
            {
                panel.Button.Visible = show;
            }
        }

        Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart).SetParallel();

<<<<<<< HEAD
        tween.TweenProperty(leaderboard, "offset_top", show ? -100 * Math.Min(4, Leaderboard.ScoreCount) : -68, 0.25);
=======
        tween.TweenProperty(leaderboard, "offset_top", -100 * (show ? Math.Min(4, Leaderboard.ScoreCount) : 1), 0.25);
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
        tween.TweenProperty(dim, "color", Color.Color8(0, 0, 0, (byte)(show ? 128 : 0)), 0.25);

        if (!show)
        {
            tween.TweenProperty(lbScrollContainer, "scroll_vertical", 0, 0.15);
        }
    }
}
