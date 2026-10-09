using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;


public class VirtualScreen
    {

        //the width and height of the screen
        public int Width { get; private set; }
        public int Height { get; private set; }

        //the render texture that the game draws to before upscaling
        public RenderTexture2D Target { get; private set; }

        //where and how the rendertexture is drawn
        public Rectangle SourceRect { get; private set; }
        public Rectangle DestRect { get; private set; }

        public VirtualScreen(int virtualWidth, int virtualHeight)
        {
            //set the width and height to the appropriate values
            Width = virtualWidth;
            Height = virtualHeight;

            //initialize the render texture and make it CRISPY
            Target = Raylib.LoadRenderTexture(virtualWidth, virtualHeight);
            Raylib.SetTextureFilter(Target.Texture, TextureFilter.Point);

            //set the source rect and initialize the scale
            SourceRect = new Rectangle(0, 0, Width, -Height);
            UpdateDimensions();
        }

        public void UpdateDimensions()
        {
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            //see how big we can scale before the aspect ration doesnt work
            //float scale = Math.Min((float)windowWidth / Width, (float)windowHeight / Height);

            int scale = Math.Max(1,
                (int)Math.Floor(
                    Math.Min(
                        (float)windowWidth / Width,
                        (float)windowHeight / Height
                    )
                )
            );

            //update DestRect
            DestRect = new Rectangle(
                    (windowWidth - (Width * scale)) * 0.5f,
                    (windowHeight - (Height * scale)) * 0.5f,
                    Width * scale,
                    Height * scale
                );
        }

        //helper function to get the mouse position more easily
        public Vector2 GetMousePosition()
        {
            Vector2 mouse = Raylib.GetMousePosition();
            float x = Math.Clamp((mouse.X - DestRect.X) / DestRect.Width * Width, 0, Width);
            float y = Math.Clamp((mouse.Y - DestRect.Y) / DestRect.Height * Height, 0, Height);
            return new Vector2(x, y);
        }

        public void Unload()
        {
            Raylib.UnloadRenderTexture(Target);
        }
    }
