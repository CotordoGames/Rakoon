using Raylib_cs;
using System;
using System.Windows;


//technically a helper, but important the clean up program.cs, which i may rename to main.
public static class EngineInit
{
    public static ConfigSettings Config {  get; private set; } = new ConfigSettings();
    public static AudioManager Audio { get; private set; } = new AudioManager();
    public static VirtualScreen Screen { get; private set; } = null!;
    public static Font Rakoon { get; private set; }

    static string configPath = "assets/persistantData/konfig.json";

    public static void Initialize(
        string windowTitle,
        int windowWidth, int windowHeight,
        int virtualWidth, int virtualHeight,
        string banksFolderPath,
        string fontPath,
        string portraitAtlasPath,
        string configPath = "assets/persistantData/konfig.json"
        )
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
        }
        catch (IOException)
        {

        }

        EngineInit.configPath = configPath;

        //load the settings
        Config = ConfigSettings.LoadOrDefault(configPath);

        int w = Config.WindowWidth > 0 ? Config.WindowWidth : windowWidth;
        int h = Config.WindowHeight > 0 ? Config.WindowHeight : windowHeight;



        //create the window and start up raylib in general
        ConfigFlags flags = ConfigFlags.ResizableWindow;
        if (Config.VSync) flags |= ConfigFlags.VSyncHint;
        Raylib.SetConfigFlags(flags);
        Raylib.InitWindow(w, h, windowTitle);
        Raylib.SetTargetFPS(Config.LimitFPS && Config.FpsCount > 0 ? Config.FpsCount : 0); //deltatime proofed

        Raylib.SetExitKey(KeyboardKey.Null);

        //initialize audio with the settings values and the banks path
        Audio.Initialize(banksFolderPath);
        Audio.SetMasterVolume(Config.MasterVolume);
        Audio.SetMusicVolume(Config.MusicVolume);
        Audio.SetSfxVolume(Config.SfxVolume);

        //toggle fullscreen if needed
        if (Config.StartFullScreen) Raylib.ToggleFullscreen();

        //create the game screen/canvas
        Screen = new VirtualScreen(virtualWidth, virtualHeight);

        //load dialogue related assets and create the DM
        Rakoon = Raylib.LoadFontEx(fontPath, 10, null, 0);
        Texture2D atlas = Raylib.LoadTexture(portraitAtlasPath);
        DialogueManager.Init(atlas);
    }

    public static void Update() => Audio.Update();

    public static void SaveConfig() => Config.Save(configPath);

    public static void ShutDown()
    {
        Audio.ShutDown();
        Screen.Unload();
        Raylib.UnloadFont(Rakoon);
        Raylib.CloseWindow();
    }
}

public class EngineDebug
{
    public static void Log(string message)
    {
        try
        {
            Console.WriteLine($"[ENGINE LOG]: {message}");
        }
        catch (IOException)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}

static class Layers
{
    public const uint Default = 1, Player = 2, Enemy = 4;
}