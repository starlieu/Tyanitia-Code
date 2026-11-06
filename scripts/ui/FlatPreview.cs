using System;
using System.Globalization;
using Godot;

public partial class FlatPreview : Panel
{
    public Map Map;
    public bool UseSoundManagerStreamPlayer;
    public bool Playing = false;
    public double Time = 0;
    public double Speed = 1;

<<<<<<< HEAD
    private Color bright = new(0.35f, 0.95f, 0.88f, 0.58f);
    private Color transparent = new(0.35f, 0.95f, 0.88f, 0);
=======
    private Color bright = new(1, 1, 1, 0.2f);
    private Color transparent = new(1, 1, 1, 0);
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    private ColorRect[] tiles = new ColorRect[9];
    private int lastPassedNote = 0;
    private double[] noteTimestamps;

    public override void _Ready()
    {
        for (int i = 0; i < 9; i++)
        {
<<<<<<< HEAD
            ColorRect tile = new() { Name = i.ToString(CultureInfo.CurrentCulture), Color = transparent, MouseFilter = MouseFilterEnum.Ignore };
=======
            ColorRect tile = new() { Name = i.ToString(CultureInfo.CurrentCulture), Color = transparent };
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

            AddChild(tile);

            float left = i % 3 / 3f;
            float top = (float)Math.Floor(i / 3f) / 3;
            float third = 1 / 3f;

            tile.SetAnchor(Side.Left, left);
            tile.SetAnchor(Side.Top, top);
            tile.SetAnchor(Side.Right, left + third);
            tile.SetAnchor(Side.Bottom, top + third);

            tiles[i] = tile;
        }
    }

<<<<<<< HEAD
    public override void _Draw()
    {
        var background = new Color("0b1421");
        var line = new Color(0.72f, 0.84f, 0.9f, 0.28f);
        DrawRect(new Rect2(Vector2.Zero, Size), background, true);
        for (int i = 1; i < 3; i++)
        {
            DrawLine(new Vector2(Size.X * i / 3, 0), new Vector2(Size.X * i / 3, Size.Y), line, 1);
            DrawLine(new Vector2(0, Size.Y * i / 3), new Vector2(Size.X, Size.Y * i / 3), line, 1);
        }
        DrawRect(new Rect2(Vector2.Zero, Size), new Color(0.54f, 0.9f, 0.85f, 0.5f), false, 2);
    }

=======
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    public override void _Process(double delta)
    {
        if (Map == null)
            return;

        float alpha = (float)Math.Min(1, delta * 6);

        foreach (ColorRect tile in tiles)
        {
            tile.Color = tile.Color.Lerp(transparent, alpha);
        }

        double oldTime = Time;

        if (UseSoundManagerStreamPlayer)
        {
            if (SoundManager.Map == null || SoundManager.Map.Name != Map.Name)
            {
                return;
            }

            Playing = !SoundManager.Song.StreamPaused;
            Time = SoundManager.Song.GetPlaybackPosition() * 1000;
        }
        else if (Playing)
        {
            Time += delta * Speed * 1000;
        }

        if (Time < oldTime)
        {
            lastPassedNote = Util.Misc.BinarySearch(noteTimestamps, Time);
        }

        for (int i = Math.Clamp(lastPassedNote + 1, 0, Math.Max(0, Map.Notes.Length - 1)); i < Map.Notes.Length; i++)
        {
            var note = Map.Notes[i];

            if (Time >= note.Millisecond)
            {
                Vector2I pos = new(Math.Clamp((int)Math.Floor(note.X + 1.5), 0, 2), Math.Clamp((int)Math.Floor(-note.Y + 1.5), 0, 2));
                ColorRect tile = tiles[pos.X + 3 * pos.Y];

                tile.Color = bright;
                lastPassedNote = i;
            }
            else
            {
                break;
            }
        }
    }

    public void Setup(Map map, bool useSoundManagerStreamPlayer = false)
    {
        if (Map != null && Map.Name == map.Name)
            return;

        Map = map;
        UseSoundManagerStreamPlayer = useSoundManagerStreamPlayer;
        lastPassedNote = -1;
        noteTimestamps = Array.ConvertAll(Map.Notes, note => (double)note.Millisecond);
    }

    public void Seek(double seek)
    {
        if (Map == null)
            return;

        Time = seek;

        lastPassedNote = Util.Misc.BinarySearch(noteTimestamps, seek);
    }
}
