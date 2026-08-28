using System;
using System.Numerics;
using Raylib_cs;


class Program
{
    //initialize the player object
    public static Player player = new Player();

    //initialize a level
    public static Level level;

    public static Tileset? tileset;

    public static List<Texture2D> bgTextures = new List<Texture2D>();

    public static bool Debug = false;

    public static AudioManager audio = new AudioManager();


    static void Main()
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("App's working, thank u :)");

        //make the windows resizable
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);

        //set the window width and high variables
        int windowWidth = 1280;
        int windowHeight = 720;

        //initialize the window with the width and height that was set
        Raylib.InitWindow(windowWidth, windowHeight, "raccoon :)");
        audio.Initialize("assets/banks/Desktop");
        Raylib.SetTargetFPS(60);

        audio.SetMasterVolume(0.75f);
        audio.SetMusicVolume(0.75f);
        audio.SetSfxVolume(0.75f);

        //the low resolution screen the the game world is drawn to
        VirtualScreen screen = new VirtualScreen(320, 180);

        //initialize the player object
        player = new Player();
        ObjectManager.AddObject(player);

        //initialize a level
        level = LevelManager.LoadAndSpawn("assets/levels/level1");
        audio.PlayBGM(level.BGMusic);
        tileset = new Tileset(Path.Combine("assets/tilesets", level.Tileset, "tm.png"), 8);
        LoadBackgrounds();

        //set the players position to the spawn point
        player.Position = new Vector2(level.SpawnPoint.X - player.Size.X/2, level.SpawnPoint.Y - player.Size.Y/2 - 8);

        //create the camera
        GameCamera camera = new GameCamera(320, 180);
        camera.SetTarget(player);
        camera.SetLevelBounds(level.Width, level.Height, tileset.TileSize);

        while (!Raylib.WindowShouldClose())
        {

            //make the game fullscreen if F11 is pressed
            if (Raylib.IsKeyPressed(KeyboardKey.F11))
            {
                Raylib.ToggleFullscreen();
            }
            //toggle debug
            if (Raylib.IsKeyPressed(KeyboardKey.F2))
            {
                Debug = !Debug;
            }

            //get the deltatime we need
            float dt = Raylib.GetFrameTime();

            //update the audio manager
            audio.Update();

            //update all object's logic
            ObjectManager.UpdateWorld(dt);

            //update the room if needed

            if(LevelManager.PendingRoom != null)
            {
                //set up vars
                string room = LevelManager.PendingRoom;
                string targetDoorId = LevelManager.PendingTargetDoorId;
                LevelManager.PendingRoom = null;

                //do the stuff
                ObjectManager.ClearWorld();
                level = LevelManager.LoadAndSpawn(Path.Combine("assets/levels/", room));
                tileset = new Tileset(Path.Combine("assets/tilesets", level.Tileset, "tm.png"), 8);

                //new music
                audio.PlayBGM(level.BGMusic);

                LoadBackgrounds();
                camera.SetLevelBounds(level.Width, level.Height, tileset.TileSize);

                ObjectManager.AddObject(player);


                //search for the correct door to spawn at
                Vector2 spawnpos = level.SpawnPoint;
                Door? targetDoor = level.LevelObjects.OfType<Door>().FirstOrDefault(d => d.DoorId == targetDoorId);

                if(targetDoor != null)
                {
                    spawnpos = targetDoor.Position;
                }
                else if (!string.IsNullOrEmpty(targetDoorId))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Warning: no door with id '{targetDoorId}' found in room '{room}', using level spawn point.");
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                //actually set the players position
                player.Position = new Vector2(spawnpos.X, spawnpos.Y);
            }

            //update camera
            camera.Update(dt);

            //very first thing, check if the window is resized for rendering reasons
            if (Raylib.IsWindowResized())
            {
                screen.UpdateDimensions();
            }
            var mousePos = screen.GetMousePosition();

            //set the visible rectangle
            Rectangle currentCameraView = camera.GetViewRect();

            //reset the player if they go under
            if(player.Position.Y > (level.Height * 8) + 16)
            {
                Death();
            }

            /*
            Rendering Part 01: actual game stuff.

            this is where the game will do its rendering to a low level render texture rather than the full screen size.

            used for most world space items.
            */

            Raylib.BeginTextureMode(screen.Target);

                Raylib.ClearBackground(Color.SkyBlue);

                BackgroundRenderer.Draw(level, bgTextures, currentCameraView, 320, 180);

            Raylib.BeginMode2D(camera.RaylibCamera);

                //draw layer 1, the decor ish layer
                    LevelRenderer.DrawLayer(level, level.Layer1, tileset, currentCameraView);

                //draw all of the objects
                    ObjectManager.DrawWorld(currentCameraView);

                //draw layer 2
                    LevelRenderer.DrawLayer(level, level.Layer2, tileset, currentCameraView);

                //draw collisions if we must
                if (Debug)
                {
                    ObjectManager.DrawDebugWorld(currentCameraView);
                }

            Raylib.EndMode2D();

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
                Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth() / 8, Raylib.GetScreenHeight(), new Color(0, 0, 0, 128));
                Raylib.DrawText(Raylib.GetFPS().ToString(), 0, 0, Raylib.GetScreenHeight()/45, new Color(0, 255, 0, 255));
                Raylib.DrawText($"Player Velocity:\nX: {player.Velocity.X}\nY: {player.Velocity.Y}", 0, Raylib.GetScreenHeight() / 45, Raylib.GetScreenHeight() / 45, new Color(0, 255, 0, 255));
            }

            Raylib.EndDrawing();

        }

        audio.ShutDown();
        screen.Unload();
        Raylib.CloseWindow();
    }

    public static void Death()
    {
        Raylib.WaitTime(1);
        player.Position = new Vector2(level.SpawnPoint.X - player.Size.X / 2, level.SpawnPoint.Y - player.Size.Y / 2);
        player.Velocity = Vector2.Zero;
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
}