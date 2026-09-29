using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Numerics;
using Raylib_cs;

public class Door : GameObject
{

    //texture to represent the door in-game
    Texture2D dore;

    //room and door id, pretty self explanatory
    public string Room = "";
    public string DoorId = "";

    public override void Start()
    {
        //controls what order it gets drawn in-- in this case almost always the back
        DrawOrder = -10;

        //load the texture
        dore = Raylib.LoadTexture("assets/sprites/objects/dore.png");

        //it shouldnt be solid; set the size to 16x16
        IsSolid = false;
        Size = new Vector2(16, 16);

        //initialize the graphic
        Graphic = new Sprite2D(dore);
    }

    public override void Update(float deltaTime)
    {
        if (!LevelManager.CanEnterDoor) return;

        foreach(var obj in CurrentlyColliding)
        {
            if(obj is Autumn && Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                LevelManager.RequestTransition(Room, DoorId);
                break;
            }
        }
    }

    public override void LoadFromJson(JsonElement json)
    {
        //get room and door id from the JSON
        Room = json.GetProperty("room").GetString() ?? "";
        DoorId = json.GetProperty("DoorId").GetString() ?? "";

    }
}