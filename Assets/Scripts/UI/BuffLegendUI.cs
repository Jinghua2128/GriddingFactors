using UnityEngine;
using UnityEngine.UI;

public class BuffLegendUI : MonoBehaviour
{
    private static readonly string[] Names =
    {
        "attack", "defend", "heal", "piercing", "wildcard"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        GameObject go = new GameObject("BuffLegendUI");
        go.AddComponent<BuffLegendUI>();
    }

    private void Start()
    {
        GameObject bingo = GameObject.Find("BingoPannel");

        if (bingo == null)
            return;

        GameObject root = new GameObject(
            "BuffLegend",
            typeof(RectTransform));

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(bingo.transform, false);
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0f, 0.5f);
        rootRect.anchoredPosition = new Vector2(-320f, -440f);
        rootRect.sizeDelta = new Vector2(250f, 48f);

        for (int i = 0; i < Names.Length; i++)
        {
            GameObject iconGo = new GameObject(
                Names[i],
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            RectTransform icon =
                iconGo.GetComponent<RectTransform>();

            icon.SetParent(rootRect, false);
            icon.anchorMin = new Vector2(0f, 0.5f);
            icon.anchorMax = new Vector2(0f, 0.5f);
            icon.pivot = new Vector2(0f, 0.5f);
            icon.anchoredPosition = new Vector2(i * 48f, 0f);
            icon.sizeDelta = new Vector2(40f, 40f);

            Image image = iconGo.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(
                "UI/" + Names[i] + "_buff");
            image.preserveAspect = true;
            image.raycastTarget = false;

            Outline outline = iconGo.AddComponent<Outline>();
            outline.effectColor = new Color32(32, 39, 43, 255);
            outline.effectDistance = new Vector2(0.8f, -0.8f);
        }
    }
}
