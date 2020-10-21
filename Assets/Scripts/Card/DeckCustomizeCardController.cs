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

    public Canvas selectedCardCanvas;
    private Canvas originalCanvas;
    private Vector2 localScale;
    private Vector2 originalLocation;
    private int originalSorterOrder;

    private void Awake()
    {
        //col = GetComponent<Collider2D>();
        localScale = transform.localScale;
        originalLocation = transform.position;
        originalCanvas = transform.parent.GetComponent<Canvas>();
        originalSorterOrder = GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder;
    }

    public void OnMouseDown()
    {
        clickedTime = Time.time;
        StartCoroutine(EnlargeCard());
        highlight.enabled = false;
        CollectionController.collectionController.RemoveCardFromNew(card);
    }

    public void OnMouseUp()
    {
        StopAllCoroutines();
        transform.SetParent(originalCanvas.transform);
        transform.localScale = localScale;
        transform.position = originalLocation;
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

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        transform.SetParent(selectedCardCanvas.transform);
        GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = selectedCardCanvas.sortingOrder + 1;
        transform.position = new Vector2(originalLocation.x, originalLocation.y + HandController.handController.cardHighlightHeight);
        transform.localScale = new Vector2(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize);
    }
}
