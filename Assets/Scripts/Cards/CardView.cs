using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image artwork;
    [SerializeField] private Image cardTint;
    [SerializeField] private TMP_Text valueText;

    [Header("Artwork")]
    [SerializeField] private Sprite attackArt;
    [SerializeField] private Sprite defendArt;
    [SerializeField] private Sprite focusArt;
    [SerializeField] private Sprite healArt;
    [SerializeField] private Sprite piercingArt;

    private CardInstance card;
    private Action<CardInstance> onPlayed;
    private TMP_Text ultimateBadge;

    private void Awake()
    {
        ApplyCardLayout();

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(HandleClick);
    }

    private void ApplyCardLayout()
    {
        RectTransform root = transform as RectTransform;

        if (root == null)
            return;

        root.sizeDelta = new Vector2(240f, 330f);

        if (titleText != null)
            titleText.fontSize = 24;

        if (valueText != null)
            valueText.fontSize = 46;

        if (descriptionText != null)
            descriptionText.fontSize = 14;

        LayoutElement layout = GetComponent<LayoutElement>();

        if (layout != null)
        {
            layout.preferredWidth = 240f;
            layout.preferredHeight = 330f;
        }

        GameObject clipGo = new GameObject(
            "ArtClip",
            typeof(RectTransform),
            typeof(RectMask2D));

        RectTransform clip = clipGo.GetComponent<RectTransform>();
        clip.SetParent(root, false);
        clip.anchorMin = new Vector2(0f, 1f);
        clip.anchorMax = new Vector2(1f, 1f);
        clip.pivot = new Vector2(0.5f, 1f);
        clip.anchoredPosition = new Vector2(0f, -12f);
        clip.sizeDelta = new Vector2(-24f, 170f);

        if (artwork != null)
        {
            RectTransform art = artwork.rectTransform;
            art.SetParent(clip, false);
            art.anchorMin = Vector2.zero;
            art.anchorMax = Vector2.one;
            art.offsetMin = Vector2.zero;
            art.offsetMax = Vector2.zero;
            artwork.preserveAspect = false;
        }

        SetCardTextRect(
            titleText, new Vector2(0f, -42f),
            new Vector2(220f, 34f));

        SetCardTextRect(
            valueText, new Vector2(0f, -88f),
            new Vector2(220f, 56f));

        SetCardTextRect(
            descriptionText, new Vector2(0f, -140f),
            new Vector2(220f, 36f));

        if (cardTint != null)
        {
            cardTint.rectTransform.anchorMin = Vector2.zero;
            cardTint.rectTransform.anchorMax = Vector2.one;
            cardTint.rectTransform.offsetMin = Vector2.zero;
            cardTint.rectTransform.offsetMax = Vector2.zero;
        }

        Transform frame = transform.Find("FrameOverlay");

        if (frame is RectTransform frameRect)
        {
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
        }

        if (cardTint != null)
            cardTint.transform.SetSiblingIndex(0);

        clip.SetSiblingIndex(1);

        if (frame != null)
            frame.SetSiblingIndex(2);

        titleText.transform.SetSiblingIndex(3);
        valueText.transform.SetSiblingIndex(4);
        descriptionText.transform.SetSiblingIndex(5);
    }

    private static void SetCardTextRect(
        TMP_Text text,
        Vector2 position,
        Vector2 size)
    {
        if (text == null)
            return;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    public void Bind(
        CardInstance instance,
        Action<CardInstance> callback)
    {
        card = instance;
        onPlayed = callback;

        titleText.text = card.Data.displayName;

        Sprite art = null;
        Color accent = new Color32(32, 39, 43, 255);
        Color tint = new Color32(255, 252, 247, 255);

        switch (card.Data.type)
        {
            case CardType.Attack:
                descriptionText.text = $"Deal {card.Value} damage";
                art = attackArt;
                accent = new Color32(201, 111, 92, 255);
                tint = new Color32(241, 213, 203, 255);
                break;

            case CardType.Defend:
                descriptionText.text = $"Gain {card.Value} Shield";
                art = defendArt;
                accent = new Color32(79, 143, 135, 255);
                tint = new Color32(202, 220, 214, 255);
                break;

            case CardType.Focus:
                descriptionText.text = $"+{card.Value} next Attack";
                art = focusArt;
                accent = new Color32(176, 138, 62, 255);
                tint = new Color32(235, 224, 196, 255);
                break;

            case CardType.Heal:
                descriptionText.text = $"Restore {card.Value} HP";
                art = healArt;
                accent = new Color32(79, 122, 91, 255);
                tint = new Color32(208, 224, 210, 255);
                break;

            case CardType.PiercingAttack:
                descriptionText.text = $"Deal {card.Value} damage\nIgnores Shield";
                art = piercingArt;
                accent = new Color32(138, 120, 168, 255);
                tint = new Color32(221, 212, 231, 255);
                break;

            default:
                descriptionText.text = "Unknown effect";
                break;
        }

        if (artwork != null)
        {
            artwork.sprite = art;
            artwork.enabled = art != null;
        }

        if (cardTint != null)
            cardTint.color = tint;

        if (valueText != null)
        {
            valueText.text = card.Data.type == CardType.Focus
                ? "+" + card.Value
                : card.Value.ToString();
            valueText.color = accent;
        }

        SetUltimateVisual(card.Data.type == CardType.PiercingAttack);
    }

    private void SetUltimateVisual(bool ultimate)
    {
        if (ultimate && ultimateBadge == null)
        {
            GameObject go = new GameObject(
                "UltimateBadge",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));

            go.transform.SetParent(transform, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(10f, -8f);
            rect.sizeDelta = new Vector2(100f, 18f);

            ultimateBadge = go.GetComponent<TMP_Text>();
            ultimateBadge.font = titleText.font;
            ultimateBadge.fontSharedMaterial = titleText.fontSharedMaterial;
            ultimateBadge.text = "ULTIMATE";
            ultimateBadge.fontSize = 10;
            ultimateBadge.color = new Color32(90, 70, 120, 255);
            ultimateBadge.alignment = TextAlignmentOptions.Left;
            ultimateBadge.textWrappingMode = TextWrappingModes.NoWrap;
            ultimateBadge.raycastTarget = false;

            go.transform.SetAsLastSibling();
        }

        if (ultimateBadge != null)
            ultimateBadge.gameObject.SetActive(ultimate);

        Transform frame = transform.Find("FrameOverlay");

        if (frame == null)
            return;

        Image frameImage = frame.GetComponent<Image>();

        if (frameImage == null)
            return;

        Outline glow = frameImage.GetComponent<Outline>();

        if (glow == null)
            glow = frameImage.gameObject.AddComponent<Outline>();

        glow.enabled = ultimate;
        glow.effectColor = new Color32(184, 172, 205, 220);
        glow.effectDistance = new Vector2(3f, -3f);
    }

    private void HandleClick()
    {
        if (card != null)
            onPlayed?.Invoke(card);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}
