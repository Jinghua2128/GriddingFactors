
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerballController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BingoBoard board;
    [SerializeField] private TMP_Text ballNumberText;
    [SerializeField] private TMP_Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button drawButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button continueToCombatButton;

    // RoundManager listens to this event.
    public event Action PhaseFinished;

    private PowerballBag bag;

    private int currentNumber;
    private bool awaitingPlacement;
    private bool phaseComplete;
    private bool initialized;

    private void Awake()
    {
        bag = new PowerballBag();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        board.Initialize();

        if (board.Cells == null)
        {
            Debug.LogError("BingoBoard initialization failed.");
            return;
        }

        foreach (BingoCellUI cell in board.Cells)
            cell.Clicked += HandleCellClicked;

        drawButton.onClick.AddListener(DrawNumber);
        rerollButton.onClick.AddListener(Reroll);
        continueToCombatButton.onClick.AddListener(
            ContinueToCombat
        );

        initialized = true;
    }

    // Only RoundManager starts a new Bingo phase.
    public void BeginRound()
    {
        if (!initialized)
            return;

        awaitingPlacement = false;
        phaseComplete = false;
        currentNumber = 0;

        board.ClearHighlights();

        ballNumberText.text = "?";

        drawButton.interactable =
            bag.RemainingCount > 0;

        rerollButton.interactable = false;
        continueToCombatButton.interactable = false;

        if (bag.RemainingCount == 0)
        {
            ballNumberText.text = "-";
            FinishPhase("Bag empty.");
            return;
        }

        SetStatus("Draw a ball.");
    }

    private void DrawNumber()
    {
        if (!drawButton.interactable)
            return;

        drawButton.interactable = false;

        DrawAndCheck(true);
    }

    private void Reroll()
    {
        if (!rerollButton.interactable)
            return;

        rerollButton.interactable = false;

        board.ClearHighlights();

        // A reroll should feel useful: draw a number that
        // can actually be placed on the current board.
        if (!bag.TryDrawMatching(HasMatchingCell, out currentNumber))
        {
            FinishPhase("No match - continue.");
            return;
        }

        ballNumberText.text = currentNumber.ToString();

        board.HighlightMatches(currentNumber);

        awaitingPlacement = true;

        SetStatus("Pick a matching cell.");
    }

    private bool HasMatchingCell(int number)
    {
        foreach (BingoCellUI cell in board.Cells)
        {
            if (cell.CanPlace(number))
                return true;
        }

        return false;
    }

    private void DrawAndCheck(bool allowReroll)
    {
        if (!bag.TryDraw(out currentNumber))
        {
            FinishPhase("Bag empty.");
            return;
        }

        ballNumberText.text =
            currentNumber.ToString();

        // Full-board reward effects come later.
        if (!board.HasEmptyCells())
        {
            FinishPhase("Board full - " + currentNumber);

            return;
        }

        int matchingCells =
            board.HighlightMatches(currentNumber);

        if (matchingCells > 0)
        {
            awaitingPlacement = true;

            SetStatus("Pick a matching cell.");

            return;
        }

        if (allowReroll && bag.RemainingCount > 0)
        {
            rerollButton.interactable = true;

            SetStatus("No match - reroll.");

            return;
        }

        FinishPhase("No match - continue.");
    }

    private void HandleCellClicked(BingoCellUI cell)
    {
        if (!awaitingPlacement)
            return;

        if (!board.TryPlaceNumber(cell, currentNumber))
            return;

        awaitingPlacement = false;

        AudioManager.Play(Sfx.Place);

        string message = $"Placed {currentNumber}.";

        if (board.LastNewLines.Count > 0)
        {
            AudioManager.Play(Sfx.Bingo, 0.75f);

            message += "\nBINGO! ";

            for (int i = 0;
                 i < board.LastNewLines.Count;
                 i++)
            {
                if (i > 0)
                    message += ", ";

                message += BingoLineChecker.GetLineName(
                    board.LastNewLines[i]
                );
            }
        }

        FinishPhase(message);
    }

    private void FinishPhase(string message)
    {
        awaitingPlacement = false;
        phaseComplete = true;

        board.ClearHighlights();

        drawButton.interactable = false;
        rerollButton.interactable = false;

        continueToCombatButton.interactable = true;

        SetStatus(message);
    }

    private void ContinueToCombat()
    {
        if (!phaseComplete)
            return;

        phaseComplete = false;
        continueToCombatButton.interactable = false;

        PhaseFinished?.Invoke();
    }

    // Called later when a new enemy encounter begins.
    public void BeginNewEncounter()
    {
        bag.Reset();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log(message);
    }

    private void OnDestroy()
    {
        if (board != null && board.Cells != null)
        {
            foreach (BingoCellUI cell in board.Cells)
                cell.Clicked -= HandleCellClicked;
        }

        if (drawButton != null)
            drawButton.onClick.RemoveListener(DrawNumber);

        if (rerollButton != null)
            rerollButton.onClick.RemoveListener(Reroll);

        if (continueToCombatButton != null)
        {
            continueToCombatButton.onClick.RemoveListener(
                ContinueToCombat
            );
        }
    }
}
