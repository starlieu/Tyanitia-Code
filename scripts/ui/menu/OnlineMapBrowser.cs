using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

public partial class OnlineMapBrowser : Control
{
    private const string ApiUrl = "https://rhythia-api.nyarw.moe/maps/";
    private const string ApiHost = "rhythia-api.nyarw.moe";

    private sealed class OnlineMap
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public string Artist { get; init; } = "";
        public string DifficultyName { get; init; } = "";
        public string Status { get; init; } = "";
        public string FileUrl { get; init; } = "";
        public string CoverUrl { get; init; } = "";
        public string[] Mappers { get; init; } = [];
        public bool IsVerified { get; init; }
        public bool HasNotableMapper { get; init; }
        public int Difficulty { get; init; }
        public int Length { get; init; }
        public int NoteCount { get; init; }
    }

    private readonly List<OnlineMap> maps = [];
    private LineEdit search;
    private OptionButton difficultyFilter;
    private OptionButton sortDirection;
    private OptionButton pageSizePicker;
    private CheckButton verifiedOnly;
    private CheckButton notableOnly;
    private Label statusLabel;
    private Label pageLabel;
    private GridContainer results;
    private ScrollContainer resultsScroll;
    private Button previousButton;
    private Button nextButton;
    private Button reloadButton;
    private Timer searchTimer;
    private HttpRequest catalogRequest;
    private int requestGeneration;
    private int renderGeneration;
    private int totalMaps;
    private int offset;
    private int pageSize;
    private bool isLoading;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        BuildInterface();
        pageSize = MobileUi.IsMobile ? 8 : 18;
        pageSizePicker.Select(pageSizePicker.GetItemIndex(pageSize));
        results.Resized += UpdateColumnCount;
        Callable.From(UpdateColumnCount).CallDeferred();
    }

    public void Open()
    {
        Visible = true;
        if (maps.Count == 0)
        {
            LoadMaps();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Visible && @event is InputEventKey { Pressed: true, Keycode: Key.Escape })
        {
            Visible = false;
            GetViewport().SetInputAsHandled();
        }
    }

    private void BuildInterface()
    {
        ColorRect backdrop = new()
        {
            Color = new Color(0, 0, 0, 0.88f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(backdrop);

        PanelContainer dialog = new()
        {
            AnchorLeft = MobileUi.IsMobile ? 0.015f : 0.025f,
            AnchorTop = MobileUi.IsMobile ? 0.015f : 0.025f,
            AnchorRight = MobileUi.IsMobile ? 0.985f : 0.975f,
            AnchorBottom = MobileUi.IsMobile ? 0.985f : 0.975f,
            MouseFilter = MouseFilterEnum.Stop,
        };
        StyleBoxFlat panelStyle = new()
        {
            BgColor = new Color(0.015f, 0.015f, 0.02f, 1),
            BorderColor = new Color("343434"),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = MobileUi.IsMobile ? 14 : 18,
            ContentMarginTop = MobileUi.IsMobile ? 10 : 14,
            ContentMarginRight = MobileUi.IsMobile ? 14 : 18,
            ContentMarginBottom = MobileUi.IsMobile ? 10 : 14,
        };
        dialog.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(dialog);

        VBoxContainer layout = new() { Theme = GD.Load<Theme>("res://themes/theme.tres") };
        layout.AddThemeConstantOverride("separation", MobileUi.IsMobile ? 8 : 10);
        dialog.AddChild(layout);

        HBoxContainer header = new();
        layout.AddChild(header);
        Label title = new()
        {
            Text = "MAP BROWSER",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
        };
        title.AddThemeFontSizeOverride("font_size", MobileUi.IsMobile ? 30 : 26);
        header.AddChild(title);

        reloadButton = new Button
        {
            Text = "Refresh",
            Icon = TyanitiaIcons.Get("random"),
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 132 : 112, MobileUi.IsMobile ? 60 : 44),
        };
        reloadButton.Pressed += LoadMaps;
        header.AddChild(reloadButton);

        Button closeButton = new()
        {
            Text = "Close",
            Icon = TyanitiaIcons.Get("quit"),
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 116 : 96, MobileUi.IsMobile ? 60 : 44),
        };
        closeButton.Pressed += () => Visible = false;
        header.AddChild(closeButton);

        search = new LineEdit
        {
            PlaceholderText = "Search maps, mappers, or difficulty...",
            ClearButtonEnabled = true,
            CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 60 : 42),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        search.TextChanged += _ =>
        {
            offset = 0;
            searchTimer.Start();
        };
        layout.AddChild(search);

        HFlowContainer filters = new();
        filters.AddThemeConstantOverride("h_separation", MobileUi.IsMobile ? 8 : 12);
        filters.AddThemeConstantOverride("v_separation", 6);
        layout.AddChild(filters);

        difficultyFilter = new OptionButton
        {
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 184 : 150, MobileUi.IsMobile ? 58 : 40),
        };
        difficultyFilter.AddItem("All difficulties", -1);
        for (int i = 0; i < Constants.DIFFICULTIES.Length; i++)
        {
            difficultyFilter.AddItem(Constants.DIFFICULTIES[i], i);
        }
        difficultyFilter.ItemSelected += _ => ReloadFromStart();
        filters.AddChild(difficultyFilter);

        sortDirection = new OptionButton
        {
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 164 : 144, MobileUi.IsMobile ? 58 : 40),
        };
        sortDirection.AddItem("Recently added", 0);
        sortDirection.AddItem("Oldest first", 1);
        sortDirection.ItemSelected += _ => ReloadFromStart();
        filters.AddChild(sortDirection);

        pageSizePicker = new OptionButton
        {
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 150 : 128, MobileUi.IsMobile ? 58 : 40),
        };
        foreach (int count in MobileUi.IsMobile ? new[] { 8, 18 } : new[] { 8, 18, 36 })
        {
            pageSizePicker.AddItem($"{count} per page", count);
        }
        pageSizePicker.ItemSelected += index =>
        {
            pageSize = pageSizePicker.GetItemId((int)index);
            ReloadFromStart();
        };
        filters.AddChild(pageSizePicker);

        verifiedOnly = new CheckButton
        {
            Text = "Verified only",
            CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 58 : 40),
        };
        verifiedOnly.Toggled += _ => ReloadFromStart();
        filters.AddChild(verifiedOnly);

        notableOnly = new CheckButton
        {
            Text = "Notable only",
            CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 58 : 40),
        };
        notableOnly.Toggled += _ => ReloadFromStart();
        filters.AddChild(notableOnly);

        statusLabel = new Label { Text = "Search the online map library." };
        layout.AddChild(statusLabel);

        resultsScroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        };
        resultsScroll.GetVScrollBar().CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 38 : 20, 0);
        layout.AddChild(resultsScroll);

        results = new GridContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        results.AddThemeConstantOverride("h_separation", MobileUi.IsMobile ? 10 : 12);
        results.AddThemeConstantOverride("v_separation", MobileUi.IsMobile ? 10 : 12);
        resultsScroll.AddChild(results);

        HBoxContainer pagination = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 58 : 42),
        };
        layout.AddChild(pagination);

        previousButton = new Button { Text = "Previous", CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 124 : 102, MobileUi.IsMobile ? 54 : 38) };
        previousButton.Pressed += () =>
        {
            offset = Math.Max(0, offset - pageSize);
            LoadMaps();
        };
        pagination.AddChild(previousButton);

        pageLabel = new Label
        {
            Text = "0 maps",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 210 : 190, 0),
        };
        pagination.AddChild(pageLabel);

        nextButton = new Button { Text = "Next", CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 100 : 82, MobileUi.IsMobile ? 54 : 38) };
        nextButton.Pressed += () =>
        {
            offset += pageSize;
            LoadMaps();
        };
        pagination.AddChild(nextButton);

        searchTimer = new Timer { OneShot = true, WaitTime = 0.35 };
        searchTimer.Timeout += LoadMaps;
        AddChild(searchTimer);
    }

    private void UpdateColumnCount()
    {
        if (results == null)
        {
            return;
        }

        results.Columns = Size.X < 800 ? 1 : Size.X < 1420 ? 2 : 3;
    }

    private void ReloadFromStart()
    {
        offset = 0;
        LoadMaps();
    }

    private void LoadMaps()
    {
        if (!IsInsideTree())
        {
            return;
        }

        int generation = ++requestGeneration;
        if (catalogRequest != null && IsInstanceValid(catalogRequest))
        {
            catalogRequest.CancelRequest();
            catalogRequest.QueueFree();
            catalogRequest = null;
        }

        isLoading = true;
        reloadButton.Disabled = true;
        previousButton.Disabled = true;
        nextButton.Disabled = true;
        statusLabel.Text = "Loading maps...";

        List<string> query = [$"limit={pageSize}", $"offset={offset}"];
        string searchText = search.Text.Trim();
        int selectedDifficulty = difficultyFilter.GetSelectedId();
        int searchedDifficulty = Array.FindIndex(
            Constants.DIFFICULTIES,
            difficulty => string.Equals(difficulty, searchText, StringComparison.OrdinalIgnoreCase)
        );
        if (selectedDifficulty >= 0)
        {
            query.Add($"difficulty={selectedDifficulty}");
        }
        else if (searchedDifficulty >= 0)
        {
            query.Add($"difficulty={searchedDifficulty}");
        }
        else if (searchText.Length > 0)
        {
            query.Add($"query={Uri.EscapeDataString(searchText)}");
        }

        if (verifiedOnly.ButtonPressed)
        {
            query.Add("verified=true");
        }
        if (notableOnly.ButtonPressed)
        {
            query.Add("notable=true");
        }

        query.Add($"sortDirection={(sortDirection.GetSelectedId() == 1 ? "asc" : "desc")}");

        HttpRequest request = new() { Timeout = 45 };
        catalogRequest = request;
        AddChild(request);
        request.RequestCompleted += (result, responseCode, _, body) =>
        {
            request.QueueFree();
            if (generation != requestGeneration)
            {
                return;
            }

            catalogRequest = null;
            isLoading = false;
            reloadButton.Disabled = false;

            if (result != (long)HttpRequest.Result.Success || responseCode is < 200 or >= 300)
            {
                statusLabel.Text = $"Could not load maps (HTTP {responseCode}, request result {result}).";
                GD.PushWarning(statusLabel.Text);
                UpdatePageButtons();
                return;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(body);
                JsonElement root = document.RootElement;
                if (
                    root.ValueKind != JsonValueKind.Object
                    || !root.TryGetProperty("maps", out JsonElement mapArray)
                    || mapArray.ValueKind != JsonValueKind.Array
                )
                {
                    throw new JsonException("API response does not contain a maps array.");
                }

                maps.Clear();
                foreach (JsonElement item in mapArray.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    OnlineMap map = ParseMap(item);
                    if (map.Id.Length > 0 && map.Title.Length > 0 && map.FileUrl.Length > 0)
                    {
                        maps.Add(map);
                    }
                }

                totalMaps = ReadInteger(root, "total");
                offset = ReadInteger(root, "offset");
                UpdateResults();
            }
            catch (JsonException exception)
            {
                statusLabel.Text = "The map service returned invalid data.";
                GD.PushError($"Could not parse online maps: {exception.Message}");
                UpdatePageButtons();
            }
        };

        Error error = request.Request(ApiUrl + "?" + string.Join("&", query), ["Accept: application/json"]);
        if (error != Error.Ok)
        {
            request.QueueFree();
            if (generation == requestGeneration)
            {
                catalogRequest = null;
                isLoading = false;
                reloadButton.Disabled = false;
                statusLabel.Text = $"Could not connect to the map service ({error}).";
                GD.PushWarning(statusLabel.Text);
                UpdatePageButtons();
            }
        }
    }

    private static OnlineMap ParseMap(JsonElement item)
    {
        List<string> mappers = [];
        bool hasNotableMapper = false;
        if (item.TryGetProperty("mappers", out JsonElement mapperArray) && mapperArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement mapper in mapperArray.EnumerateArray())
            {
                if (mapper.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                string name = ReadString(mapper, "name");
                if (name.Length > 0)
                {
                    mappers.Add(name);
                }
                hasNotableMapper |= ReadBoolean(mapper, "isNotable");
            }
        }

        string coverUrl = "";
        if (item.TryGetProperty("covers", out JsonElement covers) && covers.ValueKind == JsonValueKind.Object)
        {
            coverUrl = ReadString(covers, MobileUi.IsMobile ? "128" : "256");
            if (coverUrl.Length == 0)
            {
                coverUrl = ReadString(covers, "128");
            }
        }

        return new OnlineMap
        {
            Id = ReadString(item, "id"),
            Title = ReadString(item, "title"),
            Artist = ReadString(item, "artist"),
            DifficultyName = ReadString(item, "difficultyName"),
            Status = ReadString(item, "status"),
            FileUrl = ReadString(item, "fileUrl"),
            CoverUrl = coverUrl,
            Mappers = [.. mappers],
            IsVerified = ReadBoolean(item, "isVerified"),
            HasNotableMapper = hasNotableMapper,
            Difficulty = ReadInteger(item, "difficulty"),
            Length = ReadInteger(item, "length"),
            NoteCount = ReadInteger(item, "noteCount"),
        };
    }

    private static string ReadString(JsonElement element, string property)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(property, out JsonElement value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";
    }

    private static bool ReadBoolean(JsonElement element, string property)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(property, out JsonElement value)
            && value.ValueKind == JsonValueKind.True;
    }

    private static int ReadInteger(JsonElement element, string property)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(property, out JsonElement value)
            && value.TryGetInt32(out int result)
            ? result
            : 0;
    }

    private void UpdateResults()
    {
        renderGeneration++;
        foreach (Node child in results.GetChildren())
        {
            child.QueueFree();
        }

        foreach (OnlineMap map in maps)
        {
            AddMapCard(map, renderGeneration);
        }

        int first = maps.Count == 0 ? 0 : offset + 1;
        int last = offset + maps.Count;
        pageLabel.Text = maps.Count == 0 ? $"0 of {totalMaps}" : $"{first}-{last} of {totalMaps}";
        statusLabel.Text = isLoading
            ? "Loading maps..."
            : totalMaps == 0
                ? "No maps match those filters."
                : $"Showing {first}-{last} of {totalMaps} maps.";
        previousButton.Disabled = isLoading || offset == 0;
        nextButton.Disabled = isLoading || offset + maps.Count >= totalMaps;
        reloadButton.Disabled = isLoading;
        resultsScroll.ScrollVertical = 0;
    }

    private void AddMapCard(OnlineMap map, int generation)
    {
        PanelContainer card = new()
        {
            CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 148 : 132),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        StyleBoxFlat cardStyle = new()
        {
            BgColor = new Color("283138"),
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 0,
            ContentMarginTop = 0,
            ContentMarginRight = 0,
            ContentMarginBottom = 0,
        };
        card.AddThemeStyleboxOverride("panel", cardStyle);
        results.AddChild(card);

        HBoxContainer cardLayout = new();
        cardLayout.AddThemeConstantOverride("separation", 0);
        card.AddChild(cardLayout);

        TextureRect cover = new()
        {
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 132 : 116, 0),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            Texture = GD.Load<Texture2D>("res://textures/empty.png"),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        cardLayout.AddChild(cover);
        LoadCover(map, cover, generation);

        MarginContainer detailsMargin = new();
        detailsMargin.AddThemeConstantOverride("margin_left", 10);
        detailsMargin.AddThemeConstantOverride("margin_right", 8);
        detailsMargin.AddThemeConstantOverride("margin_top", 6);
        detailsMargin.AddThemeConstantOverride("margin_bottom", 6);
        detailsMargin.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        detailsMargin.SizeFlagsVertical = SizeFlags.ExpandFill;
        cardLayout.AddChild(detailsMargin);

        VBoxContainer details = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        details.AddThemeConstantOverride("separation", 2);
        detailsMargin.AddChild(details);

        Label title = new()
        {
            Text = string.IsNullOrWhiteSpace(map.Artist) ? map.Title : $"{map.Artist} - {map.Title}",
            ClipText = true,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
        };
        title.AddThemeFontSizeOverride("font_size", MobileUi.IsMobile ? 22 : 18);
        details.AddChild(title);

        string mapperNames = map.Mappers.Length == 0 ? "Unknown mapper" : string.Join(", ", map.Mappers);
        Label mapper = new() { Text = $"by {mapperNames}", ClipText = true };
        mapper.AddThemeColorOverride(
            "font_color",
            map.HasNotableMapper ? new Color("ff626b") : new Color("dedede")
        );
        details.AddChild(mapper);

        string difficulty = string.IsNullOrWhiteSpace(map.DifficultyName)
            ? Constants.DIFFICULTIES[Math.Clamp(map.Difficulty, 0, Constants.DIFFICULTIES.Length - 1)]
            : map.DifficultyName;
        Label difficultyLabel = new() { Text = difficulty, ClipText = true };
        difficultyLabel.AddThemeColorOverride(
            "font_color",
            Constants.DIFFICULTY_COLORS[Math.Clamp(map.Difficulty, 0, Constants.DIFFICULTY_COLORS.Length - 1)]
        );
        details.AddChild(difficultyLabel);

        HBoxContainer metadata = new();
        metadata.AddThemeConstantOverride("separation", 8);
        details.AddChild(metadata);
        Label notable = new() { Text = map.HasNotableMapper ? "NOTABLE" : "NON-NOTABLE" };
        notable.AddThemeColorOverride(
            "font_color",
            map.HasNotableMapper ? new Color("00b9ff") : new Color("b0b0b0")
        );
        metadata.AddChild(notable);

        Label verified = new() { Text = map.IsVerified ? "VERIFIED" : "UNVERIFIED" };
        verified.AddThemeColorOverride(
            "font_color",
            map.IsVerified ? new Color("71e58b") : new Color("b0b0b0")
        );
        metadata.AddChild(verified);

        Label noteCount = new()
        {
            Text = $"{map.NoteCount} notes",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        metadata.AddChild(noteCount);
        metadata.AddChild(
            new Label
            {
                Text = FormatDuration(map.Length),
                HorizontalAlignment = HorizontalAlignment.Right,
            }
        );

        Button downloadButton = new()
        {
            Text = MobileUi.IsMobile ? "Get" : "Download",
            TooltipText = "Download map",
            Icon = TyanitiaIcons.Get("import"),
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 84 : 122, 0),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        downloadButton.Pressed += () => DownloadMap(map, downloadButton);
        cardLayout.AddChild(downloadButton);
    }

    private void LoadCover(OnlineMap map, TextureRect cover, int generation)
    {
        if (map.CoverUrl.Length == 0 || !TryGetApiUrl(map.CoverUrl, out string coverUrl))
        {
            return;
        }

        HttpRequest request = new() { Timeout = 30 };
        AddChild(request);
        request.RequestCompleted += (result, responseCode, _, body) =>
        {
            request.QueueFree();
            if (
                generation != renderGeneration
                || !IsInstanceValid(cover)
                || !cover.IsInsideTree()
                || result != (long)HttpRequest.Result.Success
                || responseCode is < 200 or >= 300
            )
            {
                return;
            }

            Image image = Util.Misc.LoadImageFromBuffer(body);
            if (image != null)
            {
                cover.Texture = ImageTexture.CreateFromImage(image);
            }
        };

        Error error = request.Request(coverUrl);
        if (error != Error.Ok)
        {
            request.QueueFree();
            GD.PushWarning($"Could not request cover art for {map.Id}: {error}");
        }
    }

    private static string FormatDuration(int milliseconds)
    {
        TimeSpan duration = TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
        return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
    }

    private void UpdatePageButtons()
    {
        previousButton.Disabled = offset == 0;
        nextButton.Disabled = offset + maps.Count >= totalMaps;
    }

    private static bool TryGetApiUrl(string path, out string url)
    {
        url = "";
        if (
            !Uri.TryCreate(new Uri(ApiUrl), path, out Uri uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.Equals(uri.Host, ApiHost, StringComparison.OrdinalIgnoreCase)
        )
        {
            return false;
        }

        url = uri.ToString();
        return true;
    }

    private void DownloadMap(OnlineMap map, Button button)
    {
        if (!TryGetApiUrl(map.FileUrl, out string fileUrl))
        {
            GD.PushWarning($"Rejected invalid map download URL for {map.Id}.");
            button.Text = "Invalid URL";
            button.Disabled = true;
            return;
        }

        button.Disabled = true;
        button.Text = "Downloading...";
        statusLabel.Text = $"Downloading {map.Title}...";

        HttpRequest request = new() { Timeout = 60 };
        AddChild(request);
        request.RequestCompleted += async (result, responseCode, _, body) =>
        {
            request.QueueFree();
            if (result != (long)HttpRequest.Result.Success || responseCode is < 200 or >= 300)
            {
                button.Disabled = false;
                button.Text = "Retry";
                statusLabel.Text = $"Could not download {map.Title} (HTTP {responseCode}, request result {result}).";
                GD.PushWarning(statusLabel.Text);
                return;
            }

            if (body.Length < 4 || body[0] != 0x50 || body[1] != 0x4B)
            {
                button.Disabled = false;
                button.Text = "Retry";
                statusLabel.Text = $"The downloaded file for {map.Title} is not a valid map archive.";
                GD.PushWarning(statusLabel.Text);
                return;
            }

            string downloadPath = null;
            try
            {
                string importDirectory = Path.Combine(Constants.USER_FOLDER, "cache", "imports");
                Directory.CreateDirectory(importDirectory);
                downloadPath = Path.Combine(importDirectory, $"online-{Guid.NewGuid():N}.phxm");
                await File.WriteAllBytesAsync(downloadPath, body);
                await MapParser.BulkImport([downloadPath], notify: true);
                button.Text = "Downloaded";
                statusLabel.Text = $"Imported {map.Title}.";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                button.Disabled = false;
                button.Text = "Retry";
                statusLabel.Text = $"Could not import {map.Title}: {exception.Message}";
                GD.PushError(statusLabel.Text);
            }
            finally
            {
                if (downloadPath != null)
                {
                    try
                    {
                        File.Delete(downloadPath);
                    }
                    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                    {
                        GD.PushWarning($"Could not remove temporary map download: {exception.Message}");
                    }
                }
            }
        };

        Error error = request.Request(fileUrl);
        if (error != Error.Ok)
        {
            request.QueueFree();
            button.Disabled = false;
            button.Text = "Retry";
            statusLabel.Text = $"Could not start download ({error}).";
            GD.PushWarning(statusLabel.Text);
        }
    }
}
