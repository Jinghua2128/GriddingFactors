
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

    private void HandleBuff(string buffCode)
    {
        switch (buffCode)
        {
            // ATTACK
            case "A1":
                PendingAttackBonus += 2;
                Debug.Log("A1: +2 next Attack");
                break;

            case "A2":
                PendingAfterActionDamage += 2;
                Debug.Log("A2: +2 after-action damage");
                break;

            // DEFENSE
            case "D1":
                PendingRoundShield += 3;
                Debug.Log("D1: +3 Shield this round");
                break;

            case "D2":
                PendingEnemyAttackReduction += 2;
                Debug.Log("D2: Reduce enemy attack by 2");
                break;

            // HEALING
            case "H1":
                ImmediateHealingRequested?.Invoke(2);
                Debug.Log("H1: Immediate healing +2");
                break;

            case "H2":
                PendingEndRoundHealing += 1;
                Debug.Log("H2: Heal 1 at end of round");
                break;

            // UTILITY
            case "U1":
                ImmediateFocusRequested?.Invoke(2);
                Debug.Log("U1: Immediate Focus +2");
                break;

            case "U2":
                nextRoundExtraDraws++;

                Debug.Log(
                    "U2: Extra card scheduled for next round."
                );
                break;
        }
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
            5
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
