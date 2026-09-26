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
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Clear();

        EngineInit.configPath = configPath;

        //create the window and start up raylib in general
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(windowWidth, windowHeight, windowTitle);
        Raylib.SetTargetFPS(60); //deltatime proofed

        Raylib.SetExitKey(KeyboardKey.Null);

        //load the settings
        Config = ConfigSettings.LoadOrDefault(configPath);

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