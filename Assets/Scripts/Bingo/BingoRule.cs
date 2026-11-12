
public enum BingoRuleType
{
    Even,
    Odd,
    DivisibleBy,
    GreaterThan,
    LessThan,
    Prime,
    Wildcard,
    Any
}

public readonly struct BingoRule
{
    public readonly BingoRuleType Type;
    public readonly int Value;

    public BingoRule(BingoRuleType type, int value = 0)
    {
        Type = type;
        Value = value;
    }

    public bool Matches(int number)
    {
        if (number < 1 || number > 75)
            return false;

        switch (Type)
        {
            case BingoRuleType.Even:
                return number % 2 == 0;

            case BingoRuleType.Odd:
                return number % 2 != 0;

            case BingoRuleType.DivisibleBy:
                return Value != 0 && number % Value == 0;

            case BingoRuleType.GreaterThan:
                return number > Value;

            case BingoRuleType.LessThan:
                return number < Value;

            case BingoRuleType.Prime:
                if (number < 2) return false;

                for (int i = 2; i * i <= number; i++)
                {
                    if (number % i == 0)
                        return false;
                }

                return true;

            case BingoRuleType.Wildcard:
                return true;

            default:
                return false;
        }
    }

    public string GetLabel()
    {
        switch (Type)
        {
            case BingoRuleType.Even: return "EVEN";
            case BingoRuleType.Odd: return "ODD";
            case BingoRuleType.DivisibleBy: return "DIV" + Value;
            case BingoRuleType.GreaterThan: return ">" + Value;
            case BingoRuleType.LessThan: return "<" + Value;
            case BingoRuleType.Prime: return "PRIME";
            case BingoRuleType.Wildcard: return "WILD";
            default: return "?";
        }
    }
}
