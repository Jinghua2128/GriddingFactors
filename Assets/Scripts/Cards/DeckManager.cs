using System.Collections.Generic;
using UnityEngine;

public class DeckManager
{
    private readonly List<CardInstance> drawPile = new();
    private readonly List<CardInstance> discardPile = new();
    private readonly List<CardInstance> hand = new();
    private readonly List<CardInstance> allCards = new();
    public IReadOnlyList<CardInstance> AllCards => allCards;
    public int DrawCount => drawPile.Count;
    public int DiscardCount => discardPile.Count;
    public int HandCount => hand.Count;

    public int TotalCards =>
        drawPile.Count + discardPile.Count + hand.Count;

    public DeckManager(
        CardData attack,
        CardData defend,
        CardData focus,
        CardData piercing = null)
    {
        AddCopies(attack, 4);
        AddCopies(defend, 3);
        AddCopies(focus, 2);

        Shuffle(drawPile);

        if (piercing != null)
        {
            // One ultimate card, guaranteed in the opening hand.
            CardInstance ultimate = new CardInstance(piercing);

            allCards.Add(ultimate);
            drawPile.Insert(0, ultimate);
        }
    }


    private void AddCopies(CardData data, int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddCard(data);
        }
    }


    public IReadOnlyList<CardInstance> DrawHand(int amount)
    {
        if (hand.Count > 0)
        {
            Debug.LogWarning("Current hand must be played first.");
            return hand;
        }

        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
                RefillDrawPile();

            if (drawPile.Count == 0)
                break;

            CardInstance card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
        }

        return hand;
    }

    public bool PlayCard(CardInstance selectedCard)
    {
        if (!hand.Remove(selectedCard))
            return false;

        // Played card goes to discard.
        discardPile.Add(selectedCard);

        // Unplayed cards return to the draw pile.
        drawPile.AddRange(hand);
        hand.Clear();

        // Shuffle the returned cards into the deck.
        Shuffle(drawPile);

        return true;
    }

    private void RefillDrawPile()
    {
        if (discardPile.Count == 0)
            return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();

        Shuffle(drawPile);

        Debug.Log("Discard pile reshuffled.");
    }

    private void Shuffle(List<CardInstance> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }
    
    public void AddCard(CardData data)
    {
        if (data == null)
        {
            Debug.LogError("Cannot add null card data.");
            return;
        }

        CardInstance instance = new CardInstance(data);

        allCards.Add(instance);
        drawPile.Add(instance);

        Shuffle(drawPile);
    }

    public bool TryUpgradeCard(CardInstance instance)
    {
        if (instance == null || !allCards.Contains(instance))
            return false;

        instance.Upgrade();
        return true;
    }

}
