using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopTargetOverlay : MonoBehaviour
{
    private RectTransform root;
    private RectTransform content;
    private TMP_Text titleText;
    private TMP_Text subtitleText;
    private TMP_FontAsset font;
    private Material fontMaterial;

    public void Build(
        RectTransform parent,
        TMP_FontAsset fontAsset,
        Material material)
    {
        font = fontAsset;
        fontMaterial = material;

        GameObject rootGo = new GameObject(
            "ShopTargetOverlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        root = rootGo.GetComponent<RectTransform>();
        root.SetParent(parent, false);
        Stretch(root);

        Image dim = rootGo.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        dim.raycastTarget = true;

        GameObject panelGo = new GameObject(
            "Panel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform panel = panelGo.GetComponent<RectTransform>();
        panel.SetParent(root, false);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(760f, 700f);

        Image panelImage = panelGo.GetComponent<Image>();
        panelImage.color = new Color32(251, 248, 242, 255);

        Outline outline = panelGo.AddComponent<Outline>();
        outline.effectColor = new Color32(32, 39, 43, 45);
        outline.effectDistance = new Vector2(2f, -2f);

        titleText = CreateText(
            panel, "Title", "Choose a cell", 30,
            new Color32(32, 39, 43, 255), TextAlignmentOptions.Center);
        SetRect(titleText.rectTransform, new Vector2(0f, -24f),
            new Vector2(700f, 50f), new Vector2(0.5f, 1f));

        subtitleText = CreateText(
            panel, "Subtitle", "", 18,
            new Color32(104, 119, 122, 255), TextAlignmentOptions.Center);
        SetRect(subtitleText.rectTransform, new Vector2(0f, -82f),
            new Vector2(700f, 30f), new Vector2(0.5f, 1f));

        GameObject contentGo = new GameObject(
            "Content",
            typeof(RectTransform));

        content = contentGo.GetComponent<RectTransform>();
        content.SetParent(panel, false);
        content.anchorMin = new Vector2(0.5f, 0.5f);
        content.anchorMax = new Vector2(0.5f, 0.5f);
        content.pivot = new Vector2(0.5f, 0.5f);
        content.anchoredPosition = new Vector2(0f, -26f);
        content.sizeDelta = new Vector2(700f, 480f);

        Button cancel = CreateButton(
            panel, "Cancel", "Cancel", 18,
            new Color32(231, 155, 136, 255),
            new Vector2(180f, 52f));

        RectTransform cancelRect = cancel.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0f);
        cancelRect.anchorMax = new Vector2(0.5f, 0f);
        cancelRect.pivot = new Vector2(0.5f, 0f);
        cancelRect.anchoredPosition = new Vector2(0f, 28f);

        cancel.onClick.AddListener(Hide);
        rootGo.SetActive(false);
    }

    public void ShowCells(
        BingoBoard board,
        IReadOnlyList<int> eligible,
        string title,
        Action<int> onPick)
    {
        if (root == null)
            return;

        ClearContent();
        titleText.text = title;
        subtitleText.text = "Highlighted cells can be upgraded.";

        GameObject gridGo = new GameObject(
            "Grid",
            typeof(RectTransform),
            typeof(GridLayoutGroup));

        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.SetParent(content, false);
        Stretch(gridRect);

        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(88f, 88f);
        grid.spacing = new Vector2(8f, 8f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.MiddleCenter;

        HashSet<int> allowed = new HashSet<int>(eligible);

        for (int i = 0; i < board.Cells.Length; i++)
        {
            int index = i;
            BingoCellUI cell = board.Cells[i];
            bool canPick = allowed.Contains(i);

            string label = cell.CurrentRule.GetLabel() +
                "\nR" + (i / 5 + 1) + "C" + (i % 5 + 1);

            if (!string.IsNullOrEmpty(cell.BuffCode))
                label += "\n" + cell.BuffCode;

            Button button = CreateButton(
                gridRect, "Cell" + i, label, 12,
                canPick
                    ? new Color32(202, 220, 214, 255)
                    : new Color32(232, 230, 224, 180),
                Vector2.zero);

            button.interactable = canPick;

            if (canPick)
            {
                button.onClick.AddListener(() =>
                {
                    onPick(index);
                    Hide();
                });
            }
        }

        root.gameObject.SetActive(true);
        root.SetAsLastSibling();
    }

    public void ShowCards(
        IReadOnlyList<CardInstance> cards,
        string title,
        Action<CardInstance> onPick)
    {
        if (root == null)
            return;

        ClearContent();
        titleText.text = title;
        subtitleText.text = "Pick a card to upgrade.";

        GameObject listGo = new GameObject(
            "List",
            typeof(RectTransform),
            typeof(GridLayoutGroup));

        RectTransform listRect = listGo.GetComponent<RectTransform>();
        listRect.SetParent(content, false);
        Stretch(listRect);

        GridLayoutGroup list = listGo.GetComponent<GridLayoutGroup>();
        list.cellSize = new Vector2(560f, 44f);
        list.spacing = new Vector2(0f, 6f);
        list.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        list.constraintCount = 1;
        list.childAlignment = TextAnchor.MiddleCenter;

        foreach (CardInstance card in cards)
        {
            CardInstance picked = card;

            string label = card.Data.displayName +
                "   " + card.Value + " -> " +
                (card.Value + card.Data.upgradeIncrease);

            Button button = CreateButton(
                listRect, "Card", label, 20,
                new Color32(235, 224, 196, 255),
                Vector2.zero);

            button.onClick.AddListener(() =>
            {
                onPick(picked);
                Hide();
            });
        }

        root.gameObject.SetActive(true);
        root.SetAsLastSibling();
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        float size,
        Color color,
        TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        go.transform.SetParent(parent, false);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = font;
        if (fontMaterial != null)
            text.fontSharedMaterial = fontMaterial;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        return text;
    }

    private Button CreateButton(
        Transform parent,
        string name,
        string label,
        float fontSize,
        Color color,
        Vector2 size)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        if (size != Vector2.zero)
            rect.sizeDelta = size;

        Image image = go.GetComponent<Image>();
        image.color = color;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;

        AudioManager.AddClick(button);
        ButtonFrame.Apply(button);

        TMP_Text text = CreateText(
            rect, "Label", label, fontSize,
            new Color32(32, 39, 43, 255),
            TextAlignmentOptions.Center);

        Stretch(text.rectTransform);

        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 position,
        Vector2 size,
        Vector2 anchor)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
