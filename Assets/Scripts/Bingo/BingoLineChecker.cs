
using System;
using System.Collections.Generic;

public enum BingoLineDirection
{
    Horizontal,
    Vertical,
    Diagonal
}

public class BingoLineChecker
{
    // Indexes match the 25 BingoCell children,
    // starting at the top-left (index 0).
    private static readonly int[][] Lines =
    {
        // Horizontal: 0-4
        new[] { 0, 1, 2, 3, 4 },
        new[] { 5, 6, 7, 8, 9 },
        new[] { 10, 11, 12, 13, 14 },
        new[] { 15, 16, 17, 18, 19 },
        new[] { 20, 21, 22, 23, 24 },

        // Vertical: 5-9
        new[] { 0, 5, 10, 15, 20 },
        new[] { 1, 6, 11, 16, 21 },
        new[] { 2, 7, 12, 17, 22 },
        new[] { 3, 8, 13, 18, 23 },
        new[] { 4, 9, 14, 19, 24 },

        // Diagonal: 10-11
        new[] { 0, 6, 12, 18, 24 },
        new[] { 4, 8, 12, 16, 20 }
    };

    private readonly bool[] awarded = new bool[12];

    public int CompletedCount { get; private set; }

    public List<int> FindNewLines(bool[] marked)
    {
        if (marked == null || marked.Length != 25)
            throw new ArgumentException(
                "Bingo board must contain 25 cells."
            );

        List<int> newLines = new List<int>();

        for (int line = 0; line < Lines.Length; line++)
        {
            // Never award the same line twice.
            if (awarded[line])
                continue;

            bool complete = true;

            foreach (int cellIndex in Lines[line])
            {
                if (!marked[cellIndex])
                {
                    complete = false;
                    break;
                }
            }

            if (!complete)
                continue;

            awarded[line] = true;
            CompletedCount++;
            newLines.Add(line);
        }

        return newLines;
    }

    public bool IsAwarded(int line)
    {
        if (line < 0 || line >= 12)
            throw new ArgumentOutOfRangeException(nameof(line));

        return awarded[line];
    }


    public static BingoLineDirection GetDirection(int line)
    {
        if (line < 0 || line >= 12)
            throw new ArgumentOutOfRangeException(nameof(line));

        if (line < 5)
            return BingoLineDirection.Horizontal;

        if (line < 10)
            return BingoLineDirection.Vertical;

        return BingoLineDirection.Diagonal;
    }

    public static string GetLineName(int line)
    {
        if (line < 0 || line >= 12)
            throw new ArgumentOutOfRangeException(nameof(line));

        if (line < 5)
            return $"Row {line + 1}";

        if (line < 10)
            return $"Column {line - 4}";

        return line == 10 ? "Diagonal ↘" : "Diagonal ↙";
    }

    public static string GetUltimateName(int line)
    {
        switch (GetDirection(line))
        {
            case BingoLineDirection.Horizontal:
                return "×2 next Attack";

            case BingoLineDirection.Vertical:
                return "Block next enemy Attack";

            case BingoLineDirection.Diagonal:
                return "Double next card + Heal 5";

            default:
                return "";
        }
    }

    // Allows us to highlight completed lines later.
    public static IReadOnlyList<int> GetLineCells(int line)
    {
        return Lines[line];
    }

    // Call only when starting an entirely new run.
    public void Reset()
    {
        Array.Clear(awarded, 0, awarded.Length);
        CompletedCount = 0;
    }
}
