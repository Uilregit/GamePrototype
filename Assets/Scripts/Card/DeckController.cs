using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Wrapper used to contain decks of each color type
[System.Serializable]
public class ListWrapper
{
    public List<CardController> deck;

    public void SetDeck(List<CardController> l)
    {
        deck = new List<CardController>();
        deck = l.ConvertAll(x => x);
    }
}

public class DeckController : MonoBehaviour
{
    public static DeckController deckController;

    private ListWrapper[] deck;
    private List<CardController> drawPile;
    private List<CardController> discardPile;

    //Creates currentDeck and makes it a copy of the default deck
    private void Awake()
    {
        if (DeckController.deckController == null)
            DeckController.deckController = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

    }

    public void PopulateDecks()
    {
        drawPile = new List<CardController>();
        foreach (ListWrapper cards in deck)
            foreach (CardController c in cards.deck)
                drawPile.Add(c);
        discardPile = new List<CardController>();
    }

    //Draws any card
    public CardController DrawAnyCard()
    {
        //If the draw deck is empty, fill it back up
        if (drawPile.Count == 0)
        {
            ResetDecks();
            ShuffleDrawPile();
        }
        CardController drawnCard = drawPile[0];
        drawPile.RemoveAt(0);

        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
        return drawnCard;
    }

    //Repopulate deck if empty, draw a mana card if there is one in the draw pile
    public CardController DrawManaCard()
    {
        //If the draw deck is empty, fill it back up
        if (drawPile.Count == 0)
        {
            ResetDecks();
            ShuffleDrawPile();
        }
        CardController drawnCard = null;
        int index = -1;
        for (int i = 0; i < drawPile.Count; i++)
        {
            if (drawPile[i].GetCard().manaCost > 0)
            {
                drawnCard = drawPile[i];
                index = i;
                break;
            }
        }
        if (index != -1)
            drawPile.RemoveAt(index);

        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);

        return drawnCard;
    }

    //Repopulate deck if empty, draw a energy card if there is one in the draw pile
    public CardController DrawEnergyCard()
    {
        //If the draw deck is empty, fill it back up
        if (drawPile.Count == 0)
        {
            ResetDecks();
            ShuffleDrawPile();
        }
        CardController drawnCard = null;
        int index = -1;
        for (int i = 0; i < drawPile.Count; i++)
            if (drawPile[i].GetCard().manaCost == 0)
            {
                drawnCard = drawPile[i];
                index = i;
                break;
            }
        if (index != -1)
            drawPile.RemoveAt(index);

        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
        return drawnCard;
    }

    //Shuffles the draw pile
    public void ShuffleDrawPile()
    {
        drawPile = drawPile.OrderBy(x => System.Guid.NewGuid()).ToList();
    }

    //Makes a copy of the entire default deck, all colors
    public void ResetDecks()
    {
        drawPile = new List<CardController>();
        drawPile = discardPile;
        discardPile = new List<CardController>();
        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
    }

    public void ReportUsedCard(CardController card)
    {
        if (!card.GetCard().exhaust)
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

    public void SetDecks(ListWrapper[] newList)
    {
        deck = newList;

        PopulateDecks();
        ShuffleDrawPile();
    }

    public List<CardController> GetDeck()
    {
        List<CardController> output = new List<CardController>();
        foreach (ListWrapper cards in deck)
            output.AddRange(cards.deck);
        return output;
    }

    public void ResetCardValues()
    {
        foreach (ListWrapper list in deck)
        {
            foreach (CardController card in list.deck)
            {
                card.GetCard().SetTempDuration(0);
                card.GetCard().SetTempEffectValue(0);
            }
        }

        foreach (CardController card in drawPile)
        {
            card.ResetEnergyCostDiscount();
            card.ResetManaCostDiscount();
        }
        foreach (CardController card in discardPile)
        {
            card.ResetEnergyCostDiscount();
            card.ResetManaCostDiscount();
        }
    }
}
