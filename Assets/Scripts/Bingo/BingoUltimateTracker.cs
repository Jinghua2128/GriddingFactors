
using System;
using System.Collections.Generic;
using UnityEngine;

public class BingoUltimateTracker : MonoBehaviour
{
    [SerializeField] private BingoBoard board;

    public int HorizontalCharges { get; private set; }
    public int VerticalCharges { get; private set; }
    public int DiagonalCharges { get; private set; }

    public event Action Changed;

    private void Start()
    {
        if (board == null)
        {
            Debug.LogError("Ultimate Tracker: Board missing.");
            return;
        }

        board.LinesCompleted += HandleLinesCompleted;
    }

    private void HandleLinesCompleted(IReadOnlyList<int> lines)
    {
        foreach (int line in lines)
        {
            switch (BingoLineChecker.GetDirection(line))
            {
                case BingoLineDirection.Horizontal:
                    HorizontalCharges++;
                    break;

                case BingoLineDirection.Vertical:
                    VerticalCharges++;
                    break;

                case BingoLineDirection.Diagonal:
                    DiagonalCharges++;
                    break;
            }
        }

        Changed?.Invoke();
    }

    // Use ALL Horizontal charges on the next Attack.
    public int ConsumeHorizontalMultiplier()
    {
        if (HorizontalCharges == 0)
            return 1;

        int multiplier = 1 << HorizontalCharges;

        HorizontalCharges = 0;
        Changed?.Invoke();

        return multiplier;
    }

    // Use ALL Diagonal charges on the next card.
    public int ConsumeDiagonalMultiplier(out int healing)
    {
        if (DiagonalCharges == 0)
        {
            healing = 0;
            return 1;
        }

        int multiplier = 1 << DiagonalCharges;
        healing = 5 * DiagonalCharges;

        DiagonalCharges = 0;
        Changed?.Invoke();

        return multiplier;
    }

    // Block one enemy Attack per charge.
    public bool TryConsumeVerticalBlock()
    {
        if (VerticalCharges <= 0)
            return false;

        VerticalCharges--;
        Changed?.Invoke();

        return true;
    }

    // Only call when starting a completely new run.
    public void ResetForNewRun()
    {
        HorizontalCharges = 0;
        VerticalCharges = 0;
        DiagonalCharges = 0;

        Changed?.Invoke();
    }

    private void OnDestroy()
    {
        if (board != null)
            board.LinesCompleted -= HandleLinesCompleted;
    }
}
