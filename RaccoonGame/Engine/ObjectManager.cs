using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Raylib_cs;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public static class ObjectManager
{
    //master lists with all objects that we need to check in different scenarios
    public static List<GameObject> AllObjects = new List<GameObject>();
    public static List<GameObject> Solids = new List<GameObject>();
    public static List<GameObject> SemiSolids = new List<GameObject>();
    public static List<CameraZone> CameraZones = new List<CameraZone>();

    //persistant list of everything that can be drawn kept in order by DrawOrder
    //this survives frames
    static readonly List<GameObject> drawList = new List<GameObject>();

    //set this to true whenever we need to remove an object
    static bool drawListNeedsPurge = false;

    //array of objects that are currently on screen
    static readonly List<GameObject> debugVisible = new List<GameObject>(256);

    public static Font DebugFont = LoadDebugFont();
    
    static Font LoadDebugFont()
    {
        Font f = Raylib.LoadFontEx("assets/debug.ttf", 64, null, 0);
        Raylib.GenTextureMipmaps(ref f.Texture);
        Raylib.SetTextureFilter(f.Texture, TextureFilter.Trilinear);
        return f;
    }

    //a function to add an object into the master lists
    public static void AddObject(GameObject obj)
    {
        AllObjects.Add(obj);
        drawList.Add(obj);
        obj.Start();
        if (obj.IsSolid) Solids.Add(obj);
        if (obj is CameraZone cz) CameraZones.Add(cz);
        if (obj is SemiSolidRect) SemiSolids.Add(obj);
        if (obj.DebugColor == default) obj.DebugColor = Color.Blue;
        for(int i = 0; i < obj.Components.Count; i++)
        {
            obj.Components[i].Owner = obj; //apply the owner of the components to the owner feild of each
        }
    }

    //hendles clearing out unused objects each frame
    static void EnsureDrawListReady()
    {
        //if something was removed, remove the now dead objects
        if (drawListNeedsPurge)
        {
            int w = 0; //write index-- where the next surviving object goes

            //r is the read index, it goes through each list entry
            for (int r = 0; r < drawList.Count; r++)
            {
                //object is active? move it  to the write position where not dead objects go
                if (drawList[r].Active) drawList[w++] = drawList[r];

                //dead objects are skipped and r moves one, but w doesnt
            }

            //remove leftover dupes
            drawList.RemoveRange(w, drawList.Count - w);
            drawListNeedsPurge = false; //reset
        }

        //insertion sort by draw order
        //if the list is already sorted, the while loop never runs, thus saving time
        for(int i = 1; i < drawList.Count; i++)
        {
            var item = drawList[i]; //object we are placing
            int j = i - 1; //compare against the one just before by draw order
            while(j >= 0 && drawList[j].DrawOrder > item.DrawOrder)
            {
                drawList[j + 1] = drawList[j];
                j--;
            }
            drawList[j + 1] = item;
        }
    }

    //deletes all objects, great for switching a scene
    public static void ClearWorld()
    {
        AllObjects.Clear();
        Solids.Clear();
        CameraZones.Clear();
        SemiSolids.Clear();
        drawList.Clear();
    }

    static void Step(GameObject entity, float MoveX, float MoveY)
    {
        //x axis first-- resolved separately for wall sliding
        entity.Position.X += MoveX;
        Rectangle boxX = entity.BoundingBox; //snapshot of where we are after moving

        foreach(var solid  in Solids)
        {
            if(!IsBlocking(entity, solid)) continue;

            Rectangle sb = solid.BoundingBox;

            if(Raylib.CheckCollisionRecs(boxX, sb))
            {
                if(MoveX > 0)
                    entity.Position.X = sb.X - entity.ColliderSize.X - entity.ColliderOffset.X;
                else if (MoveX < 0)
                    entity.Position.X = sb.X + sb.Width - entity.ColliderOffset.X;

                entity.Velocity.X = 0;
                boxX = entity.BoundingBox; //refresh the snapshot
            }
        }

        // y axis
        entity.Position.Y += MoveY;
        Rectangle boxY = entity.BoundingBox; //snapshot of where we are after moving

        foreach (var solid in Solids)
        {
            if (!IsBlocking(entity, solid)) continue;

            Rectangle sb = solid.BoundingBox;

            if (Raylib.CheckCollisionRecs(boxY, sb))
            {
                if (MoveY > 0)
                    entity.Position.Y = sb.Y - entity.ColliderSize.Y - entity.ColliderOffset.Y;
                else if (MoveY < 0)
                    entity.Position.Y = sb.Y + sb.Height - entity.ColliderOffset.Y;

                entity.Velocity.Y = 0;
                boxY = entity.BoundingBox; //refresh the snapshot
            }
        }

        //semisolids
        if(MoveY > 0)
        {
            foreach(var semi in SemiSolids)
            {
                if(!semi.Active) continue;

                Rectangle sb = semi.BoundingBox;

                float prevBottom = boxY.Y + boxY.Height - MoveY;

                if (prevBottom > sb.Y) continue;

                if (Raylib.CheckCollisionRecs(boxY, sb))
                {
                    entity.Position.Y = sb.Y - entity.ColliderSize.Y - entity.ColliderOffset.Y;
                    entity.Velocity.Y = 0;
                    boxY = entity.BoundingBox; //refresh the snapshot
                    
                }
            }
        }
    }

    static void UpdateProbes(GameObject entity)
    {
        Rectangle b = entity.BoundingBox; //reuse for all 3 probes

        Rectangle groundProbe = new Rectangle(b.X, b.Y + b.Height, b.Width, 1);
        Rectangle wallProbeLeft = new Rectangle(b.X - 1, b.Y, 1, b.Height);
        Rectangle wallProbeRight = new Rectangle(b.X + b.Width, b.Y, 1, b.Height);

        //local bools rather than refs because simpler
        bool grounded = false, wallLeft = false, wallRight = false;

        foreach(var solid in Solids)
        {
            //you cant be grounded on someting that isnt solid, ect
            if (!IsBlocking(entity, solid)) continue;

            Rectangle sb = solid.BoundingBox;

            //after a probe finds something, no need to check any more for that condition
            if (!grounded && Raylib.CheckCollisionRecs(groundProbe, sb)) grounded = true;
            if (!wallLeft && Raylib.CheckCollisionRecs(wallProbeLeft, sb)) wallLeft = true;
            if (!wallRight && Raylib.CheckCollisionRecs(wallProbeRight, sb)) wallRight = true;


            //nothing left to find
            if (grounded && wallLeft && wallRight) break;
        }

        //semisolids
        if(!grounded && entity.Velocity.Y == 0)
        {
            foreach(var semi in SemiSolids)
            {
                if(!semi.Active) continue;

                if(Raylib.CheckCollisionRecs(groundProbe, semi.BoundingBox) && groundProbe.Y + groundProbe.Height > semi.BoundingBox.Y)
                {
                    grounded = true;
                    break; //one is all it takes
                }
            }
        }


        //pass results to entity
        entity.IsGrounded = grounded;
        entity.IsTouchingWallLeft = wallLeft;
        entity.IsTouchingWallRight = wallRight;
    }


    //runs all object's update functions
    public static void UpdateWorld(float deltaTime)
    {
        for(int i = AllObjects.Count - 1; i >= 0; i--)
        {
            var obj = AllObjects[i];

            //if its not active, dont do anything
            if (!obj.Active)
            {
                drawListNeedsPurge = true;
                if (obj.IsSolid) Solids.Remove(obj);
                if (obj is CameraZone cz2) CameraZones.Remove(cz2);
                if (obj is SemiSolidRect) SemiSolids.Remove(obj);
                AllObjects.RemoveAt(i);
                continue;
            }

            obj.Update(deltaTime);
            obj.Graphic?.Update(deltaTime);
            obj.UpdateComponents(deltaTime);

            if (obj.CanMove)
            {
                //bool grounded = false;
                //bool wallLeft = false;
                //bool wallRight = false;

                ////dt shit
                //float stepX = obj.Velocity.X * deltaTime * 60.0f;
                //float stepY = obj.Velocity.Y * deltaTime * 60.0f;

                //int steps = Math.Max(1, (int)MathF.Ceiling(Math.Max(MathF.Abs(stepX), MathF.Abs(stepY)) / 4f));
                //for(int s = 0; s < steps; s++)
                //{
                //    MoveAndCollide(obj, stepX / steps, stepY / steps, ref grounded, ref wallLeft, ref wallRight);
                //}

                ////MoveAndCollide(obj, obj.Velocity.X, obj.Velocity.Y, ref grounded, ref wallLeft, ref wallRight);
                //obj.IsGrounded = grounded;
                //obj.IsTouchingWallLeft = wallLeft;
                //obj.IsTouchingWallRight = wallRight;

                float stepX = obj.Velocity.X * deltaTime * 60.0f;
                float stepY = obj.Velocity.Y * deltaTime * 60.0f;

                int steps = Math.Max(1, (int)MathF.Ceiling(Math.Max(MathF.Abs(stepX), MathF.Abs(stepY)) / 4f));

                for(int s = 0; s < steps; s++)
                {
                    Step(obj, stepX / steps, stepY / steps);
                }

                UpdateProbes(obj);
            } 

        }

        //additional pass in order for objects to "know" what all they are collidion with

        //foreach(var obj in AllObjects)
        //{
        //    obj.CurrentlyColliding.Clear();
        //}

        //for(int i = 0; i < AllObjects.Count; i++)
        //{
        //    var a = AllObjects[i];
        //    if (!a.Active) continue;

        //for (int j = i + 1; j < AllObjects.Count; j++)
        //{
        //    var b = AllObjects[j];
        //    if (!b.Active) continue;

        //    if (Raylib.CheckCollisionRecs(a.BoundingBox, b.BoundingBox))
        //    {
        //        a.CurrentlyColliding.Add(b);
        //        b.CurrentlyColliding.Add(a);
        //    }
        //}
        //}

        for (int i = 0; i < AllObjects.Count; i++)
        {
            var a = AllObjects[i];

            a.CurrentlyColliding.Clear();

            if (!a.Active || !a.TracksCollisions) continue;

            Rectangle aBox = a.BoundingBox; //cache it

            for (int j = 0; j < AllObjects.Count; j++)
            {
                var b = AllObjects[j];
                if (!b.Active || b == a) continue;

                if (Raylib.CheckCollisionRecs(aBox, b.BoundingBox))
                {
                    a.CurrentlyColliding.Add(b);
                }
            }
        }
    }

    //draw all of the objects
    public static void DrawWorld(Rectangle cameraView)
    {
        if (!Program.DrawObjects) return;

        //sort and comp[act first so the loop below has an accurate list
        EnsureDrawListReady();

        //just a plain for-loop
        for(int i = 0; i < drawList.Count; i++)
        {
            var obj = drawList[i];

            //skip all inactive or non-drawable objects before we do any work because we are lazy fucks
            if (!obj.Active || obj.Graphic == null) continue;

            //only render stuff the camera can see
            Rectangle drawBounds = new Rectangle(obj.Position.X, obj.Position.Y, obj.Size.X, obj.Size.Y);
            if (!Raylib.CheckCollisionRecs(drawBounds, cameraView)) continue;

            Vector2 drawSize = obj.Size * obj.VisualScale; // objects now can multiply the graphic's draw size
            Vector2 drawPosition = obj.Position + new Vector2((obj.Size.X - drawSize.X) / 2, obj.Size.Y - drawSize.Y); //adjust the position

            obj.Graphic.Draw(Vector2.Round(drawPosition), Vector2.Round(drawSize), obj.TintColor);
        }
    }

    public static void DrawDebugWorld(Rectangle cameraView, float LineThick)
    {
        EnsureDrawListReady(); //compact + sort yadayda

        //cull once-- empty the buffer and collect only active objects that overlap the camera
        debugVisible.Clear();
        for(int i = 0; i < drawList.Count; i++)
        {
            var o = drawList[i];
            if (!o.Active) continue;

            Rectangle bounds = new Rectangle(o.Position.X, o.Position.Y, o.Size.X, o.Size.Y);
            if (Raylib.CheckCollisionRecs(bounds, cameraView)) debugVisible.Add(o);
        }

        //since draw list was already sorted, this list is too by default.

        //loop over the rectangles only once and keep them separate from the text so raylib can batch them
        for (int i = 0; i < debugVisible.Count; i++)
        {
            var obj = debugVisible[i];

            //collider rect in world space rounded to whole pixels
            Rectangle debugRect = new Rectangle(Vector2.Round(obj.Position) + Vector2.Round(obj.ColliderOffset), Vector2.Round(obj.ColliderSize));

            if(obj is CameraZone)
            {
                if (Program.DebugCameraZones)
                {
                    Raylib.DrawRectangleRec(debugRect, new Color(0, 255, 0, 24));
                    Raylib.DrawRectangleLinesEx(debugRect, LineThick, new Color(0, 255, 0, 255));
                }
            }
            else
            {
                Raylib.DrawRectangleRec(debugRect, new Color((byte)obj.DebugColor.R, (byte)obj.DebugColor.G, (byte)obj.DebugColor.B, (byte)48));
                Raylib.DrawRectangleLinesEx(debugRect, LineThick, obj.DebugColor);
            }
        }

        //second loop-- only text labels
        if (Program.DebugText)
        {
            for (int i = 0; i < debugVisible.Count; i++)
            {
                var textobj = debugVisible[i];

                //hide camera zone labels unless camera zone debugging is on
                if (textobj is CameraZone && !Program.DebugCameraZones) continue;

                //camera zones get extra info, but everything else gets generalized becuase im hella racist
                string label = textobj is CameraZone camz ? $"{textobj.GetType()}\n" +
                    $"size: {textobj.ColliderSize}" +
                    $"borders: {camz.BorderX}, {camz.BorderY}\n" +
                    $"offsets: {camz.Offset}"
                    : 
                    $"{textobj.GetType()}\n" +
                    $"size: {textobj.ColliderSize}";

                Raylib.DrawTextPro(DebugFont, label, textobj.Position, Vector2.Zero, 0f, 4, 0, Color.White);
            }
        }
    }

    //helper
    static bool IsBlocking(GameObject entity, GameObject solid)
    {
        return solid != entity
            && solid.Active
            && (entity.BlockedBy & solid.Layer) != 0;
    }
}
