using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Numerics;
using Raylib_cs;

public class Door : GameObject
{
    Texture2D dore;

    public string Room = "";
    public string DoorId = "";

    public override void Start()
    {
        DrawOrder = -10;

        dore = Raylib.LoadTexture("assets/sprites/objects/dore.png");

        IsSolid = false;
        Size = new Vector2(16, 16);

        Graphic = new Sprite2D(dore);
    }

    public override void Update(float deltaTime)
    {
        foreach(var obj in CurrentlyColliding)
        {
            if(obj is Player && Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                LevelManager.RequestTransition(Room, DoorId);
                break;
            }
        }
    }

    public override void LoadFromJson(JsonElement json)
    {
        Room = json.GetProperty("room").GetString() ?? "";
        DoorId = json.GetProperty("DoorId").GetString() ?? "";

    }
}