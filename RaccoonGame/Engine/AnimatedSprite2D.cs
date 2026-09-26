using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

class AnimatedSprite2D : Drawable
{
    //define basic and/or required variables
    public Texture2D Texture;
    public int FrameWidth;
    public int FrameHeight;
    public int Columns;

    //dictionary and more vars
    readonly Dictionary<string, Animation> animations = new();
    Animation? current;
    string? currentName;
    int frameIndex;
    float frameTimer;
    bool playing = true;

    public bool IsPlaying => playing;

    //constructor
    public AnimatedSprite2D(Texture2D texture, int frameWidth, int frameHeight)
    {
        Texture = texture;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        Columns = texture.Width / FrameWidth;
    }

    //function to add an animation
    public void AddAnimation(string name, int[] frames, float frameTime, bool loop = true) => animations[name] = new Animation(frames, frameTime, loop);

    //plays the selected animation
    public void Play(string name, bool restart = false)
    {
        if (currentName == name && !restart) return;
        if (!animations.TryGetValue(name, out var anim)) return;

        currentName = name;
        current = anim;
        frameIndex = 0;
        frameTimer = 0f;
        playing = true;
    }


    //update; runs every frame, in this case 60 time a second
    public override void Update(float deltaTime)
    {
        //if there is no current sprite, skip drawing alltogether
        if (current == null || !playing) return;

        //update the animations frame depending
        frameTimer += deltaTime;
        if(frameTimer >= current.FrameTime)
        {
            frameTimer -= current.FrameTime;
            frameIndex++;

            if(frameIndex >= current.Frames.Length)
            {
                if (current.Loop) frameIndex = 0;
                else { frameIndex = current.Frames.Length  - 1; playing = false; }
            }
        }
    }

    public override void Draw(Vector2 position, Vector2 size, Color tint)
    {
        if (current == null) return;

        int frame = current.Frames[frameIndex];
        int col = frame % Columns;
        int row = frame / Columns;

        float srcW = FlipX ? -FrameWidth : FrameWidth;
        float srcH = FlipY ? -FrameHeight : FrameHeight;
        Rectangle src = new Rectangle(col * FrameWidth, row * FrameHeight, srcW, FrameHeight);
        Rectangle dest = new Rectangle(position.X, position.Y, size.X, size.Y);

        Raylib.DrawTexturePro(Texture, src, dest, Vector2.Zero, 0f, tint);
    }

    class Animation
    {
        public int[] Frames;
        public float FrameTime;
        public bool Loop;
        public Animation(int[] frames, float frameTime, bool loop)
        {
            Frames = frames;
            FrameTime = frameTime;
            Loop = loop;
        }
    }
}
