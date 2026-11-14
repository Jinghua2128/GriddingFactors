using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatHudUI : MonoBehaviour
{
    private class BuffSlot
    {
        public GameObject Root;
        public TMP_Text Count;
    }

    private CombatManager combat;
    private BingoAttackBuffs buffs;

    private TMP_Text playerHpText;
    private TMP_Text enemyHpText;
    private Image playerHpFill;
    private Image enemyHpFill;

    private GameObject playerBuffBox;
    private GameObject enemyBuffBox;

    private BuffSlot attackSlot;
    private BuffSlot defenseSlot;
    private BuffSlot healSlot;
    private BuffSlot focusSlot;
    private BuffSlot enemyShieldSlot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        GameObject go = new GameObject("CombatHudUI");
        go.AddComponent<CombatHudUI>();
    }

    private void Start()
    {
        Transform panel = FindInactive("CombatPanel");

        if (panel == null)
            return;

        TMP_Text source = FindInactive("PlayerStatusText")?
            .GetComponent<TMP_Text>();

        if (source == null)
            source = FindInactive("EnemyStatusText")?
                .GetComponent<TMP_Text>();

        TMP_FontAsset font = source != null
            ? source.font
            : Resources.Load<TMP_FontAsset>(
                "Fonts & Materials/LiberationSans SDF");

        Material material = source != null
            ? source.fontSharedMaterial
            : null;

        combat = FindFirstObjectByType<CombatManager>(
            FindObjectsInactive.Include);
        buffs = FindFirstObjectByType<BingoAttackBuffs>(
            FindObjectsInactive.Include);

        HideOldPanels(panel);
        MoveHand(panel);
        MoveCharacters(panel);

        BuildHealthBars(panel, font, material);
        BuildBuffBoxes(panel, font, material);
    }

    private void Update()
    {
        if (combat == null)
            return;

        int hp = combat.PlayerHP;
        int max = Mathf.Max(1, combat.PlayerMaxHP);

        if (playerHpText != null)
            playerHpText.text = $"HP  {hp} / {max}";

        SetBarFill(playerHpFill, (float)hp / max);

        int ehp = combat.EnemyHP;
        int emax = Mathf.Max(1, combat.EnemyMaxHP);

        if (enemyHpText != null)
            enemyHpText.text = $"ENEMY  {ehp} / {emax}";

        SetBarFill(enemyHpFill, (float)ehp / emax);

        int attack = buffs != null ? buffs.ActiveAttackBuff : 0;
        int defense = buffs != null ? buffs.ActiveDefenseBuff : 0;
        int heal = buffs != null ? buffs.ActiveHealBuff : 0;

        defense += combat.PlayerShield;

        int focus = combat.PlayerFocus;
        int enemyShield = combat.EnemyShield;

        SetSlot(attackSlot, attack);
        SetSlot(defenseSlot, defense);
        SetSlot(healSlot, heal);
        SetSlot(focusSlot, focus);
        SetSlot(enemyShieldSlot, enemyShield);

        if (playerBuffBox != null)
        {
            playerBuffBox.SetActive(
                attack > 0 || defense > 0 ||
                heal > 0 || focus > 0);
        }

        if (enemyBuffBox != null)
            enemyBuffBox.SetActive(enemyShield > 0);
    }

    private static void SetBarFill(Image fill, float fraction)
    {
        if (fill == null)
            return;

        RectTransform rect = fill.rectTransform;
        rect.anchorMax = new Vector2(
            Mathf.Clamp01(fraction), 1f);
    }

    private static void SetSlot(BuffSlot slot, int value)
    {
        if (slot == null || slot.Root == null)
            return;

        bool show = value > 0;
        slot.Root.SetActive(show);

        if (show && slot.Count != null)
            slot.Count.text = value.ToString();
    }

    private static void HideOldPanels(Transform panel)
    {
        Transform player = panel.Find("PlayerPanel");
        Transform enemy = panel.Find("EnemyPanel");
        Transform log = panel.Find("BattleLogText");

        if (player != null)
            player.gameObject.SetActive(false);

        if (enemy != null)
            enemy.gameObject.SetActive(false);

        if (log != null)
            log.gameObject.SetActive(false);
    }

    private static void MoveHand(Transform panel)
    {
        Transform hand = panel.Find("HandPanel");

        if (hand is RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -280f);
            rect.sizeDelta = new Vector2(1060f, 340f);
        }

        Transform cont = panel.Find("ContinueButton");

        if (cont is RectTransform contRect)
        {
            contRect.anchorMin = new Vector2(0.5f, 0.5f);
            contRect.anchorMax = new Vector2(0.5f, 0.5f);
            contRect.pivot = new Vector2(0.5f, 0.5f);
            contRect.anchoredPosition = new Vector2(700f, -455f);
            contRect.sizeDelta = new Vector2(260f, 56f);
        }
    }

    private static void MoveCharacters(Transform panel)
    {
        Transform player = panel.Find("PlayerCharacter");
        Transform enemy = panel.Find("EnemyCharacter");

        if (player is RectTransform p)
            p.anchoredPosition = new Vector2(-360f, 80f);

        if (enemy is RectTransform e)
            e.anchoredPosition = new Vector2(360f, 80f);
    }

    private void BuildHealthBars(
        Transform panel,
        TMP_FontAsset font,
        Material material)
    {
        RectTransform player = CreateRoot(
            panel, "PlayerHealth", new Vector2(-702f, -180f),
            new Vector2(324f, 92f));

        playerHpText = CreateText(
            player, "HP", "HP  30 / 30", 28,
            Vector2.zero, new Vector2(300f, 38f),
            TextAlignmentOptions.Left, font, material);

        playerHpFill = CreateBar(
            player, new Vector2(0f, -44f),
            new Vector2(300f, 18f),
            new Color32(231, 155, 136, 255));

        RectTransform enemy = CreateRoot(
            panel, "EnemyHealth", new Vector2(702f, -180f),
            new Vector2(324f, 92f));

        enemyHpText = CreateText(
            enemy, "HP", "ENEMY  12 / 12", 24,
            Vector2.zero, new Vector2(300f, 38f),
            TextAlignmentOptions.Left, font, material);

        enemyHpFill = CreateBar(
            enemy, new Vector2(0f, -44f),
            new Vector2(300f, 18f),
            new Color32(231, 155, 136, 255));
    }

    private void BuildBuffBoxes(
        Transform panel,
        TMP_FontAsset font,
        Material material)
    {
        RectTransform playerBox = CreatePanel(
            panel, "PlayerBuffs", new Vector2(-702f, -335f),
            new Vector2(324f, 100f));

        playerBuffBox = playerBox.gameObject;

        attackSlot = CreateSlot(
            playerBox, "attack", 18f,
            font, material);

        defenseSlot = CreateSlot(
            playerBox, "defend", 92f,
            font, material);

        healSlot = CreateSlot(
            playerBox, "heal", 166f,
            font, material);

        focusSlot = CreateSlot(
            playerBox, "focus", 240f,
            font, material);

        playerBuffBox.SetActive(false);

        RectTransform enemyBox = CreatePanel(
            panel, "EnemyBuffs", new Vector2(702f, -335f),
            new Vector2(324f, 100f));

        enemyBuffBox = enemyBox.gameObject;

        enemyShieldSlot = CreateSlot(
            enemyBox, "defend", 18f,
            font, material);

        enemyBuffBox.SetActive(false);
    }

    private static BuffSlot CreateSlot(
        RectTransform parent,
        string iconName,
        float x,
        TMP_FontAsset font,
        Material material)
    {
        GameObject root = new GameObject(
            iconName,
            typeof(RectTransform));

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -10f);
        rect.sizeDelta = new Vector2(58f, 72f);

        GameObject iconGo = new GameObject(
            "Icon",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform iconRect =
            iconGo.GetComponent<RectTransform>();
        iconRect.SetParent(rect, false);
        iconRect.anchorMin = new Vector2(0f, 1f);
        iconRect.anchorMax = new Vector2(0f, 1f);
        iconRect.pivot = new Vector2(0f, 1f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(52f, 52f);

        Image icon = iconGo.GetComponent<Image>();
        icon.sprite = Resources.Load<Sprite>(
            "UI/" + iconName + "_buff");
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        Outline outline = iconGo.AddComponent<Outline>();
        outline.effectColor = new Color32(32, 39, 43, 255);
        outline.effectDistance = new Vector2(0.8f, -0.8f);

        GameObject countGo = new GameObject(
            "Count",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform countRect =
            countGo.GetComponent<RectTransform>();
        countRect.SetParent(iconRect, false);
        countRect.anchorMin = new Vector2(1f, 0f);
        countRect.anchorMax = new Vector2(1f, 0f);
        countRect.pivot = new Vector2(0f, 1f);
        countRect.anchoredPosition = new Vector2(2f, -2f);
        countRect.sizeDelta = new Vector2(24f, 24f);

        Image chip = countGo.GetComponent<Image>();
        chip.color = new Color32(255, 252, 247, 255);
        chip.raycastTarget = false;

        Outline chipOutline = countGo.AddComponent<Outline>();
        chipOutline.effectColor = new Color32(32, 39, 43, 255);
        chipOutline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject countTextGo = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        RectTransform countTextRect =
            countTextGo.GetComponent<RectTransform>();
        countTextRect.SetParent(countRect, false);
        countTextRect.anchorMin = Vector2.zero;
        countTextRect.anchorMax = Vector2.one;
        countTextRect.offsetMin = Vector2.zero;
        countTextRect.offsetMax = Vector2.zero;

        TMP_Text count = countTextGo.GetComponent<TMP_Text>();
        count.font = font;
        if (material != null)
            count.fontSharedMaterial = material;
        count.fontSize = 16;
        count.fontStyle = FontStyles.Bold;
        count.color = new Color32(32, 39, 43, 255);
        count.alignment = TextAlignmentOptions.Center;
        count.raycastTarget = false;

        root.SetActive(false);

        return new BuffSlot
        {
            Root = root,
            Count = count
        };
    }

    private static RectTransform CreateRoot(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return rect;
    }

    private static RectTransform CreatePanel(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = go.GetComponent<Image>();
        image.color = new Color32(255, 252, 247, 238);
        image.raycastTarget = false;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color32(32, 39, 43, 60);
        outline.effectDistance = new Vector2(2f, -2f);

        return rect;
    }

    private static TMP_Text CreateText(
        RectTransform parent,
        string name,
        string value,
        float size,
        Vector2 position,
        Vector2 box,
        TextAlignmentOptions alignment,
        TMP_FontAsset font,
        Material material)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = box;

        TMP_Text text = go.GetComponent<TMP_Text>();
        text.font = font;
        if (material != null)
            text.fontSharedMaterial = material;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color32(32, 39, 43, 255);
        text.alignment = alignment;
        text.raycastTarget = false;

        return text;
    }

    private static Image CreateBar(
        RectTransform parent,
        Vector2 position,
        Vector2 size,
        Color fillColor)
    {
        GameObject bgGo = new GameObject(
            "Bar",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.SetParent(parent, false);
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.anchoredPosition = position;
        bgRect.sizeDelta = size;

        Image bg = bgGo.GetComponent<Image>();
        bg.color = new Color32(32, 39, 43, 30);
        bg.raycastTarget = false;

        GameObject fillGo = new GameObject(
            "Fill",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.SetParent(bgRect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fill = fillGo.GetComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Simple;
        fill.raycastTarget = false;

        return fill;
    }

    private static Transform FindInactive(string name)
    {
        foreach (Transform t in FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            if (t.name == name)
                return t;
        }

        return null;
    }
}
