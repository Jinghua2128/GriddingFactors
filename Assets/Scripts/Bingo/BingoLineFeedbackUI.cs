
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BingoLineFeedbackUI : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private BingoBoard board;
    [SerializeField] private BingoUltimateTracker ultimates;

    [Header("UI")]
    [SerializeField] private TMP_Text bingoMessageText;
    [SerializeField] private TMP_Text pendingUltimatesText;
    
    [Header("Bingo Ultimates")]
    [SerializeField] private BingoUltimateTracker ultimateTracker;

    private void Start()
    {
        if (board == null || ultimates == null)
        {
            Debug.LogError("Missing Bingo references.");
            return;
        }

        board.Initialize();

        board.LinesCompleted += HandleNewLines;
        ultimates.Changed += RefreshUltimateText;

        RefreshCompletedCells();
        RefreshUltimateText();

        if (bingoMessageText != null)
            bingoMessageText.text = "";
    }

    private void HandleNewLines(IReadOnlyList<int> lines)
    {
        RefreshCompletedCells();

        List<string> names = new List<string>();

        foreach (int line in lines)
        {
            names.Add(BingoLineChecker.GetLineName(line));
        }

        if (bingoMessageText != null)
        {
            bingoMessageText.text =
                "BINGO!\n" + string.Join(" + ", names);
        }
    }

    private void RefreshCompletedCells()
    {
        if (board.Cells == null)
            return;

        int[] counts = new int[board.Cells.Length];

        for (int line = 0; line < 12; line++)
        {
            if (!board.IsLineCompleted(line))
                continue;

            foreach (int cellIndex in
                     BingoLineChecker.GetLineCells(line))
            {
                counts[cellIndex]++;
            }
        }

        for (int i = 0; i < counts.Length; i++)
        {
            board.Cells[i].SetCompletedLineCount(counts[i]);
        }
    }

    private void RefreshUltimateText()
    {
        if (pendingUltimatesText == null)
            return;

        int horizontal = ultimates.HorizontalCharges;
        int vertical = ultimates.VerticalCharges;
        int diagonal = ultimates.DiagonalCharges;

        List<string> bonuses = new List<string>();

        if (horizontal > 0)
            bonuses.Add($"Attack x{1 << horizontal}");

        if (vertical > 0)
            bonuses.Add($"Block {vertical}");

        if (diagonal > 0)
            bonuses.Add($"Card x{1 << diagonal}, Heal {diagonal * 5}");

        pendingUltimatesText.text = bonuses.Count == 0
            ? ""
            : "BONUS\n" + string.Join("\n", bonuses);
    }

    private void OnDestroy()
    {
        if (board != null)
            board.LinesCompleted -= HandleNewLines;

        if (ultimates != null)
            ultimates.Changed -= RefreshUltimateText;
    }
}
