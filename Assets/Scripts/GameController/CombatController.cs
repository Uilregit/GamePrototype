using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public static CombatController combatController;

    public GameObject displayCard;

    public float cardStartingHeight;
    public float cardSpacing;
    public float cardSize;
    public float playCardSize;
    public float playDuration;
    public float betweenCardDuration;
    public float endTurnDamageCheckDuration;

    private List<CardController> queueCards;

    // Start is called before the first frame update
    void Start()
    {
        if (CombatController.combatController == null)
            CombatController.combatController = this;
        else
            Destroy(this.gameObject);

        queueCards = new List<CardController>();
    }

    public void Refresh()
    {
        //Odd number of cards
        if (queueCards.Count % 2 == 1)
        {
            for (int i = 0; i < queueCards.Count; i++)
            {
                Vector2 cardLocation = new Vector2((i - queueCards.Count / 2) * cardSpacing, cardStartingHeight);
                queueCards[i].transform.position = cardLocation;
                queueCards[i].transform.localScale = new Vector2(cardSize, cardSize);
            }
        }
        //Even number of cards
        else
        {
            for (int i = 0; i < queueCards.Count; i++)
            {
                Vector2 cardLocation = new Vector2((i - queueCards.Count / 2 + 0.5f) * cardSpacing, cardStartingHeight);
                queueCards[i].transform.position = cardLocation;
                queueCards[i].transform.localScale = new Vector2(cardSize, cardSize);
            }
        }
    }

    public void AddCard(CardController card)
    {
        CardController c = Instantiate(displayCard).GetComponent<CardController>();
        c.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        c.SetCard(card.GetCard());
        c.SetTarget(card.GetTarget());
        queueCards.Add(c);
        Refresh();
        //c.GetComponent<CardDragController>().SetActive(false);
    }

    public void RemoveCard(CardController cardController)
    {
        if (cardController.GetCard().returnOnCancel == true) //If allowed, draw the canceled card back
            HandController.handController.DrawSpecificCard(cardController);
        else if (cardController.GetCard().exhaust == false) //If not allowed and exhaust (special case for non generated boomerang) discard the card
            DeckController.deckController.ReportUsedCard(cardController);

        queueCards.Remove(cardController);
        Destroy(cardController.gameObject);
    }

    public CardController RemoveLastCard()
    {
        CardController output = queueCards[queueCards.Count - 1];
        queueCards.Remove(output);
        TurnController.turnController.UseResources(-output.GetCard().energyCost, -output.GetCard().manaCost);
        Destroy(output.gameObject);
        Refresh();
        return output;
    }

    public List<CardController> RemoveAllCards()
    {
        List<CardController> output = queueCards;
        queueCards = new List<CardController>();
        foreach (CardController card in output)
        {
            TurnController.turnController.UseResources(-card.GetCard().energyCost, -card.GetCard().manaCost);
            Destroy(card.gameObject);
        }
        Refresh();
        return output;
    }

    public IEnumerator ExecuteQueue()
    {
        /* 
        while (queueCards.Count > 0)   //Enable for using attack queue
        {
            yield return new WaitForSeconds(betweenCardDuration);
            yield return StartCoroutine(PlayCard(queueCards[0]));
            Destroy(queueCards[0].gameObject);
            queueCards.RemoveAt(0);
            Refresh();
        }

        yield return new WaitForSeconds(betweenCardDuration);
        

        yield return new WaitForSeconds(endTurnDamageCheckDuration);

        if (TurnController.turnController.GetNumberOfEnemies() > 0)
            StartCoroutine(TurnController.turnController.EnemyTurn());
        else
        {
            TurnController.turnController.ResetCurrentEnergy();
            TurnController.turnController.ResetEnergyDisplay();
            StartCoroutine(GameController.gameController.Victory());
        }
        */
        yield return new WaitForSeconds(0);
    }

    IEnumerator PlayCard(CardController card)
    {
        card.transform.localScale = new Vector2(playCardSize, playCardSize);
        yield return new WaitForSeconds(playDuration);
        card.TriggerEffect();
    }
}
