using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;

public class Ladder : GameObject
{
    //nevers moves; inst solid
    public override void Start()
    {
        IsSolid = false;
        CanMove = false;
    }

    //just needs to be here
    public override void Update(float deltaTime)
    {
        
    }

    //load it from a level JSON
    public override void LoadFromJson(JsonElement json)
    {
        float height = json.TryGetProperty("height", out var h) ? h.GetSingle() : 8f;
        Size = new Vector2(8.0f, height);
    }
}