using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckCustomizeCardController : MonoBehaviour
{
    public float clickThreshold;

    private CardController card;
    private float clickedTime;
    public Text count;
    public Image greyOut;
    public Outline highlight;
    public Collider2D col;

    private void Awake()
    {
        //col = GetComponent<Collider2D>();
    }

    public void OnMouseDown()
    {
        clickedTime = Time.time;
        highlight.enabled = false;
        CollectionController.collectionController.RemoveCardFromNew(card);
        /*
        //Enlarge for easy viewing
        transform.localScale = new Vector2(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize);
        //col.size = colliderSize / HandController.handController.cardHighlightSize * HandController.handController.cardStartingSize;
        float x = Mathf.Clamp(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).x, -HandController.handController.cardHighlightXBoarder, HandController.handController.cardHighlightXBoarder);
        float y = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).y + HandController.handController.cardHighlightHeight;
        transform.position = new Vector2(x, y);
        transform.SetAsLastSibling();

        base.OnMouseDown();*/
    }

    public void OnMouseUp()
    {
        if (Time.time - clickedTime <= clickThreshold)
            SelectCard();
    }
    
    public void SelectCard()
    {
        CollectionController.collectionController.AddCard(card);
    }

    public void SetCard(CardController newCard)
    {
        card = newCard;
        GetComponent<CardDisplay>().SetCard(newCard, false);
    }

    public void SetCount (int newCount)
    {
        count.text = "x" + newCount.ToString();
    }

    public void Hide()
    {
        count.enabled = false;
        GetComponent<CardDisplay>().Hide();
        col.enabled = false;
    }

    public void Show()
    {
        count.enabled = true;
        GetComponent<CardDisplay>().Show();
        col.enabled = true;
    }
}
