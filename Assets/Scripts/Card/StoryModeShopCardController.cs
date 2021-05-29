using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryModeShopCardController : MonoBehaviour
{
    private CardController card;
    private Dictionary<StoryModeController.RewardsType, int> materials;
    private float clickedTime;
    private Collider2D col;
    private CardDisplay cardDisplay;
    private bool picked = false;

    public Text[] materialValues;
    public Image[] materialIcons;
    public Image[] materialBack;

    public Image soldOutBack;
    public Text soldOutText;
    private float clickThreshold = 0.2f;

    public Canvas selectedCardCanvas;
    private Canvas originalCanvas;
    private Vector3 localScale;
    private Vector3 originalLocation;
    //private int originalSorterOrder;

    private void Awake()
    {
        cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        col = GetComponent<Collider2D>();
        localScale = transform.localScale;
        originalLocation = transform.position;
        originalCanvas = transform.parent.GetComponent<Canvas>();
        //originalSorterOrder = GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder;
    }

    public void SetCard(CardController newCard)
    {
        card = newCard;
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(newCard, true);
    }

    public void SetMaterials(Dictionary<StoryModeController.RewardsType, int> materialsList)
    {
        materials = materialsList;

        int i = 0;
        foreach (StoryModeController.RewardsType m in materialsList.Keys)
        {
            Color c = StoryModeController.story.GetRewardsColor(m);
            materialIcons[i].sprite = StoryModeController.story.GetRewardSprite(m);
            materialIcons[i].color = c;
            materialValues[i].text = materialsList[m].ToString();
            materialValues[i].color = c;
            i++;
        }

        for (int j = i; j < 3; j++)
        {
            materialIcons[j].color = Color.clear;
            materialValues[j].color = Color.clear;
        }

        ResetBuyable();
    }

    public void OnMouseDown()
    {
        clickedTime = Time.time;
        StartCoroutine(EnlargeCard());
    }

    public void OnMouseUp()
    {
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, true);
        StopAllCoroutines();
        transform.SetParent(originalCanvas.transform);
        transform.localScale = localScale;
        transform.position = originalLocation;
        cardDisplay.SetToolTip(false, -1, 1, false);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = originalSorterOrder;
        if (Time.time - clickedTime <= clickThreshold)
            SelectCard();
    }

    public void SelectCard()
    {
        picked = true;

        Hide();
        soldOutBack.enabled = true;
        soldOutText.enabled = true;
        /*
        CollectionController.collectionController.AddRewardsCard(card, false);
        ResourceController.resource.ChangeGold(-price);
        ShopController.shop.ReportBoughtCard(card);
        ScoreController.score.UpdateGoldUsed(price);
        */

        col.enabled = false;
        //cardDisplay.Hide();
    }

    public void ResetBuyable()
    {
        if (picked)
            return;

        bool hasEnoughMaterials = true;
        int index = 0;
        foreach (StoryModeController.RewardsType m in materials.Keys)
        {
            if (!StoryModeController.story.GetItemsBought().ContainsKey(m) || StoryModeController.story.GetItemsBought()[m] < materials[m])
            {
                hasEnoughMaterials = false;

                materialBack[index].color = Color.red;
            }
            index++;
        }

        if (hasEnoughMaterials)
            Show();
        else
            Hide();
    }

    public void Hide()
    {
        cardDisplay.SetHighLight(false);
        col.enabled = false;
    }

    public void Show()
    {
        cardDisplay.SetHighLight(true);
        col.enabled = true;
    }

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        transform.SetParent(selectedCardCanvas.transform);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = selectedCardCanvas.sortingOrder + 1;
        transform.position = new Vector3(Mathf.Clamp(originalLocation.x, HandController.handController.cardHighlightXBoarder * -1, HandController.handController.cardHighlightXBoarder), originalLocation.y + HandController.handController.cardHighlightHeight, 0);
        transform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, false);
        cardDisplay.SetToolTip(true, -1, 1, false);
    }
}