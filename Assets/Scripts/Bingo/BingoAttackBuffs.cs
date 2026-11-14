
using System;
using UnityEngine;

public class BingoAttackBuffs : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BingoBoard board;

    // Attack buffs
    public int PendingAttackBonus { get; private set; }
    public int PendingAfterActionDamage { get; private set; }

    // Defense buffs
    public int PendingRoundShield { get; private set; }
    public int PendingEnemyAttackReduction { get; private set; }

    // Healing buffs
    public int PendingEndRoundHealing { get; private set; }

    // Utility buffs
    private int nextRoundExtraDraws;
    private int currentRoundExtraDraws;

    public int NextRoundExtraDraws => nextRoundExtraDraws;

    public int ActiveAttackBuff =>
        PendingAttackBonus + PendingAfterActionDamage;

    public int ActiveDefenseBuff =>
        PendingRoundShield + PendingEnemyAttackReduction;

    public int ActiveHealBuff =>
        PendingEndRoundHealing;

    // Immediate effects are handled by RoundManager.
    public event Action<int> ImmediateHealingRequested;
    public event Action<int> ImmediateFocusRequested;

    private void OnEnable()
    {
        if (board == null)
        {
            Debug.LogError("Bingo board reference missing.");
            return;
        }

        board.CellBuffTriggered += HandleBuff;
    }

    private void OnDisable()
    {
        if (board != null)
            board.CellBuffTriggered -= HandleBuff;
    }

    private void HandleBuff(string buffCode, int buffLevel)
    {
        int bonus = Mathf.Max(0, buffLevel);

        switch (buffCode)
        {
            case "A1":
                PendingAttackBonus += 2 + bonus;
                break;

            case "A2":
                PendingAfterActionDamage += 2 + bonus;
                break;

            case "D1":
                PendingRoundShield += 3 + bonus;
                break;

            case "D2":
                PendingEnemyAttackReduction += 2 + bonus;
                break;

            case "H1":
                ImmediateHealingRequested?.Invoke(2 + bonus);
                break;

            case "H2":
                PendingEndRoundHealing += 1 + bonus;
                break;

            case "U1":
                ImmediateFocusRequested?.Invoke(2 + bonus);
                break;

            case "U2":
                nextRoundExtraDraws += 1 + bonus;
                break;

            case "LUCKY":
                // Centre wildcard: matches any number, no stat buff.
                return;

            default:
                Debug.LogWarning(
                    $"Unknown Bingo buff: {buffCode}"
                );
                return;
        }

        Debug.Log(
            $"Bingo buff activated: {buffCode}" +
            $" (Level {buffLevel})"
        );
    }


    // Called once when a new Bingo round begins.
    public void BeginBingoRound()
    {
        currentRoundExtraDraws = nextRoundExtraDraws;
        nextRoundExtraDraws = 0;
    }

    // Called when CombatManager draws the hand.
    public int ConsumeHandSize(int normalSize = 3)
    {
        int handSize = Mathf.Min(
            normalSize + currentRoundExtraDraws,
            4
        );

        currentRoundExtraDraws = 0;

        return handSize;
    }

    public int ConsumeAttackBonus()
    {
        int value = PendingAttackBonus;
        PendingAttackBonus = 0;
        return value;
    }

    public int ConsumeAfterActionDamage()
    {
        int value = PendingAfterActionDamage;
        PendingAfterActionDamage = 0;
        return value;
    }

    public int ConsumeRoundShield()
    {
        int value = PendingRoundShield;
        PendingRoundShield = 0;
        return value;
    }

    public int ConsumeEnemyAttackReduction()
    {
        int value = PendingEnemyAttackReduction;
        PendingEnemyAttackReduction = 0;
        return value;
    }

    public int ConsumeEndRoundHealing()
    {
        int value = PendingEndRoundHealing;
        PendingEndRoundHealing = 0;
        return value;
    }

    public void ClearRoundDefenseEffects()
    {
        PendingRoundShield = 0;
        PendingEnemyAttackReduction = 0;
    }

    public void ClearEncounterEffects()
    {
        PendingAttackBonus = 0;
        PendingAfterActionDamage = 0;
        PendingEndRoundHealing = 0;

        nextRoundExtraDraws = 0;
        currentRoundExtraDraws = 0;

        ClearRoundDefenseEffects();
    }
}
