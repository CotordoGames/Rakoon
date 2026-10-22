using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;

public class Mouser : Enemy
{
    static Texture2D texture;
    static bool TextureLoaded = false;

    public override void EnemyStart()
    {
        if (!TextureLoaded)
        {
            texture = Raylib.LoadTexture("assets/sprites/objects/mouse.png");
            TextureLoaded = true;
        }

        Graphic = new AnimatedSprite2D(texture, 8, 8);

        knockback = 6f;


        var anim = (AnimatedSprite2D)Graphic;
        anim.AddAnimation("run", [0, 1], 0.05f, true);
        anim.Play("run", true);

    }

    public override void OnTakeDamage()
    {
        Velocity = new Vector2(-knockback * direction, -4);
    }

    public override void EnemyUpdate(float deltaTime)
    {
        Graphic?.FlipX = direction < 0;
    }

    public override void EnemyLoad(JsonElement json)
    {
        Size = new Vector2(8, 8);
    }
}