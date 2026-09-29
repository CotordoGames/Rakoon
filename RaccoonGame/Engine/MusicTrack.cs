using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;


//code here for anyone who doesnt want to use FMOD-- its a VERY limited system though.
public class MusicTrack
{
    public Music Music;
    public string Name = "";
    public string Author = "";
    public float LoopStart = 0f;
    public float LoopEnd = 0f;

    public static MusicTrack? Load(string folderPath)
    {
        string oggPath = Path.Combine(folderPath, "mus.ogg");
        if (!File.Exists(oggPath)) return null;

        var track = new MusicTrack { Music = Raylib.LoadMusicStream(oggPath) };

        string metaPath = Path.Combine(folderPath, "meta.json");
        if (File.Exists(metaPath)){
            //var opts = new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true };

            var meta = JsonSerializer.Deserialize(File.ReadAllText(metaPath), AppJsonContext.Default.MusicMeta);

            if(meta != null)
            {
                track.Name = meta.Name;
                track.Author = meta.Author;
                track.LoopStart = meta.LoopStart;
                track.LoopEnd = meta.LoopEnd;
            }
        }

        //if there is no loop end in the json just treat the EOF as a loop point
        if(track.LoopEnd <= track.LoopStart)
        {
            track.LoopEnd = Raylib.GetMusicTimeLength(track.Music);
        }

        return track;
    }

    public void Update()
    {
        Raylib.UpdateMusicStream(Music);

        if(Raylib.GetMusicTimePlayed(Music) >= LoopEnd)
        {
            Raylib.SeekMusicStream(Music, LoopStart);
        }
    }

    public void Unload() => Raylib.UnloadMusicStream(Music);
}

public class MusicMeta
{
    public string Name = "";
    public string Author = "";
    public float LoopStart;
    public float LoopEnd;
}