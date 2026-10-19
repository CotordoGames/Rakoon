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
    static int portraitColumns = 1;

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
    public static void Init(Texture2D atlas)
    {
        portraitAtlas = atlas;
        portraitColumns = Math.Max(1, atlas.Width / PortraitSize);
    }


    /// <summary>
    /// triggers a dialogue interaction
    /// </summary>
    /// <param name="lines">the lines in this dialogue interaction</param>
    /// <param name="speeds">the speed at ehich each line is typed</param>
    /// <param name="portraits">the portrait to disp;ay for each line(-1  for none)</param>
    /// <param name="openingAnimation">whether the textbox displays it's opening animation</param>
    /// <param name="height">how tall the textbox is in pixels</param>
    /// <param name="tbtype">the type of textbox that shall be used for the interaction</param>
    public static void TriggerDialogue(string[] lines, float[] speeds, int[]? portraits = null, bool openingAnimation = true, int height = 56, int tbtype = 0)
    {
        //(if i were smart, id just use an enum for this)
        //TB TYPE LAYOUT:
        //0: regular, textbox is at bottom and takes up width of the screen -16 pixels
        //1: top regular, appears at top but still has a normal width

        /*2: "dynamic notice". appears at top, narrower for more direct messages. 
         * TriggerNotice() doesnt support multiple lines and/or the typewriter effect, if those are needed, use this
         */

        int w = tbtype == 2 ? Program.ScreenWidth - 64 : Program.ScreenWidth - 16;
        int x = (Program.ScreenWidth - w) / 2;
        int y = tbtype == 1 || tbtype == 2 ? 24 : Program.ScreenHeight - height - 8;

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
        if (Program.player is Autumn autumn)
        {
            autumn.state = Autumn.PlayerState.idle;
        }
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

        foreach(var paragraph in box.Lines[lineIndex].Split("\n"))
        {
            foreach (var word in paragraph.Split(' '))
            {
                //see if the line is too long
                string test = current.Length == 0 ? word : current + " " + word;
                if (Raylib.MeasureTextEx(Program.Rakoon, test, 10f, 0f).X > textW && current.Length > 0)
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
            current = "";
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
        if (Raylib.IsKeyPressed(EngineInit.Config.Confirm))
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

        Rectangle TextBoxRect;

        TextBoxRect.Height = (int)(box.H * t);
        TextBoxRect.Width = (int)(box.W * t);
        TextBoxRect.X = box.X + (box.W - TextBoxRect.Width) / 2;
        TextBoxRect.Y = box.Y + (box.H - TextBoxRect.Height) / 2;

        //draw the box itself(and the outline)
        Raylib.DrawRectangleRec(TextBoxRect, new Color((byte)50, (byte)45, (byte)77, (byte)192));
        Raylib.DrawRectangleLinesEx(TextBoxRect, 1.0f, new Color((byte)71, (byte)67, (byte)148, (byte)255));
        if (progress < 1f) return;

        //set up values for the portrait and text drawing
        int textX = box.X + 4;
        int portraitIndex = box.Portraits[lineIndex];
        if(portraitIndex >= 0)
        {
            int col = portraitIndex % portraitColumns;
            int row = portraitIndex / portraitColumns;

            //draw the protrait
            var src = new Rectangle(col * PortraitSize, row * PortraitSize, PortraitSize, PortraitSize);
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