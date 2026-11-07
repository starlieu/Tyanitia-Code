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

    private sealed class OnlineMap
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public string Artist { get; init; } = "";
        public string Status { get; init; } = "";
        public string FileUrl { get; init; } = "";
        public string[] Mappers { get; init; } = [];
        public bool IsVerified { get; init; }
        public bool HasNotableMapper { get; init; }
        public int Difficulty { get; init; }
        public int Length { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    private readonly List<OnlineMap> maps = [];
    private LineEdit mapSearch;
    private LineEdit mapperSearch;
    private OptionButton notableFilter;
    private OptionButton verifiedFilter;
    private OptionButton sortOrder;
    private Label statusLabel;
    private VBoxContainer results;
    private Button reloadButton;
    private bool isLoading;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        BuildInterface();
    }

    public void Open()
    {
        Visible = true;
        if (maps.Count == 0)
        {
            LoadMaps();
        }
        else
        {
            UpdateResults();
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
            Color = new Color(0, 0, 0, 0.78f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        AddChild(backdrop);
        backdrop.AnchorRight = 1;
        backdrop.AnchorBottom = 1;

        PanelContainer dialog = new()
        {
            AnchorLeft = MobileUi.IsMobile ? 0.02f : 0.06f,
            AnchorTop = MobileUi.IsMobile ? 0.02f : 0.06f,
            AnchorRight = MobileUi.IsMobile ? 0.98f : 0.94f,
            AnchorBottom = MobileUi.IsMobile ? 0.98f : 0.94f,
            MouseFilter = MouseFilterEnum.Stop,
        };
        StyleBoxFlat panelStyle = new()
        {
            BgColor = new Color("0a111b"),
            BorderColor = new Color("328c8f"),
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 18,
            CornerRadiusTopRight = 18,
            CornerRadiusBottomLeft = 18,
            CornerRadiusBottomRight = 18,
            ContentMarginLeft = MobileUi.IsMobile ? 22 : 18,
            ContentMarginTop = MobileUi.IsMobile ? 18 : 14,
            ContentMarginRight = MobileUi.IsMobile ? 22 : 18,
            ContentMarginBottom = MobileUi.IsMobile ? 18 : 14,
        };
        dialog.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(dialog);

        VBoxContainer layout = new() { Theme = GD.Load<Theme>("res://themes/theme.tres") };
        layout.AddThemeConstantOverride("separation", MobileUi.IsMobile ? 12 : 10);
        dialog.AddChild(layout);

        HBoxContainer header = new();
        layout.AddChild(header);
        Label title = new()
        {
            Text = "ONLINE MAPS",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
        };
        title.AddThemeFontSizeOverride("font_size", MobileUi.IsMobile ? 30 : 26);
        header.AddChild(title);

        reloadButton = new Button { Text = "Refresh", Icon = TyanitiaIcons.Get("random"), CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 150 : 112, MobileUi.IsMobile ? 64 : 44) };
        reloadButton.AddThemeConstantOverride("icon_max_width", 25);
        reloadButton.Pressed += LoadMaps;
        header.AddChild(reloadButton);

        Button closeButton = new() { Text = "Close", Icon = TyanitiaIcons.Get("quit"), CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 140 : 100, MobileUi.IsMobile ? 64 : 44) };
        closeButton.AddThemeConstantOverride("icon_max_width", 25);
        closeButton.Pressed += () => Visible = false;
        header.AddChild(closeButton);

        HBoxContainer searches = new();
        searches.AddThemeConstantOverride("separation", 8);
        layout.AddChild(searches);

        mapSearch = new LineEdit
        {
            PlaceholderText = "Search map title or artist",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            ClearButtonEnabled = true,
            CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 64 : 44),
        };
        mapSearch.TextChanged += _ => UpdateResults();
        searches.AddChild(mapSearch);

        mapperSearch = new LineEdit
        {
            PlaceholderText = "Search mapper",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            ClearButtonEnabled = true,
            CustomMinimumSize = new Vector2(0, MobileUi.IsMobile ? 64 : 44),
        };
        mapperSearch.TextChanged += _ => UpdateResults();
        searches.AddChild(mapperSearch);

        HFlowContainer filters = new();
        filters.AddThemeConstantOverride("separation", 8);
        layout.AddChild(filters);
        AddFilter(filters, "Notable mapper", ["Any", "Notable", "Not notable"], out notableFilter);
        AddFilter(filters, "Verification", ["Any", "Verified", "Unverified"], out verifiedFilter);
        AddFilter(filters, "Sort", ["Newest", "Title A-Z", "Title Z-A", "Difficulty"], out sortOrder);

        notableFilter.ItemSelected += _ => UpdateResults();
        verifiedFilter.ItemSelected += _ => UpdateResults();
        sortOrder.ItemSelected += _ => UpdateResults();

        statusLabel = new Label { Text = "Connect to the map service to browse maps." };
        layout.AddChild(statusLabel);

        ScrollContainer scroll = new()
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        };
        layout.AddChild(scroll);
        scroll.GetVScrollBar().CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 46 : 24, 0);

        results = new VBoxContainer();
        results.AddThemeConstantOverride("separation", 6);
        results.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(results);
    }

    private static void AddFilter(Container parent, string labelText, string[] options, out OptionButton option)
    {
        Label label = new() { Text = labelText, VerticalAlignment = VerticalAlignment.Center };
        parent.AddChild(label);

        option = new OptionButton { CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 180 : 140, MobileUi.IsMobile ? 60 : 42) };
        foreach (string item in options)
        {
            option.AddItem(item);
        }
        parent.AddChild(option);
    }

    private void LoadMaps()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;
        reloadButton.Disabled = true;
        statusLabel.Text = "Loading maps...";

        HttpRequest request = new() { Timeout = 30 };
        AddChild(request);
        request.RequestCompleted += (result, responseCode, _, body) =>
        {
            isLoading = false;
            reloadButton.Disabled = false;
            request.QueueFree();

            if (result != (long)HttpRequest.Result.Success || responseCode is < 200 or >= 300)
            {
                statusLabel.Text = $"Could not load maps (HTTP {responseCode}, request result {result}).";
                GD.PushWarning(statusLabel.Text);
                return;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(body);
                if (!document.RootElement.TryGetProperty("maps", out JsonElement mapArray) || mapArray.ValueKind != JsonValueKind.Array)
                {
                    throw new JsonException("API response does not contain a maps array.");
                }

                maps.Clear();
                foreach (JsonElement item in mapArray.EnumerateArray())
                {
                    OnlineMap map = ParseMap(item);
                    if (map.Id.Length > 0 && map.Title.Length > 0 && map.FileUrl.Length > 0)
                    {
                        maps.Add(map);
                    }
                }

                UpdateResults();
            }
            catch (JsonException exception)
            {
                statusLabel.Text = "The map service returned invalid data.";
                GD.PushError($"Could not parse online maps: {exception.Message}");
            }
        };

        Error error = request.Request(ApiUrl, ["Accept: application/json"]);
        if (error != Error.Ok)
        {
            isLoading = false;
            reloadButton.Disabled = false;
            request.QueueFree();
            statusLabel.Text = $"Could not connect to the map service ({error}).";
            GD.PushWarning(statusLabel.Text);
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
                string name = ReadString(mapper, "name");
                if (name.Length > 0)
                {
                    mappers.Add(name);
                }

                hasNotableMapper |= ReadBoolean(mapper, "isNotable");
            }
        }

        DateTimeOffset.TryParse(
            ReadString(item, "createdAt"),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out DateTimeOffset createdAt
        );

        return new OnlineMap
        {
            Id = ReadString(item, "id"),
            Title = ReadString(item, "title"),
            Artist = ReadString(item, "artist"),
            Status = ReadString(item, "status"),
            FileUrl = ReadString(item, "fileUrl"),
            Mappers = [.. mappers],
            IsVerified = ReadBoolean(item, "isVerified"),
            HasNotableMapper = hasNotableMapper,
            Difficulty = ReadInteger(item, "difficulty"),
            Length = ReadInteger(item, "length"),
            CreatedAt = createdAt,
        };
    }

    private static string ReadString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";
    }

    private static bool ReadBoolean(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.True;
    }

    private static int ReadInteger(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out JsonElement value) && value.TryGetInt32(out int result) ? result : 0;
    }

    private void UpdateResults()
    {
        if (results == null)
        {
            return;
        }

        foreach (Node child in results.GetChildren())
        {
            child.QueueFree();
        }

        IEnumerable<OnlineMap> filtered = maps.Where(map =>
            map.Title.Contains(mapSearch.Text, StringComparison.OrdinalIgnoreCase)
            || map.Artist.Contains(mapSearch.Text, StringComparison.OrdinalIgnoreCase)
        );
        if (mapperSearch.Text.Length > 0)
        {
            filtered = filtered.Where(map =>
                map.Mappers.Any(mapper => mapper.Contains(mapperSearch.Text, StringComparison.OrdinalIgnoreCase))
            );
        }

        if (notableFilter.Selected == 1)
        {
            filtered = filtered.Where(map => map.HasNotableMapper);
        }
        else if (notableFilter.Selected == 2)
        {
            filtered = filtered.Where(map => !map.HasNotableMapper);
        }

        if (verifiedFilter.Selected == 1)
        {
            filtered = filtered.Where(map => map.IsVerified);
        }
        else if (verifiedFilter.Selected == 2)
        {
            filtered = filtered.Where(map => !map.IsVerified);
        }

        filtered = sortOrder.Selected switch
        {
            1 => filtered.OrderBy(map => map.Title, StringComparer.OrdinalIgnoreCase),
            2 => filtered.OrderByDescending(map => map.Title, StringComparer.OrdinalIgnoreCase),
            3 => filtered.OrderByDescending(map => map.Difficulty).ThenBy(map => map.Title, StringComparer.OrdinalIgnoreCase),
            _ => filtered.OrderByDescending(map => map.CreatedAt),
        };

        OnlineMap[] visibleMaps = [.. filtered];
        statusLabel.Text = $"{visibleMaps.Length} map(s) shown from {maps.Count} available.";
        if (visibleMaps.Length == 0)
        {
            results.AddChild(new Label { Text = maps.Count == 0 ? "No maps are available." : "No maps match those filters." });
            return;
        }

        foreach (OnlineMap map in visibleMaps)
        {
            AddMapRow(map);
        }
    }

    private void AddMapRow(OnlineMap map)
    {
        PanelContainer row = new();
        StyleBoxFlat rowStyle = new()
        {
            BgColor = new Color("141f2a"),
            BorderColor = new Color("344e59"),
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
            ContentMarginLeft = 12,
            ContentMarginTop = 9,
            ContentMarginRight = 12,
            ContentMarginBottom = 9,
        };
        row.AddThemeStyleboxOverride("panel", rowStyle);
        results.AddChild(row);

        HBoxContainer rowLayout = new();
        rowLayout.AddThemeConstantOverride("separation", 10);
        row.AddChild(rowLayout);

        VBoxContainer description = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        description.AddThemeConstantOverride("separation", 2);
        rowLayout.AddChild(description);

        Label title = new()
        {
            Text = string.IsNullOrWhiteSpace(map.Artist) ? map.Title : $"{map.Artist} - {map.Title}",
            ClipText = true,
        };
        title.AddThemeFontSizeOverride("font_size", MobileUi.IsMobile ? 24 : 19);
        description.AddChild(title);

        string mappers = map.Mappers.Length == 0 ? "Unknown mapper" : string.Join(", ", map.Mappers);
        string flags = $"{(map.HasNotableMapper ? "Notable mapper" : "Non-notable mapper")} | {(map.IsVerified ? "Verified" : "Unverified")} | {map.Status}";
        string duration = TimeSpan.FromMilliseconds(Math.Max(0, map.Length)).ToString(@"m\:ss", CultureInfo.InvariantCulture);
        description.AddChild(new Label
        {
            Text = $"{mappers} | {flags} | Difficulty {map.Difficulty} | {duration}",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        });

        Button downloadButton = new()
        {
            Text = "Download",
            Icon = TyanitiaIcons.Get("import"),
            CustomMinimumSize = new Vector2(MobileUi.IsMobile ? 160 : 128, MobileUi.IsMobile ? 66 : 50),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
        };
        downloadButton.AddThemeConstantOverride("icon_max_width", 25);
        downloadButton.Pressed += () => DownloadMap(map, downloadButton);
        rowLayout.AddChild(downloadButton);
    }

    private void DownloadMap(OnlineMap map, Button button)
    {
        if (
            !Uri.TryCreate(new Uri(ApiUrl), map.FileUrl, out Uri fileUri)
            || fileUri.Scheme != Uri.UriSchemeHttps
            || !string.Equals(fileUri.Host, "rhythia-api.nyarw.moe", StringComparison.OrdinalIgnoreCase)
        )
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

        Error error = request.Request(fileUri.ToString());
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
