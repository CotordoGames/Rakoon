using Raylib_cs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

public class Autumn : GameObject
{
    //ground movement values, changed to be uncapped and have friction instead
    public const float WalkAcceleration = 5f;
    public const float RunAcceleration = 8f;
    public const float GroundDrag = 2f;
    public const float GroundFriction = 7f;

    //air movement values-- no speed needed now
    public const float AirAcceleration = 5f;
    public const float MaxAirSpeed = 3.5f;

    //general movement values
    public const float JumpForce = 5.5f;
    public const float AirJumpForce = 3;
    public const int Jumps = 1;
    public int CurrentJumps = 2;
    public float Gravity = 18;
    public float FallGravity = 22;
    public const float MaxFallSpeed = 6f;
    public bool WasGrounded = false;

    //dash
    public bool isDashing = false;
    public float DashSpeed = 4.0f;

    //wall jump
    public const float WallSlideMaxSpeed = 1.0f;
    public Vector2 WallJumpForce = new Vector2(3.0f, 5.0f);
    public const float WallJumpLockDuration = 0.15f;
    float wallJumpLockTimer = 0f;
    bool wallSliding = false;

    public const float ClimbSpeed = 2.0f;

    //"feel good" values
    const float CoyoteTime = 0.1f;
    const float JumpBufferTime = 0.1f;
    const float ApexThresHold = 1.5f;
    const float ApexGravityMult = 0.5f;

    //timers
    float coyoteTimer = 0f;
    float jumpBufferTimer = 0f;

    //health
    public static HealthSystem health;
    public static bool isDead = false;
    static Vector2 deathPos;
    static Vector2 deathVel;

    static Texture2D texture;
    static bool TextureLoaded = false;

    PlayerState state;

    public Autumn() 
    {
        health = new HealthSystem(20, HandleDeath, null);
    }

    //tracks the players state
    public enum PlayerState
    {
        idle, jump, fall, walk, run, dash, wallSlide, climb
    }
    public override void Start()
    {

        //set the player to solid; make sure the texture is loaded
        IsSolid = true;

        //set important object values
        CanMove = true;
        DebugColor = Color.Blue;

        //branch; if yuri mode is true, then the player plays as cubon
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
        anim.AddAnimation("walk", new[] { 0, 1 }, 0.08f);
        anim.AddAnimation("run", new[] { 0, 1 }, 0.04f);
        anim.AddAnimation("jump", new[] { 1 }, 1f, loop: false);
        anim.AddAnimation("dash", new[] { 2, 3, 4 }, 0.1f, loop: false);
        anim.AddAnimation("fall", new[] { 5, 6 }, 0.08f, loop: true);
        anim.Play("idle");

        state = PlayerState.idle;
    }

    public override void Update(float dt)
    {
        if (isDead)
        {
            Position = deathPos;
            Velocity = deathVel;
            isDead = false;
        }

        //update the current direction each frame
        float direction = Raylib.IsKeyDown(KeyboardKey.Right) - Raylib.IsKeyDown(KeyboardKey.Left);

        //update the wall jump lock timer
        if(wallJumpLockTimer > 0f) wallJumpLockTimer -= dt;

        //update coyote time and jump buffer
        coyoteTimer = IsGrounded ? CoyoteTime : coyoteTimer - dt;
        jumpBufferTimer = Raylib.IsKeyPressed(KeyboardKey.Z) ? JumpBufferTime : jumpBufferTimer - dt;


        //cut velocity for when the player releases the jump key
        if(Raylib.IsKeyReleased(KeyboardKey.Z) && Velocity.Y < 0f)
        {
            Velocity.Y *= 0.5f;
        }

        if (!WasGrounded && IsGrounded)
        {
            Program.audio.PlayOneShot("land");
        }

        TryStartDash(direction);
        UpdateDash();

        bool onLadder = false;
        foreach (var obj in CurrentlyColliding)
        {
            if (obj is Spring)
            {
                var spring = (Spring)obj;
                Program.audio.PlayOneShot("spring");
                if (Raylib.IsKeyDown(KeyboardKey.Z))
                {
                    Velocity.Y = -spring.BounceHeight * 1.25f;
                }
                else
                {
                    Velocity.Y = -spring.BounceHeight;
                }
            }
            if (obj is Ladder && Raylib.IsKeyDown(KeyboardKey.Up))
            {
                onLadder = true;
                break;
            }
        }

        //update the current state to change how the players values get update this frame
        DetermineState(direction, onLadder);

        switch (state)
        {
            case PlayerState.idle:
                Velocity.X = MoveToward(Velocity.X, 0f, GroundFriction * dt);
                break;

            case PlayerState.walk:
                if (IsGrounded) CurrentJumps = Jumps;
                GroundMove(direction, WalkAcceleration, dt);
                break;

            case PlayerState.run:
                if (IsGrounded) CurrentJumps = Jumps;
                GroundMove(direction, RunAcceleration, dt);
                break;

            case PlayerState.jump:
                AirMove(direction, dt);
                ApplyGravity(dt);
                break;

            case PlayerState.fall:
                AirMove(direction, dt);
                ApplyGravity(dt);
                break;

            case PlayerState.dash:
                Velocity.X = DashSpeed * (Graphic!.FlipX ? -1 : 1);
                Velocity.Y = 0;
                break;

            case PlayerState.wallSlide:
                Velocity.X = 0;
                Velocity.Y = Math.Min(Velocity.Y + Gravity * dt, WallSlideMaxSpeed);
                CurrentJumps = Jumps;
                break;

            case PlayerState.climb:
                float vertical = (Raylib.IsKeyDown(KeyboardKey.Down) ? 1 : 0) - (Raylib.IsKeyDown(KeyboardKey.Up) ? 1 : 0);
                Velocity.Y = vertical * ClimbSpeed;
                Velocity.X = 0;
                break;
        }

        if(IsGrounded && state is not (PlayerState.walk or PlayerState.run))
        {
            CurrentJumps = Jumps;
        }

        TryJump();
        UpdateFacing(direction);
        UpdateAnimation();

        WasGrounded = IsGrounded;
    }

    void DetermineState(float Direction, bool onLadder)
    {
        if(onLadder && (Raylib.IsKeyDown(KeyboardKey.Up) || Raylib.IsKeyDown(KeyboardKey.Down)))
        {
            state = PlayerState.climb;
            wallSliding = false;
            return;
        }

        if (isDashing) { state = PlayerState.dash; wallSliding = false; return; }
        if (IsGrounded)
        {
            wallSliding = false;
            state = Direction == 0 ? PlayerState.idle : Raylib.IsKeyDown(KeyboardKey.LeftShift) ? PlayerState.run : PlayerState.walk;
            return;
        }

        bool pressingLeft = IsTouchingWallLeft && Direction < 0;
        bool pressingRight = IsTouchingWallRight && Direction > 0;
        wallSliding = (pressingLeft || pressingRight) && Velocity.Y != 0;

        state = wallSliding ? PlayerState.wallSlide : Velocity.Y < 0 ? PlayerState.jump : PlayerState.fall;
    }

    void GroundMove(float direction, float accel, float dt)
    {
        Velocity.X += direction * accel * dt;
        Velocity.X -= Velocity.X * GroundDrag * dt;
    }

    void AirMove(float direction, float dt)
    {
        if (wallJumpLockTimer > 0f) return;
        if (direction != 0) Velocity.X = MoveToward(Velocity.X, direction * MaxAirSpeed, AirAcceleration * dt);
    }

    void ApplyGravity(float dt)
    {
        float gravityScale = MathF.Abs(Velocity.Y) < ApexThresHold ? ApexGravityMult : 1f;
        Velocity.Y += (Velocity.Y < 0 ? Gravity : FallGravity) * gravityScale * dt;
        Velocity.Y = Math.Min(Velocity.Y, MaxFallSpeed);
    }

    void TryJump()
    {
        if(jumpBufferTimer <= 0f) return;

        if (wallSliding)
        {
            int wallDir = IsTouchingWallLeft ? -1 : 1;
            Velocity.X = -wallDir * WallJumpForce.X;
            Velocity.Y = -WallJumpForce.Y;
            wallJumpLockTimer = WallJumpLockDuration;
            jumpBufferTimer = 0f;
            Program.audio.PlayOneShot("jump");
            return;
        }

        if(coyoteTimer > 0f)
        {
            Velocity.Y = -JumpForce;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            Program.audio.PlayOneShot("jump");
        }
        else if(CurrentJumps > 0)
        {
            Velocity.Y = -AirJumpForce;
            CurrentJumps--;
            jumpBufferTimer = 0f;
            Program.audio.PlayOneShot("jump");
        }
    }

    void TryStartDash(float direction)
    {
        if (!Raylib.IsKeyPressed(KeyboardKey.X) || isDashing) return;
        if(Graphic is not AnimatedSprite2D anim) return;

        isDashing = true;
        Program.audio.PlayOneShot("dash");
        anim.Play("dash");
    }

    void UpdateDash()
    {
        if(isDashing && Graphic is AnimatedSprite2D anim && !anim.IsPlaying)
        {
            isDashing = false;
        }
    }

    void UpdateFacing(float direction)
    {
        if (Graphic == null || direction == 0 || isDashing) return; //no flip mid dash
        Graphic.FlipX = direction < 0;
    }

    void UpdateAnimation()
    {
        if (Graphic is not AnimatedSprite2D anim) return;

        string clip = state switch
        {
            PlayerState.idle => "idle",
            PlayerState.walk => "walk",
            PlayerState.run => "run",
            PlayerState.jump => "jump",
            PlayerState.dash => "dash",
            PlayerState.wallSlide => "fall",
            PlayerState.climb => "walk",
            _ => "idle"
        };
        anim.Play(clip);
    }

    public void HandleDeath()
    {
        isDead = true;
        deathPos = new Vector2(Program.level.SpawnPoint.X - Program.player.Size.X / 2, Program.level.SpawnPoint.Y - Program.player.Size.Y / 2);
        deathVel = Vector2.Zero;
        Program.audio.PlayOneShot("death");
        health.FullHeal();
    }

    public void HandleTakeDamage()
    {

    }

    //a math helper function to allow smooth acceleration
    static float MoveToward(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
        {
            return target;
        }
        return current + MathF.Sign(target - current) * maxDelta;
    }
}