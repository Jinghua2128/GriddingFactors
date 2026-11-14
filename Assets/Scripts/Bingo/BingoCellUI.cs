using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BingoCellUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text ruleText;
    [SerializeField] private TMP_Text buffText;

    private BingoRule rule;
    private string buffCode;
    public string BuffCode => buffCode;

    public bool IsMarked { get; private set; }
    public int PlacedNumber { get; private set; }

    [Header("Interaction")]
    [SerializeField] private Button bingoButton;
    [SerializeField] private GameObject eligibleHighlight;

    public event Action<BingoCellUI> Clicked;

    public bool IsEligible { get; private set; }

    private Image eligibleHighlightImage;
    private Outline eligibleOutline;
    private float pulseTime;

    [Header("Completed Bingo Visuals")]
    [SerializeField] private GameObject completedLineHighlight;
    [SerializeField] private TMP_Text lineCountBadge;

    public BingoRule CurrentRule => rule;

    public int BuffLevel { get; private set; }

    public void UpgradeBuff()
    {
        BuffLevel++;
        RefreshVisuals();
    }


    public bool SetRule(BingoRule newRule)
    {
        if (IsMarked)
            return false;

        rule = newRule;
        RefreshVisuals();

        return true;
    }


    public void SetCompletedLineCount(int count)
    {
        bool completed = count > 0;

        if (completedLineHighlight != null)
            completedLineHighlight.SetActive(completed);

        if (lineCountBadge != null)
        {
            lineCountBadge.gameObject.SetActive(completed);

            if (completed)
            {
                lineCountBadge.text =
                    count == 1 ? "★" : $"L{count}";
            }
        }
    }

    public void SetEligible(bool eligible)
    {
        IsEligible = eligible && !IsMarked;

        if (eligibleHighlight != null)
            eligibleHighlight.SetActive(IsEligible);

        if (bingoButton != null)
            bingoButton.interactable = IsEligible;

        if (eligibleOutline != null)
            eligibleOutline.enabled = IsEligible;

        if (buttonImage != null)
        {
            if (IsEligible)
            {
                buttonImage.color = Color.Lerp(
                    GetRuleColor(),
                    Color.white,
                    0.18f);
            }
            else if (!IsMarked)
            {
                buttonImage.color = GetRuleColor();
            }
        }

        if (!IsEligible)
        {
            transform.localScale = Vector3.one;

            if (eligibleHighlightImage != null)
            {
                Color c = eligibleHighlightImage.color;
                c.a = 0.25f;
                eligibleHighlightImage.color = c;
            }
        }
    }

    private void Update()
    {
        if (!IsEligible || IsMarked)
            return;

        pulseTime += Time.unscaledDeltaTime * 4.5f;

        float t = (Mathf.Sin(pulseTime) + 1f) * 0.5f;

        if (eligibleHighlightImage != null)
        {
            Color c = eligibleHighlightImage.color;
            c.a = Mathf.Lerp(0.18f, 0.55f, t);
            eligibleHighlightImage.color = c;
        }

        if (eligibleOutline != null)
        {
            Color c = eligibleOutline.effectColor;
            c.a = Mathf.Lerp(0.5f, 1f, t);
            eligibleOutline.effectColor = c;
        }

        transform.localScale =
            Vector3.one * Mathf.Lerp(1f, 1.045f, t);
    }

    private void HandleClick()
    {
        if (IsEligible)
            Clicked?.Invoke(this);
    }

    private void Awake()
    {
        if (buttonImage == null)
            buttonImage = GetComponentInChildren<Image>();

        if (bingoButton == null)
            bingoButton = GetComponentInChildren<Button>(true);

        if (bingoButton != null)
            bingoButton.onClick.AddListener(HandleClick);

        if (eligibleHighlight != null)
            eligibleHighlightImage =
                eligibleHighlight.GetComponent<Image>();

        if (buttonImage != null)
        {
            eligibleOutline =
                buttonImage.GetComponent<Outline>();

            if (eligibleOutline == null)
                eligibleOutline =
                    buttonImage.gameObject.AddComponent<Outline>();

            eligibleOutline.effectColor =
                new Color32(255, 220, 90, 255);
            eligibleOutline.effectDistance =
                new Vector2(3f, -3f);
            eligibleOutline.enabled = false;
        }

        SetEligible(false);

        RefreshVisuals();
    }

    public void Configure(BingoRule newRule, string newBuff)
    {
        BuffLevel = 0;
        rule = newRule;
        buffCode = newBuff;

        IsMarked = false;
        PlacedNumber = 0;

        SetCompletedLineCount(0);
        SetEligible(false);
        RefreshVisuals();
    }

    public bool CanPlace(int number)
    {
        return !IsMarked && rule.Matches(number);
    }

    public bool TryPlace(int number)
    {
        if (!CanPlace(number))
            return false;

        IsMarked = true;
        PlacedNumber = number;

        RefreshVisuals();
        return true;
    }

    private void RefreshVisuals()
    {
        if (buttonImage == null || ruleText == null ||
            buffText == null)
            return;

        if (IsMarked)
        {
            buttonImage.color = new Color32(210, 165, 65, 255);
            ruleText.text = PlacedNumber.ToString();
        }
        else
        {
            buttonImage.color = GetRuleColor();
            ruleText.text = rule.GetLabel();
        }

        buffText.text = BuffLevel > 0
            ? $"{buffCode} +{BuffLevel}"
            : buffCode;
        buffText.color = GetBuffColor();
        
    }

    private Color GetRuleColor()
    {
        switch (rule.Type)
        {
            case BingoRuleType.Even:
                return new Color32(70, 130, 200, 255);

            case BingoRuleType.Odd:
                return new Color32(140, 90, 190, 255);

            case BingoRuleType.DivisibleBy:
                return new Color32(60, 160, 155, 255);

            case BingoRuleType.GreaterThan:
            case BingoRuleType.LessThan:
                return new Color32(210, 140, 75, 255);

            case BingoRuleType.Prime:
                return new Color32(190, 85, 110, 255);

            case BingoRuleType.Wildcard:
            case BingoRuleType.Any:
                return new Color32(220, 180, 65, 255);

            default:
                return Color.gray;
        }
    }

    private Color GetBuffColor()
    {
        if (string.IsNullOrEmpty(buffCode))
            return Color.white;

        switch (buffCode[0])
        {
            case 'A':
                return new Color32(255, 120, 110, 255);

            case 'D':
                return new Color32(120, 200, 255, 255);

            case 'H':
                return new Color32(120, 255, 160, 255);

            case 'U':
                return new Color32(225, 170, 255, 255);

            default:
                return Color.white;
        }
    }
    private void OnDestroy()
    {
        if (bingoButton != null)
            bingoButton.onClick.RemoveListener(HandleClick);
    }
}
