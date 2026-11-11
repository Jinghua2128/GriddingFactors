
using UnityEngine;

public enum EnemyActionType
{
    Attack,
    Defend,
    Special
}

public static class EnemyActionSelector
{
    public static EnemyActionType Choose(bool hasSpecial)
    {
        int roll = Random.Range(0, 100);

        // First enemy: Attack 70%, Defend 30%.
        if (!hasSpecial)
        {
            return roll < 70
                ? EnemyActionType.Attack
                : EnemyActionType.Defend;
        }

        // Later enemies:
        // Attack 50%, Defend 30%, Special 20%.
        if (roll < 50)
            return EnemyActionType.Attack;

        if (roll < 80)
            return EnemyActionType.Defend;

        return EnemyActionType.Special;
    }
}
