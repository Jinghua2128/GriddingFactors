using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FocusHudUI : MonoBehaviour
{
    private TMP_Text focusSource;
    private TMP_Text badgeText;
    private GameObject badge;
    private CombatManager combat;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        GameObject go = new GameObject("FocusHudUI");
        go.AddComponent<FocusHudUI>();
    }

    private void Start()
    {
        GameObject bingo = GameObject.Find("BingoPannel");

        if (bingo == null)
            return;

        focusSource = bingo.transform
            .Find("BingoFocusText")?
            .GetComponent<TMP_Text>();

        if (focusSource == null)
            return;

        combat = FindFirstObjectByType<CombatManager>(
            FindObjectsInactive.Include);

        focusSource.gameObject.SetActive(false);

        Sprite sprite = Resources.Load<Sprite>("UI/focus_buff");

        GameObject iconGo = new GameObject(
            "FocusIcon",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform iconRect =
            iconGo.GetComponent<RectTransform>();

        iconRect.SetParent(bingo.transform, false);
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(600f, 290f);
        iconRect.sizeDelta = new Vector2(48f, 48f);

        Image icon = iconGo.GetComponent<Image>();
        icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        Outline iconOutline = iconGo.AddComponent<Outline>();
        iconOutline.effectColor = new Color32(32, 39, 43, 255);
        iconOutline.effectDistance = new Vector2(1f, -1f);

        GameObject badgeGo = new GameObject(
            "FocusBadge",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform badgeRect =
            badgeGo.GetComponent<RectTransform>();

        badgeRect.SetParent(iconRect, false);
        badgeRect.anchorMin = new Vector2(1f, 0f);
        badgeRect.anchorMax = new Vector2(1f, 0f);
        badgeRect.pivot = new Vector2(0f, 1f);
        badgeRect.anchoredPosition = new Vector2(2f, -2f);
        badgeRect.sizeDelta = new Vector2(22f, 22f);

        Image badgeImage = badgeGo.GetComponent<Image>();
        badgeImage.color = new Color32(255, 252, 247, 255);

        Outline outline = badgeGo.AddComponent<Outline>();
        outline.effectColor = new Color32(32, 39, 43, 255);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textGo = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        RectTransform textRect =
            textGo.GetComponent<RectTransform>();

        textRect.SetParent(badgeRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        badgeText = textGo.GetComponent<TMP_Text>();
        badgeText.font = focusSource.font;
        badgeText.fontSharedMaterial = focusSource.fontSharedMaterial;
        badgeText.text = "";
        badgeText.fontSize = 14;
        badgeText.fontStyle = FontStyles.Bold;
        badgeText.color = new Color32(32, 39, 43, 255);
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.raycastTarget = false;

        badge = badgeGo;
        badge.SetActive(false);
    }

    private void Update()
    {
        if (badge == null || combat == null)
            return;

        int focus = Mathf.Clamp(combat.PlayerFocus, 0, 6);
        bool show = focus >= 2;

        badge.SetActive(show);

        if (show)
            badgeText.text = focus.ToString();
    }
}
