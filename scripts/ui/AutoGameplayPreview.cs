using System;
using System.Linq;
using Godot;

/// <summary>Live 3D map preview using the game's mesh, skin and autoplay cursor path.</summary>
public partial class AutoGameplayPreview : Control
{
    private Map map;
    private Note[] notes = [];
    private Attempt attempt;
    private AutoplayHandler autoplay;
    private double playhead;
    private double frameTimer;
    private SubViewport gameViewport;
    private Camera3D camera;
    private MeshInstance3D grid;
    private MeshInstance3D guides;
    private MeshInstance3D cursor;
    private MultiMeshInstance3D noteInstances;
    private SkinProfile skin;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
        var output = new TextureRect
        {
            Name = "GameplayImage",
            ShowBehindParent = true,
            MouseFilter = MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered
        };
        AddChild(output);
        output.SetAnchorsPreset(LayoutPreset.FullRect);
        gameViewport = new SubViewport
        {
            Name = "GameplayViewport",
            Size = MobileUi.IsMobile ? new Vector2I(1280, 720) : new Vector2I(1920, 1080),
            OwnWorld3D = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled
        };
        AddChild(gameViewport);
        output.Texture = gameViewport.GetTexture();
        var world = new Node3D();
        gameViewport.AddChild(world);
        camera = new Camera3D { Position = new Vector3(0, 0, 3.76f), Fov = 70, Near = 0.05f, Far = 100, Current = true };
        world.AddChild(camera);
        grid = new MeshInstance3D { Mesh = new QuadMesh { Size = new Vector2(4, 4) } };
        world.AddChild(grid);
        guides = new MeshInstance3D { Mesh = new QuadMesh { Size = new Vector2(4.04f, 4.04f) }, Position = new Vector3(0, 0, -0.2f) };
        world.AddChild(guides);
        cursor = new MeshInstance3D { Mesh = new QuadMesh { Size = new Vector2(0.263f, 0.263f) }, Position = new Vector3(0, 0, 0.04f) };
        world.AddChild(cursor);
        noteInstances = new MultiMeshInstance3D
        {
            Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, UseColors = true }
        };
        world.AddChild(noteInstances);
        MapManager.Selected.ValueChanged += (_, _) => SelectMap(MapManager.Selected.Value);
        MapManager.MapsInitialized += _ => CallDeferred(nameof(SelectInitialMap));
        Lobby.Instance.ModifiersChanged += _ => RebuildAttempt();
        SkinManager.Instance.Loaded += ApplySkin;
        ApplySkin();
        SelectInitialMap();
    }

    public void SelectInitialMap() => SelectMap(MapManager.Selected.Value ?? (MapManager.Maps.Count > 0 ? MapManager.Maps[0] : null));

    private void SelectMap(Map selected)
    {
        if (selected == null || selected == map) return;
        map = selected;
        notes = selected.Notes ?? [];
        playhead = 0;
        RebuildAttempt();
    }

    private void RebuildAttempt()
    {
        if (map == null) return;
        var mods = Lobby.Modifiers.Where(mod => mod is not AutoplayModifier).ToList();
        mods.Add(new AutoplayModifier());
        attempt = new Attempt(map, 1, 0, new CameraLock(), mods);
        autoplay = new AutoplayHandler(attempt);
    }

    private static StandardMaterial3D TextureMaterial(Texture2D image) => new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        AlbedoTexture = image,
        DisableFog = true
    };

    private void ApplySkin(SkinProfile loaded = null)
    {
        skin = loaded ?? SkinManager.Instance?.Skin;
        if (skin == null || grid == null) return;
        grid.MaterialOverride = TextureMaterial(skin.GridImage);
        guides.MaterialOverride = TextureMaterial(skin.GridGuidesImage);
        cursor.MaterialOverride = TextureMaterial(skin.CursorImage);
        Mesh mesh = skin.NoteMesh?.Duplicate(true) as Mesh;
        if (mesh is not ArrayMesh arrayMesh || arrayMesh.GetSurfaceCount() == 0)
        {
            var fallback = new BoxMesh { Size = new Vector3(0.6f, 0.6f, 0.18f) };
            fallback.Material = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                VertexColorUseAsAlbedo = true
            };
            mesh = fallback;
        }
        else
        {
            for (int i = 0; i < arrayMesh.GetSurfaceCount(); i++)
            {
                var material = arrayMesh.SurfaceGetMaterial(i) as StandardMaterial3D;
                material = material?.Duplicate(true) as StandardMaterial3D ?? new StandardMaterial3D();
                material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
                material.DisableFog = true;
                material.VertexColorUseAsAlbedo = true;
                material.VertexColorIsSrgb = true;
                arrayMesh.SurfaceSetMaterial(i, material);
            }
        }
        noteInstances.Multimesh.InstanceCount = 0;
        noteInstances.Multimesh.Mesh = mesh;
    }

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree() || gameViewport == null) return;
        if (map == null && MapManager.Maps.Count > 0) SelectInitialMap();
        if (map != null && SoundManager.Map?.Name == map.Name && SoundManager.Song?.Playing == true)
            playhead = SoundManager.Song.GetPlaybackPosition() * 1000;
        else
            playhead += delta * 1000;
        if (map != null && map.Length > 0 && playhead > map.Length)
        {
            playhead %= map.Length;
            autoplay?.Reset(playhead);
        }
        frameTimer += delta;
        if (frameTimer < 1.0 / 60) return;
        frameTimer = 0;
        RenderGameplay();
        gameViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        QueueRedraw();
    }

    private void RenderGameplay()
    {
        var settings = SettingsManager.Instance?.Settings;
        if (settings == null || skin == null || noteInstances.Multimesh.Mesh == null) return;
        camera.Fov = (float)(double)settings.FoV;
        guides.Visible = settings.GridGuides;
        (cursor.Mesh as QuadMesh).Size = Vector2.One * (float)(Constants.CURSOR_SIZE * settings.CursorScale);
        cursor.Transparency = 1f - (float)(double)settings.CursorOpacity;
        Vector2 position = autoplay?.GetCursorPosition(playhead) ?? Vector2.Zero;
        cursor.Position = new Vector3(position.X, position.Y, 0.04f);
        cursor.Rotation = new Vector3(0, 0, (float)((double)settings.CursorRotation * playhead / 1000 * Mathf.Pi / 180));
        camera.Position = new Vector3(position.X, position.Y, 0) * (float)(double)settings.CameraParallax + new Vector3(0, 0, 3.76f);

        attempt.Progress = playhead;
        float approachTime = Mathf.Max(0.05f, (float)(double)settings.ApproachTime);
        int first = LowerBound((int)(playhead - (settings.Pushback ? Constants.HIT_WINDOW : 0)));
        int last = LowerBound((int)(playhead + approachTime * 1000));
        NoteRenderer.RenderToMultiMesh(noteInstances.Multimesh, playhead, notes, first, last, attempt, settings, skin, 512);
    }

    public override void _Draw()
    {
        Vector2 size = Size;
        if (size.X < 200 || size.Y < 200) return;
        var font = GetThemeDefaultFont();
        DrawRect(new Rect2(1, 1, size.X - 2, size.Y - 2), new Color("83909e"), false, 2);
        float progress = map == null || map.Length <= 0 ? 0 : (float)(playhead / map.Length);
        DrawRect(new Rect2(32, 34, size.X - 64, 5), new Color("1d242f"));
        DrawRect(new Rect2(32, 34, (size.X - 64) * progress, 5), new Color("00e0cc"));
        DrawString(font, new Vector2(32, 27), map?.PrettyTitle ?? "AUTOPLAY", HorizontalAlignment.Left, size.X - 240, 16, Colors.White);
        DrawString(font, new Vector2(size.X - 155, 27), $"{Util.String.FormatTime(playhead / 1000)} / {Util.String.FormatTime((map?.Length ?? 0) / 1000)}", HorizontalAlignment.Left, 140, 15, new Color("d1d6dd"));
        float left = size.X * 0.20f, right = size.X * 0.80f;
        float top = size.Y * 0.15f, bottom = size.Y * 0.82f;
        Color white = new("f2f0f6");
        float corner = 34;
        foreach (float x in new[] { left, right })
        foreach (float y in new[] { top, bottom })
        {
            float dx = x == left ? corner : -corner;
            float dy = y == top ? corner : -corner;
            DrawLine(new Vector2(x, y), new Vector2(x + dx, y), white, 4);
            DrawLine(new Vector2(x, y), new Vector2(x, y + dy), white, 4);
        }
        int passed = LowerBound((int)playhead);
        DrawString(font, new Vector2(30, size.Y * 0.49f), $"{passed} NOTES", HorizontalAlignment.Left, 160, 19, white);
        DrawString(font, new Vector2(size.X - 170, size.Y * 0.49f), "100.00%", HorizontalAlignment.Left, 150, 22, white);
        DrawRect(new Rect2(left + 14, size.Y - 42, right - left - 28, 7), new Color("15202b"));
        DrawRect(new Rect2(left + 14, size.Y - 42, (right - left - 28) * 0.82f, 7), new Color("00e0cc"));
        DrawRect(new Rect2(right - 0.18f * (right - left) - 14, size.Y - 42, 0.18f * (right - left), 7), new Color("ff2eb8"));
    }

    private int LowerBound(int milliseconds)
    {
        int left = 0, right = notes.Length;
        while (left < right)
        {
            int mid = (left + right) / 2;
            if (notes[mid].Millisecond < milliseconds) left = mid + 1;
            else right = mid;
        }
        return left;
    }
}
