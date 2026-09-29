using System;
using System.Collections.Generic;
using System.Text;

public class Enemy : GameObject
{
    public HealthSystem health;
    public Enemy()
    {
        health = new HealthSystem(10, HandleDeath, null);
    }

    public override void Start()
    {
        
    }

    public override void Update(float deltaTime)
    {
        //mandatory, temporarily unused
    }

    public void HandleDeath()
    {
        //essentially destroys the object
        Active = false;
    }

    public void HandleTakeDamage()
    {

    }
}