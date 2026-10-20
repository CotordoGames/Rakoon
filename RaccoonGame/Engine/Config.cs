using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

public class ConfigSettings
{
    public float MasterVolume = 0.5f;
    public float MusicVolume = 0.5f;
    public float SfxVolume = 0.5f;
    public bool StartFullScreen = false;
    public bool YuriMode = false;
    public bool VSync = true;
    public bool LimitFPS = true;
    public int FpsCount = 60;
    public int WindowWidth = 1080;
    public int WindowHeight = 720;

    //keybinds
    public KeyboardKey Left = KeyboardKey.Left;
    public KeyboardKey Right = KeyboardKey.Right;
    public KeyboardKey Up = KeyboardKey.Up;
    public KeyboardKey Down = KeyboardKey.Down;
    public KeyboardKey Jump = KeyboardKey.Z;
    public KeyboardKey Confirm = KeyboardKey.Z;
    public KeyboardKey Dash = KeyboardKey.X;
    public KeyboardKey Deny = KeyboardKey.X;
    public KeyboardKey Run = KeyboardKey.LeftShift;
    public KeyboardKey Hurt = KeyboardKey.Space;

    public ConfigSettings() { } //evil default constructor

    public static ConfigSettings LoadOrDefault(string path)
    {

        if(!File.Exists(path)) return new ConfigSettings();

        try
        {
            //var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true };
            var conf = JsonSerializer.Deserialize(File.ReadAllText(path), AppJsonContext.Default.ConfigMeta);
            if (conf == null) return new ConfigSettings();

            return new ConfigSettings
            {
                MasterVolume = conf.MasterVolume,
                MusicVolume = conf.MusicVolume,
                SfxVolume = conf.SfxVolume,
                StartFullScreen = conf.StartFullScreen,
                YuriMode = conf.YuriMode,
                VSync = conf.VSync,
                LimitFPS = conf.LimitFPS,
                FpsCount = conf.FpsCount,
                WindowWidth = conf.WindowWidth,
                WindowHeight = conf.WindowHeight,
                Left = conf.Left, Right = conf.Right,
                Up = conf.Up, Down = conf.Down,
                Dash = conf.Dash, Confirm = conf.Confirm,
                Hurt = conf.Hurt, Jump = conf.Jump,
                Deny = conf.Deny, Run = conf.Run
            };
        }
        catch(Exception ex)
        {
            EngineDebug.Log($"[Config] Failed to parse {path}: {ex.Message} -- using defaults");
            return new ConfigSettings();
        }

        //set up the json parser and parse meta
        //string metaJson = File.ReadAllText(path);
        //var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true };
        //var conf = JsonSerializer.Deserialize<ConfigMeta>(metaJson, options)
        //    ?? throw new InvalidDataException($"{path} failed to parse.");

        //MasterVolume = conf.MasterVolume; MusicVolume = conf.MusicVolume; SfxVolume = conf.SfxVolume;
        //StartFullScreen = conf.StartFullScreen; YuriMode = conf.YuriMode;
    }

    public void Save(string path)
    {
        var meta = new ConfigMeta
        {
            MasterVolume = MasterVolume, MusicVolume = MusicVolume, SfxVolume = SfxVolume,
            StartFullScreen = StartFullScreen, YuriMode = YuriMode, VSync = VSync, FpsCount = FpsCount,
            LimitFPS = LimitFPS, WindowWidth = WindowWidth, WindowHeight = WindowHeight
        };

        //File.WriteAllText(path, JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true }));

        string json = JsonSerializer.Serialize(meta, AppJsonContext.Default.ConfigMeta);

        File.WriteAllText(path, json);
    }
}

public class ConfigMeta
{
    public float MasterVolume = 0.5f;
    public float MusicVolume = 0.5f;
    public float SfxVolume = 0.5f;
    public bool StartFullScreen = false;
    public bool YuriMode = false;
    public bool VSync = true;
    public bool LimitFPS = true;
    public int FpsCount = 60;
    public int WindowWidth = 1080;
    public int WindowHeight = 720;

    //keybinds
    public KeyboardKey Left = KeyboardKey.Left;
    public KeyboardKey Right = KeyboardKey.Right;
    public KeyboardKey Up = KeyboardKey.Up;
    public KeyboardKey Down = KeyboardKey.Down;
    public KeyboardKey Jump = KeyboardKey.Z;
    public KeyboardKey Confirm = KeyboardKey.Z;
    public KeyboardKey Dash = KeyboardKey.X;
    public KeyboardKey Deny = KeyboardKey.X;
    public KeyboardKey Run = KeyboardKey.LeftShift;
    public KeyboardKey Hurt = KeyboardKey.Space;

}