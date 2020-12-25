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
    public bool allowHold = true;
    public int maxReplaceCount;
    public int playerNumber;

    public float cardStartingHeight;
    public float cardHighlightHeight;
    public float cardStartingSize;
    public float cardHighlightSize;
    public float cardAimSize;
    public float cardHoldSize;
    public float cardHighlightXBoarder;
    public float cardSpacing;
    public float cardCastVertThreshold;

    //public GameObject cardTemplate;

    private int currentReplaceCount = 0;
    private CardController currentlyHeldCard;

    private List<CardController> hand;
    private List<CardController> drawnCards;
    private List<CardController> drawQueue;
    // Start is called before the first frame update
    void Awake()
    {
        if (HandController.handController == null)
            HandController.handController = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        hand = new List<CardController>();
        drawnCards = new List<CardController>();
        drawQueue = new List<CardController>();

        if (InformationLogger.infoLogger.debug)
        {
            maxReplaceCount = 20;
            allowHold = true;
        }
        else
        {
            maxReplaceCount = UnlocksController.unlock.GetUnlocks().replaceUnlocked;
            allowHold = UnlocksController.unlock.GetUnlocks().holdUnlocked;
        }
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
    private CardController InstantiateAnyCard(bool fromDrawPile)
    {
        GameObject card = Instantiate(cardTemplate);
        card.GetComponent<RectTransform>().rotation = CameraController.camera.transform.rotation;
        card.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        CardController cardController = card.GetComponent<CardController>();
        CardController drawnCard = DeckController.deckController.DrawAnyCard(fromDrawPile);
        cardController.SetCardController(drawnCard);

        if (fromDrawPile)
            cardController.transform.position = new Vector3(-5, -3, 0);
        else
            cardController.transform.position = new Vector3(5, -3, 0);
        cardController.cardDisplay.SetHighLight(true);
        cardController.GetComponent<CardDragController>().SetActive(false);

        return cardController;
    }

    private CardController InstantiateSpecificCard(CardController thisCard, bool fromDrawPile, bool fromNothing)
    {
        GameObject card = Instantiate(cardTemplate);
        card.GetComponent<RectTransform>().rotation = CameraController.camera.transform.rotation;
        card.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        CardController cardController = card.GetComponent<CardController>();
        cardController.SetCardController(thisCard);

        if (fromNothing)
        {
            cardController.transform.position = new Vector3(10, 0);
            cardController.cardDisplay.cardWhiteOut.enabled = true;
        }
        else
        {
            if (fromDrawPile)
                cardController.transform.position = new Vector3(-5, -3, 0);
            else
                cardController.transform.position = new Vector3(5, -3, 0);
        }
        cardController.cardDisplay.SetHighLight(true);
        cardController.GetComponent<CardDragController>().SetActive(false);

        return cardController;
    }

    public IEnumerator ResolveDrawQueue()
    {
        while (drawQueue.Count > 0)
        {
            drawnCards.Add(drawQueue[0]);
            drawQueue.RemoveAt(0);
            yield return StartCoroutine(AnimateDrawCard());
        }

        yield return new WaitForSeconds(0.3f * TimeController.time.timerMultiplier);

        hand.AddRange(drawnCards);
        drawnCards = new List<CardController>();
        yield return StartCoroutine(ResetCardPositions());
        foreach (CardController card in hand)
            card.GetComponent<CardDragController>().SetActive(true);
        ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    private IEnumerator AnimateDrawCard()
    {
        float elapsedTime = 0;
        float spaceBetweenCards = cardSpacing * 1.2f;
        if (drawnCards.Count > 6)
            spaceBetweenCards = cardSpacing * 6 / drawnCards.Count * 1.2f;

        List<Vector3> originalPositions = new List<Vector3>();
        List<Vector3> desiredPosition = new List<Vector3>();

        foreach (CardController card in drawnCards)
            originalPositions.Add(card.transform.position);

        for (int i = 0; i < drawnCards.Count; i++)
            if (drawnCards.Count % 2 == 1)
                desiredPosition.Add(-new Vector3((i - drawnCards.Count / 2) * spaceBetweenCards, 0, 0));
            //Even number of cards
            else
                desiredPosition.Add(-new Vector3((i - drawnCards.Count / 2 + 0.5f) * spaceBetweenCards, 0, 0));

        while (elapsedTime < 0.5f * TimeController.time.timerMultiplier)
        {
            for (int i = 0; i < drawnCards.Count; i++)
                if (drawnCards[i].cardDisplay.cardWhiteOut.enabled == false)
                drawnCards[i].transform.position = Vector3.Lerp(originalPositions[i], desiredPosition[i], elapsedTime / 0.1f * TimeController.time.timerMultiplier);
            else
                {
                    drawnCards[i].cardDisplay.cardWhiteOut.color = Color.Lerp(Color.white, Color.clear, elapsedTime / 0.1f * TimeController.time.timerMultiplier);
                    drawnCards[i].transform.position = desiredPosition[i];
                }    
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < drawnCards.Count; i++)
        {
            drawnCards[i].transform.position = desiredPosition[i];
            drawnCards[i].cardDisplay.cardWhiteOut.enabled = false;
        }

        yield return new WaitForSeconds(0.1f * TimeController.time.timerMultiplier);
    }

    //Adds a new card to the hand and reorders card positions from ANY color
    public void DrawAnyCard(bool fromDrawPile = true)
    {
        if (hand.Count + drawnCards.Count + drawQueue.Count < maxHandSize)
        {
            drawQueue.Add(InstantiateAnyCard(fromDrawPile));
        }
    }

    //Adds a new Mana card to the hand and reorders card positions from ANY color
    //Returns true if successful, false if not
    public bool DrawManaCard(bool fromDrawPile = true)
    {
        if (hand.Count + drawnCards.Count + drawQueue.Count < maxHandSize)
        {
            CardController card = DeckController.deckController.DrawManaCard(fromDrawPile);
            if ((object)card != null)
            {
                drawQueue.Add(InstantiateSpecificCard(card, fromDrawPile, false));
                return true;
            }
        }
        return false;
    }

    //Adds a new card to the hand and reorders card positions from ANY color
    //Returns true if successful, false if not
    public bool DrawEnergyCard(bool fromDrawPile = true)
    {
        if (hand.Count + drawnCards.Count + drawQueue.Count < maxHandSize)
        {
            CardController card = DeckController.deckController.DrawEnergyCard(fromDrawPile);
            if ((object)card != null)
            {
                drawQueue.Add(InstantiateSpecificCard(card, fromDrawPile, false));
                return true;
            }
        }
        return false;
    }

    public bool DrawSpecificCard(CardController card, bool fromDrawPile = true)
    {
        if (hand.Count + drawnCards.Count + drawQueue.Count < maxHandSize)
        {
            CardController c = DeckController.deckController.DrawSpecificCard(card.GetCard(), fromDrawPile);
            if ((object)c != null)
            {
                drawQueue.Add(InstantiateSpecificCard(card, fromDrawPile, false));
                return true;
            }
        }
        return false;
    }

    public void CreateSpecificCard(CardController card)
    {
        if (hand.Count + drawnCards.Count + drawQueue.Count < maxHandSize)
        {
            drawQueue.Add(InstantiateSpecificCard(card, true, true));
        }
    }

    //Redraw all card positions so they're centered
    public IEnumerator ResetCardPositions()
    {
        float spaceBetweenCards = cardSpacing;
        if (hand.Count > 6)
            spaceBetweenCards = cardSpacing * 6 / hand.Count;

        List<Vector3> originalPositions = new List<Vector3>();
        List<Vector3> originalSize = new List<Vector3>();
        List<Vector3> desiredPosition = new List<Vector3>();
        Vector3 desiredSize = new Vector3(cardStartingSize, cardStartingSize, 1);

        foreach (CardController card in hand)
        {
            originalPositions.Add(card.transform.position);
            originalSize.Add(card.transform.localScale);
            card.GetComponent<CardDragController>().SetActive(false);
        }

        for (int i = 0; i < hand.Count; i++)
        {
            //Odd number of cards
            if (hand.Count % 2 == 1)
                desiredPosition.Add(new Vector3(-(i - hand.Count / 2) * spaceBetweenCards, cardStartingHeight, 0));
            //Even number of cards
            else
                desiredPosition.Add(new Vector3(-(i - hand.Count / 2 + 0.5f) * spaceBetweenCards, cardStartingHeight, 0));
            hand[hand.Count - 1 - i].transform.SetAsLastSibling();
        }

        float elapsedTime = 0;
        while (elapsedTime < 0.1f * TimeController.time.timerMultiplier)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                hand[i].transform.position = Vector3.Lerp(originalPositions[i], desiredPosition[i], elapsedTime / 0.1f * TimeController.time.timerMultiplier);
                hand[i].transform.localScale = Vector3.Lerp(originalSize[i], desiredSize, elapsedTime / 0.1f * TimeController.time.timerMultiplier);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].transform.position = desiredPosition[i];
            hand[i].transform.localScale = desiredSize;
            hand[i].GetComponent<CardDragController>().SetActive(true);
            hand[i].GetComponent<CardDragController>().SetOriginalLocation(desiredPosition[i]);
        }
    }

    public void HoldCard(CardController heldCard)
    {
        if (currentlyHeldCard == null)
        {
            currentlyHeldCard = heldCard;
            hand.Remove(heldCard);
            StartCoroutine(ResetCardPositions());
            heldCard.GetComponent<CardDragController>().SetHeld(true);

            GameObject.FindGameObjectWithTag("Hold").GetComponent<Collider>().enabled = false;
        }
    }

    public void UnholdCard(bool returnToHand)
    {
        //May be technical debt to destroy old card instead of moving it back
        if (currentlyHeldCard != null)
        {
            if (returnToHand)
                CreateSpecificCard(currentlyHeldCard);
            Destroy(currentlyHeldCard.gameObject);
            currentlyHeldCard = null;

            GameObject.FindGameObjectWithTag("Hold").GetComponent<Collider>().enabled = true;
        }
    }

    public void ReplaceCard(CardController replacedCard)
    {
        if (currentReplaceCount < maxReplaceCount)
        {
            hand.Remove(replacedCard);
            //StartCoroutine(ResetCardPositions());
            DeckController.deckController.ReportUsedCard(replacedCard);
            DrawAnyCard();
            Destroy(replacedCard.gameObject);

            StartCoroutine(ResolveDrawQueue());

            currentReplaceCount += 1;
            ResetReplaceText();

            if (currentReplaceCount == maxReplaceCount)
                GameObject.FindGameObjectWithTag("Replace").GetComponent<Collider>().enabled = false;

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
                                replacedCard.GetCard().manaCost.ToString(),
                                "0");
        }
    }

    //Removes the card from the hand and disable movement for the caster of the card
    public void RemoveCard(CardController removedCard)
    {
        hand.Remove(removedCard);
        StartCoroutine(ResetCardPositions());
        ResetCardDisplays();
        ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());

        //Disables movement of all players with the removed card casterColor
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
            if (player.GetComponent<PlayerController>().GetColorTag() == removedCard.GetCard().casterColor)
                player.GetComponent<PlayerMoveController>().CommitMove();
    }

    //Draw cards untill hand is full
    public IEnumerator DrawFullHand()
    {
        for (int i = hand.Count; i < startingHandSize + TurnController.turnController.GetCardDrawChange(); i++)
            DrawAnyCard();

        foreach (CardController card in hand)
            card.GetComponent<Collider2D>().enabled = true;

        ResetReplaceText();

        yield return StartCoroutine(ResolveDrawQueue());
        //ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    public void ClearHand()
    {
        List<CardController> exhaustCards = new List<CardController>();
        foreach (CardController card in hand)
        {
            if (card.GetCard().exhaust) //Only put non exhaust cards into the draw pile, otherwise it's just destroyed
            {
                exhaustCards.Add(card);
                StartCoroutine(ClearExhaustCard(card));
            }
            else
                InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                    InformationLogger.infoLogger.gameID,
                                    RoomController.roomController.selectedLevel.ToString(),
                                    RoomController.roomController.roomName,
                                    TurnController.turnController.turnID.ToString(),
                                    TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                    card.GetCard().casterColor.ToString(),
                                    card.GetCard().name,
                                    "True",
                                    "False",
                                    "False",
                                    "False",
                                    "None",
                                    "None",
                                    "None",
                                    "None",
                                    "None",
                                    card.GetCard().energyCost.ToString(),
                                    card.GetCard().manaCost.ToString(),
                                    "0");
        }
        foreach (CardController card in exhaustCards)
            hand.Remove(card);

        foreach (CardController card in hand)
            card.GetComponent<Collider2D>().enabled = false;

        StartCoroutine(ResetCardPositions());
    }

    private IEnumerator ClearExhaustCard(CardController card)
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.5f * TimeController.time.timerMultiplier)
        {
            card.cardDisplay.cardWhiteOut.enabled = true;
            card.cardDisplay.cardWhiteOut.color = Color.Lerp(Color.clear, Color.white, elapsedTime / 0.5f);
            card.transform.position = Vector3.Lerp(new Vector3(card.transform.position.x, cardStartingHeight, 0), new Vector3(card.transform.position.x, cardStartingHeight + 0.5f, 0), elapsedTime / 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        card.cardDisplay.Hide();
        elapsedTime = 0;
        card.cardDisplay.anim.SetTrigger("CardDisappear");
        while (elapsedTime < 0.5f * TimeController.time.timerMultiplier)
        {
            card.transform.position = Vector3.Lerp(new Vector3(card.transform.position.x, cardStartingHeight+0.5f, 0), new Vector3(card.transform.position.x, cardStartingHeight + 1, 0), elapsedTime / 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(card.gameObject);
    }

    public void EmptyHand()
    {
        foreach (CardController card in hand)
            Destroy(card.gameObject);
        hand = new List<CardController>();
    }

    //Called from TurnController
    public void ResetCardPlayability(int energy, int mana)
    {
        foreach (CardController card in hand)
            card.ResetPlayability(energy, mana);

        if (currentlyHeldCard != null)
            currentlyHeldCard.ResetPlayability(energy, mana);
    }

    public void ResetReplaceCounter()
    {
        currentReplaceCount = 0;
        ResetReplaceText();

        GameObject.FindGameObjectWithTag("Replace").GetComponent<Collider>().enabled = true;
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
            c.cardDisplay.SetCard(c);
        if (currentlyHeldCard != null)
            currentlyHeldCard.GetComponent<CardDisplay>().SetCard(currentlyHeldCard);
    }
}
