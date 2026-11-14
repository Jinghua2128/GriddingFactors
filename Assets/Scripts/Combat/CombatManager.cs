
using System;
using System.Collections.Generic;
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
    [SerializeField] private CardData piercingCard;

    [Header("Card UI")]
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform handPanel;

    [Header("Combat UI")]
    [SerializeField] private TMP_Text playerStatusText;
    [SerializeField] private TMP_Text enemyStatusText;
    [SerializeField] private TMP_Text battleLogText;
    [SerializeField] private Button continueButton;

    [Header("Bingo Buffs")]
    [SerializeField] private BingoAttackBuffs bingoAttackBuffs;

    [Header("Bingo Ultimates")]
    [SerializeField] private BingoUltimateTracker ultimateTracker;

    public event Action<CombatRoundOutcome> RoundFinished;

    private CombatantState player;
    private CombatantState enemy;
    private DeckManager deck;

    private int turnNumber;
    private bool waitingForCard;
    private bool initialized;

    private CombatRoundOutcome? pendingOutcome;

    private int enemyAttackDamage = 2;
    private string enemyDisplayName = "Lucky Slime";

    public bool IsInitialized => initialized;

    public int PlayerHP =>
        player != null ? player.HP : 0;

    public int PlayerMaxHP =>
        player != null ? player.MaxHP : 0;

    public int PlayerFocus =>
        player != null ? player.Focus : 0;

    public int PlayerShield =>
        player != null ? player.Shield : 0;

    public int EnemyHP =>
        enemy != null ? enemy.HP : 0;

    public int EnemyMaxHP =>
        enemy != null ? enemy.MaxHP : 0;

    public int EnemyShield =>
        enemy != null ? enemy.Shield : 0;

    public IReadOnlyList<CardInstance> ShopCards =>
        deck != null
            ? deck.AllCards
            : Array.Empty<CardInstance>();

    #if UNITY_EDITOR
    [ContextMenu("DEBUG: Win Current Encounter")]
    private void DebugWinCurrentEncounter()
    {
        if (!Application.isPlaying ||
            !initialized ||
            !waitingForCard)
            return;

        waitingForCard = false;

        ClearHandUI();

        enemy.TakeDamage(enemy.HP + enemy.Shield);
        player.ClearShield();

        FinishCombatRound(
            "DEBUG: Encounter cleared.",
            CombatRoundOutcome.EnemyDefeated
        );
    }
    #endif

    public void Initialize()
    {
        if (initialized)
            return;

        if (attackCard == null ||
            defendCard == null ||
            focusCard == null ||
            piercingCard == null ||
            cardPrefab == null ||
            handPanel == null ||
            playerStatusText == null ||
            enemyStatusText == null ||
            battleLogText == null ||
            continueButton == null ||
            bingoAttackBuffs == null ||
            ultimateTracker == null)
        {
            Debug.LogError(
                "CombatManager: Missing Inspector reference.",
                this
            );
            return;
        }

        player = new CombatantState(30);
        enemy = new CombatantState(12);

        deck = new DeckManager(
            attackCard,
            defendCard,
            focusCard,
            piercingCard
        );

        continueButton.onClick.AddListener(
            ContinueAfterCombat
        );

        continueButton.gameObject.SetActive(false);

        initialized = true;

        RefreshUI();
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

        int handSize =
            bingoAttackBuffs.ConsumeHandSize(3);

        var hand = deck.DrawHand(handSize);

        AudioManager.Play(Sfx.CardDraw, 0.7f);

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
        if (!initialized ||
            !waitingForCard ||
            card == null)
            return;

        if (!deck.PlayCard(card))
            return;

        AudioManager.Play(Sfx.Card);

        waitingForCard = false;

        ClearHandUI();

        string result = ResolvePlayerCard(card);

        result += ResolveAfterPlayerAction();

        if (enemy.Shield > 0)
        {
            result +=
                $"\nEnemy's remaining {enemy.Shield} Shield expired.";
        }

        enemy.ClearShield();

        if (enemy.IsDefeated)
        {
            result += ResolveEndOfRoundHealing();

            player.ClearShield();

            FinishCombatRound(
                result + "\nENEMY DEFEATED!",
                CombatRoundOutcome.EnemyDefeated
            );

            return;
        }

        EnemyActionType action =
            EnemyActionSelector.Choose(false);

        result += ResolveEnemyAction(action);

        if (!player.IsDefeated)
        {
            result += ResolveEndOfRoundHealing();
        }

        player.ClearShield();

        CombatRoundOutcome outcome =
            player.IsDefeated
                ? CombatRoundOutcome.PlayerDefeated
                : CombatRoundOutcome.Continue;

        FinishCombatRound(result, outcome);
    }

    private string ResolvePlayerCard(CardInstance card)
    {
        int cardMultiplier =
            ultimateTracker.ConsumeDiagonalMultiplier(
                out int diagonalHealing
            );

        string result;

        switch (card.Data.type)
        {
            case CardType.Attack:
            case CardType.PiercingAttack:
            {
                bool piercing =
                    card.Data.type == CardType.PiercingAttack;

                int baseDamage = card.Value;
                int focusBonus = player.ConsumeFocus();

                int bingoBonus =
                    bingoAttackBuffs.ConsumeAttackBonus();

                int horizontalMultiplier =
                    ultimateTracker.ConsumeHorizontalMultiplier();

                int totalDamage =
                    (baseDamage + focusBonus + bingoBonus)
                    * horizontalMultiplier
                    * cardMultiplier;

                int shieldBefore = enemy.Shield;

                int hpLost = piercing
                    ? enemy.TakePiercingDamage(totalDamage)
                    : enemy.TakeDamage(totalDamage);

                int blocked = piercing
                    ? 0
                    : Mathf.Min(shieldBefore, totalDamage);

                result =
                    $"{(piercing ? "Piercing Attack" : "Attack")}" +
                    $": {totalDamage} damage." +
                    $"\nHorizontal x{horizontalMultiplier}" +
                    $" | Diagonal x{cardMultiplier}" +
                    $"\nShield blocked: {blocked}" +
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
                    $"\nDiagonal x{cardMultiplier}";

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
                    $"\nDiagonal x{cardMultiplier}" +
                    $"\nTotal Focus: +{player.Focus}";

                break;
            }

            case CardType.Heal:
            {
                int healing =
                    card.Value * cardMultiplier;

                int before = player.HP;

                player.Heal(healing);

                int actualHealing = player.HP - before;

                result =
                    $"Heal restores {actualHealing} HP." +
                    $"\nDiagonal x{cardMultiplier}";

                break;
            }

            default:
            {
                result = "Card effect not implemented.";
                break;
            }
        }

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

        if (enemy.IsDefeated)
        {
            return
                "\nFollow-up Strike had no remaining target.";
        }

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

    private string ResolveEnemyAction(
        EnemyActionType action
    )
    {
        switch (action)
        {
            case EnemyActionType.Attack:
            {
                int baseDamage = enemyAttackDamage;

                if (ultimateTracker.TryConsumeVerticalBlock())
                {
                    return
                        "\nVERTICAL BINGO! Enemy Attack blocked!";
                }

                int reduction =
                    bingoAttackBuffs.ConsumeEnemyAttackReduction();

                int effectiveDamage = Mathf.Max(
                    0,
                    baseDamage - reduction
                );

                int actualReduction =
                    baseDamage - effectiveDamage;

                int shieldBefore = player.Shield;

                int hpLost =
                    player.TakeDamage(effectiveDamage);

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
        CombatRoundOutcome outcome
    )
    {
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
        if (handPanel == null)
            return;

        foreach (Transform child in handPanel)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void RefreshUI()
    {
        if (!initialized)
            return;

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

    // SHOP SYSTEM

    public bool TryAddShopCard(CardData data)
    {
        if (!initialized || data == null)
            return false;

        deck.AddCard(data);

        return true;
    }

    public bool TryUpgradeShopCard(CardInstance instance)
    {
        if (!initialized || instance == null)
            return false;

        return deck.TryUpgradeCard(instance);
    }

    public void PrepareForShop()
    {
        if (!initialized)
            return;

        waitingForCard = false;
        pendingOutcome = null;

        player.ResetTemporaryEffects();

        ClearHandUI();
        continueButton.gameObject.SetActive(false);

        RefreshUI();
    }

    // ENCOUNTER PROGRESSION

    public void BeginNewEncounter(EncounterDefinition definition)
    {
        if (!initialized)
        {
            Debug.LogError(
                "CombatManager must be initialized first."
            );
            return;
        }

        if (definition.MaxHP <= 0)
        {
            Debug.LogError(
                "Encounter MaxHP must be greater than zero."
            );
            return;
        }

        enemy = new CombatantState(definition.MaxHP);

        enemyAttackDamage = definition.AttackDamage;
        enemyDisplayName = definition.Name;

        // Keep the rest of your existing code.
        enemy = new CombatantState(
            definition.MaxHP
        );

        enemyAttackDamage =
            definition.AttackDamage;

        enemyDisplayName =
            definition.Name;

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

        


    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                ContinueAfterCombat
            );
        }
    }
}
