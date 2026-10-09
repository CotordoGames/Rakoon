using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using FMOD.Studio;
using Raylib_cs;

public class HealthBar : Drawable
{
    //the health system to display
    public HealthSystem Health;

    //color of the bar
    public Color FillColor = new Color((byte)215, (byte)77, (byte)76, (byte)255);

    //color behind the bar
    public Color BackgroundColor = new Color((byte)50, (byte)45, (byte)77, (byte)192);

    //border color
    public Color BorderColor = new Color((byte)71, (byte)67, (byte)148, (byte)255);



    //constructor
    public HealthBar(HealthSystem health)
    {
        Health = health;
    }

    public override void Draw(Vector2 position, Vector2 size, Color tint)
    {
        Vector2 BarPosition = new Vector2(position.X + Raylib.MeasureTextEx(Program.Rakoon, "HEALTH: ", 10, 0).X, position.Y);
        Vector2 TextPosition = new Vector2(((BarPosition.X + (size.X / 2))) - (Raylib.MeasureTextEx(Program.Rakoon, Health.CurrentHP.ToString(), 10, 0).X / 2), BarPosition.Y);

        //draw "HEALTH: "
        Raylib.DrawTextEx(Program.Rakoon, "HEALTH: ", new Vector2(position.X - 1, position.Y + 1), 10, 0, Color.Black);
        Raylib.DrawTextEx(Program.Rakoon, "HEALTH: ", position, 10, 0, Color.White);

        //draw the health bars Background
        Raylib.DrawRectangleV(BarPosition, size, BackgroundColor);

        //calculate the actual scale needed based on the health
        float pct = Health.MaxHP > 0 ? (float)Health.CurrentHP / Health.MaxHP : 0f;

        //draw the bar itself
        Raylib.DrawRectangleV(new Vector2(BarPosition.X + 2, BarPosition.Y + 2), new Vector2((size.X * pct) - 4, size.Y - 4), FillColor);

        //outline
        Raylib.DrawRectangleLines((int)BarPosition.X, (int)BarPosition.Y, (int)size.X, (int)size.Y, BorderColor);

        //draw the actual number of HP we have
        Raylib.DrawTextEx(Program.Rakoon, Health.CurrentHP.ToString(), new Vector2(TextPosition.X - 1, TextPosition.Y), 10, 0, Color.Black);
        Raylib.DrawTextEx(Program.Rakoon, Health.CurrentHP.ToString(), new Vector2(TextPosition.X + 1, TextPosition.Y), 10, 0, Color.Black);
        Raylib.DrawTextEx(Program.Rakoon, Health.CurrentHP.ToString(), new Vector2(TextPosition.X, TextPosition.Y + 1), 10, 0, Color.Black);
        Raylib.DrawTextEx(Program.Rakoon, Health.CurrentHP.ToString(), new Vector2(TextPosition.X, TextPosition.Y - 1), 10, 0, Color.Black);
        Raylib.DrawTextEx(Program.Rakoon, Health.CurrentHP.ToString(), TextPosition, 10, 0, Color.White);
    }
}