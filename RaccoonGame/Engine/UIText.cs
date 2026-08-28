using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

class UIText : Drawable
{
    public string text = "";

    public UIText(string Text)
    {
        text = Text;
    }

    public override void Draw(Vector2 position, Vector2 size, Color tint)
    {
        Raylib.DrawText(text, (int)position.X, (int)position.Y, 8, tint);
    }
}