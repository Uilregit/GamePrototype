using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            inventoryIcons[i].sprite = StoryModeController.story.GetRewardSprite(inventoryList[i]);
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

        //Initialize the daily cards
        Random.InitState(day);
        int i = 0;
        foreach (StoryModeShopCardController card in DailyDeals)
        {
            card.GetComponent<CardController>().SetCard(LootController.loot.GetCard(), false);
            card.SetCard(card.GetComponent<CardController>());

            Dictionary<StoryModeController.RewardsType, int> totalMaterials = card.GetComponent<CardController>().GetCard().GetCraftingMaterials();
            Dictionary<StoryModeController.RewardsType, int> discountedMaterials = new Dictionary<StoryModeController.RewardsType, int>();
            foreach (StoryModeController.RewardsType m in totalMaterials.Keys)
                discountedMaterials[m] = (int)Mathf.Max(1, Mathf.Floor(totalMaterials[m] * 0.5f));            //Half off materials, but always costs at least 1
            card.SetMaterials(discountedMaterials);

            if (StoryModeController.story.GetDailyBought()[day][i])
                card.SetCardBought();

            i++;
        }

        //Iniitialize the weekly cards
        Random.InitState(week);
        i = 0;
        foreach (StoryModeShopCardController card in WeeklyWares)
        {
            card.GetComponent<CardController>().SetCard(LootController.loot.GetCard(), false);
            card.SetCard(card.GetComponent<CardController>());

            Dictionary<StoryModeController.RewardsType, int> totalMaterials = card.GetComponent<CardController>().GetCard().GetCraftingMaterials();
            Dictionary<StoryModeController.RewardsType, int> discountedMaterials = new Dictionary<StoryModeController.RewardsType, int>();
            foreach (StoryModeController.RewardsType m in totalMaterials.Keys)
                discountedMaterials[m] = (int)Mathf.Max(1, Mathf.Floor(totalMaterials[m] * 0.75f));            //25% off materials, but always costs at least 1
            card.SetMaterials(discountedMaterials);

            if (StoryModeController.story.GetWeeklyBought()[week][i])
                card.SetCardBought();

            i++;
        }
    }

    public void ReportCardSelected(StoryModeShopCardController card, Dictionary<StoryModeController.RewardsType, int> materials, bool craftable, bool bought)
    {
        currentCard = card;
        currentCardMaterials = materials;

        selectedCard.SetCard(card.GetCardController(), false);
        rarityText.text = card.GetCardController().GetCard().rarity.ToString().ToUpper();

        int i = 0;
        foreach (StoryModeController.RewardsType m in materials.Keys)
        {
            selectedMaterialsIcons[i].sprite = StoryModeController.story.GetRewardSprite(m);
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
            img.color = PartyController.party.GetPlayerColor(card.GetCardController().GetCard().casterColor);

        int duplicate = 0;
        if (StoryModeController.story.GetCardUnlocked().ContainsKey(card.GetCardController().GetCard().name))
            duplicate = StoryModeController.story.GetCardUnlocked()[card.GetCardController().GetCard().name];

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
}
