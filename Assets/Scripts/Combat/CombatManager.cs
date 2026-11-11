using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CombatRoundOutcome
{
    Continue,
    EnemyDefeated,
    PlayerDefeated
}

public class CombatManager : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private CardData attackCard;
    [SerializeField] private CardData defendCard;
    [SerializeField] private CardData focusCard;

    [Header("Card UI")]
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform handPanel;

    [Header("Combat UI")]
    [SerializeField] private TMP_Text playerStatusText;
    [SerializeField] private TMP_Text enemyStatusText;
    [SerializeField] private TMP_Text battleLogText;
    [SerializeField] private Button continueButton;

    public event Action<CombatRoundOutcome> RoundFinished;

    private CombatantState player;
    private CombatantState enemy;
    private DeckManager deck;

    private int turnNumber;
    private bool waitingForCard;
    private bool initialized;

    private CombatRoundOutcome? pendingOutcome;

    [Header("Bingo Buffs")]
    [SerializeField] private BingoAttackBuffs bingoAttackBuffs;
    public bool IsInitialized => initialized;

    public int PlayerHP =>
        player != null ? player.HP : 0;

    public int PlayerMaxHP =>
        player != null ? player.MaxHP : 0;

    public int PlayerFocus =>
        player != null ? player.Focus : 0;

    [Header("Bingo Ultimates")]
    [SerializeField] private BingoUltimateTracker ultimateTracker;

    private int enemyAttackDamage = 2;
    private string enemyDisplayName = "Lucky Slime";

    #if UNITY_EDITOR
    [ContextMenu("DEBUG: Win Current Encounter")]
    private void DebugWinCurrentEncounter()
    {
        if (!Application.isPlaying ||
            !initialized ||
            !waitingForCard)
            return;

        waitingForCard = false;

        // Bypass the fight for progression testing only.
        ClearHandUI();

        enemy.TakeDamage(enemy.HP + enemy.Shield);

        player.ClearShield();

        FinishCombatRound(
            "DEBUG: Encounter cleared.",
            CombatRoundOutcome.EnemyDefeated
        );
    }
    #endif


    public void AddFocusImmediately(int amount)
    {
        if (!initialized || player.IsDefeated)
        {
            Debug.LogWarning(
                "Cannot add Focus: player is not ready."
            );
            return;
        }

        int before = player.Focus;

        player.AddFocus(amount);

        int gained = player.Focus - before;

        Debug.Log(
            $"U1: Gained +{gained} Focus. " +
            $"Current Focus: +{player.Focus}"
        );

        RefreshUI();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        if (attackCard == null ||
            defendCard == null ||
            focusCard == null ||
            cardPrefab == null ||
            handPanel == null ||
            playerStatusText == null ||
            enemyStatusText == null ||
            battleLogText == null ||
            continueButton == null ||
            bingoAttackBuffs == null)
        {
            Debug.LogError(
                "CombatManager: Missing Inspector reference."
            );
            return;
        }

        player = new CombatantState(30);
        enemy = new CombatantState(12);

        deck = new DeckManager(
            attackCard,
            defendCard,
            focusCard
        );

        continueButton.onClick.AddListener(
            ContinueAfterCombat
        );

        continueButton.gameObject.SetActive(false);

        initialized = true;
    }

    public void BeginCombatPhase()
    {
        Initialize();

        if (!initialized ||
            player.IsDefeated ||
            enemy.IsDefeated)
            return;

        int roundShield =
            bingoAttackBuffs.ConsumeRoundShield();

        if (roundShield > 0)
        {
            player.AddShield(roundShield);

            Debug.Log(
                $"Bingo D1: Player gained {roundShield} Shield."
            );
        }

        turnNumber++;
        waitingForCard = true;
        pendingOutcome = null;

        continueButton.gameObject.SetActive(false);

        ClearHandUI();

                
        int handSize = bingoAttackBuffs.ConsumeHandSize(3);

        var hand = deck.DrawHand(handSize);

        Debug.Log(
            $"Combat hand: {handSize} cards drawn."
        );


        foreach (CardInstance card in hand)
        {
            CardView view = Instantiate(
                cardPrefab,
                handPanel
            );

            view.Bind(card, HandleCardPlayed);
        }

        battleLogText.text =
            $"ROUND {turnNumber}\nChoose one card.";

        RefreshUI();
    }

    private void HandleCardPlayed(CardInstance card)
    {
        if (!waitingForCard)
            return;

        if (!deck.PlayCard(card))
            return;

        waitingForCard = false;

        ClearHandUI();

        // Player's selected card.
        string result = ResolvePlayerCard(card);

        // Hook for Bingo after-action buffs.
        // Effects will be connected next.
        result += ResolveAfterPlayerAction();

        // Old enemy Shield expires after the
        // complete player action.
        if (enemy.Shield > 0)
        {
            result +=
                $"\nEnemy's remaining {enemy.Shield} Shield expired.";
        }

        enemy.ClearShield();

        // Enemy cannot act after being defeated.
        if (enemy.IsDefeated)
        {
            // Finish pending H2 healing even though
            // the enemy will not take its turn.
            result += ResolveEndOfRoundHealing();

            player.ClearShield();

            FinishCombatRound(
                result + "\nENEMY DEFEATED!",
                CombatRoundOutcome.EnemyDefeated
            );

            return;
        }


        // First enemy: 70% Attack, 30% Defend.
        EnemyActionType action =
            EnemyActionSelector.Choose(false);

        
        result += ResolveEnemyAction(action);

        // Only living players receive end-of-round healing.
        // If enemy damage defeated the player,
        // the run ends immediately.
        if (!player.IsDefeated)
        {
            result += ResolveEndOfRoundHealing();
        }
        // Player Shield expires at the end of the round.
        player.ClearShield();


        CombatRoundOutcome outcome =
            player.IsDefeated
                ? CombatRoundOutcome.PlayerDefeated
                : CombatRoundOutcome.Continue;

        FinishCombatRound(result, outcome);
    }

    private string ResolvePlayerCard(CardInstance card)
    {
        // Diagonal applies to the next card of ANY type.
        int cardMultiplier =
            ultimateTracker.ConsumeDiagonalMultiplier(
                out int diagonalHealing
            );

        string result;

        switch (card.Data.type)
        {
            case CardType.Attack:
            {
                int baseDamage = card.Value;
                int focusBonus = player.ConsumeFocus();

                int bingoBonus =
                    bingoAttackBuffs.ConsumeAttackBonus();

                // Horizontal only applies to Attack.
                int attackMultiplier =
                    ultimateTracker.ConsumeHorizontalMultiplier();

                // All additive bonuses FIRST.
                int totalDamage =
                    baseDamage + focusBonus + bingoBonus;

                // Multipliers AFTER additive bonuses.
                totalDamage *=
                    attackMultiplier * cardMultiplier;

                int shieldBefore = enemy.Shield;
                int hpLost = enemy.TakeDamage(totalDamage);
                int blocked =
                    Mathf.Min(shieldBefore, totalDamage);

                result =
                    $"Attack: {baseDamage}" +
                    $" + Focus: {focusBonus}" +
                    $" + Bingo: {bingoBonus}" +
                    $"\nHorizontal ×{attackMultiplier}" +
                    $" | Diagonal ×{cardMultiplier}" +
                    $"\nTotal damage: {totalDamage}" +
                    $"\nBlocked: {blocked}" +
                    $" | HP lost: {hpLost}";

                break;
            }

            case CardType.Defend:
            {
                int shield =
                    card.Value * cardMultiplier;

                player.AddShield(shield);

                result =
                    $"Defend: +{shield} Shield" +
                    $"\nDiagonal ×{cardMultiplier}";

                break;
            }

            case CardType.Focus:
            {
                int before = player.Focus;

                int focusGain =
                    card.Value * cardMultiplier;

                player.AddFocus(focusGain);

                int actualGain = player.Focus - before;

                result =
                    $"Focus: +{actualGain}" +
                    $"\nDiagonal ×{cardMultiplier}" +
                    $"\nTotal Focus: +{player.Focus}";

                break;
            }

            default:
            {
                result = "Card effect not implemented.";
                break;
            }
        }

        // Diagonal's healing is a separate reward.
        // It does NOT get multiplied by cardMultiplier.
        if (diagonalHealing > 0)
        {
            int hpBefore = player.HP;

            player.Heal(diagonalHealing);

            int actualHealing = player.HP - hpBefore;

            result +=
                $"\nDiagonal heals {actualHealing} HP.";
        }

        return result;
    }


    private string ResolveAfterPlayerAction()
    {
        int damage =
            bingoAttackBuffs.ConsumeAfterActionDamage();

        if (damage <= 0)
            return "";

        // The effect is consumed even when the
        // player already defeated the enemy.
        if (enemy.IsDefeated)
            return "\nFollow-up Strike had no remaining target.";

        int shieldBefore = enemy.Shield;

        int hpLost = enemy.TakeDamage(damage);

        int blocked = Mathf.Min(
            shieldBefore,
            damage
        );

        return
            $"\nBingo Follow-Up Strike: {damage} damage." +
            $" Blocked: {blocked}, HP lost: {hpLost}.";
    }


    private string ResolveEnemyAction(EnemyActionType action)
    {
        switch (action)
        {

            case EnemyActionType.Attack:
                {
                    int baseDamage = enemyAttackDamage;
                    // Check whether a Vertical Bingo can block
                    // this enemy Attack completely.
                    if (ultimateTracker.TryConsumeVerticalBlock())
                    {
                        return
                            "\nVERTICAL BINGO! Enemy Attack blocked!";
                    }
                    // D2 reduces the enemy's attack damage
                    // before Shield absorbs anything.
                    int reduction =
                        bingoAttackBuffs.ConsumeEnemyAttackReduction();

                    int effectiveDamage = Mathf.Max(
                        0,
                        baseDamage - reduction
                    );

                    int actualReduction =
                        baseDamage - effectiveDamage;

                    int shieldBefore = player.Shield;

                    int hpLost = player.TakeDamage(effectiveDamage);

                    int blocked = Mathf.Min(
                        shieldBefore,
                        effectiveDamage
                    );

                    return
                        $"\nEnemy Attack: {baseDamage}" +
                        $"\nBingo reduction: -{actualReduction}" +
                        $"\nEffective damage: {effectiveDamage}" +
                        $"\nBlocked by Shield: {blocked}" +
                        $"\nHP lost: {hpLost}";
                }


            case EnemyActionType.Defend:
                {
                    enemy.AddShield(3);

                    return "\nEnemy gains 3 Shield.";
                }

            default:
                return "\nUnknown enemy action.";
        }
    }

    private void FinishCombatRound(
    string message,
        CombatRoundOutcome outcome)        

    {
        // Current-round defensive effects expire
        // whether the enemy attacked or defended.
        bingoAttackBuffs.ClearRoundDefenseEffects();
        pendingOutcome = outcome;
        if (outcome != CombatRoundOutcome.Continue)
        {
            bingoAttackBuffs.ClearEncounterEffects();
        }
        battleLogText.text = message;

        RefreshUI();

        continueButton.gameObject.SetActive(true);
    }

    private void ContinueAfterCombat()
    {
        if (!pendingOutcome.HasValue)
            return;

        CombatRoundOutcome result =
            pendingOutcome.Value;

        pendingOutcome = null;

        continueButton.gameObject.SetActive(false);

        RoundFinished?.Invoke(result);
    }

    private void ClearHandUI()
    {
        foreach (Transform child in handPanel)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void RefreshUI()
    {
        playerStatusText.text =
            $"PLAYER\n" +
            $"HP: {player.HP}/{player.MaxHP}\n" +
            $"Shield: {player.Shield}\n" +
            $"Focus: +{player.Focus}";
        enemyStatusText.text =
            $"{enemyDisplayName}\n" +
            $"HP: {enemy.HP}/{enemy.MaxHP}\n" +
            $"Shield: {enemy.Shield}";

    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                ContinueAfterCombat
            );
        }
    }
    public void HealPlayerImmediately(int amount)
    {
        if (!initialized || player.IsDefeated)
        {
            Debug.LogWarning(
                "Cannot heal: player is not ready."
            );
            return;
        }

        int hpBefore = player.HP;

        // Reuse the existing CombatantState.
        player.Heal(amount);

        int actualHealing = player.HP - hpBefore;

        Debug.Log(
            $"H1 healed {actualHealing} HP. " +
            $"Player HP: {player.HP}/{player.MaxHP}"
        );

        RefreshUI();
    }

    private string ResolveEndOfRoundHealing()
    {
        int healing =
            bingoAttackBuffs.ConsumeEndRoundHealing();

        if (healing <= 0)
            return "";

        int hpBefore = player.HP;

        player.Heal(healing);

        int actualHealing = player.HP - hpBefore;

        return
            $"\nBingo H2 restores {actualHealing} HP." +
            $" Player HP: {player.HP}/{player.MaxHP}.";
    }

    public void BeginNewEncounter(EncounterDefinition definition)
    {
        if (!initialized)
        {
            Debug.LogError(
                "CombatManager must be initialized first."
            );
            return;
        }

        // Create a NEW enemy for this encounter.
        enemy = new CombatantState(definition.MaxHP);

        enemyAttackDamage = definition.AttackDamage;
        enemyDisplayName = definition.Name;

        // Reset temporary combat effects.
        // HP remains unchanged.
        player.ResetTemporaryEffects();

        // The deck remains unchanged.
        // The Bingo board and Ultimates remain unchanged.

        turnNumber = 0;
        waitingForCard = false;
        pendingOutcome = null;

        ClearHandUI();
        continueButton.gameObject.SetActive(false);

        RefreshUI();

        Debug.Log(
            $"NEW ENCOUNTER: {enemyDisplayName}" +
            $" | Enemy HP: {enemy.MaxHP}" +
            $" | Attack: {enemyAttackDamage}" +
            $" | Player HP: {player.HP}/{player.MaxHP}" +
            $" | Cards: {deck.TotalCards}"
        );
    }
    

}
