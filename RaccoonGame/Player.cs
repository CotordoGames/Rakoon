using Raylib_cs;
using System;
using System.Numerics;
using System.Text.Json;

public class Player : GameObject
{
    //ground movement values, changed to be uncapped and have friction instead
    public const float GroundAcceleration = 5f;
    public const float GroundDrag = 2f;
    public const float GroundFriction = 7f;

    //air movement values-- no speed needed now
    public const float AirAcceleration = 5f;
    public const float MaxAirSpeed = 2.8f;

    //general movement values
    public const int JumpForce = 3;
    public const int Jumps = 2;
    public int CurrentJumps = 2;
    public float Gravity = 18;
    public float FallGravity = 22;
    public const float MaxFallSpeed = 6f;
    public bool WasGrounded = false;
    public bool isDashing = false;
    public float DashSpeed = 4.0f;

    //"feel good" values
    const float CoyoteTime = 0.1f;
    const float JumpBufferTime = 0.1f;
    const float ApexThresHold = 1.5f;
    const float ApexGravityMult = 0.5f;

    //timers
    float coyoteTimer = 0f;
    float jumpBufferTimer = 0f;

    static Texture2D texture;
    static bool TextureLoaded = false;
    public override void Start()
    {

        //set the player to solid; make sure the texture is loaded
        IsSolid = true;

        //set important object values
        CanMove = true;
        DebugColor = Color.Blue;

        if (EngineInit.Config.YuriMode)
        {
            if (!TextureLoaded)
            {
                texture = Raylib.LoadTexture("assets/sprites/player/cubon.png");
                TextureLoaded = true;
            }

            //tailored to cubon
            Size = new Vector2(16, 24);
            ColliderSize = new Vector2(10, 14);
            ColliderOffset = new Vector2(2, 10);

            //set the graphic to the texture
            Graphic = new AnimatedSprite2D(texture, 16, 24);


        }
        else
        {
            if (!TextureLoaded)
            {
                texture = Raylib.LoadTexture("assets/sprites/player/autumn.png");
                TextureLoaded = true;
            }

            //tailored to autumn
            Size = new Vector2(16, 16);
            ColliderSize = new Vector2(8, 13);
            ColliderOffset = new Vector2(4, 3);

            //set the graphic to the texture
            Graphic = new AnimatedSprite2D(texture, 16, 16);

        }

        //initialize the animator and create animations
        var anim = (AnimatedSprite2D)Graphic;
        anim.AddAnimation("idle", new[] { 0 }, 1f);
        anim.AddAnimation("run", new[] { 0, 1 }, 0.08f);
        anim.AddAnimation("jump", new[] { 1 }, 1f, loop: false);
        anim.AddAnimation("dash", new[] { 2, 3, 4 }, 0.1f, loop: false);
        anim.AddAnimation("fall", new[] { 5, 6 }, 0.08f, loop: true);
        anim.Play("idle");
    }

    public override void Update(float deltaTime)
    {
        if(!CanMove) return;

        int Direction = Raylib.IsKeyDown(KeyboardKey.Right) - Raylib.IsKeyDown(KeyboardKey.Left);

        //flip the players' sprite
        if (Graphic != null)
        {
            if (Direction > 0)
            {
                Graphic.FlipX = false;
            }
            else if (Direction < 0)
            {
                Graphic.FlipX = true;
            }
        }

        //setup coyotetime
        coyoteTimer = IsGrounded ? CoyoteTime : coyoteTimer - deltaTime;
        jumpBufferTimer = Raylib.IsKeyPressed(KeyboardKey.Z) ? JumpBufferTime : jumpBufferTimer - deltaTime;

        if(Raylib.IsKeyPressed(KeyboardKey.X) && !isDashing && Graphic is AnimatedSprite2D dashTrigger)
        {
            Program.audio.PlayOneShot("dash");
            isDashing = true;
            dashTrigger.Play("dash", restart: true);
            if(Direction != 0)
            {
                Velocity.Y = 0;
                Gravity = 1.0f;
            }
            Velocity.X = DashSpeed * Direction;
        }

        if(isDashing && Graphic is AnimatedSprite2D dashCheck && !dashCheck.IsPlaying)
        {
            Gravity = 18f;
            isDashing = false;
        }



        //ground movement and jump check
        if (IsGrounded)
        {
            Gravity = 18f;
            CurrentJumps = Jumps;

            /*if(Direction != 0)
            {
                Velocity.X = float.Lerp(Velocity.X, Direction * GroundSpeed, GroundAcceleration * deltaTime * 60);
            }
            else
            {
                Velocity.X = float.Lerp(Velocity.X, Direction * GroundSpeed, Deceleration * deltaTime * 60);
            }*/

            //increase velocity when the player walks

            if (!WasGrounded)
            {
                Program.audio.PlayOneShot("land");
            }

            if(Direction != 0)
            {
                Velocity.X += Direction * GroundAcceleration * deltaTime;

                //friction with drag
                Velocity.X -= Velocity.X * GroundDrag * deltaTime;
                
            }
            else
            {
                //friction
                Velocity.X = MoveToward(Velocity.X, 0f, GroundFriction * deltaTime);
            }
           
        }
        else //air movement and jump controls
        {
            if(Direction != 0)
            {
                float target = Direction * MaxAirSpeed;
                bool movingTowardCap = Math.Sign(Velocity.X) == Direction || Velocity.X == 0f;
                bool belowCap = MathF.Abs(Velocity.X) < MaxAirSpeed;

                if(movingTowardCap && belowCap)
                {
                    Velocity.X = MoveToward(Velocity.X, target, AirAcceleration * deltaTime);
                    Velocity.X = Math.Clamp(Velocity.X, -MaxAirSpeed, MaxAirSpeed);
                }
                else if (!movingTowardCap)
                {
                    Velocity.X = MoveToward(Velocity.X, target, AirAcceleration * deltaTime);
                }
            }

            float gravityScale = MathF.Abs(Velocity.Y) < ApexThresHold ? ApexGravityMult : 1f;
            Velocity.Y += Gravity * gravityScale * deltaTime;
            Velocity.Y = Math.Min(Velocity.Y, MaxFallSpeed);

            if(Raylib.IsKeyReleased(KeyboardKey.Z) && !isDashing)
            {
                Gravity = 22;
                Velocity.Y /= 2;
            }

        }

        if(jumpBufferTimer > 0f && (coyoteTimer > 0f || CurrentJumps > 0))
        {
            Program.audio.PlayOneShot("jump");
            Velocity.Y = -JumpForce * CurrentJumps;
            Gravity = 18f;

            if(coyoteTimer > 0f)
            {
                coyoteTimer = 0f;
            }
            else
            {
                CurrentJumps--;
            }

            CurrentJumps--;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        foreach (var obj in CurrentlyColliding)
        {
            if (obj is Ladder && Raylib.IsKeyDown(KeyboardKey.Up))
            {
                Velocity.X = 0;
                Velocity.Y = -2;
                break;
            }
        }


        if (Graphic is AnimatedSprite2D anim2)
        {

            if (isDashing)
            {
                anim2.Play("dash");
            }
            else if (!IsGrounded) 
            {
                if(Velocity.Y < 0f)
                {
                    anim2.Play("jump");
                }
                else
                {
                    anim2.Play("fall");
                }
            }
            else if (Direction != 0) anim2.Play("run");
            else anim2.Play("idle");
        }

        WasGrounded = IsGrounded;
    }

    static float MoveToward(float current, float target, float maxDelta)
    {
        if(MathF.Abs(target - current) <= maxDelta)
        {
            return target;
        }
        return current + MathF.Sign(target - current) * maxDelta;
    }
}
