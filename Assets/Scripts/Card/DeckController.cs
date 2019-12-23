using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Wrapper used to contain decks of each color type
[System.Serializable]
public class ListWrapper
{
    public List<Card> deck;
}

public class DeckController : MonoBehaviour
{
    public static DeckController deckController;

    public ListWrapper[] deck;
    private List<Card> drawPile;
    private List<Card> discardPile;

    //Creates currentDeck and makes it a copy of the default deck
    private void Awake()
    {
        if (DeckController.deckController == null)
            DeckController.deckController = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        PopulateDecks();
        ShuffleDrawPile();
    }

    public void PopulateDecks()
    {
        drawPile = new List<Card>();
        foreach (ListWrapper cards in deck)
            drawPile.AddRange(cards.deck);
        discardPile = new List<Card>();
    }

    //Draws any card
    public Card DrawAnyCard()
    {
        Card drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        //If the draw deck is empty, fill it back up
        if (drawPile.Count == 0)
        {
            ResetDecks();
            ShuffleDrawPile();
        }
        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
        return drawnCard;
    }

    //Shuffles the draw pile
    public void ShuffleDrawPile()
    {
        List<Card> output = new List<Card>();
        while (drawPile.Count > 0)
        {
            int index = Random.Range(0, drawPile.Count);
            output.Add(drawPile[index]);
            drawPile.RemoveAt(index);
        }
        drawPile = output;
    }

    public void ShuffleDiscardPile()
    {
        List<Card> output = new List<Card>();
        while (discardPile.Count > 0)
        {
            int index = Random.Range(0, discardPile.Count);
            output.Add(discardPile[index]);
            discardPile.RemoveAt(index);
        }
        discardPile = output;
    }

    //Makes a copy of the entire default deck, all colors
    public void ResetDecks()
    {
        ShuffleDiscardPile();
        drawPile.AddRange(discardPile);
        discardPile = new List<Card>();
        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
    }

    public void ReportUsedCard(Card card)
    {
        if (!card.exhaust)
            discardPile.Add(card);
        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
    }

    public int GetDrawPileSize()
    {
        return drawPile.Count;
    }

    public int GetDiscardPileSize()
    {
        return discardPile.Count;
    }

    public void AddCard(Card newCard)
    {
        switch (newCard.casterColor)
        {
            case Card.CasterColor.Red:
                deck[0].deck.Add(newCard);
                break;
            case Card.CasterColor.Blue:
                deck[1].deck.Add(newCard);
                break;
            case Card.CasterColor.Green:
                deck[2].deck.Add(newCard);
                break;
            case Card.CasterColor.Gray:
                deck[3].deck.Add(newCard);
                break;
        }
    }
}
