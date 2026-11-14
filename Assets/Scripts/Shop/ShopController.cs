
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
    [SerializeField] private Sprite boxFrameSprite;

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

    private ShopTargetOverlay targetOverlay;

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
        targetOverlay = gameObject.AddComponent<ShopTargetOverlay>();

        if (transform is RectTransform rect && shopStatusText != null)
            targetOverlay.Build(
                rect,
                shopStatusText.font,
                shopStatusText.fontSharedMaterial);

        AddShopBackdrop();
        AddShopSign();

        AddBoxFrame(cardOfferText.transform.parent as RectTransform);
        AddBoxFrame(ruleOfferText.transform.parent as RectTransform);
        AddBoxFrame(buffOfferText.transform.parent as RectTransform);

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
            ? "Upgrade Card"
            : $"Add {offeredCard.displayName}";

        ruleOfferText.text = "Upgrade Condition";

        buffOfferText.text = "Upgrade Buff";

        BuildTargetLists();

        shopStatusText.text = "Choose one upgrade.";

        SetButtonLabel(buyCardButton, $"Buy {cardPrice}");
        SetButtonLabel(buyRuleButton, $"Buy {RulePrice}");
        SetButtonLabel(buyBuffButton, $"Buy {BuffPrice}");

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

        shopCoinsText.text = $"Coins {run.Coins}";

        cardPreviewText.text = cardIsUpgrade
            ? "Choose a card"
            : DescribeCard(offeredCard);

        rulePreviewText.text = ruleTargets.Count == 0
            ? "No empty cells"
            : "Choose a cell";

        buffPreviewText.text = buffTargets.Count == 0
            ? "No buff cells"
            : "Choose a cell";

        buyCardButton.interactable =
            !cardPurchased &&
            run.Coins >= cardPrice &&
            (!cardIsUpgrade || cardTargets.Count > 0);

        buyRuleButton.interactable =
            !rulePurchased &&
            run.Coins >= RulePrice &&
            ruleTargets.Count > 0;

        buyBuffButton.interactable =
            !buffPurchased &&
            run.Coins >= BuffPrice &&
            buffTargets.Count > 0;

        SetButtonLabel(buyCardButton,
            cardPurchased ? "Purchased" : $"Buy {cardPrice}");
        SetButtonLabel(buyRuleButton,
            rulePurchased ? "Purchased" : $"Buy {RulePrice}");
        SetButtonLabel(buyBuffButton,
            buffPurchased ? "Purchased" : $"Buy {BuffPrice}");
    }

    private void HandleSelectionChanged(int _)
    {
        RefreshViews();
    }

    private void BuyCard()
    {
        if (cardPurchased || run.Coins < cardPrice)
            return;

        if (cardIsUpgrade)
        {
            if (targetOverlay == null || cardTargets.Count == 0)
                return;

            targetOverlay.ShowCards(
                cardTargets,
                "Upgrade which card?",
                ApplyCardPurchase);

            return;
        }

        ApplyCardPurchase(null);
    }

    private void ApplyCardPurchase(CardInstance target)
    {
        if (cardPurchased || run.Coins < cardPrice)
            return;

        bool applied = cardIsUpgrade
            ? combat.TryUpgradeShopCard(target)
            : combat.TryAddShopCard(offeredCard);

        if (!applied)
            return;

        if (!run.TrySpendCoins(cardPrice))
        {
            Debug.LogError("Unexpected card payment failure.");
            return;
        }

        cardPurchased = true;

        CompletePurchase("Card purchased.");
    }

    private void BuyRule()
    {
        if (rulePurchased || run.Coins < RulePrice)
            return;

        if (targetOverlay == null || ruleTargets.Count == 0)
            return;

        targetOverlay.ShowCells(
            board,
            ruleTargets,
            "Upgrade which cell?",
            ApplyRulePurchase);
    }

    private void ApplyRulePurchase(int index)
    {
        if (rulePurchased || run.Coins < RulePrice)
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

        CompletePurchase("Condition upgraded.");
    }

    private void BuyBuff()
    {
        if (buffPurchased || run.Coins < BuffPrice)
            return;

        if (targetOverlay == null || buffTargets.Count == 0)
            return;

        targetOverlay.ShowCells(
            board,
            buffTargets,
            "Upgrade which cell?",
            ApplyBuffPurchase);
    }

    private void ApplyBuffPurchase(int index)
    {
        if (buffPurchased || run.Coins < BuffPrice)
            return;

        if (!board.TryUpgradeBuff(index))
            return;

        if (!run.TrySpendCoins(BuffPrice))
        {
            Debug.LogError("Unexpected buff payment failure.");
            return;
        }

        buffPurchased = true;

        CompletePurchase("Buff upgraded.");
    }

    private void CompletePurchase(string message)
    {
        AudioManager.Play(Sfx.Buy);

        shopStatusText.text = message;

        RefreshViews();
        PurchasesChanged?.Invoke();
    }

    private void AddShopBackdrop()
    {
        Sprite backdrop = Resources.Load<Sprite>("UI/shop_bg");

        if (backdrop == null)
            return;

        GameObject go = new GameObject(
            "ShopBackdrop",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.GetComponent<Image>();
        image.sprite = backdrop;
        image.color = new Color(1f, 1f, 1f, 0.12f);
        image.preserveAspect = true;
        image.raycastTarget = false;

        go.transform.SetAsFirstSibling();
    }

    private void AddShopSign()
    {
        Sprite sign = Resources.Load<Sprite>("UI/shop_sign");

        if (sign == null)
            return;

        GameObject go = new GameObject(
            "ShopSign",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -10f);
        rect.sizeDelta = new Vector2(200f, 130f);

        Image image = go.GetComponent<Image>();
        image.sprite = sign;
        image.preserveAspect = true;
        image.raycastTarget = false;

        go.transform.SetAsLastSibling();
    }

    private void AddBoxFrame(RectTransform panel)
    {
        if (panel == null || boxFrameSprite == null)
            return;

        GameObject go = new GameObject(
            "BoxFrame",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(panel, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.GetComponent<Image>();
        image.sprite = boxFrameSprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;
        image.color = Color.white;

        go.transform.SetAsFirstSibling();
    }

    private static void SetButtonLabel(Button button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();

        if (label != null)
            label.text = text;
    }

    private static string DescribeCard(CardData card)
    {
        switch (card.type)
        {
            case CardType.Attack:
                return $"Deal {card.baseValue} damage";
            case CardType.Defend:
                return $"Gain {card.baseValue} Shield";
            case CardType.Focus:
                return $"+{card.baseValue} next Attack";
            case CardType.Heal:
                return $"Restore {card.baseValue} HP";
            case CardType.PiercingAttack:
                return $"Deal {card.baseValue} damage, pierces Shield";
            default:
                return "";
        }
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
