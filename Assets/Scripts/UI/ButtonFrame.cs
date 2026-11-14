using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonFrame
{
    private static Sprite boxSprite;

    public static void Apply(Button button)
    {
        if (button == null)
            return;

        if (button.GetComponentInParent<CardView>() != null)
            return;

        if (button.GetComponentInParent<BingoCellUI>() != null)
            return;

        if (button.name.StartsWith("Cell") ||
            button.name == "Item")
            return;

        RectTransform rect = button.GetComponent<RectTransform>();

        if (rect == null ||
            rect.rect.height < 48f ||
            rect.rect.width < 80f)
            return;

        Image image = button.GetComponent<Image>();

        if (image == null)
            return;

        if (boxSprite == null)
            boxSprite = Resources.Load<Sprite>("UI/button_box");

        if (boxSprite == null)
            return;

        Transform old = button.transform.Find("ButtonFrame");

        if (old != null)
            Destroy(old.gameObject);

        image.sprite = boxSprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();

        if (label != null)
            label.color = new Color32(32, 39, 43, 255);
    }

    private static void Destroy(Object target)
    {
        Object.Destroy(target);
    }
}
