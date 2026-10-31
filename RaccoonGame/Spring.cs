using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Text.Json;
using Raylib_cs;
using System.Linq.Expressions;

public class Spring : GameObject
{
    public float BounceHeight = 8;

    static Texture2D texture;
    static bool TextureLoaded = false;

    public static bool mboing = false;

    public AnimatedSprite2D? anim;
    public override void Start()
    {
        Size = new Vector2(8, 8);
        ColliderSize = new Vector2(8, 4);
        ColliderOffset = new Vector2(0, 4);
        IsSolid = true;

        if (!TextureLoaded)
        {
            texture = Raylib.LoadTexture("assets/sprites/objects/spring.png");
            TextureLoaded = true;
        } 

        Graphic = new AnimatedSprite2D(texture, 8, 8);

        anim = (AnimatedSprite2D)Graphic;

        anim.AddAnimation("default", [0], 1, loop: true);
        anim.AddAnimation("bounce", [ 1, 2, 3, 0], 0.075f, loop: false);

        anim.Play("default", restart: true);

    }

    public override void Update(float deltaTime)
    {
        foreach (var obj in CurrentlyColliding)
        {
            if(obj is Autumn)
            {
                anim?.Play("bounce", false);
            }
        }

        if (anim != null && !anim.IsPlaying)
        {
            anim.Play("default", restart: true);
        }
    }

    public override void LoadFromJson(JsonElement json)
    {
        if(TryGetPropertyCI(json, "BounceHeight", out var bh))
        {
            BounceHeight = bh.GetSingle();
            EngineDebug.Log(bh.GetSingle().ToString());
        }
    }
}