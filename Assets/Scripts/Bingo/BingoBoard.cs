using System;
using System.Collections.Generic;
using UnityEngine;

public class BingoBoard : MonoBehaviour
{
    private static readonly
        (BingoRuleType type, int value, string buff)[] Layout =
    {
        // Row 1
        (BingoRuleType.DivisibleBy, 4, "H1"),
        (BingoRuleType.GreaterThan, 30, "U2"),
        (BingoRuleType.Odd, 0, "D1"),
        (BingoRuleType.DivisibleBy, 4, "A1"),
        (BingoRuleType.Odd, 0, "D2"),

        // Row 2
        (BingoRuleType.Odd, 0, "H2"),
        (BingoRuleType.DivisibleBy, 5, "D2"),
        (BingoRuleType.Even, 0, "U2"),
        (BingoRuleType.Prime, 0, "A1"),
        (BingoRuleType.GreaterThan, 45, "A1"),

        // Row 3
        (BingoRuleType.LessThan, 40, "D1"),
        (BingoRuleType.Even, 0, "A2"),
        (BingoRuleType.Wildcard, 0, "LUCKY"),
        (BingoRuleType.DivisibleBy, 3, "H1"),
        (BingoRuleType.Prime, 0, "H1"),

        // Row 4
        (BingoRuleType.Even, 0, "A1"),
        (BingoRuleType.Prime, 0, "H2"),
        (BingoRuleType.DivisibleBy, 5, "A2"),
        (BingoRuleType.LessThan, 25, "D1"),
        (BingoRuleType.DivisibleBy, 3, "U1"),

        // Row 5
        (BingoRuleType.Prime, 0, "A2"),
        (BingoRuleType.Even, 0, "D2"),
        (BingoRuleType.DivisibleBy, 6, "H2"),
        (BingoRuleType.Odd, 0, "U1"),
        (BingoRuleType.DivisibleBy, 3, "A2")
    };

    private readonly BingoLineChecker lineChecker =
        new BingoLineChecker();

    public IReadOnlyList<int> LastNewLines { get; private set; }
        = Array.Empty<int>();

    public int CompletedLineCount =>
        lineChecker.CompletedCount;
    
    public event Action<IReadOnlyList<int>> LinesCompleted;

    public bool IsLineCompleted(int line)
    {
        return lineChecker.IsAwarded(line);
    }

    public BingoCellUI[] Cells { get; private set; }

    public event Action<string> CellBuffTriggered;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (Cells != null)
            return;

        if (transform.childCount != Layout.Length)
        {
            Debug.LogError("BingoGrid must have 25 cells.");
            return;
        }

        BingoCellUI[] found = new BingoCellUI[Layout.Length];

        for (int i = 0; i < Layout.Length; i++)
        {
            found[i] = transform.GetChild(i)
                .GetComponent<BingoCellUI>();

            if (found[i] == null)
            {
                Debug.LogError($"Missing BingoCellUI at {i}");
                return;
            }
        }

        Cells = found;

        for (int i = 0; i < Layout.Length; i++)
        {
            var data = Layout[i];

            Cells[i].Configure(
                new BingoRule(data.type, data.value),
                data.buff
            );
        }
    }

    public int HighlightMatches(int number)
    {
        int count = 0;

        foreach (BingoCellUI cell in Cells)
        {
            bool matches = cell.CanPlace(number);

            cell.SetEligible(matches);

            if (matches)
                count++;
        }

        return count;
    }

    public void ClearHighlights()
    {
        if (Cells == null)
            return;

        foreach (BingoCellUI cell in Cells)
            cell.SetEligible(false);
    }

    public bool HasEmptyCells()
    {
        if (Cells == null)
            return false;

        foreach (BingoCellUI cell in Cells)
        {
            if (!cell.IsMarked)
                return true;
        }

        return false;
    }


    public bool TryPlaceNumber(BingoCellUI cell, int number)
    {
        if (Cells == null ||
            Array.IndexOf(Cells, cell) < 0 ||
            !cell.IsEligible)
        {
            return false;
        }

        if (!cell.TryPlace(number))
            return false;

        ClearHighlights();
        // Tell other systems which buff was activated.
        CellBuffTriggered?.Invoke(cell.BuffCode);

        // Collect the occupied state of all 25 cells.
        bool[] marked = new bool[Cells.Length];

        for (int i = 0; i < Cells.Length; i++)
        {
            marked[i] = Cells[i].IsMarked;
        }

        // Detect every newly completed line.
        LastNewLines = lineChecker.FindNewLines(marked);

        foreach (int line in LastNewLines)
        {
            Debug.Log(
                $"BINGO! {BingoLineChecker.GetLineName(line)}" +
                $" | Reward: {BingoLineChecker.GetUltimateName(line)}"
            );
        }

        Debug.Log(
            $"Completed lines: {CompletedLineCount}/12"
        );

        if (LastNewLines.Count > 0)
        {
            LinesCompleted?.Invoke(LastNewLines);
        }

        return true;
    }
}
