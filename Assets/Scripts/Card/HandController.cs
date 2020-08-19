using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public static HandController handController;

    public GameObject cardTemplate;

    public int maxHandSize;
    public int startingHandSize;
    public int maxReplaceCount;
    public int playerNumber;

    public float cardStartingHeight;
    public float cardHighlightHeight;
    public float cardStartingSize;
    public float cardHighlightSize;
    public float cardHighlightXBoarder;
    public float cardSpacing;
    public float cardCastVertThreshold;

    //public GameObject cardTemplate;

    public Color redCasterColor;
    public Color blueCasterColor;
    public Color greenCasterColor;
    public Color enemyCasterColor;

    private int currentReplaceCount = 0;
    private CardController currentlyHeldCard;

    private List<CardController> hand;
    // Start is called before the first frame update
    void Awake()
    {
        if (HandController.handController == null)
            HandController.handController = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        hand = new List<CardController>();
    }

    /*
    //Returns a random card from the deck from ONE color
    private CardController GetCard(int index)
    {
        CardController card = Instantiate(cardTemplate).GetComponent<CardController>();
        card.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        card.SetCard(deck.DrawCard(index));

        return card;
    }

    //Adds a new card to the hand and reorders card positions from ONE color
    private void DrawCard(int index)
    {
        hand.Add(GetCard(index));
        ResetCardPositions();
    }
    */

    //Returns a random card from the deck fron ANY color
    private CardController GetAnyCard()
    {
        GameObject card = Instantiate(cardTemplate);
        card.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        CardController cardController = card.GetComponent<CardController>();
        CardController drawnCard = DeckController.deckController.DrawAnyCard();
        cardController.SetCardController(drawnCard);

        return cardController;
    }

    private CardController GetSpecificCard(CardController thisCard)
    {
        GameObject card = Instantiate(cardTemplate);
        card.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        CardController cardController = card.GetComponent<CardController>();
        cardController.SetCardController(thisCard);

        return cardController;
    }

    //Adds a new card to the hand and reorders card positions from ANY color
    public void DrawAnyCard()
    {
        if (hand.Count < maxHandSize)
        {
            hand.Add(GetAnyCard());
            ResetCardPositions();
            ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
        }
    }

    //Adds a new Mana card to the hand and reorders card positions from ANY color
    //Returns true if successful, false if not
    public bool DrawManaCard()
    {
        if (hand.Count < maxHandSize)
        {
            CardController card = DeckController.deckController.DrawManaCard();
            if ((object)card != null)
            {
                hand.Add(GetSpecificCard(card));
                ResetCardPositions();
                ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
                return true;
            }
        }
        return false;
    }

    //Adds a new card to the hand and reorders card positions from ANY color
    //Returns true if successful, false if not
    public bool DrawEnergyCard()
    {
        if (hand.Count < maxHandSize)
        {
            CardController card = DeckController.deckController.DrawEnergyCard();
            if ((object)card != null)
            {
                hand.Add(GetSpecificCard(card));
                ResetCardPositions();
                ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
                return true;
            }
        }
        return false;
    }

    public void DrawSpecificCard(CardController card)
    {
        if (hand.Count < maxHandSize)
        {
            hand.Add(GetSpecificCard(card));
            ResetCardPositions();
            ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
        }
    }

    //Redraw all card positions so they're centered
    public void ResetCardPositions()
    {
        //Odd number of cards
        if (hand.Count % 2 == 1)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                Vector2 cardLocation = new Vector2((i - hand.Count / 2) * cardSpacing, cardStartingHeight);
                hand[hand.Count - 1 - i].SetLocation(cardLocation);
                hand[hand.Count - 1 - i].transform.localScale = new Vector2(cardStartingSize, cardStartingSize);
            }
        }
        //Even number of cards
        else
        {
            for (int i = 0; i < hand.Count; i++)
            {
                Vector2 cardLocation = new Vector2((i - hand.Count / 2 + 0.5f) * cardSpacing, cardStartingHeight);
                hand[hand.Count - 1 - i].SetLocation(cardLocation);
                hand[hand.Count - 1 - i].transform.localScale = new Vector2(cardStartingSize, cardStartingSize);
            }
        }
    }

    public void HoldCard(CardController heldCard)
    {
        if (currentlyHeldCard == null)
        {
            currentlyHeldCard = heldCard;
            hand.Remove(heldCard);
            ResetCardPositions();
            heldCard.GetComponent<CardDragController>().SetHeld(true);

            GameObject.FindGameObjectWithTag("Hold").GetComponent<Collider2D>().enabled = false;
        }
    }

    public void UnholdCard(bool returnToHand)
    {
        //May be technical debt to destroy old card instead of moving it back
        if (currentlyHeldCard != null)
        {
            if (returnToHand)
                DrawSpecificCard(currentlyHeldCard);
            Destroy(currentlyHeldCard.gameObject);
            currentlyHeldCard = null;

            GameObject.FindGameObjectWithTag("Hold").GetComponent<Collider2D>().enabled = true;
        }
    }

    public void ReplaceCard(CardController replacedCard)
    {
        if (currentReplaceCount < maxReplaceCount)
        {
            hand.Remove(replacedCard);
            ResetCardPositions();
            DeckController.deckController.ReportUsedCard(replacedCard);
            DrawAnyCard();
            Destroy(replacedCard.gameObject);

            currentReplaceCount += 1;
            ResetReplaceText();

            if (currentReplaceCount == maxReplaceCount)
                GameObject.FindGameObjectWithTag("Replace").GetComponent<Collider2D>().enabled = false;

            InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.selectedLevel.ToString(),
                                RoomController.roomController.roomName,
                                TurnController.turnController.turnID.ToString(),
                                TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                replacedCard.GetCard().casterColor.ToString(),
                                replacedCard.GetCard().name,
                                "False",
                                "True",
                                "False",
                                "False",
                                "None",
                                "None",
                                "None",
                                "None",
                                "None",
                                replacedCard.GetCard().energyCost.ToString(),
                                replacedCard.GetCard().manaCost.ToString());
        }
    }

    //Removes the card from the hand and disable movement for the caster of the card
    public void RemoveCard(CardController removedCard)
    {
        hand.Remove(removedCard);
        ResetCardPositions();
        ResetCardDisplays();

        //Disables movement of all players with the removed card casterColor
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject caster = players[0];
        foreach (GameObject player in players)
            if (player.GetComponent<PlayerController>().GetColorTag() == removedCard.GetCard().casterColor)
                player.GetComponent<PlayerMoveController>().CommitMove();
    }

    //Draw cards untill hand is full
    public void DrawFullHand()
    {
        /*
        for (int i = 0; i < playerNumber; i++)
        {
            for (int j = 0; j < startingHandSizePerPlayer; j++)
            {
                if (deck.GetCurrentDeckSize(i) == 0)
                    deck.ResetDeck(i);
                DrawCard(i);
            }
        }*/
        for (int i = hand.Count; i < startingHandSize; i++)
            DrawAnyCard();
        ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());

        ResetReplaceText();
    }

    public void ClearHand()
    {
        foreach (CardController card in hand)
        {
            if (!card.GetCard().exhaust) //Only put non exhaust cards into the draw pile, otherwise it's just destroyed
                DeckController.deckController.ReportUsedCard(card);

            InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.selectedLevel.ToString(),
                                RoomController.roomController.roomName,
                                TurnController.turnController.turnID.ToString(),
                                TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                card.GetCard().casterColor.ToString(),
                                card.GetCard().name,
                                "False",
                                "False",
                                "True",
                                "False",
                                "None",
                                "None",
                                "None",
                                "None",
                                "None",
                                card.GetCard().energyCost.ToString(),
                                card.GetCard().manaCost.ToString());

            Destroy(card.gameObject);
        }
        hand = new List<CardController>();

        if (currentlyHeldCard != null)
            InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                        InformationLogger.infoLogger.gameID,
                        RoomController.roomController.selectedLevel.ToString(),
                        RoomController.roomController.roomName,
                        TurnController.turnController.turnID.ToString(),
                        TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                        currentlyHeldCard.GetCard().casterColor.ToString(),
                        currentlyHeldCard.GetCard().name,
                        "True",
                        "False",
                        "False",
                        "False",
                        "None",
                        "None",
                        "None",
                        "None",
                        "None",
                        currentlyHeldCard.GetCard().energyCost.ToString(),
                        currentlyHeldCard.GetCard().manaCost.ToString());

        ResetCardPositions();
    }

    //Called from TurnController
    public void ResetCardPlayability(int energy, int mana)
    {
        foreach (CardController card in hand)
        {
            card.ResetPlayability(energy, mana);
        }
        if (currentlyHeldCard != null)
            currentlyHeldCard.ResetPlayability(energy, mana);
    }

    public Color GetCasterColor(Card.CasterColor color)
    {
        switch (color)
        {
            case Card.CasterColor.Red:
                return redCasterColor;
            case Card.CasterColor.Blue:
                return blueCasterColor;
            case Card.CasterColor.Green:
                return greenCasterColor;
            case Card.CasterColor.Enemy:
                return enemyCasterColor;
            default:
                return enemyCasterColor;
        }
    }

    public void ResetReplaceCounter()
    {
        currentReplaceCount = 0;
        ResetReplaceText();

        GameObject.FindGameObjectWithTag("Replace").GetComponent<Collider2D>().enabled = true;
    }

    private void ResetReplaceText()
    {
        GameObject.FindGameObjectWithTag("Replace").transform.GetChild(1).GetComponent<Text>().text = "x" + (maxReplaceCount - currentReplaceCount).ToString();
    }

    public CardController GetHeldCard()
    {
        return currentlyHeldCard;
    }

    public List<CardController> GetHand()
    {
        return hand;
    }

    public void ResetCardDisplays()
    {
        foreach (CardController c in hand)
            c.SetCard(c.GetCard());
        if (currentlyHeldCard != null)
            currentlyHeldCard.SetCard(currentlyHeldCard.GetCard());
    }
}
