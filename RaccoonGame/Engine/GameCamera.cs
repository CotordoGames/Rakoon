using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;
using System.Text;

public class GameCamera
{
    public Vector2 Position; //top left of view in pixel space
    public int ViewWidth, ViewHeight;
    public Vector2 Smoothing = new Vector2(0.125f, 0.1f);

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


    //current offset(that gets set as a result of camera zones)
    public Vector2 PersistantOffset = Vector2.Zero;

    //a set of camera zones which can currently be interacted with
    HashSet<CameraZone> activeZones = new HashSet<CameraZone>();
    public void Update(float deltaTime)
    {
        //if theres nothing to follow, dont update
        if(target == null)
        {
            return;
        }

        //get the center of the target
        Vector2 targetCenter = target.Position + target.Size / 2.0f;

        //where the camera wants to go
        Vector2 desired = targetCenter - new Vector2(ViewWidth / 2.0f, ViewHeight / 2.0f) + PersistantOffset;

        //add any zones whgich have become active to the list of zones we check
        var currentZones = new HashSet<CameraZone>();
        foreach(var zone in ObjectManager.CameraZones)
        {
            if (zone.Active && Raylib.CheckCollisionPointRec(targetCenter, zone.BoundingBox))
            {
                currentZones.Add(zone);
            }
        }

        //check how we are interacting with the zones
        foreach (var zone in currentZones) if (!activeZones.Contains(zone)) zone.OnEnter(this);
        foreach (var zone in activeZones) if (!currentZones.Contains(zone)) zone.OnExit();
        activeZones = currentZones;

        //apply the effects of any zones we interact with
        foreach (var zone in activeZones)
        {
            zone.Apply(this, target, ref desired);
        }

        //make sure the camera never leaves the bounds of the level
        float maxX = Math.Max(0, levelPixelWidth - ViewWidth);
        float maxY = Math.Max(0, levelPixelHeight - ViewHeight);
        desired.X = Math.Clamp(desired.X, 0, maxX);
        desired.Y = Math.Clamp(desired.Y, 0, maxY);

        //update the position
        Position = new Vector2(
            float.Lerp(Position.X, desired.X, Smoothing.X * deltaTime * 60),
            float.Lerp(Position.Y, desired.Y, Smoothing.Y * deltaTime * 60));
    }

    //get the viewports transform
    public Rectangle GetViewRect() => new Rectangle(Position.X, Position.Y, ViewWidth, ViewHeight);

    //the actual camera
    public Camera2D RaylibCamera => new Camera2D
    {
        Target = Vector2.Round(Position),
        Offset = Vector2.Zero,
        Rotation = 0.0f,
        Zoom = 1f
    };
}