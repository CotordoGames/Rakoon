using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public class EnemyTrigger : GameObject
{
    public bool FlipDirection = true;
    public bool Jump = false;
    public float JumpHeight = 6;
    public override void Start()
    {
        IsSolid = false;
        CanMove = false;
    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void LoadFromJson(JsonElement json)
    {
        if (TryGetPropertyCI(json, "FlipDirection", out var flip)) FlipDirection = flip.GetBoolean();
        if (TryGetPropertyCI(json, "Jump", out var jump)) Jump = jump.GetBoolean();
        if (TryGetPropertyCI(json, "JumpHeight", out var jh)) JumpHeight = jh.GetSingle();

        if (TryGetPropertyCI(json, "width", out var w)) Size.X = w.GetSingle();
        if (TryGetPropertyCI(json, "height", out var h)) Size.Y = h.GetSingle();
    }
}