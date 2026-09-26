using System;
using System.Collections.Generic;
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

    public ConfigSettings() { } //evil default constructor

    public static ConfigSettings LoadOrDefault(string path)
    {

        if(!File.Exists(path)) return new ConfigSettings();

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true };
            var conf = JsonSerializer.Deserialize<ConfigMeta>(File.ReadAllText(path), options);
            if (conf == null) return new ConfigSettings();

            return new ConfigSettings
            {
                MasterVolume = conf.MasterVolume,
                MusicVolume = conf.MusicVolume,
                SfxVolume = conf.SfxVolume,
                StartFullScreen = conf.StartFullScreen,
                YuriMode = conf.YuriMode
            };
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Config] Failed to parse {path}: {ex.Message} -- using defaults");
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
            StartFullScreen = StartFullScreen, YuriMode = YuriMode
        };

        File.WriteAllText(path, JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true }));
    }
}

public class ConfigMeta
{
    public float MasterVolume = 0.5f;
    public float MusicVolume = 0.5f;
    public float SfxVolume = 0.5f;
    public bool StartFullScreen = false;
    public bool YuriMode = false;
}