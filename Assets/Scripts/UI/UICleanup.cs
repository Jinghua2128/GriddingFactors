using System.Collections;
using TMPro;
using UnityEngine;

public class UICleanup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        GameObject go = new GameObject("UICleanup");
        go.AddComponent<UICleanup>();
    }

    private IEnumerator Start()
    {
        yield return null;

        foreach (TMP_Text text in FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            if (text.text == "New Text")
                text.gameObject.SetActive(false);
        }

        Hide("ShopSummaryText");
        Hide("ShopCoinsText");
    }

    private static void Hide(string name)
    {
        GameObject go = GameObject.Find(name);

        if (go != null)
            go.SetActive(false);
    }
}
