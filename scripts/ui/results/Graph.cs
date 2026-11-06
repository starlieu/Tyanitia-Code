using Godot;

public partial class Graph : ColorRect
{
    public override void _Draw()
    {
        Color hitColor = new(0x00ff00ff);
        Color missColor = new(0xff000044);
        Color deathColor = new(0xffff00ff);

        var attempt = Game.Attempt;
        float[] hitsInfo = attempt.IsReplay ? attempt.Replays[0].Notes : attempt.HitsInfo;
        float deathTime = (float)(attempt.IsReplay ? attempt.MaxReplayLength : attempt.DeathTime);

        for (ulong i = attempt.FirstNote; i < (ulong)hitsInfo.Length; i++)
        {
            float offset = hitsInfo[i];
            float ms = attempt.Map.Notes[i].Millisecond;
            float noteProgress = ms / attempt.Map.Length;

            if (ms > deathTime && deathTime != -1)
            {
                break;
            }

            if (offset < 0)
            {
                int position = (int)(Size.X * noteProgress);
                DrawLine(Vector2.Right * position, new(position, Size.Y), missColor, 1);
            }
            else
            {
                DrawRect(new(Size.X * noteProgress, Size.Y * (offset / 55), Vector2.One), hitColor);
            }
        }

        if (deathTime >= 0)
        {
            int position = (int)(Size.X * deathTime / attempt.Map.Length);

            if (position < Size.X)
            {
                DrawLine(Vector2.Right * position, new(position, Size.Y), deathColor, 3);
            }
        }
    }
}
