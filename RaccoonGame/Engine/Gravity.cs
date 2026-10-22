using System;
using System.Collections.Generic;
using System.Text;

public class GravityComponent : Component
{
    public float Gravity = 18f;
    public float MaxFallSpeed = 8f;

    public override void Update(float dt)
    {
        Owner.Velocity.Y = MathF.Min(Owner.Velocity.Y + Gravity * dt, MaxFallSpeed);
    }
}