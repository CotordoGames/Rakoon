using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
public class Sprite2D : Drawable
{
    //input values
    public Texture2D Texture;
    public Rectangle SourceRect;
    public bool flipX;


    //how it creates itself
    public Sprite2D(Texture2D texture)
    {
        Texture = texture;
        SourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
    }

    //drawing itsself
    public override void Draw(Vector2 position, Vector2 size, Color tint)
    {
        //do the flip thing
        if (FlipX)
        {
            SourceRect = new Rectangle(0, 0, -Texture.Width, SourceRect.Height);
        }
        else
        {
            SourceRect = new Rectangle(0, 0, Texture.Width, SourceRect.Height);
        }

        //check actual flipx
        if (FlipX)
        {
            flipX = true;
        }
        else
        {
            flipX = false;
        }
        Rectangle destRect = new Rectangle(position.X, position.Y, size.X, size.Y);
        Raylib.DrawTexturePro(Texture, SourceRect, destRect, Vector2.Zero, 0.0f, tint);
    }
}

