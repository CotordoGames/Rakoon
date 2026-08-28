using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;
using System.Text;

public class GameCamera
{
    public Vector2 Position; //top left of view in pixel space
    public int ViewWidth, ViewHeight;
    public float Smoothing = 1.0f;

    //what the camera follows and how smoothed it is
    GameObject? target;
    int levelPixelWidth, levelPixelHeight;

    //constructor
    public GameCamera(int viewWidth, int viewHeight)
    {
        ViewWidth = viewWidth;
        ViewHeight = viewHeight;
    }

    //sets the target
    public void SetTarget(GameObject obj) => target = obj;

    //sets the level bounds
    public void SetLevelBounds(int levelWidthTiles, int levelHeightTiles, int tileSize)
    {
        levelPixelWidth = levelWidthTiles * tileSize;
        levelPixelHeight = levelHeightTiles * tileSize;
    }

    //updates; runs every frame
    public void Update(float deltaTime)
    {
        //doesnt need to update if theres nothing to follow
        if (target == null) return;

        //get the center of the target
        Vector2 targetCenter = target.Position + target.Size / 2.0f;

        //where the camera wants to go
        Vector2 desired = targetCenter - new Vector2(ViewWidth / 2.0f, ViewHeight / 2.0f);

        //make sure the camera never leaves the bounds of the level
        float maxX = Math.Max(0, levelPixelWidth - ViewWidth);
        float maxY = Math.Max(0, levelPixelHeight - ViewHeight);
        desired.X = Math.Clamp(desired.X, 0, maxX);
        desired.Y = Math.Clamp(desired.Y, 0, maxY);

        //update the position
        Position = Vector2.Round(Vector2.Lerp(Position, desired, Smoothing * deltaTime * 60));
    }

    //get the viewports transform
    public Rectangle GetViewRect() => new Rectangle(Position.X, Position.Y, ViewWidth, ViewHeight);

    //the actual camera
    public Camera2D RaylibCamera => new Camera2D
    {
        Target = Position,
        Offset = Vector2.Zero,
        Rotation = 0.0f,
        Zoom = 1f
    };
}