using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryModeShopController : MonoBehaviour
{
    public static StoryModeShopController shop;

    public Image[] inventoryIcons;
    public Text[] inventoryNumbers;

    public StoryModeShopCardController[] DailyDeals;
    public StoryModeShopCardController[] WeeklyWares;
    public StoryModeShopCardController[] CraftingCards;

    public Text rarityText;
    public CardDisplay selectedCard;
    public Text duplicateText;
    public Image duplicateBar;
    public Image[] duplicateBarBack;
    public Image[] selectedMaterialsIcons;
    public Text[] selectedMaterialsNames;
    public Text[] selectedMaterialAmounts;
    public Text[] stockMaterialAmounts;
    public Image craftButton;

    public Image soldOut;

    public Color hasEnoughColor;
    public Color notEnoughColor;
    public Color buttonEnabledColor;
    public Color buttonDisabledColor;

    private StoryModeShopCardController currentCard;
    private Dictionary<StoryModeController.RewardsType, int> currentCardMaterials;
    private bool craftButtonSelectable = true;

    // Start is called before the first frame update
    void Start()
    {
        if (StoryModeShopController.shop == null)
            StoryModeShopController.shop = this;
        else
            Destroy(this.gameObject);

        PopulateCards();
        UpdateInventoryMaterials();
    }

    public void UpdateInventoryMaterials()
    {
        List<StoryModeController.RewardsType> inventoryList = new List<StoryModeController.RewardsType>() { StoryModeController.RewardsType.BlankCard, StoryModeController.RewardsType.WeaponBlueprint, StoryModeController.RewardsType.RubyShard, StoryModeController.RewardsType.SapphireShard, StoryModeController.RewardsType.EmeraldShard, StoryModeController.RewardsType.SpessartineShard, StoryModeController.RewardsType.QuartzShard, StoryModeController.RewardsType.OnyxShard };

        for (int i = 0; i < inventoryList.Count; i++)
        {
            inventoryIcons[i].sprite = StoryModeController.story.GetRewardSprite(inventoryList[i], i);
            inventoryIcons[i].color = StoryModeController.story.GetRewardsColor(inventoryList[i]);
            if (StoryModeController.story.GetItemsBought().ContainsKey(inventoryList[i]))
                inventoryNumbers[i].text = StoryModeController.story.GetItemsBought()[inventoryList[i]].ToString();
            else
                inventoryNumbers[i].text = "0";
            inventoryNumbers[i].color = StoryModeController.story.GetRewardsColor(inventoryList[i]);
        }
    }

    public void PopulateCards()
    {
        int day = StoryModeController.story.GetDailySeed();
        int week = StoryModeController.story.GetWeeklySeed();

        float equipmentChance = 0.3f;

        //Initialize the daily cards
        PopulateCards(DailyDeals, day, equipmentChance, 0.5f, true);
        //Iniitialize the weekly cards
        PopulateCards(WeeklyWares, week, equipmentChance, 0.75f, false);
    }

    private void PopulateCards(StoryModeShopCardController[] cards, int seed, float equipmentChance, float discountMultiplier, bool isDay)
    {
        Random.InitState(seed);
        int i = 0;
        foreach (StoryModeShopCardController card in cards)
        {
            Dictionary<StoryModeController.RewardsType, int> totalMaterials = new Dictionary<StoryModeController.RewardsType, int>();

            if (Random.Range(0.0f, 1.0f) <= equipmentChance)
            {
                card.GetComponent<CardController>().SetEquipment(LootController.loot.GetRandomEquipment(), Card.CasterColor.Passive);
                card.SetEquipment(card.GetComponent<CardController>().GetEquipment());
                totalMaterials = card.GetComponent<CardController>().GetEquipment().GetCraftingMaterials();
            }
            else
            {
                card.GetComponent<CardController>().SetCard(LootController.loot.GetCard(), false);
                card.SetCard(card.GetComponent<CardController>());
                totalMaterials = card.GetComponent<CardController>().GetCard().GetCraftingMaterials();
            }

            Dictionary<StoryModeController.RewardsType, int> discountedMaterials = new Dictionary<StoryModeController.RewardsType, int>();
            foreach (StoryModeController.RewardsType m in totalMaterials.Keys)
                discountedMaterials[m] = (int)Mathf.Max(1, Mathf.Floor(totalMaterials[m] * discountMultiplier));            //Half off materials, but always costs at least 1
            card.SetMaterials(discountedMaterials);

            if (isDay)
            {
                if (StoryModeController.story.GetDailyBought().ContainsKey(seed) && StoryModeController.story.GetDailyBought()[seed][i])
                    card.SetCardBought();
            }
            else
            {
                if (StoryModeController.story.GetWeeklyBought().ContainsKey(seed) && StoryModeController.story.GetWeeklyBought()[seed][i])
                    card.SetCardBought();
            }
            i++;
        }
    }

    public void ReportCardSelected(StoryModeShopCardController card, Dictionary<StoryModeController.RewardsType, int> materials, bool craftable, bool bought)
    {
        currentCard = card;
        currentCardMaterials = materials;

        if (card.GetEquipment() != null)
        {
            selectedCard.SetEquipment(card.GetEquipment(), Card.CasterColor.Passive);
            rarityText.text = card.GetEquipment().rarity.ToString().ToUpper();
        }
        else
        {
            selectedCard.SetCard(card.GetCardController(), false);
            rarityText.text = card.GetCardController().GetCard().rarity.ToString().ToUpper();
        }


        int i = 0;
        foreach (StoryModeController.RewardsType m in materials.Keys)
        {
            selectedMaterialsIcons[i].sprite = StoryModeController.story.GetRewardSprite(m, i);
            selectedMaterialsIcons[i].color = StoryModeController.story.GetRewardsColor(m);

            selectedMaterialsNames[i].text = m.ToString();

            selectedMaterialAmounts[i].text = "x" + materials[m].ToString();

            if (StoryModeController.story.GetItemsBought().ContainsKey(m))
            {
                stockMaterialAmounts[i].text = StoryModeController.story.GetItemsBought()[m].ToString();

                if (StoryModeController.story.GetItemsBought()[m] >= materials[m])
                    stockMaterialAmounts[i].color = hasEnoughColor;
                else
                    stockMaterialAmounts[i].color = notEnoughColor;
            }
            else
            {
                stockMaterialAmounts[i].text = "0";
                stockMaterialAmounts[i].color = notEnoughColor;
            }

            i++;
        }

        for (int j = i; j < 3; j++)
        {
            selectedMaterialsIcons[j].color = Color.clear;
            selectedMaterialsNames[j].text = "";
            selectedMaterialAmounts[j].text = "";
            stockMaterialAmounts[j].text = "";
        }

        foreach (Image img in duplicateBarBack)
        {
            if (card.GetEquipment() != null)
                img.color = PartyController.party.GetPlayerColor(Card.CasterColor.Enemy);
            else
                img.color = PartyController.party.GetPlayerColor(card.GetCardController().GetCard().casterColor);
        }

        //Find the number of copies that's already owned for this card
        int duplicate = 0;

        if (card.GetEquipment() != null)
            duplicate = CollectionController.collectionController.GetCountOfEquipmentInCollection(card.GetEquipment());
        else
            duplicate = CollectionController.collectionController.GetCountOfCardInCollection(card.GetCardController().GetCard());

        duplicateText.text = duplicate + "/4";
        duplicateBar.transform.localScale = new Vector2((float)duplicate / 4.0f, 1);

        if (craftable && !bought)
            craftButton.color = buttonEnabledColor;
        else
            craftButton.color = buttonDisabledColor;

        craftButtonSelectable = craftable && !bought;

        soldOut.gameObject.SetActive(bought);

        CameraController.camera.transform.position = new Vector3(-7, 0, CameraController.camera.transform.position.z);
    }

    public void ReportCraftButtonPressed()
    {
        if (!craftButtonSelectable)
            return;

        StoryModeController.story.ReportCardBought(currentCard.GetCardController().GetCard().name, currentCardMaterials);
        PopulateCards();
        currentCard.SetCardBought();
        CameraController.camera.transform.position = new Vector3(0, 0, CameraController.camera.transform.position.z);
    }

    public void ReportBackButtonPressed()
    {
        CameraController.camera.transform.position = new Vector3(0, 0, CameraController.camera.transform.position.z);
    }

    public void ReportReturnButtonPressed()
    {
        StoryModeController.story.GoToMapScene();
        StoryModeController.story.SetMenuBar(true);
    }
}
