using System;
using System.Collections.Generic;
using System.Text;

public class HealthSystem
{
    //class health values
    public int MaxHP;
    public int CurrentHP;

    //functions used by whatever object contains it to do certain things when it takes damage or dies
    private readonly Action? _onDeath;
    private readonly Action? _onTakeDamage;

    //constructor
    public HealthSystem(int maxHP, Action? onDeath = null, Action? onTakeDamage = null)
    {
        MaxHP = maxHP;
        CurrentHP = MaxHP;
        _onDeath = onDeath;
        _onTakeDamage = onTakeDamage;
    }

    //for subtracting health
    public void TakeDamage(int damage)
    {
        if(CurrentHP <= 0) return; //if its already dea, dont take anymore away

        CurrentHP = Math.Max(0, CurrentHP - damage);

        if(CurrentHP == 0)
        {
            _onDeath?.Invoke();
            return;
        }

        _onTakeDamage?.Invoke();
    }

    //for giving it health
    public void Heal(int amount) => CurrentHP = Math.Min(MaxHP, CurrentHP  + amount);

    //for completely restoring HP
    public void FullHeal() => CurrentHP = MaxHP;


}