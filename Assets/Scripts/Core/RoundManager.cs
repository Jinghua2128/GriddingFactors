
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GamePhase
{
    Bingo,
    Combat,
    Shop,
    Victory,
    Defeat
}

public class RoundManager : MonoBehaviour
{
    [Header("Gameplay Panels")]
    [SerializeField] private GameObject bingoPanel;
    [SerializeField] private GameObject combatPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Game Systems")]
    [SerializeField] private PowerballController powerball;
    [SerializeField] private CombatManager combat;
    [SerializeField] private BingoAttackBuffs bingoBuffs;
    [SerializeField] private BingoBoard bingoBoard;

    [Header("Bingo HUD")]
    [SerializeField] private TMP_Text bingoHPText;
    [SerializeField] private TMP_Text bingoFocusText;

    [Header("Run HUD")]
    [SerializeField] private TMP_Text encounterText;
    [SerializeField] private TMP_Text coinsText;

    [Header("Shop Transition")]
    [SerializeField] private TMP_Text shopSummaryText;
    [SerializeField] private Button nextEncounterButton;
    [Header("Shop System")]
    [SerializeField] private ShopController shopController;

    [Header("Results")]
    [SerializeField] private TMP_Text victorySummaryText;
    [SerializeField] private TMP_Text defeatSummaryText;

    private RunProgress run;

    public GamePhase CurrentPhase { get; private set; }


    private void Start()
    {
        // Validate Inspector references.
        if (bingoPanel == null ||
            combatPanel == null ||
            shopPanel == null ||
            victoryPanel == null ||
            defeatPanel == null ||
            powerball == null ||
            combat == null ||
            bingoBuffs == null ||
            bingoBoard == null ||
            encounterText == null ||
            coinsText == null ||
            shopSummaryText == null ||
            nextEncounterButton == null ||
            victorySummaryText == null ||
            defeatSummaryText == null ||
            shopController == null)
        {
            Debug.LogError(
                "RoundManager: Missing Inspector references."
            );

            enabled = false;
            return;
        }

        // Create persistent run state.
        run = new RunProgress();

        // Initialize player and deck once.
        combat.Initialize();

        if (!combat.IsInitialized)
        {
            Debug.LogError("Combat initialization failed.");
            enabled = false;
            return;
        }

        // Subscribe to gameplay events.
        powerball.PhaseFinished += HandleBingoFinished;
        combat.RoundFinished += HandleCombatFinished;

        // Subscribe to Bingo buff events.
        bingoBuffs.ImmediateHealingRequested +=
            HandleImmediateHealing;

        bingoBuffs.ImmediateFocusRequested +=
            HandleImmediateFocus;

        // Subscribe to shop purchase events.
        shopController.PurchasesChanged += RefreshRunHUD;

        // Next Encounter button.
        nextEncounterButton.onClick.AddListener(
            HandleNextEncounterClicked
        );

        // Initialize Bingo.
        powerball.Initialize();

        // Start encounter 1.
        BeginEncounter();
    }


    private void BeginEncounter()
    {
        EncounterDefinition definition =
            run.CurrentEncounter;

        // New enemy, same player and deck.
        combat.BeginNewEncounter(definition);

        // Reset numbers 1-75 for this encounter.
        // Do NOT reset the Bingo board.
        powerball.BeginNewEncounter();

        RefreshRunHUD();
        ShowBingoPhase();
    }

    private void ShowBingoPhase()
    {
        CurrentPhase = GamePhase.Bingo;

        ShowPanel(bingoPanel);

        // Keep U2's next-round scheduling.
        bingoBuffs.BeginBingoRound();

        powerball.BeginRound();

        RefreshBingoHUD();

        Debug.Log(
            $"Encounter {run.EncounterNumber}: BINGO phase"
        );
    }

    private void HandleBingoFinished()
    {
        if (CurrentPhase != GamePhase.Bingo)
            return;

        CurrentPhase = GamePhase.Combat;

        ShowPanel(combatPanel);

        combat.BeginCombatPhase();

        Debug.Log(
            $"Encounter {run.EncounterNumber}: COMBAT phase"
        );
    }

    private void HandleCombatFinished(
        CombatRoundOutcome outcome)
    {
        if (CurrentPhase != GamePhase.Combat)
            return;

        switch (outcome)
        {
            case CombatRoundOutcome.Continue:
                ShowBingoPhase();
                break;

            case CombatRoundOutcome.EnemyDefeated:
                HandleEncounterVictory();
                break;

            case CombatRoundOutcome.PlayerDefeated:
                ShowDefeat();
                break;
        }
    }

    private void HandleEncounterVictory()
    {
        if (!run.TryClaimVictoryReward(out int reward))
        {
            Debug.LogWarning(
                "Victory reward has already been claimed."
            );
            return;
        }

        RefreshRunHUD();

        Debug.Log(
            $"Encounter {run.EncounterNumber} cleared!" +
            $" Earned {reward} coins." +
            $" Total coins: {run.Coins}"
        );

        if (run.IsFinalEncounter)
        {
            ShowVictory();
        }
        else
        {
            ShowShop(reward);
        }
    }


    private void ShowShop(int reward)
    {
        CurrentPhase = GamePhase.Shop;

        // Reset temporary effects from the previous
        // encounter BEFORE applying shop upgrades.
        combat.PrepareForShop();

        ShowPanel(shopPanel);

        shopSummaryText.text =
            $"ENCOUNTER {run.EncounterNumber} COMPLETE!\n" +
            $"Coins earned: +{reward}\n\n" +
            "Choose your upgrades.";

        // Open the three offers for this shop visit.
        shopController.Open(
            run,
            combat,
            bingoBoard,
            run.EncounterNumber
        );

        nextEncounterButton.interactable = true;

        RefreshRunHUD();

        Debug.Log(
            $"SHOP {run.EncounterNumber} OPENED"
        );
    }

    private void HandleNextEncounterClicked()
    {
        if (CurrentPhase != GamePhase.Shop)
            return;

        if (!run.TryAdvance())
        {
            Debug.LogWarning(
                "Cannot advance to the next encounter."
            );
            return;
        }

        nextEncounterButton.interactable = false;

        BeginEncounter();
    }

    private void ShowVictory()
    {
        CurrentPhase = GamePhase.Victory;

        ShowPanel(victoryPanel);

        victorySummaryText.text =
            "RUN COMPLETE!\n\n" +
            $"Encounters defeated: {run.CompletedEncounters}/5\n" +
            $"Bingo lines completed: {bingoBoard.CompletedLineCount}/12\n" +
            $"Final coins: {run.Coins}\n" +
            $"Player HP: {combat.PlayerHP}/{combat.PlayerMaxHP}";

        Debug.Log("GRIDDING FACTORS: VICTORY!");
    }

    private void ShowDefeat()
    {
        CurrentPhase = GamePhase.Defeat;

        ShowPanel(defeatPanel);

        defeatSummaryText.text =
            "RUN ENDED\n\n" +
            $"Encounters defeated: {run.CompletedEncounters}/5\n" +
            $"Reached encounter: {run.EncounterNumber}/5\n" +
            $"Bingo lines completed: {bingoBoard.CompletedLineCount}/12\n" +
            $"Coins remaining: {run.Coins}";

        Debug.Log("GRIDDING FACTORS: DEFEAT");
    }

    // Enables exactly one main gameplay panel.
    private void ShowPanel(GameObject selectedPanel)
    {
        bingoPanel.SetActive(selectedPanel == bingoPanel);
        combatPanel.SetActive(selectedPanel == combatPanel);
        shopPanel.SetActive(selectedPanel == shopPanel);
        victoryPanel.SetActive(selectedPanel == victoryPanel);
        defeatPanel.SetActive(selectedPanel == defeatPanel);
    }

    private void RefreshRunHUD()
    {
        encounterText.text =
            $"Encounter {run.EncounterNumber}/{run.TotalEncounters}";

        coinsText.text = $"Coins: {run.Coins}";
    }

    private void RefreshBingoHUD()
    {
        if (bingoHPText != null)
        {
            bingoHPText.text =
                $"HP: {combat.PlayerHP}/{combat.PlayerMaxHP}";
        }

        if (bingoFocusText != null)
        {
            bingoFocusText.text =
                $"Focus: +{combat.PlayerFocus}";
        }
    }

    private void HandleImmediateHealing(int amount)
    {
        combat.HealPlayerImmediately(amount);
        RefreshBingoHUD();
    }

    private void HandleImmediateFocus(int amount)
    {
        combat.AddFocusImmediately(amount);
        RefreshBingoHUD();
    }

    private void OnDestroy()
    {
        if (powerball != null)
            powerball.PhaseFinished -= HandleBingoFinished;

        if (combat != null)
            combat.RoundFinished -= HandleCombatFinished;

        if (bingoBuffs != null)
        {
            bingoBuffs.ImmediateHealingRequested -=
                HandleImmediateHealing;

            bingoBuffs.ImmediateFocusRequested -=
                HandleImmediateFocus;
        }

        if (nextEncounterButton != null)
        {
            nextEncounterButton.onClick.RemoveListener(
                HandleNextEncounterClicked
            );
        }
        if (shopController != null)
        {
            shopController.PurchasesChanged -= RefreshRunHUD;
        }
    }
}
