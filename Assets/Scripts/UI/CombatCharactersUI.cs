using UnityEngine;
using UnityEngine.UI;

public class CombatCharactersUI : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        GameObject go = new GameObject("CombatCharactersUI");
        go.AddComponent<CombatCharactersUI>();
    }

    private void Start()
    {
        Transform panel = FindInactive("CombatPanel");

        if (panel == null)
            return;

        CreateCharacter(
            panel,
            "PlayerCharacter",
            Resources.Load<Sprite>("UI/player_character"),
            new Vector2(-360f, 80f));

        CreateCharacter(
            panel,
            "EnemyCharacter",
            Resources.Load<Sprite>("UI/enemy_character"),
            new Vector2(360f, 80f));
    }

    private static void CreateCharacter(
        Transform parent,
        string name,
        Sprite sprite,
        Vector2 position)
    {
        if (sprite == null)
            return;

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
        rect.sizeDelta = new Vector2(320f, 320f);

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        go.transform.SetAsFirstSibling();
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
