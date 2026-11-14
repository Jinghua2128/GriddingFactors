using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private static readonly string[] TutorialPages =
    {
        "DRAW & PLACE\n\n" +
        "Draw a Powerball, then click any highlighted " +
        "Bingo cell that matches the number.",

        "BINGO & BUFFS\n\n" +
        "Filling a cell triggers its buff. Complete a row " +
        "to earn an Ultimate charge for combat.\n\n" +
        "Shop upgrades open the board so you can pick the cell.",

        "FIGHT & SHOP\n\n" +
        "Play cards to beat the enemy, then spend coins " +
        "between encounters.\n\n" +
        "Piercing is your Ultimate card - it ignores Shield."
    };

    private GameObject menuRoot;
    private GameObject tutorialRoot;
    private TMP_Text tutorialBody;
    private TMP_Text tutorialPage;
    private TMP_Text tutorialNextLabel;

    private TMP_FontAsset font;
    private int page;
    private bool startAfterTutorial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (UnityEngine.Object.FindFirstObjectByType<MainMenuUI>() != null)
            return;

        GameObject go = new GameObject("MainMenuUI");
        go.AddComponent<MainMenuUI>();
    }

    private void Awake()
    {
        Time.timeScale = 0f;

        font = Resources.Load<TMP_FontAsset>(
            "Fonts & Materials/LiberationSans SDF");

        if (font == null)
            font = TMP_Settings.defaultFontAsset;

        GameObject canvas = GameObject.Find("Canvas");

        if (canvas == null)
            return;

        BuildMenu(canvas.transform);
        BuildTutorial(canvas.transform);
    }

    private void BuildMenu(Transform canvas)
    {
        menuRoot = new GameObject(
            "MainMenu",
            typeof(RectTransform),
            typeof(CanvasRenderer));

        RectTransform root = menuRoot.GetComponent<RectTransform>();
        root.SetParent(canvas, false);
        Stretch(root);

        GameObject bgGo = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.SetParent(root, false);
        Stretch(bgRect);

        Image bg = bgGo.GetComponent<Image>();
        bg.sprite = Resources.Load<Sprite>("UI/main_menu");
        bg.color = bg.sprite != null
            ? Color.white
            : new Color32(246, 244, 239, 255);
        bg.preserveAspect = false;
        bg.raycastTarget = true;

        TMP_Text title = CreateText(
            root, "Title", "GRIDDING FACTORS", 68,
            new Color32(32, 39, 43, 255), TextAlignmentOptions.Center);

        SetRect(title.rectTransform, new Vector2(0f, 180f),
            new Vector2(1200f, 100f), new Vector2(0.5f, 0.5f));

        TMP_Text subtitle = CreateText(
            root, "Subtitle", "Bingo. Cards. Lucky numbers.", 24,
            new Color32(104, 119, 122, 255), TextAlignmentOptions.Center);

        SetRect(subtitle.rectTransform, new Vector2(0f, 108f),
            new Vector2(800f, 40f), new Vector2(0.5f, 0.5f));

        Button play = CreateButton(
            root, "Play", "PLAY", 300f, 68f,
            new Color32(231, 155, 136, 255), 26);

        SetRect(play.GetComponent<RectTransform>(),
            new Vector2(0f, -10f), new Vector2(300f, 68f),
            new Vector2(0.5f, 0.5f));

        play.onClick.AddListener(() => ShowTutorial(true));

        Button howTo = CreateButton(
            root, "HowToPlay", "HOW TO PLAY", 300f, 58f,
            new Color32(120, 175, 167, 255), 20);

        SetRect(howTo.GetComponent<RectTransform>(),
            new Vector2(0f, -98f), new Vector2(300f, 58f),
            new Vector2(0.5f, 0.5f));

        howTo.onClick.AddListener(() => ShowTutorial(false));

        menuRoot.transform.SetAsLastSibling();
    }

    private void BuildTutorial(Transform canvas)
    {
        tutorialRoot = new GameObject(
            "Tutorial",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform root = tutorialRoot.GetComponent<RectTransform>();
        root.SetParent(canvas, false);
        Stretch(root);

        Image dim = tutorialRoot.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
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
        panel.sizeDelta = new Vector2(760f, 600f);

        Image panelImage = panelGo.GetComponent<Image>();
        panelImage.color = new Color32(251, 248, 242, 255);

        Outline outline = panelGo.AddComponent<Outline>();
        outline.effectColor = new Color32(32, 39, 43, 40);
        outline.effectDistance = new Vector2(2f, -2f);

        TMP_Text title = CreateText(
            panel, "Title", "HOW TO PLAY", 34,
            new Color32(32, 39, 43, 255), TextAlignmentOptions.Center);

        SetRect(title.rectTransform, new Vector2(0f, -30f),
            new Vector2(600f, 50f), new Vector2(0.5f, 1f));

        Button close = CreateButton(
            panel, "Close", "X", 40f, 40f,
            new Color32(231, 155, 136, 255), 18);

        SetRect(close.GetComponent<RectTransform>(),
            new Vector2(-20f, -20f), new Vector2(40f, 40f),
            new Vector2(1f, 1f));

        close.onClick.AddListener(CloseTutorial);

        tutorialBody = CreateText(
            panel, "Body", "", 21,
            new Color32(32, 39, 43, 255), TextAlignmentOptions.TopLeft);

        SetRect(tutorialBody.rectTransform, new Vector2(40f, -100f),
            new Vector2(680f, 300f), new Vector2(0f, 1f));

        tutorialPage = CreateText(
            panel, "Page", "", 16,
            new Color32(104, 119, 122, 255), TextAlignmentOptions.Center);

        SetRect(tutorialPage.rectTransform, new Vector2(0f, 40f),
            new Vector2(140f, 30f), new Vector2(0.5f, 0f));

        Button back = CreateButton(
            panel, "Back", "BACK", 160f, 54f,
            new Color32(120, 175, 167, 255), 20);

        SetRect(back.GetComponent<RectTransform>(),
            new Vector2(40f, 32f), new Vector2(160f, 54f),
            new Vector2(0f, 0f));

        back.onClick.AddListener(BackPage);

        Button next = CreateButton(
            panel, "Next", "NEXT", 200f, 54f,
            new Color32(231, 155, 136, 255), 20);

        SetRect(next.GetComponent<RectTransform>(),
            new Vector2(-40f, 32f), new Vector2(200f, 54f),
            new Vector2(1f, 0f));

        tutorialNextLabel = next.GetComponentInChildren<TMP_Text>();
        next.onClick.AddListener(NextPage);

        tutorialRoot.SetActive(false);
    }

    private void ShowTutorial(bool startAfter)
    {
        startAfterTutorial = startAfter;
        page = 0;

        menuRoot.SetActive(false);
        tutorialRoot.SetActive(true);
        tutorialRoot.transform.SetAsLastSibling();

        RefreshTutorial();
    }

    private void RefreshTutorial()
    {
        tutorialBody.text = TutorialPages[page];
        tutorialPage.text = (page + 1) + " / " + TutorialPages.Length;

        tutorialNextLabel.text = page < TutorialPages.Length - 1
            ? "NEXT"
            : (startAfterTutorial ? "START" : "CLOSE");
    }

    private void NextPage()
    {
        if (page < TutorialPages.Length - 1)
        {
            page++;
            RefreshTutorial();
            return;
        }

        if (startAfterTutorial)
        {
            StartGame();
        }
        else
        {
            CloseTutorial();
        }
    }

    private void BackPage()
    {
        if (page > 0)
        {
            page--;
            RefreshTutorial();
            return;
        }

        CloseTutorial();
    }

    private void StartGame()
    {
        tutorialRoot.SetActive(false);
        menuRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    private void CloseTutorial()
    {
        tutorialRoot.SetActive(false);
        menuRoot.SetActive(true);
        Time.timeScale = 0f;
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
        if (font != null)
            text.fontSharedMaterial = font.material;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        return text;
    }

    private Button CreateButton(
        Transform parent,
        string name,
        string label,
        float width,
        float height,
        Color color,
        float fontSize)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(width, height);

        Image image = go.GetComponent<Image>();
        image.color = color;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;

        TMP_Text text = CreateText(
            rect, "Label", label, fontSize,
            new Color32(32, 39, 43, 255),
            TextAlignmentOptions.Center);

        Stretch(text.rectTransform);

        ButtonFrame.Apply(button);

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
