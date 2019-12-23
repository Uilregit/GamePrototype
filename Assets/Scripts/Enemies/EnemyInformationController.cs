using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInformationController : MonoBehaviour
{
    [SerializeField] private GameObject enemyDisplayCard;
    [SerializeField] private float cardSpacing;
    [SerializeField] private float cardStartingHeight;
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    private LineRenderer targetLine;
    private GameObject[] displayedCards;
    private GameObject usedCard;

    private EnemyController enemyController;

    // Start is called before the first frame update
    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        targetLine = GetComponent<LineRenderer>();
        DrawCards();
    }

    //Create attack and move range and Display cards
    private void OnMouseDown()
    {
        int bonusMoveRange = GetComponent<HealthController>().GetBonusMoveRange();
        TileCreator.tileCreator.CreateTiles(this.gameObject, transform.position, Card.CastShape.Circle, enemyController.moveRange + bonusMoveRange, enemyController.moveRangeColor, new string[] { "Player", "Blockade" }, 0);
        List<Vector2> movePositions = TileCreator.tileCreator.GetTilePositions(0);
        foreach (Vector2 position in movePositions)
        {
            TileCreator.tileCreator.CreateTiles(this.gameObject, position, Card.CastShape.Circle, enemyController.attackRange, enemyController.attackRangeColor, new string[] { "" }, 1);
            TileCreator.tileCreator.CreateTiles(this.gameObject, transform.position, Card.CastShape.Circle, enemyController.moveRange + bonusMoveRange, enemyController.moveRangeColor, new string[] { "Player", "Blockade" }, 1);
        }
        ShowCards();
    }

    //Destroy attack and move range
    private void OnMouseUp()
    {
        TileCreator.tileCreator.DestryTiles(this.gameObject);
        HideCards();
        HideCards();
    }

    private void DrawCards()
    {
        displayedCards = new GameObject[enemyController.attackSquence.Count];
        //Odd number of cards
        if (enemyController.attackSquence.Count % 2 == 1)
        {
            for (int i = 0; i < enemyController.attackSquence.Count; i++)
            {
                //Place card evenly spaced centered around the middle
                float cardLocationX = transform.position.x + (i - enemyController.attackSquence.Count / 2) * cardSpacing;
                //Place the card between an acceptable range to be on the oppopsite vertical side of the caster
                float cardLocationY = Mathf.Clamp(transform.position.y + Mathf.Sign(transform.position.y - 1) * -1 * cardStartingHeight, minHeight, maxHeight);

                Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);

                displayedCards[i] = Instantiate(enemyDisplayCard);
                displayedCards[i].transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
                displayedCards[i].transform.position = cardLocation;
                displayedCards[i].GetComponent<CardController>().SetCard(enemyController.attackSquence[i]);
            }
        }
        //Even number of cards
        else
        {
            for (int i = 0; i < enemyController.attackSquence.Count; i++)
            {
                //Place card evenly spaced centered around the middle
                float cardLocationX = transform.position.x + (i - enemyController.attackSquence.Count / 2 + 0.5f) * cardSpacing;
                //Place the card between an acceptable range to be on the oppopsite vertical side of the caster
                float cardLocationY = Mathf.Clamp(transform.position.y + Mathf.Sign(transform.position.y - 1) * -1 * cardStartingHeight, minHeight, maxHeight);

                Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);

                displayedCards[i] = Instantiate(enemyDisplayCard);
                displayedCards[i].transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
                displayedCards[i].transform.position = cardLocation;
                displayedCards[i].GetComponent<CardController>().SetCard(enemyController.attackSquence[i]);
            }
        }
        HideCards();
    }

    private void HideCards()
    {
        foreach (GameObject card in displayedCards)
            card.GetComponent<CardDisplay>().Hide();
    }

    private void ShowCards()
    {
        foreach (GameObject card in displayedCards)
            card.GetComponent<CardDisplay>().Show();
    }

    public void TriggerCard(int cardIndex, GameObject target)
    {
        displayedCards[cardIndex].GetComponent<CardEffectsController>().TriggerEffect(this.gameObject, target.transform.position);
    }

    public SimHealthController SimulateTriggerCard(int cardIndex, GameObject target, SimHealthController simH)
    {
        return displayedCards[cardIndex].GetComponent<CardEffectsController>().SimulateTriggerEffect(this.gameObject, target.transform.position, simH);
    }

    public void ShowUsedCard(Card card, GameObject target)
    {
        //Place card to the opposite horrizontal side of the target from the caster
        float cardLocationX = transform.position.x + (cardSpacing / 2 + 0.5f) * Mathf.Sign(transform.position.x - target.transform.position.x);
        //Place the card at the caster height between an acceptable range
        float cardLocationY = Mathf.Clamp(transform.position.y, minHeight, maxHeight);

        Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);

        usedCard = Instantiate(enemyDisplayCard);
        usedCard.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        usedCard.transform.position = cardLocation;
        usedCard.GetComponent<CardController>().SetCard(card);
    }

    public void DestroyUsedCard()
    {
        HideTargetLine();
        Destroy(usedCard);
    }

    public void ShowTargetLine(GameObject target)
    {
        targetLine.SetPosition(0, transform.position);
        targetLine.SetPosition(1, target.transform.position);
        targetLine.enabled = true;
    }

    private void HideTargetLine()
    {
        targetLine.enabled = false;
    }
}
