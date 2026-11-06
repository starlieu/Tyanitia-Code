using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class NoteRenderer : Renderer, IRenderer<Note>
{
    [Export]
    private Runner runner;

    public MultiMeshInstance3D NoteMultiMesh { get; set; }

    private StandardMaterial3D material = new();

    public override void _Ready()
    {
        runner ??= GetParent().GetParent<Runner>();

        NoteMultiMesh = new()
        {
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Multimesh = new() { UseColors = true, TransformFormat = MultiMesh.TransformFormatEnum.Transform3D },
        };

        AddChild(NoteMultiMesh);
    }

    public override void Setup(SettingsProfile settings, SkinProfile skin)
    {
        base.Setup(settings, skin);

        NoteMultiMesh.Multimesh.InstanceCount = 0;
        NoteMultiMesh.Multimesh.VisibleInstanceCount = -1;

        var mesh = NoteMultiMesh.Multimesh.Mesh;

        for (int i = 0; i < mesh.GetSurfaceCount(); i++)
        {
            if (mesh.SurfaceGetMaterial(i) == null)
            {
                mesh.SurfaceSetMaterial(i, material);
            }

            if (mesh.SurfaceGetMaterial(i) is not StandardMaterial3D mat)
                continue;

            mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
            mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            mat.DisableFog = true;
            mat.DisableAmbientLight = true;
            mat.DisableSpecularOcclusion = true;
            mat.VertexColorUseAsAlbedo = true;
            mat.VertexColorIsSrgb = true;
        }
    }

    public void Render(double delta, double time, IList<Note> notes) => renderNotes(time, notes, 0, notes.Count);

    private void renderNotes<T>(double time, IList<T> notes, int startIndex, int endIndex)
        where T : ITimelineObject
    {
        RenderToMultiMesh(NoteMultiMesh.Multimesh, time, notes, startIndex, endIndex, runner.Attempt, Settings, Skin);
    }

    // Also used by the settings preview, so it renders the same map objects as the game.
    public static void RenderToMultiMesh<T>(
        MultiMesh multimesh, double time, IList<T> notes, int startIndex, int endIndex,
        Attempt attempt, SettingsProfile settings, SkinProfile skin, int maxInstances = int.MaxValue)
        where T : ITimelineObject
    {
        float ar = (float)(double)settings.ApproachRate;
        float ad = (float)(double)settings.ApproachDistance;
        float hardRockScale = attempt.HardRock ? 0.75f : 1f;
        float at = Math.Max(0.01f, (float)(double)settings.ApproachTime * hardRockScale);
        float noteSize = (float)(double)settings.NoteSize * hardRockScale;
        float fadeIn = (float)(double)settings.FadeIn / 100;
        float fadeOut = (float)(double)settings.FadeOut / 100;
        float noteOpacity = (float)(double)settings.NoteOpacity;
        float exponent = Math.Max(Mathf.Epsilon, (float)(double)settings.NoteOpacityExponent);
        float hitWindowDepth = settings.Pushback ? (float)Constants.HIT_WINDOW * ar / 1000 : 0;
        var transform = new Transform3D(new(noteSize / 2, 0, 0), new(0, noteSize / 2, 0), new(0, 0, noteSize / 2), Vector3.Zero);
        int count = Math.Min(maxInstances, Math.Max(0, endIndex - startIndex));
        if (count > multimesh.InstanceCount)
            multimesh.InstanceCount = count;
        multimesh.VisibleInstanceCount = count;

        for (int i = 0; i < count; i++)
        {
            var note = (Note)(object)notes[startIndex + i];
            float remaining = note.Millisecond - (float)time;
            if (remaining < (settings.Pushback ? -Constants.HIT_WINDOW * attempt.Speed : 0)
                || remaining > at * 1000 * attempt.Speed || note.LastResult == HitResult.Hit)
            {
                multimesh.SetInstanceColor(i, new Color(0xffffff00));
                continue;
            }
            float depth = (note.Millisecond - (float)attempt.Progress) / (1000 * at) * ad / (float)attempt.Speed;
            float progress = 1 - Math.Max(0, (depth + hitWindowDepth) / Math.Max(0.01f, ad + hitWindowDepth));
            note.Opacity = fadeIn > 0 ? Math.Min(1, progress / fadeIn) : 1;
            if (fadeOut > 0 && attempt.UseFadeOut)
                note.Opacity -= 1 - Math.Min(1, (1 - progress) / fadeOut);
            note.Transform = transform;
            note.Transform.Origin = new(note.X, note.Y, -depth);
            foreach (var mod in attempt.Modifiers)
                if (mod is IObjectRenderModifier<Note> modifier)
                    modifier.ModifyRenderObject(note, attempt);
            var colors = skin.NoteColors;
            var color = colors.Length == 0 ? Colors.White : colors[note.Index % colors.Length];
            if (attempt.HardRock)
                color = color.Lerp(new Color("ff4b78"), 0.45f);
            color.A = Math.Clamp((float)Math.Pow(Math.Max(0, note.Opacity * noteOpacity), exponent), 0, 1);
            multimesh.SetInstanceTransform(i, note.Transform);
            multimesh.SetInstanceColor(i, color);
        }
    }

    public override void Process(double delta, Attempt attempt)
    {
        var type = typeof(Note);

        if (!attempt.Objects.TryGetValue(type, out var notes))
        {
            return;
        }

        renderNotes(attempt.Progress, notes, runner.ObjectIndicesStart[type], runner.ObjectIndicesEnd[type]);
    }
}
