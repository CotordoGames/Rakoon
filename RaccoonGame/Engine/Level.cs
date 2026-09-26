using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;

public class Level
{
    //the width, height, bgm, and background variables
    public int Width; public int Height;
    public string BGMusic = "";
    public List<string> BGImages = new List<string>();
    public List<float> BGSpeeds = new List<float>();
    public List<Vector2> BGOffsets = new List<Vector2>();

    //tileset
    public string Tileset = "";

    //objects that the level spawns
    public List<GameObject> LevelObjects = new List<GameObject>();

    //metadata
    public string Name = "";
    public string Author = "";
    public int Version;

    //the tilesets
    public List<byte> Layer1 = new List<byte>();
    public List<byte> Layer2 = new List<byte>();

    //player spwn where? player spawn here.
    public Vector2 SpawnPoint;

    public static Level Load(string FolderPath)
    {
        //create an empty level to write into
        var level = new Level();

        //set up the json parser and parse meta
        string metaJson = File.ReadAllText(Path.Combine(FolderPath, "meta.json"));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true };
        var meta = JsonSerializer.Deserialize<LevelMeta>(metaJson, options)
            ?? throw new InvalidDataException($"{FolderPath}/meta.json failed to parse.");

        //set the level properties according to the JSON
        level.Name = meta.Name;
        level.Author = meta.Author;
        level.Version = meta.Version;
        level.Width = meta.Width;
        level.Height = meta.Height;
        level.BGMusic = meta.Bgm;
        level.BGImages = meta.BGImages;
        level.BGSpeeds = meta.BGSpeeds;
        level.Tileset = meta.Tileset;
        level.SpawnPoint = meta.SpawnPoint;
        level.BGOffsets = meta.BGOffsets;

        //load the raw level data
        byte[] raw = File.ReadAllBytes(Path.Combine(FolderPath, "level.rl"));
        int expected = level.Width * level.Height * 2;
        if(raw.Length != expected)
        {
            throw new InvalidDataException(
                $"{FolderPath}/level.rl: expected {expected} bytes for {level.Width}*{level.Height}, got {raw.Length}. meta.json is out of sync.");
        }

        //put it into the respective lists
        int cellCount = level.Width * level.Height;
        level.Layer1 = new List<byte>(cellCount);
        level.Layer2 = new List<byte>(cellCount);
        for(int i = 0; i < cellCount; i++)
        {
            level.Layer1.Add(raw[i * 2]);
            level.Layer2.Add(raw[i * 2 + 1]);
        }

        //begin to parse the objects.json of the level
        //grab the location of the object data
        string objPath = Path.Combine(FolderPath, "objects.json");

        //make sure the objects file exists
        if (File.Exists(objPath))
        {
            //parse the file and load it into objDoc
            using var objDoc = JsonDocument.Parse(File.ReadAllText(objPath));

            //for each object in the doc
            foreach(var entry in objDoc.RootElement.EnumerateArray())
            {
                //store the values of the object for use
                string name = entry.GetProperty("name").GetString() ?? "";
                float x = entry.GetProperty("x").GetSingle();
                float y = entry.GetProperty("y").GetSingle();

                //create an object from the registry
                GameObject? obj = ObjectRegistry.Create(name);
                if(obj == null)
                {
                    //spooky red error
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Warning: unrecognized object '{name}' in {objPath}, skipped.");
                    Console.ForegroundColor = ConsoleColor.Green;
                    continue;
                }

                //load data into the object and add it to the list of active objects
                obj.Position = new Vector2(x, y);
                obj.LoadFromJson(entry);
                level.LevelObjects.Add(obj);

            }
        }

        //return the level
        return level;

    }
}

//global load function
public static class LevelManager
{
    public static string? PendingRoom = null;
    public static string PendingTargetDoorId = "";
    public static bool Transitioning = false;
    public static bool CanEnterDoor = true;
    public static float Fade = 0f;
    public static bool FadingIn = false;

    public static void RequestTransition(string room, string targetDoorId)
    {
        if (!CanEnterDoor) return;

        PendingRoom = room;
        PendingTargetDoorId = targetDoorId;

        Transitioning = true;
        CanEnterDoor = false;
    }

    public static Level LoadAndSpawn(string folderPath)
    {
        Level level = Level.Load(folderPath);
        foreach (var obj in level.LevelObjects)
        {
            obj.DebugColor = Color.Blank;
            ObjectManager.AddObject(obj);
        }

        return level;
    }
}

public class Tileset
{
    //tilesets basic data
    public Texture2D Texture;
    public int TileSize;
    public int Columns;
    public const byte Empty = 255;

    //create self
    public Tileset(string path, int tileSize)
    {
        Texture = Raylib.LoadTexture(path);
        TileSize = tileSize;
        Columns = Texture.Width / TileSize;
    }
}

public static class LevelRenderer
{
    //draw a single layer
    public static void DrawLayer(Level level, List<byte> layer, Tileset tileset, Rectangle cameraView)
    {
        //start of the list of visible tiles
        int startX = Math.Max(0, (int)(cameraView.X / tileset.TileSize));
        int startY = Math.Max(0, (int)(cameraView.Y / tileset.TileSize));

        //end
        int endX = Math.Min(level.Width - 1, (int)(cameraView.X + cameraView.Width) / tileset.TileSize);
        int endY = Math.Min(level.Height - 1, (int)(cameraView.Y + cameraView.Height) / tileset.TileSize);

        //loop through visible tiles and draw them
        for(int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                byte tile = layer[y * level.Width + x];
                if (tile == Tileset.Empty) continue;

                //what peice of the tileset to grab
                Rectangle src = new Rectangle(
                    (tile % tileset.Columns) * tileset.TileSize,
                    (tile / tileset.Columns) * tileset.TileSize,
                    tileset.TileSize, tileset.TileSize);

                //where to render the peice
                Rectangle dest = new Rectangle(
                    x * tileset.TileSize, y * tileset.TileSize,
                    tileset.TileSize, tileset.TileSize);

                //draw the tile
                Raylib.DrawTexturePro(tileset.Texture, src, dest, Vector2.Zero, 0f, Color.White);
            }
        }
    }
}

class LevelMeta
{
    //the width, height, bgm, and background variables
    public int Width; public int Height;
    public string Bgm = "";
    public List<string> BGImages = new List<string>();
    public List<float> BGSpeeds = new List<float>();
    public string Tileset = "";
    public List<Vector2> BGOffsets = new List<Vector2>();

    //where the player should spawn
    public Vector2 SpawnPoint;

    //metadata
    public string Name = "";
    public string Author = "";
    public int Version;
}