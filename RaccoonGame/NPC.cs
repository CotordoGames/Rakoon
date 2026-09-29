using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;
using Raylib_cs;
using System.Text.Json;

public class NPC : GameObject
{
    string[] lines = System.Array.Empty<string>();
    float[] speeds = System.Array.Empty<float>();
    int[] portraits = System.Array.Empty<int>();
    bool openingAnimation = true;

    public override void Start()
    {
        IsSolid = false;
        Size = new Vector2(16, 16);
        ColliderSize = new Vector2(64, 24);
        ColliderOffset = new Vector2((-ColliderSize.X/2) + 8, (-ColliderSize.Y/2) + 8);
    }

    public override void Update(float deltaTime)
    {
        //if the dialogue is already happening, no sense in trying another
        if (DialogueManager.IsOpen)
        {
            return;
        }

        foreach(var obj in CurrentlyColliding)
        {
            if(obj is Autumn && Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                Console.WriteLine("talk");
                DialogueManager.TriggerDialogue(lines, speeds, portraits, openingAnimation);
                break;
            }
        }
    }

    public override void LoadFromJson(JsonElement json)
    {
        if(TryGetPropertyCI(json, "text", out var t))
        {
            lines = t.Deserialize(AppJsonContext.Default.StringArray) ?? Array.Empty<string>();
        }

        if(TryGetPropertyCI(json, "openingAnimation", out var oa))
        {
            openingAnimation = oa.GetBoolean();
        }

        if(TryGetPropertyCI(json, "sprite", out var sprite))
        {
            Graphic = new Sprite2D(Raylib.LoadTexture($"assets/sprites/NPC/{sprite.GetString()}"));
        }

        if (TryGetPropertyCI(json, "flipX", out var flipX))
        {
            Graphic.FlipX = flipX.GetBoolean();
        }

        if(TryGetPropertyCI(json, "speeds", out var s))
        {
            speeds = s.Deserialize(AppJsonContext.Default.SingleArray) ?? Array.Empty<float>();
        }

        if(TryGetPropertyCI(json, "portraits", out var p))
        {
            portraits = p.Deserialize(AppJsonContext.Default.Int32Array) ?? Array.Empty<int>();
        }
    }
}