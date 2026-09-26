using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Numerics;

//the base Dialogue box class containing the data necicarry to create one
public class DialogueBox
{
    public int X, Y, W, H;
    public string[] Lines;
    public float[] Speeds;
    public int[] Portraits; //for a texture atlas.
    public bool OpeningAnimation; //whether to player a "growing" animation on creation.
}

public static class DialogueManager
{
    public static DialogueBox? Active;

    //values to determine how the portrait is rendered
    static Texture2D portraitAtlas;
    const int PortraitSize = 48;

    //active variables to keep track of shit
    static int lineIndex;
    static float visibleChars;
    static bool waitingOnPlayer;

    //store the text as separate shorter strings if its too long for the text box
    static List<string> wrappedLines = new();

    //for the eventual opening animation
    static float openTimer;
    const float OpenDuration = 0.36f;

    static float charTimer;

    //to tell whether the box is open or not
    public static bool IsOpen => Active != null;

    //init
    public static void Init(Texture2D atlas) => portraitAtlas = atlas;

    public static void TriggerDialogue(string[] lines, float[] speeds, int[]? portraits = null, bool openingAnimation = true, int height = 56)
    {
        int w = Program.ScreenWidth - 16;
        int x = (Program.ScreenWidth - w) / 2;
        int y = Program.ScreenHeight - height - 8;

        //put the textbox where it needs to be
        Active = new DialogueBox
        {
            X = x, Y = y, W = w, H = height,
            Lines = lines, Speeds = speeds,
            Portraits = portraits ?? new int[lines.Length],
            OpeningAnimation = openingAnimation
        };
        //open, start a line, stop the player
        openTimer = openingAnimation ? 0f : OpenDuration;
        StartLine(0);
        Program.player.CanMove = false;
    }

    //refer to trigger dialogue
    public static void TriggerNotice(string text, int height = 32, bool openingAnimation = true)
    {
        int w = Program.ScreenWidth - 64;
        int x = (Program.ScreenWidth - w) / 2;

        Active = new DialogueBox
        {
            X = x,
            Y = 8,
            W = w,
            H = height,
            Lines = new[] { text },
            Speeds = new[] { 0f },
            Portraits = new[] { -1 },
            OpeningAnimation = openingAnimation
        };
        openTimer = openingAnimation ? 0f : OpenDuration;
        StartLine(0); 
        Program.player.CanMove = false;
    }

    //starts a line
    static void StartLine(int index)
    {
        lineIndex = index;
        visibleChars = 0f;
        charTimer = 0f;
        waitingOnPlayer = false;
        WrapLine();
    }

    static void WrapLine()
    {
        //setup
        wrappedLines.Clear();
        var box = Active!;
        int textW = box.W - 8 - (box.Portraits[lineIndex] >= 0 ? PortraitSize + 6 : 0);
        string current = "";
        foreach(var word in box.Lines[lineIndex].Split(' '))
        {
            //see if the line is too long
            string test = current.Length == 0 ? word : current + " " + word;
            if(Raylib.MeasureTextEx(Program.Rakoon, test, 10f, 0f).X > textW)
            {
                //split that shit up
                wrappedLines.Add(current);
                current = word;
            }
            else
            {
                current = test;
            }
        }
        wrappedLines.Add(current);
    }

    static int TotalChars()
    {
        int sum = 0;
        foreach (var l in wrappedLines) sum += l.Length + 1; //+1 for the newline gap
        return sum > 0 ? sum - 1 : 0; //remove the last newline gap
    }

    public static void Update(float deltaTime)
    {
        //dont do anything if we arent existent
        if(Active == null) return;

        //open anim
        if(openTimer < OpenDuration)
        {
            openTimer += deltaTime;
            return;
        }

        //setup
        int total = TotalChars();
        float speed = Active.Speeds[lineIndex];

        //typewriter effect
        if (!waitingOnPlayer)
        {
            //visibleChars += speed <= 0f ? total : System.MathF.Min(total, visibleChars + speed * deltaTime);
            //if (visibleChars >= total) waitingOnPlayer = true;
            if(speed <= 0f)
            {
                visibleChars = total;
                waitingOnPlayer = true;
            }
            else
            {
                charTimer += deltaTime;

                while(charTimer >= speed && visibleChars < total)
                {
                    Program.audio.PlayOneShot("talk");
                    charTimer -= speed;
                    visibleChars++;
                }

                if(visibleChars >= total)
                {
                    waitingOnPlayer = true;
                }
            }
        }

        //advance on Z presses
        if (Raylib.IsKeyPressed(KeyboardKey.Z))
        {
            if (!waitingOnPlayer)
            {
                visibleChars = total;
                waitingOnPlayer = true;

            }
            else if (lineIndex < Active.Lines.Length - 1)
            {
                StartLine(lineIndex + 1);
            }
            else
            {
                Close();
                
            }
        }
    }

    //die.
    static void Close()
    {
        Active = null;
        Program.player.CanMove = true;
    }

    public static void Draw()
    {
        //dont do anything if we arent existant
        if (Active == null) return;
        var box = Active;

        //budget opening animation
        //float t = box.OpeningAnimation ? System.Math.Clamp(openTimer / OpenDuration, 0f, 1f) : 1f;

        float progress = box.OpeningAnimation ? Math.Clamp(openTimer / OpenDuration, 0f, 1f) : 1f;

        float t = EaseOutBack(progress);

        int h = (int)(box.H * t);
        int w = (int)(box.W * t);
        int x = box.X + (box.W - w) / 2;
        int y = box.Y + (box.H - h) / 2;

        //draw the box itself(and the outline)
        Raylib.DrawRectangle(x, y, w, h, new Color((byte)50, (byte)45, (byte)77, (byte)192));
        Raylib.DrawRectangleLines(x, y, w, h, new Color((byte)71, (byte)67, (byte)148, (byte)255));
        if (progress < 1f) return;

        //set up values for the portrait and text drawing
        int textX = box.X + 4;
        int portraitIndex = box.Portraits[lineIndex];
        if(portraitIndex >= 0)
        {
            //draw the protrait
            var src = new Rectangle(portraitIndex * PortraitSize, 0, PortraitSize, PortraitSize);
            Raylib.DrawTextureRec(portraitAtlas, src, new Vector2(box.X + 4, box.Y + 4), Color.White);
            textX += PortraitSize + 6;
        }

        //go through the text and draw it
        int shown = (int)visibleChars;
        int b_y = box.Y + 4;
        foreach(var line in wrappedLines)
        {
            string part = shown >= line.Length ? line : line.Substring(0, System.Math.Max(0, shown));
            Raylib.DrawTextEx(Program.Rakoon, part, new Vector2(textX, b_y), 10f, 0f, Color.White);
            shown -= line.Length + 1;
            b_y += 10;
            if (shown < 0) break;
        }
    }

    static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
    }
}