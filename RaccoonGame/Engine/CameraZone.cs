using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;

public class CameraZone : GameObject
{
    //flags set by LoadFromJson
    public bool HasOffset;
    public Vector2 Offset;

    public bool OneWayX;
    public int OneWayXDir = 1;
    public bool OneWayY;
    public int OneWayYDir = 1;

    public bool BorderX;
    public bool BorderY;

    //runtime
    float? capturedX, capturedY;
    float? ratchetX, ratchetY;

    public override void Start()
    {
        IsSolid = false;
        if (Size == Vector2.Zero) Size = new Vector2(8, 8);
    }

    public override void Update(float deltaTime)
    {
        //not needed
    }

    public override void LoadFromJson(JsonElement json)
    {
        float width = TryGetPropertyCI(json, "width", out var w) ? w.GetSingle() : 8f;
        float height = TryGetPropertyCI(json, "height", out var h) ? h.GetSingle() : 8f;
        Size = new Vector2(width, height);

        if(TryGetPropertyCI(json, "offsetX", out var ox) && ox.ValueKind != JsonValueKind.String &&
            TryGetPropertyCI(json, "offsetY", out var oy) && oy.ValueKind != JsonValueKind.String)
        {
            HasOffset = true;
            Offset = new Vector2(ox.GetSingle(), oy.GetSingle());
        }

        if(TryGetPropertyCI(json, "oneWayXDir", out var owx) && owx.ValueKind != JsonValueKind.String) { OneWayX = true; OneWayXDir = owx.GetInt32(); }
        if (TryGetPropertyCI(json, "oneWayYDir", out var owy) && owy.ValueKind != JsonValueKind.String) { OneWayY = true; OneWayYDir = owy.GetInt32(); }

        if (TryGetPropertyCI(json, "borderX", out var bxp)) { BorderX = true; BorderX = bxp.GetBoolean(); }
        if (TryGetPropertyCI(json, "borderY", out var byp)) { BorderY = true; BorderY = byp.GetBoolean(); }
    }

    public void OnEnter(GameCamera camera) { ratchetX = ratchetY = null; if (HasOffset) camera.PersistantOffset = Offset; }
    public void OnExit() { ratchetX = ratchetY = null; }

    public void Apply(GameCamera camera, GameObject target, ref Vector2 desired)
    {

        if (OneWayX)
        {
            ratchetX ??= desired.X;
            desired.X = OneWayXDir >= 0 ? System.Math.Max(desired.X, ratchetX.Value) : System.Math.Min(desired.X, ratchetX.Value);
            ratchetX = desired.X;
        }
        if (OneWayY)
        {
            ratchetY ??= desired.Y;
            desired.Y = OneWayYDir >= 0 ? System.Math.Max(desired.Y, ratchetY.Value) : System.Math.Min(desired.Y, ratchetY.Value);
            ratchetY = desired.Y;
        }

        if (BorderX)
        {
            float minX = BoundingBox.X;
            float maxX = BoundingBox.X + BoundingBox.Width - camera.ViewWidth;
            if (maxX < minX) maxX = minX;
            desired.X = System.Math.Clamp(desired.X, minX, maxX);
        }
        if (BorderY)
        {
            float minY = BoundingBox.Y;
            float maxY = BoundingBox.Y + BoundingBox.Height - camera.ViewHeight;
            if (maxY < minY) maxY = minY;
            desired.Y = System.Math.Clamp(desired.Y, minY, maxY);
        }
    }
}