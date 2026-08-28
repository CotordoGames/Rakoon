using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;

class HUDTest : GameObject
{
    string Text = "";
    public override void Start()
    {
        IsSolid = false;
        CanMove = false;
        DebugColor = Color.White;
        Graphic = new UIText(Text);
        Active = true;
    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void LoadFromJson(JsonElement json)
    {
        Text = json.GetProperty("text").GetString() ?? "";
        float width = json.TryGetProperty("width", out var w) ? w.GetSingle() : 8f;
        float height = json.TryGetProperty("height", out var h) ? h.GetSingle() : 8f;
        Size = new Vector2(width, height);
    }
}