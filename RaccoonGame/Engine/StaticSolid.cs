using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Text.Json;

public class StaticSolid : GameObject
{
    public override void Start()
    {
        IsSolid = true;
        //no graphic needed
    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void LoadFromJson(JsonElement json)
    {
        float width = json.TryGetProperty("width", out var w) ? w.GetSingle() : 8f;
        float height = json.TryGetProperty("height", out var h) ? h.GetSingle() : 8f;
        Size = new Vector2(width, height);
    }
}