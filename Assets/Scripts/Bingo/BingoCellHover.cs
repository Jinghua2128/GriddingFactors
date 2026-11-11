
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BingoCellHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Outline highlightOutline;
    [SerializeField] private float hoverScale = 1.05f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

        if (highlightOutline == null)
            highlightOutline = GetComponent<Outline>();

        if (highlightOutline != null)
            highlightOutline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;

        if (highlightOutline != null)
            highlightOutline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHover();
    }

    private void OnDisable()
    {
        ResetHover();
    }

    private void ResetHover()
    {
        transform.localScale = originalScale;

        if (highlightOutline != null)
            highlightOutline.enabled = false;
    }
}
