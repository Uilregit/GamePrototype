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

    private int seedForShuffle = -1;

    private ListWrapper[] deck;
    private List<CardController> drawPile;
    private List<CardController> discardPile;

    private int numberOfManaCardsInDraw = 0;
    private int numberOfEnergyCardsInDraw = 0;
    private int numberOfManaCardsInDiscard = 0;
    private int numberOfEnergyCardsInDiscard = 0;

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
        numberOfManaCardsInDraw = 0;
        numberOfEnergyCardsInDraw = 0;
        drawPile = new List<CardController>();
        foreach (ListWrapper cards in deck)
            foreach (CardController c in cards.deck)
            {
                drawPile.Add(c);
                if (c.GetCard().manaCost == 0)
                    numberOfEnergyCardsInDraw += 1;
                else
                    numberOfManaCardsInDraw += 1;
            }
        discardPile = new List<CardController>();
        numberOfEnergyCardsInDiscard = 0;
        numberOfManaCardsInDiscard = 0;
    }

    //Draws any card
    public CardController DrawAnyCard(bool fromDrawPile)
    {
        CardController drawnCard = null;

        if (fromDrawPile)
        {
            //If the draw deck is empty, fill it back up
            if (drawPile.Count == 0)
            {
                ResetDecks();
                ShuffleDrawPile();
            }

            drawnCard = drawPile[0];
            drawPile.RemoveAt(0);

            if (drawnCard.GetCard().manaCost == 0)
                numberOfEnergyCardsInDraw -= 1;
            else
                numberOfManaCardsInDraw -= 1;
        }
        else
        {
            if (discardPile.Count == 0)
                return null;

            drawnCard = discardPile[0];
            discardPile.RemoveAt(0);

            if (drawnCard.GetCard().manaCost == 0)
                numberOfEnergyCardsInDiscard -= 1;
            else
                numberOfManaCardsInDiscard -= 1;
        }
        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
        return drawnCard;
    }

    //Repopulate deck if empty, draw a mana card if there is one in the draw pile
    public CardController DrawManaCard(bool fromDrawPile = true)
    {
        CardController drawnCard = null;

        if (fromDrawPile)
        {
            //If the draw deck is empty, fill it back up
            if (drawPile.Count == 0)
            {
                ResetDecks();
                ShuffleDrawPile();
            }
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
            {
                drawPile.RemoveAt(index);
                numberOfManaCardsInDraw -= 1;
            }
        }
        else
        {
            int index = -1;
            for (int i = 0; i < discardPile.Count; i++)
            {
                if (discardPile[i].GetCard().manaCost > 0)
                {
                    drawnCard = discardPile[i];
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                discardPile.RemoveAt(index);
                numberOfManaCardsInDiscard -= 1;
            }
        }

        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);

        return drawnCard;
    }

    //Repopulate deck if empty, draw a energy card if there is one in the draw pile
    public CardController DrawEnergyCard(bool fromDrawPile = true)
    {
        CardController drawnCard = null;
        if (fromDrawPile)
        {
            //If the draw deck is empty, fill it back up
            if (drawPile.Count == 0)
            {
                ResetDecks();
                ShuffleDrawPile();
            }
            int index = -1;
            for (int i = 0; i < drawPile.Count; i++)
                if (drawPile[i].GetCard().manaCost == 0)
                {
                    drawnCard = drawPile[i];
                    index = i;
                    break;
                }
            if (index != -1)
            {
                drawPile.RemoveAt(index);
                numberOfEnergyCardsInDraw -= 1;
            }
        }
        else
        {
            int index = -1;
            for (int i = 0; i < discardPile.Count; i++)
                if (discardPile[i].GetCard().manaCost == 0)
                {
                    drawnCard = discardPile[i];
                    index = i;
                    break;
                }
            if (index != -1)
            {
                discardPile.RemoveAt(index);
                numberOfEnergyCardsInDiscard -= 1;
            }
        }

        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
        return drawnCard;
    }

    public CardController DrawSpecificCard(Card card, bool fromDrawPile = true)
    {
        CardController output = null;

        if (fromDrawPile)
        {
            foreach (CardController draw in drawPile)
                if (draw.GetCard().name == card.name)
                {
                    output = draw;
                    drawPile.Remove(output);
                    if (output.GetCard().manaCost > 0)
                        numberOfManaCardsInDraw -= 1;
                    else
                        numberOfEnergyCardsInDraw -= 1;
                    break;
                }
        }
        else
            foreach (CardController draw in discardPile)
                if (draw.GetCard().name == card.name)
                {
                    output = draw;
                    discardPile.Remove(output);
                    if (output.GetCard().manaCost > 0)
                        numberOfManaCardsInDiscard -= 1;
                    else
                        numberOfEnergyCardsInDiscard -= 1;
                    break;
                }

        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
        return output;
    }

    //Shuffles the draw pile
    public void ShuffleDrawPile()
    {
        try     //Singleplayer
        {
            if (seedForShuffle == -1)
                seedForShuffle = RoomController.roomController.GetCurrentSmallRoom().GetSeed();

            Random.InitState(seedForShuffle);
            seedForShuffle = Random.Range(1, 1000000000);

            for (int i = 0; i < drawPile.Count; i++)
            {
                int index = Random.Range(0, drawPile.Count - 1);
                CardController c = drawPile[i];
                drawPile[i] = drawPile[index];
                drawPile[index] = c;
            }
        }
        catch //Multiplayer
        {
            //Debug.Log("Multiplayer shuffle");
            for (int i = 0; i < drawPile.Count; i++)
            {
                int index = Random.Range(0, drawPile.Count - 1);
                CardController c = drawPile[i];
                drawPile[i] = drawPile[index];
                drawPile[index] = c;
            }
        }
    }

    //Makes a copy of the entire default deck, all colors
    public void ResetDecks()
    {
        drawPile = new List<CardController>();
        drawPile = discardPile;
        discardPile = new List<CardController>();
        numberOfEnergyCardsInDraw = numberOfEnergyCardsInDiscard;
        numberOfManaCardsInDraw = numberOfManaCardsInDiscard;
        numberOfEnergyCardsInDiscard = 0;
        numberOfManaCardsInDiscard = 0;
        UIController.ui.ResetPileCounts(drawPile.Count, discardPile.Count);
    }

    public void ReportUsedCard(CardController card)
    {
        if (!card.GetCard().exhaust)
        {
            if (card.GetCard().shuffleToDiscardPile)
            {
                discardPile.Add(card);
                if (card.GetCard().manaCost == 0)
                    numberOfEnergyCardsInDiscard += 1;
                else
                    numberOfManaCardsInDiscard += 1;
            }
            else
            {
                drawPile.Add(card);
                ShuffleDrawPile();
                if (card.GetCard().manaCost == 0)
                    numberOfEnergyCardsInDraw += 1;
                else
                    numberOfManaCardsInDraw += 1;
            }
        }
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
                //Debug.Log(card);
                //Debug.Log(card.GetCard());
                //Debug.Log(card.GetCard().name);
                card.GetCard().SetTempDuration(0);
                card.GetCard().SetTempEffectValue(0);
            }
        }

        foreach (CardController card in drawPile)
        {
            card.ResetEnergyCostDiscount();
            card.ResetManaCostDiscount();
            card.ResetEnergyCostCap();
            card.ResetManaCostCap();
        }
        foreach (CardController card in discardPile)
        {
            card.ResetEnergyCostDiscount();
            card.ResetManaCostDiscount();
            card.ResetEnergyCostCap();
            card.ResetManaCostCap();
        }
    }

    public int GetNumberOfEnergyCardsInDraw()
    {
        return numberOfEnergyCardsInDraw;
    }
    public int GetNumberOfManaCardsInDraw()
    {
        return numberOfManaCardsInDraw;
    }
    public int GetNumberOfEnergyCardsInDiscard()
    {
        return numberOfEnergyCardsInDiscard;
    }
    public int GetNumberOfManaCardsInDiscard()
    {
        return numberOfManaCardsInDiscard;
    }

    public List<CardController> GetDrawPile()
    {
        return drawPile;
    }
    public List<CardController> GetDiscardPile()
    {
        return discardPile;
    }
}
