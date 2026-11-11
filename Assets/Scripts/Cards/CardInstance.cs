public class CardInstance
{
    public CardData Data { get; }

    public int UpgradeLevel { get; private set; }

    public int Value =>
        Data.baseValue +
        UpgradeLevel * Data.upgradeIncrease;

    public CardInstance(CardData data)
    {
        Data = data;
        UpgradeLevel = 0;
    }

    public void Upgrade()
    {
        UpgradeLevel++;
    }
}
