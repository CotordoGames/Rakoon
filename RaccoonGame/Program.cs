using System;
using System.Numerics;
using Raylib_cs;
using System.Linq;
using System.Collections.Generic;
using System.IO;


class Program
{
    public static AudioManager audio = null!;
    public static Font Rakoon;
    public static VirtualScreen screen = null!;
    public static int ScreenWidth => screen.Width;
    public static int ScreenHeight => screen.Height;

    public static Autumn player = new Autumn();
    public static Level level = null!;
    public static bool Debug = false;
    public static bool DebugOverlay = false;
    public static bool DebugText = false;
    public static bool DebugCameraZones = false;
    public static bool DrawLayerOne = true;
    public static bool DrawLayerTwo = true;

    static Tileset tileset = null!;
    static List<Texture2D> bgTextures = new List<Texture2D>();
    public static GameCamera camera = null!;

    //player health bar
    public static HealthBar PlayerHealthBar = new HealthBar(Autumn.health);

    const float FadeSpeed = 5f;



    static void Main(string[] args)
    {

        // ---------- ENGINE INITIALIZATION ---------- //
        EngineInit.Initialize(
            windowTitle: "Autumn the Rakoon -- intro",
            windowWidth: 1280, windowHeight: 720,
            virtualWidth: 270, virtualHeight: 180,
            banksFolderPath: "assets/banks/Desktop",
            fontPath: "assets/sprites/rakoon.ttf",
            portraitAtlasPath: "assets/sprites/portraits.png"
            );



        audio = EngineInit.Audio;
        Rakoon = EngineInit.Rakoon;
        screen = EngineInit.Screen;



        // ---------- GAME SPECIFIC SETUP ---------- //
        ObjectManager.AddObject(player);

        camera = new GameCamera(screen.Width, screen.Height);
        camera.SetTarget(player);

        if (args.Length == 0)
        {
            LoadRoom("assets/levels/MoveGym", spawnAtDoorId: null);
        }
        else
        {
            LoadRoom(args[0], spawnAtDoorId: null);
        }

        camera.SetLevelBounds(level.Width, level.Height, tileset.TileSize);

        // ---------- MAIN LOOP ---------- //
        while (!Raylib.WindowShouldClose())
        {
            //get the deltatime we need
            float dt = Raylib.GetFrameTime();

            //make the game fullscreen if F11 is pressed
            if (Raylib.IsKeyPressed(KeyboardKey.F11)) Raylib.ToggleFullscreen();

            //toggle debug
            if (Raylib.IsKeyPressed(KeyboardKey.F1)) Debug = !Debug;

            //draw overlay?
            if( Raylib.IsKeyPressed(KeyboardKey.F2)) DebugOverlay = !DebugOverlay;

            //draw text?
            if (Raylib.IsKeyPressed(KeyboardKey.F3)) DebugText = !DebugText;

            //draw zones?
            if (Raylib.IsKeyPressed(KeyboardKey.F4)) DebugCameraZones = !DebugCameraZones;

            //draw layer 1?
            if (Raylib.IsKeyPressed(KeyboardKey.F5)) DrawLayerOne = !DrawLayerOne;

            //draw layer 2?
            if (Raylib.IsKeyPressed(KeyboardKey.F6)) DrawLayerTwo = !DrawLayerTwo;

            //very first thing, check if the window is resized for rendering reasons
            if (Raylib.IsWindowResized()) screen.UpdateDimensions();

            EngineInit.Update();

            //update all object's logic
            ObjectManager.UpdateWorld(dt);

            //update the dialogue manager
            DialogueManager.Update(dt);

            //update camera
            camera.Update(dt);

            HandleRoomTransition();
            HandleDeath();

            Draw();

            var mousePos = screen.GetMousePosition();

            //set the visible rectangle
            Rectangle currentCameraView = camera.GetViewRect();

            //reset the player if they go under
            if(player.Position.Y > (level.Height * 8) + 16)
            {
                Death();
            }

        }

        // ---------- SHUTDOWN ---------- //
        EngineInit.SaveConfig();
        EngineInit.ShutDown();
    }

    static void Draw()
    {
        //set the visible rectangle
        Rectangle currentCameraView = camera.GetViewRect();

        /*
        Rendering Part 01: actual game stuff.

        this is where the game will do its rendering to a low level render texture rather than the full screen size.

        used for most world space items.
        */

        Raylib.BeginTextureMode(screen.Target);

        Raylib.ClearBackground(new Color(0x23, 0x53, 0x47, 0xFF));

        BackgroundRenderer.Draw(level, bgTextures, currentCameraView, ScreenWidth, ScreenHeight);

        Raylib.BeginMode2D(camera.RaylibCamera);

        if (DrawLayerOne)
        {
            //draw layer 1, the decor ish layer
            LevelRenderer.DrawLayer(level, level.Layer1, tileset, currentCameraView);
        }

        //draw all of the objects
        ObjectManager.DrawWorld(currentCameraView);

        if(DrawLayerTwo)
        {
            //draw layer 2
            LevelRenderer.DrawLayer(level, level.Layer2, tileset, currentCameraView);
        }

        //draw collisions if we must
        //if (Debug)
        //{
        //    ObjectManager.DrawDebugWorld(currentCameraView);
        //}

        Raylib.EndMode2D();

        //draw the players health
        PlayerHealthBar.Draw(new Vector2(8, 8), new Vector2(48, 8), Color.White);

        //draw the text box
        DialogueManager.Draw();

        //draw the LM fade effect if we must
        if(LevelManager.Fade > 0f)
        {
            Raylib.DrawRectangle(0, 0, screen.Width, screen.Height, new Color((byte)0, (byte)0, (byte)0, (byte)(LevelManager.Fade * 255)));
        }

        Raylib.EndTextureMode();


        /*
        Rendering Part 02: the render texture and maybe some HUD

        this is where the render texture made earlier actually gets rendered, as well as anything that needs to be full resolution

        used for almost nothing.
        */

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.DrawTexturePro(screen.Target.Texture, screen.SourceRect, screen.DestRect, Vector2.Zero, 0.0f, Color.White);

        if (Debug)
        {
            float scale = screen.DestRect.Width / screen.Width;

            Camera2D dbgCam = camera.RaylibCamera;
            dbgCam.Zoom *= scale;
            dbgCam.Offset = new Vector2(screen.DestRect.X, screen.DestRect.Y) + dbgCam.Offset * scale;

            Raylib.BeginScissorMode((int)screen.DestRect.X, (int)screen.DestRect.Y, (int)screen.DestRect.Width, (int)screen.DestRect.Height);

            Raylib.BeginMode2D(dbgCam);
            ObjectManager.DrawDebugWorld(currentCameraView, 2f / dbgCam.Zoom);
            Raylib.EndMode2D();

            Raylib.EndScissorMode();
        }

        if (Debug && DebugOverlay)
        {
            Raylib.DrawRectangle((int)screen.DestRect.X, (int)screen.DestRect.Y, (int)screen.DestRect.Width / 6, (int)screen.DestRect.Height, new Color(0, 0, 0, 140));
            Raylib.DrawTextEx(ObjectManager.DebugFont, Raylib.GetFPS().ToString(), new Vector2((int)screen.DestRect.X, (int)screen.DestRect.Y), Raylib.GetScreenHeight() / 45, 0, new Color(0, 255, 0, 255));
            Raylib.DrawTextEx(ObjectManager.DebugFont, $"Player Velocity:\nX: {player.Velocity.X}\nY: {player.Velocity.Y}", new Vector2((int)screen.DestRect.X, (int)screen.DestRect.Y + Raylib.GetScreenHeight() / 45), Raylib.GetScreenHeight() / 45, 0, new Color(0, 255, 0, 255));
            Raylib.DrawTextEx(ObjectManager.DebugFont, $"Object Count: \n{ObjectManager.AllObjects.Count}\n{player.DashTimer}", new Vector2((int)screen.DestRect.X, (int)screen.DestRect.Y + (Raylib.GetScreenHeight() / 45) * 4), Raylib.GetScreenHeight() / 45, 0, new Color(0, 255, 0, 255));

        }

        Raylib.EndDrawing();
    }

    public static void Death()
    {
        Autumn.health?.TakeDamage(16);
        player.Velocity.Y = -10;
    }

    public static void LoadBackgrounds()
    {
        foreach(var tex in bgTextures)
        {
            Raylib.UnloadTexture(tex);
        }

        bgTextures.Clear();
        foreach(var img in level.BGImages)
        {
            var tex = Raylib.LoadTexture(Path.Combine("assets", "backgrounds", img));

            if(tex.Width == 0)
            {
                throw new FileNotFoundException($"Background image failed to load: {img}");
            }

            bgTextures.Add(tex);
        }
    }

    static void LoadRoom(string folderPath, string? spawnAtDoorId)
    {
        //clear the world, add the player
        ObjectManager.ClearWorld();
        ObjectManager.AddObject(player);

        level = LevelManager.LoadAndSpawn(folderPath);
        //TODO: unload the FUCKING tilemap
        tileset = new Tileset(Path.Combine("assets/tilesets", level.Tileset, "tm.png"), 8);
        LoadBackgrounds();

        audio.PlayBGM(level.BGMusic);

        Vector2 spawnPos = level.SpawnPoint;
        if(spawnAtDoorId != null)
        {
            var door = level.LevelObjects.Find(o => o is Door d && d.DoorId == spawnAtDoorId);
            if (door != null) spawnPos = door.Position;
        }
        //actually set the players position
        player.Position = new Vector2(spawnPos.X, spawnPos.Y);
        player.Velocity = Vector2.Zero;
        camera.Position = player.Position - new Vector2(0, (screen.Height / 2));
        camera.PersistantOffset = Vector2.Zero;

    }

    static void HandleRoomTransition()
    {
        if (!LevelManager.Transitioning) return;

        if (!LevelManager.FadingIn)
        {
            player.CanMove = false;
            LevelManager.Fade += FadeSpeed * Raylib.GetFrameTime();

            if (LevelManager.Fade >= 1f)
            {
                LoadRoom(Path.Combine("assets/levels", LevelManager.PendingRoom!), LevelManager.PendingTargetDoorId);
                camera.SetLevelBounds(level.Width, level.Height, 8);
                LevelManager.PendingRoom = null;
                LevelManager.FadingIn = true;
            }

        }
        else
        {
            LevelManager.Fade -= FadeSpeed * Raylib.GetFrameTime();

            if (LevelManager.Fade <= 0f)
            {
                LevelManager.Fade = 0f;
                LevelManager.Transitioning = false;
                LevelManager.FadingIn = false;
                LevelManager.CanEnterDoor = true;
            }
        }
    }

    static void HandleDeath()
    {
        if (player.Position.Y <= (level.Height * tileset.TileSize) + 48) return;

        player.Position = new Vector2(level.SpawnPoint.X - player.Size.X / 2, level.SpawnPoint.Y - player.Size.Y / 2);
        player.Velocity = Vector2.Zero;
    }
}