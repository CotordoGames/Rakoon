using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Numerics;

public abstract class Enemy : GameObject
{
    public HealthSystem health;
    public float direction;
    public float damage;
    public float speed;
    public float knockback = 12;

    public const float DamageCoolDown = 0.5f;
    public float DamageCoolDownTimer = 0f;
    public Enemy()
    {
        health = new HealthSystem(10, HandleDeath, HandleTakeDamage);
    }

    public override void Start()
    {
        Components.Add(new GravityComponent());
        Layer = Layers.Enemy;
        BlockedBy &= ~Layers.Player; //allows it to overlap the player
        IsSolid = false;
        CanMove = true;
        DebugColor = Color.Maroon;
        EnemyStart();
    }

    public override void Update(float deltaTime)
    {
        DamageCoolDownTimer = MathF.Max(0f, DamageCoolDownTimer - deltaTime);

        foreach (GameObject obj in CurrentlyColliding)
        {
            if (obj is EnemyTrigger et)
            {
                if (et.FlipDirection)
                {
                    direction = -direction;
                }
                if (et.Jump)
                {
                    Velocity.Y = -et.JumpHeight;
                }
            }
        }

        Velocity.X = direction * speed;
        EnemyUpdate(deltaTime);
    }

    public void HandleDeath()
    {
        OnDeath();

        //essentially destroys the object
        Active = false;
    }

    public void HandleTakeDamage()
    {
        Program.camera.TriggerCameraShake(2.5f, 50, 0.36f);
        OnTakeDamage();
        DamageCoolDownTimer = DamageCoolDown;
    }

    public virtual void OnTakeDamage() { }
    public virtual void OnDeath() { }
    public abstract void EnemyUpdate(float deltaTime);
    public abstract void EnemyStart();
    public virtual void EnemyLoad(JsonElement json) { }

    public override void LoadFromJson(JsonElement json)
    {
        if (TryGetPropertyCI(json, "direction", out var dir)) direction = dir.GetSingle();
        if (TryGetPropertyCI(json, "speed", out var spd)) speed = spd.GetSingle();
        if (TryGetPropertyCI(json, "health", out var hlth)) health.MaxHP = hlth.GetInt32();

        EnemyLoad(json);
    }
}