# Tyanitia

Tyanitia is a free-to-play rhythm game where you move your mouse across a 3×3 grid. This project is an open-source client rebrand built with Godot 4.7 and C#. It retains the source project's license; see [LICENSE](LICENSE).

## Downloads

These packages are built from this project:

| Platform | Package |
|---|---|
| Android (ARM64, debug-signed) | [Tyanitia-Android.apk](build/android/Tyanitia-Android.apk) |
| Windows x64 | [Tyanitia-Windows.zip](build/windows/Tyanitia-Windows.zip) |
| Linux x86_64 | [Tyanitia-Linux.zip](build/linux/Tyanitia-Linux.zip) |

The Android package is debug-signed for sideloading and development. A release package for distribution through an app store must be signed with your own release keystore.

## Installation

### Android

Install the APK on an ARM64 device running Android 10 (API 29) or later. If Android blocks installation, allow installs from the app used to open the APK. The package uses Godot's Mobile renderer. Maps opened through Android's file picker are copied into the app's private storage before import.

### Windows

1. Extract `Tyanitia-Windows.zip`.
2. Run `Tyanitia.exe`.

The Microsoft Visual C++ Redistributable may be required on systems where it is not already installed.

### Linux

1. Extract `Tyanitia-Linux.zip`.
2. Run the `Tyanitia` executable from the extracted folder.

If the executable bit was lost while copying the files, run `chmod +x Tyanitia` in the extracted folder.

## User data

Maps, replays, skins, and settings are stored separately from the installation:

| Platform | User data location |
|---|---|
| Android | App-private storage for `org.tyanitia.client` |
| Windows | `%appdata%\Tyanitia` |
| Linux | `~/.local/share/Tyanitia` |

The **Settings → Video** section includes frame rate, V-Sync, anti-aliasing, and a graphics renderer selector: Forward+, Mobile, or Compatibility. Renderer changes take effect after restarting the game.

## Building from source

### Requirements

- Godot 4.7 .NET editor and matching export templates.
- .NET SDK 10.0.
- Git LFS for the large binary assets.
- For Android: Java 17, Android SDK platform 36, Android build-tools, NDK `29.0.14206865`, and the matching Godot Android build template. Android C# export is experimental in Godot.

Open `project.godot` with the Godot .NET editor, restore the project with `dotnet restore Tyanitia.csproj`, then build or export using the presets in `export_presets.cfg`.

For a debug Android APK, create an output directory and export with the Android preset:

```bash
mkdir -p build/android
godot --headless --path . --export-debug Android build/android/Tyanitia-Android.apk
```

The Android preset targets API 29 or later, compiles against API 36, and enables ARM64. Release exports require a release signing keystore configured in Godot; do not commit keystore passwords.

The **Video** settings offer Forward+, Mobile, and Compatibility rendering. Godot applies the selected renderer at startup, so the game asks to restart after a change.

## Project layout

- `scenes/` — menus, gameplay, and result screens.
- `scripts/` — C# application, gameplay, settings, and platform integration.
- `textures/`, `sounds/`, `fonts/`, and `themes/` — game assets.
- `export_presets.cfg` — Windows, Linux, macOS, and Android export presets.

