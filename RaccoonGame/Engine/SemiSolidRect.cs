using System;
using System.Collections.Generic;
using System.Text;
using Raylib_cs;
using System.Numerics;
using System.Text.Json;

public class SemiSolidRect : GameObject
{
    public override void Start()
    {
        IsSolid = false; //this isnt normal collision
    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void LoadFromJson(JsonElement json)
    {
        float width = TryGetPropertyCI(json, "width", out var w) ? w.GetSingle() : 8f;
        float height = TryGetPropertyCI(json, "height", out var h) ? h.GetSingle() : 8f;
        Size = new Vector2(width, height);
    }
}