
using UnityEngine;

public enum CardType
{
    Attack,
    Defend,
    Focus,
    Heal,
    PiercingAttack
}

[CreateAssetMenu(
    fileName = "NewCard",
    menuName = "Lucky Grid/Card"
)]
public class CardData : ScriptableObject
{
    public CardType type;
    public string displayName;
    public int baseValue = 2;
    public int upgradeIncrease = 2;
}
