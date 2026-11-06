using System;
using System.ComponentModel;
using Godot;
using Updatum;

public partial class Loading : BaseScene
{
    private Color opaque = new(1, 1, 1, 1);
    private Color transparent = new(1, 1, 1, 0);

    [Export]
    private ColorRect background;

    [Export]
    private TextureRect splash;

    [Export]
    private Label progressLabel;

    [Export]
    private Panel progressBar;

    [Export]
    private Panel progressBarFill;

<<<<<<< HEAD
=======
    private ShaderMaterial splashMaterial;

>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
    public override async void _Ready()
    {
        base._Ready();

<<<<<<< HEAD
        splash.Modulate = transparent;
        progressLabel.Hide();
        progressBar.Hide();
=======
        splashMaterial = splash.Material as ShaderMaterial;
        splashMaterial.SetShaderParameter("shift", 0);

        progressLabel.Modulate = transparent;
        progressBar.Modulate = transparent;
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

        bool updateFound = false;
        try
        {
            if (OS.GetName() != "Android")
            {
                updateFound = await Releases.CheckForUpdatesAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Could not get latest release: {ex.Message}");
        }

        if (updateFound)
        {
            var popup = new OptionPopup("Update Found", "Would you like to download the new version?");

            popup.AddOption("Update", Callable.From(updateStep));
            popup.AddOption("Cancel", Callable.From(mapInitializeStep));

            popup.Canceled += mapInitializeStep;

            popup.Show();
        }
        else
        {
            mapInitializeStep();
        }
    }

    public void UpdateProgressBar(float progress)
    {
        progressBarFill.AnchorRight = progress;
    }

    public void UpdateProgressLabel(string label)
    {
        progressLabel.Text = label;
    }

    private Tween enter()
    {
<<<<<<< HEAD
        var inTween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut).SetParallel();
        inTween.TweenProperty(background, "color", Color.FromHtml("#060509"), 1.2);
        inTween.TweenProperty(splash, "modulate", opaque, 1.55).SetDelay(0.2);
=======
        var inTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetParallel();
        inTween.TweenProperty(background, "color", Color.FromHtml("#060509"), 1);
        inTween.TweenProperty(progressLabel, "modulate", opaque, 0.5);
        inTween.TweenProperty(progressBar, "modulate", opaque, 0.5);
        inTween
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out)
            .TweenMethod(
                Callable.From(
                    (float shift) =>
                    {
                        splashMaterial.SetShaderParameter("shift", shift);
                    }
                ),
                0.2,
                1.0,
                1.5
            );
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d

        return inTween;
    }

    private void exit()
    {
        var outTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetParallel();
<<<<<<< HEAD
        outTween.TweenProperty(background, "color", Color.Color8(0, 0, 0), 0.4);
        outTween.TweenProperty(splash, "modulate", transparent, 0.4);
=======
        outTween.TweenProperty(background, "color", Color.Color8(0, 0, 0), 0.25);
>>>>>>> 99c9721a1ed81aa8725aacfa2eb4dbcf2dc4ab8d
        outTween
            .Chain()
            .TweenCallback(
                Callable.From(() =>
                {
                    SceneManager.Load("res://scenes/main_menu.tscn");
                })
            );
    }

    private void updateStep()
    {
        enter();

        progressLabel.Text = $"Downloading {Releases.MANAGER.DownloadedPercentage} %";

        Releases.MANAGER.PropertyChanged += updateDownloadBar;
        Releases.MANAGER.DownloadCompleted += (_, _) =>
        {
            Releases.MANAGER.PropertyChanged -= updateDownloadBar;
            progressLabel.Text = "Installing";
        };
        Releases.UpdateToLatest();
    }

    private void mapInitializeStep()
    {
        int toSync = MapCache.FilesToSync.Value;
        bool allSynced = MapCache.FilesSynced.Value == toSync;

        if (allSynced)
        {
            progressLabel.Text = "Done!";
            progressBarFill.AnchorRight = 1;
        }
        else
        {
            MapCache.FilesSynced.ValueChanged += (_, _) =>
            {
                if (allSynced)
                {
                    return;
                }

                int synced = MapCache.FilesSynced.Value;
                float progress = synced / (float)toSync;

                progressLabel.Text = $"Initializing maps ({synced}/{toSync})";
                progressBarFill.AnchorRight = progress;

                if (progress >= 1)
                {
                    allSynced = true;
                    progressLabel.Text = "Done!";
                }
            };
        }

        var inTween = enter();

        inTween.Chain();

        inTween.TweenCallback(
            Callable.From(() =>
            {
                if (MapManager.Initialized)
                {
                    exit();
                }
                else
                {
                    MapManager.MapsInitialized += _ => Callable.From(exit).CallDeferred();
                }
            })
        );
    }

    private void updateDownloadBar(object _, PropertyChangedEventArgs @event)
    {
        if (@event.PropertyName == nameof(UpdatumManager.DownloadedPercentage))
        {
            CallDeferred("UpdateProgressLabel", $"Downloading {Releases.MANAGER.DownloadedPercentage} %");
            float progress = (float)Releases.MANAGER.DownloadedPercentage / 100;
            CallDeferred("UpdateProgressBar", progress);
        }
    }
}
