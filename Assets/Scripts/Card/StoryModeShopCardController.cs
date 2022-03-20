using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryModeShopCardController : MonoBehaviour
{
    public int index;
    public bool isDailyCard;
    public int seed;

    private CardController card;
    private Equipment equipment;
    private Dictionary<StoryModeController.RewardsType, int> materials;
    private float clickedTime;
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

    private bool bought = false;
    //private int originalSorterOrder;

    private void Awake()
    {
        cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
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

    public void SetEquipment(Equipment newEquipment)
    {
        equipment = newEquipment;
    }

    public void SetMaterials(Dictionary<StoryModeController.RewardsType, int> materialsList)
    {
        materials = materialsList;

        int i = 0;
        foreach (StoryModeController.RewardsType m in materialsList.Keys)
        {
            Color c = StoryModeController.story.GetRewardsColor(m);
            materialIcons[i].sprite = StoryModeController.story.GetRewardSprite(m, i);
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
        if (TutorialController.tutorial.GetEnabled())
            return;

        clickedTime = Time.time;
        StartCoroutine(EnlargeCard());
    }

    public void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        //transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, true);
        StopAllCoroutines();
        transform.SetParent(originalCanvas.transform);
        transform.localScale = localScale;
        transform.position = originalLocation;
        cardDisplay.SetToolTip(false, -1, 1, false);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = originalSorterOrder;
        if (Time.time - clickedTime <= clickThreshold)
            SelectCard();
        else
            cardDisplay.cardSounds.PlayUncastSound();
    }

    public void SelectCard()
    {
        cardDisplay.cardSounds.PlayCastSound();
        picked = true;

        StoryModeShopController.shop.ReportCardSelected(this, materials, GetHasEnoughMaterials(), bought);
    }

    public void SetCardBought()
    {
        if (InformationLogger.infoLogger.debug)     //If debugged, always show the card as available to be bought
            return;

        bought = true;
        cardDisplay.SetHighLight(false);
        soldOutBack.enabled = true;
        soldOutText.enabled = true;

        StoryModeController.story.SetCardBought(isDailyCard, seed, index);
        InformationLogger.infoLogger.SaveStoryModeGame();
    }

    public void SetCardUnBought()
    {
        bought = false;
        soldOutBack.enabled = false;
        soldOutText.enabled = false;
    }

    public void ResetBuyable()
    {
        bool hasEnoughMaterials = GetHasEnoughMaterials();

        Card.Rarity rarity = Card.Rarity.Common;
        if (equipment == null)
            rarity = card.GetCard().rarity;
        else
            rarity = equipment.rarity;
        if (rarity == Card.Rarity.Common && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.CommonWildCard) && StoryModeController.story.GetItemsBought()[StoryModeController.RewardsType.CommonWildCard] > 0)
            hasEnoughMaterials = true;
        else if (rarity == Card.Rarity.Rare && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.RareWildCard) && StoryModeController.story.GetItemsBought()[StoryModeController.RewardsType.RareWildCard] > 0)
            hasEnoughMaterials = true;
        else if (rarity == Card.Rarity.Legendary && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.LegendaryWildCard) && StoryModeController.story.GetItemsBought()[StoryModeController.RewardsType.LegendaryWildCard] > 0)
            hasEnoughMaterials = true;

        Debug.Log(hasEnoughMaterials);

        if (hasEnoughMaterials)
            Show();
        else
            Hide();

        if (bought)
            SetCardBought();
    }

    public bool GetHasEnoughMaterials()
    {
        bool hasEnoughMaterials = true;
        int i = 0;
        foreach (StoryModeController.RewardsType m in materials.Keys)
        {
            if (!StoryModeController.story.GetItemsBought().ContainsKey(m) || StoryModeController.story.GetItemsBought()[m] < materials[m])
            {
                hasEnoughMaterials = false;

                materialBack[i].color = Color.red;
            }
            i++;
        }

        return hasEnoughMaterials;
    }

    public void Hide()
    {
        cardDisplay.SetHighLight(false);
    }

    public void Show()
    {
        cardDisplay.SetHighLight(true);
    }

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        cardDisplay.cardSounds.PlaySelectSound();
        transform.SetParent(selectedCardCanvas.transform);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = selectedCardCanvas.sortingOrder + 1;
        transform.position = new Vector3(Mathf.Clamp(originalLocation.x, HandController.handController.cardHighlightXBoarder * -1, HandController.handController.cardHighlightXBoarder), originalLocation.y + HandController.handController.GetCardHighlightHeight(), 0);
        transform.localScale = new Vector3(HandController.handController.GetCardHighlightSize(), HandController.handController.GetCardHighlightSize(), 1);
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, false);
        cardDisplay.SetToolTip(true, -1, 1, false);
    }

    public CardController GetCardController()
    {
        return card;
    }

    public Equipment GetEquipment()
    {
        return equipment;
    }
}