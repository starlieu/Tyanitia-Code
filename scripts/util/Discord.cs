using DiscordRPC;
using Godot;

public class Discord
{
    private static string appId = "1231699688340590722";
    public static DiscordRpcClient Client { get; private set; }

    static Discord()
    {
        if (OS.GetName() == "Android")
        {
            return;
        }

        // TODO: Add logging here
        Client = new DiscordRpcClient(appId) { };

        Client.Initialize();

        Client.SetPresence(new RichPresence() { Assets = new Assets() { LargeImageKey = "short" } });
    }

    public static void UpdateState(string details) => Client?.UpdateState(details);

    public static void Dispose() => Client?.Dispose();
}
