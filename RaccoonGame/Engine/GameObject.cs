using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Raylib_cs;

public abstract class GameObject
{
    //transform
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Size;
    public Vector2 ColliderOffset;
    public bool IsGrounded = false;
    public bool IsTouchingWallLeft = false;
    public bool IsTouchingWallRight = false;
    public List<GameObject> CurrentlyColliding = new List<GameObject>();
    public int DrawOrder = 0;

    //default set to size but can be changed
    Vector2? colliderSizeOverride = null;
    public Vector2 ColliderSize
    {
        get => colliderSizeOverride ?? Size;
        set => colliderSizeOverride = value;
    }

    //extra info
    public bool IsSolid = false;
    public bool CanMove = false;
    public bool Active = true;

    //graphics
    public Drawable? Graphic { get; set; } = null;
    public Color DebugColor { get; set; } = Color.Magenta;

    //the collider
    public Rectangle BoundingBox => new Rectangle(
            Position.X + ColliderOffset.X,
            Position.Y + ColliderOffset.Y,
            ColliderSize.X,
            ColliderSize.Y
        );


    //to load objects from a JSON file
    public virtual void LoadFromJson(JsonElement json) { }

    //functions that the object uses to change itself
    public abstract void Start();
    public abstract void Update(float deltaTime);

    public static bool TryGetPropertyCI(JsonElement json, string name, out JsonElement value)
    {
        foreach(var prop in json.EnumerateObject())
        {
            if(string.Equals(prop.Name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }
        value = default;
        return false;
    }
}

