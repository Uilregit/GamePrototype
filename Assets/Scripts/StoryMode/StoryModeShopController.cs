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
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        int day = (int)(System.DateTime.UtcNow - epochStart).TotalDays;
        int week = (int)(day / 7);

        //Initialize the daily cards
        Random.InitState(day);
        foreach (StoryModeShopCardController card in DailyDeals)
        {
            card.GetComponent<CardController>().SetCard(LootController.loot.GetCard(), false);
            card.SetCard(card.GetComponent<CardController>());

            Dictionary<StoryModeController.RewardsType, int> totalMaterials = card.GetComponent<CardController>().GetCard().GetCraftingMaterials();
            Dictionary<StoryModeController.RewardsType, int> discountedMaterials = new Dictionary<StoryModeController.RewardsType, int>();
            foreach (StoryModeController.RewardsType m in totalMaterials.Keys)
                discountedMaterials[m] = (int)Mathf.Max(1, Mathf.Floor(totalMaterials[m] * 0.5f));            //Half off materials, but always costs at least 1
            card.SetMaterials(discountedMaterials);
        }

        //Iniitialize the weekly cards
        Random.InitState(week);
        foreach (StoryModeShopCardController card in WeeklyWares)
        {
            card.GetComponent<CardController>().SetCard(LootController.loot.GetCard(), false);
            card.SetCard(card.GetComponent<CardController>());

            Dictionary<StoryModeController.RewardsType, int> totalMaterials = card.GetComponent<CardController>().GetCard().GetCraftingMaterials();
            Dictionary<StoryModeController.RewardsType, int> discountedMaterials = new Dictionary<StoryModeController.RewardsType, int>();
            foreach (StoryModeController.RewardsType m in totalMaterials.Keys)
                discountedMaterials[m] = (int)Mathf.Max(1, Mathf.Floor(totalMaterials[m] * 0.75f));            //25% off materials, but always costs at least 1
            card.SetMaterials(discountedMaterials);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
