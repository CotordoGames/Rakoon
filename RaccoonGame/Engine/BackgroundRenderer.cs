using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

public static class BackgroundRenderer
{
    //draw the bgs
    public static void Draw(Level level, List<Texture2D> textures, Rectangle cameraView, int screenW, int screenH)
    {
        //loop through all the textures
        for (int i = 0; i < textures.Count; i++)
        {
            float speed = level.BGSpeeds[i];
            Vector2 offset = level.BGOffsets[i];
            var tex = textures[i];
            

            //offsets in world space
            float offsetX = cameraView.X * speed;
            float offsetY = cameraView.Y * speed;

            //whether it should tile
            bool tileX = tex.Width < screenW;
            bool tileY = tex.Height < screenH;

            if (tileX)
            {
                //start at the right place
                float startX = -Mod(offsetX, tex.Width);
                for(float x = startX; x < screenW; x += tex.Width)
                {
                    //fill up the screen and keep going
                    DrawColumn(tex, x + offset.X, offsetY - offset.Y, tileY, screenH);
                }
            }
            else
            {
                //just fill up the screen
                DrawColumn(tex, -offsetX + offset.X, offsetY - offset.Y, tileY, screenH);
            }
        }
    }

    static void DrawColumn(Texture2D tex, float x, float offsetY, bool tileY, int screenH)
    {
        if (tileY)
        {
            //set it to the beginning point in world space
            float startY = -Mod(offsetY, tex.Height);

            for(float y = startY; y < screenH; y += tex.Height)
            {
                Raylib.DrawTextureV(tex, Vector2.Round(new Vector2(x, y)), Color.White);
            }
        }
        else
        {
            Raylib.DrawTextureV(tex, Vector2.Round(new Vector2(x, -offsetY)), Color.White);
        }
    }

    static float Mod(float a, float b) => ((a % b) + b) % b;
}