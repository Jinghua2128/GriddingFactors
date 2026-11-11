using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    private CardInstance card;
    private Action<CardInstance> onPlayed;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(HandleClick);
    }

    public void Bind(
        CardInstance instance,
        Action<CardInstance> callback)
    {
        card = instance;
        onPlayed = callback;

        titleText.text = card.Data.displayName;

        switch (card.Data.type)
        {
            case CardType.Attack:
                descriptionText.text =
                    $"Deal {card.Value} damage";
                break;

            case CardType.Defend:
                descriptionText.text =
                    $"Gain {card.Value} Shield";
                break;

            case CardType.Focus:
                descriptionText.text =
                    $"+{card.Value} next Attack";
                break;

            default:
                descriptionText.text = "Unknown effect";
                break;
        }
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
