using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

// Godot port of the Sound Space Quantum Editor workflow. The original archive is
// a Windows Forms/OpenTK application and cannot run inside a Godot canvas.
public partial class MapEditorPanel : Panel
{
    private readonly List<Note> notes = [];
    private Map source;
    private OptionButton mapPicker;
    private LineEdit artist, title, mapper;
    private SpinBox bpm, snap;
    private HSlider timeline;
    private Label position, status;
    private Button playButton;
    private EditorNoteCanvas field;
    private AudioStreamPlayer player;
    private byte[] draftAudio;
    private FileDialog audioDialog;
    private int lengthMs = 180000;
    private bool updating;
    private int selected = -1;
    public int CurrentMs => (int)timeline.Value;
    public IReadOnlyList<Note> Notes => notes;
    public int SelectedIndex => selected;
    public void StopPlayback() { player?.Stop(); if (playButton != null) playButton.Text = "▶ Play"; }

    public override void _Ready()
    {
        var bg = new StyleBoxFlat { BgColor = new Color("080e18"), BorderColor = new Color("294859"), BorderWidthTop = 1 };
        AddThemeStyleboxOverride("panel", bg);
        var margin = new MarginContainer();
        AddChild(margin);
        margin.SetAnchorsPreset(LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 28);
        margin.AddThemeConstantOverride("margin_right", 28);
        margin.AddThemeConstantOverride("margin_top", MobileUi.IsMobile ? 112 : 70);
        margin.AddThemeConstantOverride("margin_bottom", MobileUi.IsMobile ? 104 : 82);
        var scroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            FollowFocus = true,
        };
        margin.AddChild(scroll);
        scroll.GetVScrollBar().CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 40 : 20, 0);
        var root = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        root.AddThemeConstantOverride("separation", 12);
        scroll.AddChild(root);
        var heading = new Label { Text = "MAP EDITOR" };
        heading.AddThemeFontSizeOverride("font_size", MobileUi.IsMobile ? 32 : 26);
        root.AddChild(heading);
        var pickerRow = new HFlowContainer();
        pickerRow.AddThemeConstantOverride("separation", 10);
        root.AddChild(pickerRow);
        mapPicker = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 250 : 220, MobileUi.IsMobile ? 64 : 46) };
        pickerRow.AddChild(mapPicker);
        var refresh = Button("Refresh maps", pickerRow, RefreshMaps);
        Button("New map", pickerRow, NewMap);
        Button("Load audio", pickerRow, () => audioDialog.PopupCentered(new Vector2I(800, 550)));
        mapPicker.ItemSelected += index => LoadMap(MapManager.Maps[(int)mapPicker.GetItemId((int)index)]);
        var meta = new HBoxContainer();
        meta.AddThemeConstantOverride("separation", 10);
        root.AddChild(meta);
        artist = Edit("Artist", meta);
        title = Edit("Title", meta);
        mapper = Edit("Mapper", meta);
        var timing = new HBoxContainer();
        timing.AddThemeConstantOverride("separation", 10);
        root.AddChild(timing);
        timing.AddChild(new Label { Text = "BPM" });
        bpm = new SpinBox { MinValue = 20, MaxValue = 400, Value = 120, Step = 0.01, CustomMinimumSize = new Vector2(100, 0) };
        timing.AddChild(bpm);
        timing.AddChild(new Label { Text = "Snap" });
        snap = new SpinBox { MinValue = 1, MaxValue = 32, Value = 4, Step = 1, CustomMinimumSize = new Vector2(80, 0) };
        timing.AddChild(snap);
        Button("− Beat", timing, () => Seek(CurrentMs - BeatMs()));
        Button("+ Beat", timing, () => Seek(CurrentMs + BeatMs()));
        Button("Delete note", timing, DeleteSelected);
        field = new EditorNoteCanvas { SizeFlagsVertical = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(240, MobileUi.IsMobile ? 230 : 180) };
        field.Editor = this;
        root.AddChild(field);
        var timeRow = new HBoxContainer();
        timeRow.AddThemeConstantOverride("separation", 10);
        root.AddChild(timeRow);
        playButton = Button("▶ Play", timeRow, TogglePlay);
        timeline = new HSlider { SizeFlagsHorizontal = SizeFlags.ExpandFill, MaxValue = lengthMs, Step = 1, CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 64 : 36) };
        timeRow.AddChild(timeline);
        timeline.ValueChanged += value => { position.Text = $"{FormatTime((int)value)} / {FormatTime(lengthMs)}"; field.QueueRedraw(); if (!updating && player.Playing) player.Seek((float)value / 1000); };
        position = new Label { Text = "0:00 / 3:00", CustomMinimumSize = new Vector2(120, 0) };
        timeRow.AddChild(position);
        var bottom = new HBoxContainer();
        root.AddChild(bottom);
        status = new Label { Text = "Choose a map or create one · left click places notes · right click deletes", SizeFlagsHorizontal = SizeFlags.ExpandFill, ClipText = true };
        bottom.AddChild(status);
        Button("Save copy", bottom, SaveCopy);
        player = new AudioStreamPlayer();
        AddChild(player);
        audioDialog = new FileDialog { Access = FileDialog.AccessEnum.Filesystem, FileMode = FileDialog.FileModeEnum.OpenFile, UseNativeDialog = false };
        audioDialog.AddFilter("*.mp3,*.ogg", "Audio files");
        AddChild(audioDialog);
        audioDialog.FileSelected += LoadAudio;
        RefreshMaps();
    }

    private static Button Button(string text, Node parent, Action action)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 64 : 40) };
        parent.AddChild(button);
        button.Pressed += action;
        return button;
    }

    private static LineEdit Edit(string placeholder, Node parent)
    {
        var edit = new LineEdit { PlaceholderText = placeholder, SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(90, MobileUi.IsMobile ? 64 : 40) };
        parent.AddChild(edit);
        return edit;
    }

    public void RefreshMaps()
    {
        mapPicker.Clear();
        for (int i = 0; i < MapManager.Maps.Count; i++)
            mapPicker.AddItem(MapManager.Maps[i].PrettyTitle, i);
        if (MapManager.Maps.Count > 0) LoadMap(MapManager.Selected.Value ?? MapManager.Maps[0]);
    }

    private void LoadMap(Map map)
    {
        if (map == null) return;
        try
        {
            source = MapParser.Decode(map.FolderPath);
            draftAudio = null;
            notes.Clear();
            notes.AddRange(source.Notes.Select((n, i) => new Note(i, n.Millisecond, n.X, n.Y)));
            artist.Text = source.Artist;
            title.Text = source.Title;
            mapper.Text = source.Mappers.FirstOrDefault() ?? "";
            lengthMs = Math.Max(1000, source.Length);
            timeline.MaxValue = lengthMs;
            Seek(0);
            player.Stop();
            player.Stream = source.AudioBuffer?.Length > 4 ? Util.Audio.LoadStream(source.AudioBuffer) : null;
            playButton.Text = "▶ Play";
            selected = -1;
            status.Text = $"Loaded {notes.Count} notes · edits are saved as a separate map";
            field.QueueRedraw();
        }
        catch (Exception e) { status.Text = "Could not load map: " + e.Message; }
    }

    private void NewMap()
    {
        source = null;
        draftAudio = null;
        notes.Clear();
        artist.Text = "";
        title.Text = "New map";
        mapper.Text = "Tyan04k_a";
        lengthMs = 180000;
        timeline.MaxValue = lengthMs;
        Seek(0);
        player.Stop();
        player.Stream = null;
        selected = -1;
        status.Text = "New map · add notes, then save a copy";
        field.QueueRedraw();
    }

    private void LoadAudio(string path)
    {
        try
        {
            draftAudio = File.ReadAllBytes(path);
            player.Stop();
            player.Stream = Util.Audio.LoadStream(draftAudio);
            lengthMs = Math.Max(1000, (int)(player.Stream.GetLength() * 1000));
            timeline.MaxValue = lengthMs;
            Seek(0);
            if (source == null) title.Text = Path.GetFileNameWithoutExtension(path);
            status.Text = $"Audio loaded: {Path.GetFileName(path)}";
        }
        catch (Exception e) { status.Text = "Could not load audio: " + e.Message; }
    }

    public override void _Process(double delta)
    {
        if (player.Playing)
        {
            updating = true;
            timeline.Value = Math.Min(lengthMs, player.GetPlaybackPosition() * 1000);
            updating = false;
            if (timeline.Value >= lengthMs) { player.Stop(); playButton.Text = "▶ Play"; }
        }
    }

    private void TogglePlay()
    {
        if (player.Stream == null) { status.Text = "This map has no audio"; return; }
        if (player.Playing) { player.Stop(); playButton.Text = "▶ Play"; }
        else { player.Play((float)CurrentMs / 1000); playButton.Text = "Ⅱ Pause"; }
    }

    private int BeatMs() => Math.Max(1, (int)Math.Round(60000 / bpm.Value));
    public int SnapMs(int ms)
    {
        double step = 60000 / bpm.Value / snap.Value;
        return Math.Clamp((int)Math.Round(Math.Round(ms / step) * step), 0, lengthMs);
    }
    private void Seek(int ms) => timeline.Value = Math.Clamp(ms, 0, lengthMs);
    public void SetSelected(int index) { selected = index; field.QueueRedraw(); }
    public void AddNote(float x, float y)
    {
        notes.Add(new Note(notes.Count, SnapMs(CurrentMs), x, y));
        notes.Sort((a, b) => a.Millisecond.CompareTo(b.Millisecond));
        selected = notes.FindIndex(n => n.Millisecond == SnapMs(CurrentMs) && n.X == x && n.Y == y);
        status.Text = $"{notes.Count} notes · unsaved";
        field.QueueRedraw();
    }
    public void MoveSelected(float x, float y)
    {
        if (selected < 0 || selected >= notes.Count) return;
        notes[selected].X = x;
        notes[selected].Y = y;
        field.QueueRedraw();
    }
    public void DeleteSelected()
    {
        if (selected < 0 || selected >= notes.Count) return;
        notes.RemoveAt(selected);
        selected = -1;
        status.Text = $"{notes.Count} notes · unsaved";
        field.QueueRedraw();
    }
    private void SaveCopy()
    {
        if (notes.Count == 0) { status.Text = "Add at least one note before saving"; return; }
        try
        {
            string id = "tyanitia-editor-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var ordered = notes.OrderBy(n => n.Millisecond).Select((n, i) => new Note(i, n.Millisecond, n.X, n.Y)).ToArray();
            var copy = new Map(Path.Combine(MapUtil.MapsFolder, id), ordered, id,
                artist.Text.Trim(), title.Text.Trim(), 0, [mapper.Text.Trim()],
                source?.Difficulty ?? 0, source?.DifficultyName, lengthMs,
                draftAudio ?? source?.AudioBuffer, source?.CoverBuffer, source?.VideoBuffer);
            MapParser.Encode(copy);
            MapCache.OrderAndSetMaps();
            RefreshMaps();
            status.Text = $"Saved copy: {copy.PrettyTitle} ({copy.Name})";
        }
        catch (Exception e) { status.Text = "Save failed: " + e.Message; }
    }
    private static string FormatTime(int ms) => $"{ms / 60000}:{ms / 1000 % 60:00}";
}

public partial class EditorNoteCanvas : Control
{
    public MapEditorPanel Editor { get; set; }
    private bool dragging;
    private Rect2 Board => new(new Vector2((Size.X - Math.Min(Size.X - 32, Size.Y - 24)) / 2, 12), new Vector2(Math.Min(Size.X - 32, Size.Y - 24), Math.Min(Size.X - 32, Size.Y - 24)));
    public override void _Ready() { Resized += QueueRedraw; MouseFilter = MouseFilterEnum.Stop; }
    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("101827"));
        var b = Board;
        DrawRect(b, new Color("0a0d18"));
        for (int i = 0; i <= 2; i++)
        {
            float at = i * b.Size.X / 2;
            DrawLine(b.Position + new Vector2(at, 0), b.Position + new Vector2(at, b.Size.Y), new Color("354654"), 2);
            DrawLine(b.Position + new Vector2(0, at), b.Position + new Vector2(b.Size.X, at), new Color("354654"), 2);
        }
        for (int i = 0; i < Editor.Notes.Count; i++)
        {
            var n = Editor.Notes[i];
            int diff = Math.Abs(n.Millisecond - Editor.CurrentMs);
            if (diff > 1800) continue;
            var p = b.Position + new Vector2((n.X + 1) * b.Size.X / 2, (1 - n.Y) * b.Size.Y / 2);
            var c = i == Editor.SelectedIndex ? new Color("ff9bd6") : diff < 120 ? new Color("05e4d0") : new Color("617887", 0.75f);
            DrawCircle(p, i == Editor.SelectedIndex ? 18 : 14, c);
            DrawArc(p, 19, 0, Mathf.Tau, 32, Colors.White, 2);
        }
    }
    public override void _GuiInput(InputEvent ev)
    {
        var b = Board;
        if (ev is InputEventMouseButton mb)
        {
            if (!mb.Pressed) { dragging = false; return; }
            if (!b.HasPoint(mb.Position)) return;
            int nearest = -1; float distance = 26;
            for (int i = 0; i < Editor.Notes.Count; i++)
            {
                var n = Editor.Notes[i];
                if (Math.Abs(n.Millisecond - Editor.CurrentMs) > 120) continue;
                var p = b.Position + new Vector2((n.X + 1) * b.Size.X / 2, (1 - n.Y) * b.Size.Y / 2);
                float d = mb.Position.DistanceTo(p);
                if (d < distance) { distance = d; nearest = i; }
            }
            if (mb.ButtonIndex == MouseButton.Right)
            {
                Editor.SetSelected(nearest);
                Editor.DeleteSelected();
            }
            else if (mb.ButtonIndex == MouseButton.Left)
            {
                if (nearest >= 0) Editor.SetSelected(nearest);
                else Editor.AddNote(Mathf.Clamp((mb.Position.X - b.Position.X) / b.Size.X * 2 - 1, -1, 1), Mathf.Clamp(1 - (mb.Position.Y - b.Position.Y) / b.Size.Y * 2, -1, 1));
                dragging = true;
            }
            AcceptEvent();
        }
        else if (ev is InputEventMouseMotion mm && dragging)
        {
            Editor.MoveSelected(Mathf.Clamp((mm.Position.X - b.Position.X) / b.Size.X * 2 - 1, -1, 1), Mathf.Clamp(1 - (mm.Position.Y - b.Position.Y) / b.Size.Y * 2, -1, 1));
            AcceptEvent();
        }
    }
}
