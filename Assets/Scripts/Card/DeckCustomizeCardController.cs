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
    private Vector3 localScale;
    private Vector3 originalLocation;
    private CardDisplay cardDisplay;
    //private int originalSorterOrder;

    private void Awake()
    {
        DontDestroyOnLoad(selectedCardCanvas);
        //col = GetComponent<Collider2D>();
        localScale = transform.localScale;
        originalLocation = transform.position;
        originalCanvas = transform.parent.GetComponent<Canvas>();
        cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        //originalSorterOrder = transform.GetChild(0).GetComponent<CardDisplay>().Show();.cardName.GetComponent<MeshRenderer>().sortingOrder;
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
        cardDisplay.SetToolTip(false, -1, 1, false);
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
        cardDisplay.SetCard(newCard, false);
    }

    public void SetCount(int newCount)
    {
        count.text = "x" + newCount.ToString();
    }

    public void Hide()
    {
        count.enabled = false;
        if ((object)cardDisplay == null)
            cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        cardDisplay.Hide();
        col.enabled = false;
    }

    public void Show()
    {
        count.enabled = true;
        if ((object)cardDisplay == null)
            cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        cardDisplay.Show();
        col.enabled = true;
        cardDisplay.lineRenderer.enabled = false;
    }

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        transform.SetParent(selectedCardCanvas.transform);
        //cardDisplay.cardName.GetComponent<MeshRenderer>().sortingOrder = selectedCardCanvas.sortingOrder + 1;
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);
        cardDisplay.SetToolTip(true, -1, 1, false);
    }
}
