using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Raylib_cs;
using System.Linq;

public static class ObjectManager
{
    //master lists with all objects that we need to check in different scenarios
    public static List<GameObject> AllObjects = new List<GameObject>();
    public static List<GameObject> Solids = new List<GameObject>();

    //a function to add an object into the master lists
    public static void AddObject(GameObject obj)
    {
        AllObjects.Add(obj);
        obj.Start();
        if (obj.IsSolid) Solids.Add(obj);
    }

    //deletes all objects, great for switching a scene
    public static void ClearWorld()
    {
        AllObjects.Clear();
        Solids.Clear();
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
                if (obj.IsSolid) Solids.Remove(obj);
                AllObjects.RemoveAt(i);
                continue;
            }

            obj.Update(deltaTime);
            obj.Graphic?.Update(deltaTime);

            if (obj.CanMove)
            {
                bool grounded = false;
                MoveAndCollide(obj, obj.Velocity.X, obj.Velocity.Y, ref grounded);
                obj.IsGrounded = grounded;
            } 
            


            //additional pass in order for objects to "know" what all they are collidion with

            foreach(var a in AllObjects)
            {
                //clear the list of objects that are colliding
                a.CurrentlyColliding.Clear();

                //dont do anything if the object isnt active
                if (!a.Active) continue;

                //check all of the objects to see if they collide
                foreach(var b in AllObjects)
                {
                    //if the object isnt active it isnt colliding
                    if(b == a || !b.Active) continue;

                    //actual collision check
                    if(Raylib.CheckCollisionRecs(a.BoundingBox, b.BoundingBox))
                    {
                        a.CurrentlyColliding.Add(b);
                    }
                }
            }

        }
    }

    //moves all objects and checks for collisions
    public static void MoveAndCollide(GameObject entity, float moveX, float moveY, ref bool isGrounded)
    {
        //check the X axis first
        entity.Position.X += moveX;
        Rectangle boxX = entity.BoundingBox;

        //loop through the list of solid active objects
        foreach(var solid in Solids)
        {
            //pass if the object isnt active or is the entity itself
            if (solid == entity || !solid.Active) continue;

            //if the object collided
            if(Raylib.CheckCollisionRecs(boxX, solid.BoundingBox))
            {
                if (moveX > 0) 
                    entity.Position.X = solid.BoundingBox.X - entity.ColliderSize.X - entity.ColliderOffset.X;
                else if(moveX < 0)
                    entity.Position.X = solid.BoundingBox.X + solid.BoundingBox.Width - entity.ColliderOffset.X;

                entity.Velocity.X = 0;
                boxX = entity.BoundingBox;
            }
        }

        //y axis time babyyy
        entity.Position.Y += moveY;
        Rectangle boxY = entity.BoundingBox;

        //loop once again
        foreach(var solid in Solids)
        {
            //dont do anything if the object collides with itself or if it inst active
            if(solid == entity || !solid.Active || !solid.Active) continue;

            if(Raylib.CheckCollisionRecs(boxY, solid.BoundingBox))
            {
                if(moveY > 0)
                {
                    entity.Position.Y = solid.BoundingBox.Y - entity.ColliderSize.Y - entity.ColliderOffset.Y;
                }
                else if(moveY < 0)
                {
                    entity.Position.Y = solid.BoundingBox.Y + solid.BoundingBox.Height - entity.ColliderOffset.Y;
                }

                entity.Velocity.Y = 0;
                boxY = entity.BoundingBox;
            }
        }

        //to check if the object is grounded
        Rectangle groundProbe = new Rectangle(
            entity.BoundingBox.X,
            entity.BoundingBox.Y + entity.BoundingBox.Height,
            entity.BoundingBox.Width,
            1);

        foreach(var solid in Solids)
        {
            if (solid == entity || !solid.Active) continue;
            if(Raylib.CheckCollisionRecs(groundProbe, solid.BoundingBox))
            {
                isGrounded = true;
                break;
            }
        }
    }

    //draw all of the objects
    public static void DrawWorld(Rectangle cameraView)
    {
        var sorted = AllObjects.Where(o => o.Active).OrderBy(o => o.DrawOrder); //sort the objects by draw order

        foreach(var obj in sorted)
        {
            //if the object isnt active, dont draw it
            if (!obj.Active) continue;

            //only render stuff the camera can see
            Rectangle drawBounds = new Rectangle(obj.Position.X, obj.Position.Y, obj.Size.X, obj.Size.Y);

            if (!Raylib.CheckCollisionRecs(drawBounds, cameraView)) continue;
            //check if the object actually HAS an image to avoid crashes
            if (obj.Graphic != null)
            {
                obj.Graphic.Draw(Vector2.Round(obj.Position), Vector2.Round(obj.Size), Color.White);
            }
        }
    }

    public static void DrawDebugWorld(Rectangle cameraView)
    {
        var sorted = AllObjects.Where(o => o.Active).OrderBy(o => o.DrawOrder); //sort the objects by draw order

        foreach(var obj in sorted)
        {
            //if the object isnt active, dont draw it
            if (!obj.Active) continue;

            //only render stuff the camera can see
            Rectangle drawBounds = new Rectangle(obj.Position.X, obj.Position.Y, obj.Size.X, obj.Size.Y);

            if (!Raylib.CheckCollisionRecs(drawBounds, cameraView)) continue;

            Rectangle debugRect = new Rectangle(Vector2.Round(obj.Position) + Vector2.Round(obj.ColliderOffset), Vector2.Round(obj.ColliderSize));
            Raylib.DrawRectangle((int)debugRect.X, (int)debugRect.Y, (int)debugRect.Width, (int)debugRect.Height, new Color(0, 0, 0, 64));
            Raylib.DrawRectangleLines((int)debugRect.X, (int)debugRect.Y, (int)debugRect.Width, (int)debugRect.Height, new Color(96, 128, 255, 255));
        }
    }
}
