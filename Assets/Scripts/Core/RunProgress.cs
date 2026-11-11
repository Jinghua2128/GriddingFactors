
public readonly struct EncounterDefinition
{
    public string Name { get; }
    public int MaxHP { get; }
    public int AttackDamage { get; }
    public int CoinReward { get; }
    public bool IsBoss { get; }

    public EncounterDefinition(
        string name,
        int maxHP,
        int attackDamage,
        int coinReward,
        bool isBoss = false)
    {
        Name = name;
        MaxHP = maxHP;
        AttackDamage = attackDamage;
        CoinReward = coinReward;
        IsBoss = isBoss;
    }
}

public class RunProgress
{
    private static readonly EncounterDefinition[] Encounters =
    {
        new("Lucky Slime", 12, 2, 20),
        new("Encounter 2 Enemy", 16, 2, 30),
        new("Encounter 3 Enemy", 20, 3, 40),
        new("Encounter 4 Enemy", 24, 4, 50),
        new("Final Boss", 36, 5, 60, true)
    };

    private int encounterIndex;

    public int Coins { get; private set; } = 10;
    public int CompletedEncounters { get; private set; }

    public int EncounterNumber => encounterIndex + 1;
    public int TotalEncounters => Encounters.Length;

    public bool IsFinalEncounter =>
        encounterIndex == Encounters.Length - 1;

    public EncounterDefinition CurrentEncounter =>
        Encounters[encounterIndex];

    public bool TryClaimVictoryReward(out int reward)
    {
        reward = 0;

        // Prevent awarding the same encounter twice.
        if (CompletedEncounters != encounterIndex)
            return false;

        reward = CurrentEncounter.CoinReward;
        Coins += reward;
        CompletedEncounters++;

        return true;
    }

    public bool TryAdvance()
    {
        // Must complete the current encounter first.
        if (CompletedEncounters != encounterIndex + 1)
            return false;

        if (IsFinalEncounter)
            return false;

        encounterIndex++;
        return true;
    }

    // Used when implementing the real shop later.
    public bool TrySpendCoins(int amount)
    {
        if (amount < 0 || Coins < amount)
            return false;

        Coins -= amount;
        return true;
    }
}
