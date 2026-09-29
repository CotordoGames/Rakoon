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
    public override void Start()
    {
        Size = new Vector2(8, 8);
        ColliderSize = new Vector2(12, 4);
        ColliderOffset = new Vector2(-2, 4);
        IsSolid = false;

        if (!TextureLoaded)
        {
            texture = Raylib.LoadTexture("assets/sprites/objects/spring.png");
            TextureLoaded = true;
        } 

        Graphic = new AnimatedSprite2D(texture, 8, 8);

        var anim = (AnimatedSprite2D)Graphic;

        anim.AddAnimation("default", [0], 1, loop: true);
        anim.AddAnimation("bounce", [ 1, 2, 3, 0], 0.075f, loop: false);

        anim.Play("default", restart: true);

    }

    public override void Update(float deltaTime)
    {
        var anim = (AnimatedSprite2D?)Graphic;
        foreach (var obj in CurrentlyColliding)
        {
            if(obj is Autumn)
            {
                anim?.Play("bounce", false);
            }
        }

        if (!anim.IsPlaying)
        {
            anim.Play("default", restart: true);
        }
    }

    public override void LoadFromJson(JsonElement json)
    {
        if(TryGetPropertyCI(json, "BounceHeight", out var bh))
        {
            BounceHeight = bh.GetSingle();
            Console.WriteLine(bh.GetSingle());
        }
    }
}