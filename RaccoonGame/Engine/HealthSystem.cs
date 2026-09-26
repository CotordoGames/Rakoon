using System;
using System.Collections.Generic;
using System.Text;

public class HealthSystem
{
    public int MaxHP;
    public int CurrentHP;
    private readonly Action? _onDeath;
    private readonly Action? _onTakeDamage;

    public HealthSystem(int maxHP, Action? onDeath = null, Action? onTakeDamage = null)
    {
        MaxHP = maxHP;
        CurrentHP = MaxHP;
        _onDeath = onDeath;
        _onTakeDamage = onTakeDamage;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP = Math.Max(0, CurrentHP - damage);

        if(CurrentHP == 0)
        {
            _onDeath?.Invoke();
            return;
        }

        _onTakeDamage?.Invoke();
    }
}