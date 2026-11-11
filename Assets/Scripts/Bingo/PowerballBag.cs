
using System.Collections.Generic;
using UnityEngine;

public class PowerballBag
{
    private readonly List<int> availableNumbers = new();

    public int RemainingCount => availableNumbers.Count;

    public PowerballBag()
    {
        Reset();
    }

    // Called at the start of each enemy encounter.
    public void Reset()
    {
        availableNumbers.Clear();

        for (int number = 1; number <= 75; number++)
        {
            availableNumbers.Add(number);
        }
    }

    // Draw one random number without replacement.
    public bool TryDraw(out int number)
    {
        if (availableNumbers.Count == 0)
        {
            number = 0;
            return false;
        }

        int index = Random.Range(0, availableNumbers.Count);

        number = availableNumbers[index];

        availableNumbers.RemoveAt(index);

        return true;
    }
}
