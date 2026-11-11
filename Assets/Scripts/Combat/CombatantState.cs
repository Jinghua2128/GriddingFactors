
using System;

public class CombatantState
{
    public int MaxHP { get; private set; }
    public int HP { get; private set; }
    public int Shield { get; private set; }
    public int Focus { get; private set; }

    public bool IsDefeated => HP <= 0;

    public CombatantState(int maxHP)
    {
        if (maxHP <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHP));

        MaxHP = maxHP;
        HP = maxHP;
    }

    public int TakeDamage(int damage)
    {
        damage = Math.Max(0, damage);

        int blocked = Math.Min(Shield, damage);
        Shield -= blocked;

        int actualDamage = Math.Min(HP, damage - blocked);
        HP -= actualDamage;

        return actualDamage;
    }

    public void Heal(int amount)
    {
        HP = Math.Min(MaxHP, HP + Math.Max(0, amount));
    }

    public void AddShield(int amount)
    {
        Shield += Math.Max(0, amount);
    }

    public void ClearShield()
    {
        Shield = 0;
    }

    public void AddFocus(int amount)
    {
        Focus = Math.Min(6, Focus + Math.Max(0, amount));
    }

    public int ConsumeFocus()
    {
        int value = Focus;
        Focus = 0;
        return value;
    }

    public void ResetTemporaryEffects()
    {
        Shield = 0;
        Focus = 0;
    }
}
