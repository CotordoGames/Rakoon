using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Raylib_cs;

public abstract class Drawable
{
    //just a way for all of these things to draw themselves
    public bool FlipX;
    public abstract void Draw(Vector2 position, Vector2 size, Color tint);
    public virtual void Update(float deltaTime) { }
}

