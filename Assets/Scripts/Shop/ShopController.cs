
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    [Header("Purchasable Card Data")]
    [SerializeField] private CardData healCard;
    [SerializeField] private CardData piercingCard;

    [Header("Shop Display")]
    [SerializeField] private TMP_Text shopCoinsText;
    [SerializeField] private TMP_Text shopStatusText;

    [Header("Card Offer")]
    [SerializeField] private TMP_Text cardOfferText;
    [SerializeField] private TMP_Text cardPreviewText;
    [SerializeField] private TMP_Dropdown cardTargetDropdown;
    [SerializeField] private Button buyCardButton;

    [Header("Rule Offer")]
    [SerializeField] private TMP_Text ruleOfferText;
    [SerializeField] private TMP_Text rulePreviewText;
    [SerializeField] private TMP_Dropdown ruleTargetDropdown;
    [SerializeField] private Button buyRuleButton;

    [Header("Buff Offer")]
    [SerializeField] private TMP_Text buffOfferText;
    [SerializeField] private TMP_Text buffPreviewText;
    [SerializeField] private TMP_Dropdown buffTargetDropdown;
    [SerializeField] private Button buyBuffButton;

    public event Action PurchasesChanged;

    private const int RulePrice = 35;
    private const int BuffPrice = 20;

    private readonly List<CardInstance> cardTargets = new();
    private readonly List<int> ruleTargets = new();
    private readonly List<int> buffTargets = new();

    private RunProgress run;
    private CombatManager combat;
    private BingoBoard board;

    private CardData offeredCard;
    private int cardPrice;
    private bool cardIsUpgrade;

    private bool cardPurchased;
    private bool rulePurchased;
    private bool buffPurchased;

    private void Awake()
    {
        buyCardButton.onClick.AddListener(BuyCard);
        buyRuleButton.onClick.AddListener(BuyRule);
        buyBuffButton.onClick.AddListener(BuyBuff);

        cardTargetDropdown.onValueChanged.AddListener(
            HandleSelectionChanged
        );
        ruleTargetDropdown.onValueChanged.AddListener(
            HandleSelectionChanged
        );
        buffTargetDropdown.onValueChanged.AddListener(
            HandleSelectionChanged
        );
    }

    public void Open(
        RunProgress runProgress,
        CombatManager combatManager,
        BingoBoard bingoBoard,
        int shopNumber)
    {
        if (healCard == null || piercingCard == null)
        {
            Debug.LogError("Shop: Missing card assets.");
            return;
        }

        run = runProgress;
        combat = combatManager;
        board = bingoBoard;

        cardPurchased = false;
        rulePurchased = false;
        buffPurchased = false;

        // Predictable offers for our first implementation.
        cardIsUpgrade = shopNumber >= 3;
        cardPrice = shopNumber == 1 ? 10 : 20;

        offeredCard = shopNumber == 1
            ? healCard
            : piercingCard;

        cardOfferText.text = cardIsUpgrade
            ? $"Upgrade a Card — {cardPrice} coins"
            : $"Add {offeredCard.displayName} — {cardPrice} coins";

        ruleOfferText.text =
            $"Upgrade Condition to ANY — {RulePrice} coins";

        buffOfferText.text =
            $"Upgrade Bingo Buff — {BuffPrice} coins";

        cardTargetDropdown.gameObject.SetActive(cardIsUpgrade);

        BuildTargetLists();

        shopStatusText.text =
            "Select your upgrades or continue without buying.";

        RefreshViews();
    }

    private void BuildTargetLists()
    {
        cardTargets.Clear();
        ruleTargets.Clear();
        buffTargets.Clear();

        List<string> cardNames = new();
        List<string> ruleNames = new();
        List<string> buffNames = new();

        foreach (CardInstance card in combat.ShopCards)
        {
            cardTargets.Add(card);

            cardNames.Add(
                $"{card.Data.displayName} " +
                $"Lv.{card.UpgradeLevel} ({card.Value})"
            );
        }

        for (int i = 0; i < board.Cells.Length; i++)
        {
            BingoCellUI cell = board.Cells[i];

            // Centre Wildcard is not a normal upgrade target.
            if (i == 12)
                continue;

            string location =
                $"R{i / 5 + 1} C{i % 5 + 1}";

            // Conditions are upgradeable only if empty.
            if (!cell.IsMarked &&
                cell.CurrentRule.Type != BingoRuleType.Any)
            {
                ruleTargets.Add(i);

                ruleNames.Add(
                    $"{location} — {cell.CurrentRule.GetLabel()}"
                );
            }

            // Buffs can be upgraded on empty or marked cells.
            if (!string.IsNullOrEmpty(cell.BuffCode))
            {
                buffTargets.Add(i);

                buffNames.Add(
                    $"{location} — {cell.BuffCode} " +
                    $"Lv.{cell.BuffLevel}" +
                    (cell.IsMarked ? " [Filled]" : "")
                );
            }
        }

        PopulateDropdown(cardTargetDropdown, cardNames);
        PopulateDropdown(ruleTargetDropdown, ruleNames);
        PopulateDropdown(buffTargetDropdown, buffNames);
    }

    private void PopulateDropdown(
        TMP_Dropdown dropdown,
        List<string> names)
    {
        dropdown.ClearOptions();

        if (names.Count == 0)
            names.Add("No eligible targets");

        dropdown.AddOptions(names);
        dropdown.SetValueWithoutNotify(0);
        dropdown.RefreshShownValue();
    }

    private static int GetSelectedCell(
        TMP_Dropdown dropdown,
        List<int> indices)
    {
        int selected = dropdown.value;

        return selected >= 0 && selected < indices.Count
            ? indices[selected]
            : -1;
    }

    private CardInstance GetSelectedCard()
    {
        int selected = cardTargetDropdown.value;

        return selected >= 0 && selected < cardTargets.Count
            ? cardTargets[selected]
            : null;
    }

    private void RefreshViews()
    {
        if (run == null)
            return;

        shopCoinsText.text = $"Coins: {run.Coins}";

        CardInstance selectedCard = GetSelectedCard();

        if (cardIsUpgrade)
        {
            cardPreviewText.text = selectedCard == null
                ? "No cards available"
                : $"{selectedCard.Data.displayName}: " +
                  $"{selectedCard.Value} → " +
                  $"{selectedCard.Value + selectedCard.Data.upgradeIncrease}";
        }
        else
        {
            cardPreviewText.text =
                $"Add one {offeredCard.displayName} " +
                $"(Value: {offeredCard.baseValue})";
        }

        int ruleIndex =
            GetSelectedCell(ruleTargetDropdown, ruleTargets);

        rulePreviewText.text = ruleIndex < 0
            ? "No empty cells available"
            : $"{board.Cells[ruleIndex].CurrentRule.GetLabel()}" +
              " → ANY\nExisting buff unchanged";

        int buffIndex =
            GetSelectedCell(buffTargetDropdown, buffTargets);

        if (buffIndex < 0)
        {
            buffPreviewText.text = "No eligible buff cells";
        }
        else
        {
            BingoCellUI cell = board.Cells[buffIndex];

            buffPreviewText.text =
                $"{cell.BuffCode} Lv.{cell.BuffLevel}" +
                $" → Lv.{cell.BuffLevel + 1}" +
                (cell.IsMarked
                    ? "\nImproved effect triggers immediately"
                    : "\nImproved effect activates when filled");
        }

        buyCardButton.interactable =
            !cardPurchased &&
            run.Coins >= cardPrice &&
            (!cardIsUpgrade || selectedCard != null);

        buyRuleButton.interactable =
            !rulePurchased &&
            run.Coins >= RulePrice &&
            ruleIndex >= 0;

        buyBuffButton.interactable =
            !buffPurchased &&
            run.Coins >= BuffPrice &&
            buffIndex >= 0;

        if (cardPurchased)
            cardPreviewText.text += "\nPURCHASED";

        if (rulePurchased)
            rulePreviewText.text += "\nPURCHASED";

        if (buffPurchased)
            buffPreviewText.text += "\nPURCHASED";
    }

    private void HandleSelectionChanged(int _)
    {
        RefreshViews();
    }

    private void BuyCard()
    {
        if (cardPurchased || run.Coins < cardPrice)
            return;

        bool applied = cardIsUpgrade
            ? combat.TryUpgradeShopCard(GetSelectedCard())
            : combat.TryAddShopCard(offeredCard);

        if (!applied)
            return;

        // No other code changes coins during this
        // synchronous button-click operation.
        if (!run.TrySpendCoins(cardPrice))
        {
            Debug.LogError("Unexpected card payment failure.");
            return;
        }

        cardPurchased = true;

        CompletePurchase("Card purchased successfully!");
    }

    private void BuyRule()
    {
        if (rulePurchased || run.Coins < RulePrice)
            return;

        int index =
            GetSelectedCell(ruleTargetDropdown, ruleTargets);

        if (index < 0)
            return;

        bool applied = board.TryUpgradeRule(
            index,
            new BingoRule(BingoRuleType.Any)
        );

        if (!applied)
            return;

        if (!run.TrySpendCoins(RulePrice))
        {
            Debug.LogError("Unexpected rule payment failure.");
            return;
        }

        rulePurchased = true;

        CompletePurchase("Number condition upgraded!");
    }

    private void BuyBuff()
    {
        if (buffPurchased || run.Coins < BuffPrice)
            return;

        int index =
            GetSelectedCell(buffTargetDropdown, buffTargets);

        if (index < 0 || !board.TryUpgradeBuff(index))
            return;

        if (!run.TrySpendCoins(BuffPrice))
        {
            Debug.LogError("Unexpected buff payment failure.");
            return;
        }

        buffPurchased = true;

        CompletePurchase("Bingo buff upgraded!");
    }

    private void CompletePurchase(string message)
    {
        shopStatusText.text = message;

        RefreshViews();
        PurchasesChanged?.Invoke();
    }

    private void OnDestroy()
    {
        buyCardButton.onClick.RemoveListener(BuyCard);
        buyRuleButton.onClick.RemoveListener(BuyRule);
        buyBuffButton.onClick.RemoveListener(BuyBuff);

        cardTargetDropdown.onValueChanged.RemoveListener(
            HandleSelectionChanged
        );
        ruleTargetDropdown.onValueChanged.RemoveListener(
            HandleSelectionChanged
        );
        buffTargetDropdown.onValueChanged.RemoveListener(
            HandleSelectionChanged
        );
    }
}
