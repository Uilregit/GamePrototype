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
    public Image wildCardButton;

    public Image soldOut;

    public Color hasEnoughColor;
    public Color notEnoughColor;
    public Color buttonEnabledColor;
    public Color buttonDisabledColor;

    public Image dailyReroll;
    public Image weeklyReroll;

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
        StoryModeController.story.SetCombatInfoMenu(false);

        if (InformationLogger.infoLogger.GetLatestDayShopOpened() != InformationLogger.infoLogger.GetRawDailySeed())
            if (StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.PlusXDailyDealsRerollPerDay))
                InformationLogger.infoLogger.SetDailyRerollsLeft(StoryModeController.story.GetItemsBought()[StoryModeController.RewardsType.PlusXDailyDealsRerollPerDay]);
        if (InformationLogger.infoLogger.GetLatestDayShopOpened() / 7 != InformationLogger.infoLogger.GetRawWeeklySeed())
            if (StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.PlusXWeeklyWaresRerollPerWeek))
                InformationLogger.infoLogger.SetWeeklyRerollsLeft(StoryModeController.story.GetItemsBought()[StoryModeController.RewardsType.PlusXWeeklyWaresRerollPerWeek]);

        InformationLogger.infoLogger.SetLatestDayShopOpened(InformationLogger.infoLogger.GetRawDailySeed());
        InformationLogger.infoLogger.SavePlayerPreferences();

        dailyReroll.transform.GetChild(0).GetComponent<Text>().text = "Reroll x" + InformationLogger.infoLogger.GetDailyRerollsLeft();
        if (InformationLogger.infoLogger.GetDailyRerollsLeft() <= 0)
            dailyReroll.gameObject.SetActive(false);

        weeklyReroll.transform.GetChild(0).GetComponent<Text>().text = "Reroll x" + InformationLogger.infoLogger.GetWeeklyRerollsLeft();
        if (InformationLogger.infoLogger.GetWeeklyRerollsLeft() <= 0)
            weeklyReroll.gameObject.SetActive(false);
    }

    public void UpdateInventoryMaterials()
    {
        List<StoryModeController.RewardsType> inventoryList = new List<StoryModeController.RewardsType>() { StoryModeController.RewardsType.BlankCard, StoryModeController.RewardsType.WeaponBlueprint, StoryModeController.RewardsType.RubyShard, StoryModeController.RewardsType.SapphireShard, StoryModeController.RewardsType.EmeraldShard, StoryModeController.RewardsType.TopazShard, StoryModeController.RewardsType.QuartzShard, StoryModeController.RewardsType.OnyxShard };

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
        RollDailyCards(false);
        RollWeeklyCards(false);
    }

    private void PopulateCards(StoryModeShopCardController[] cards, int seed, float equipmentChance, float discountMultiplier, bool isDay)
    {
        Random.InitState(seed);
        int i = 0;
        foreach (StoryModeShopCardController card in cards)
        {
            Dictionary<StoryModeController.RewardsType, int> totalMaterials = new Dictionary<StoryModeController.RewardsType, int>();
            if (isDay)
            {
                if (StoryModeController.story.GetDailyBought().ContainsKey(seed) && StoryModeController.story.GetDailyBought()[seed][i])
                    card.SetCardBought();
                else
                    card.SetCardUnBought();
            }
            else
            {
                if (StoryModeController.story.GetWeeklyBought().ContainsKey(seed) && StoryModeController.story.GetWeeklyBought()[seed][i])
                    card.SetCardBought();
                else
                    card.SetCardUnBought();
            }

            if (Random.Range(0.0f, 1.0f) <= equipmentChance)
            {
                card.GetComponent<CardController>().SetEquipment(LootController.loot.GetRandomEquipment(), Card.CasterColor.Passive);
                card.SetEquipment(card.GetComponent<CardController>().GetEquipment());
                totalMaterials = card.GetComponent<CardController>().GetEquipment().GetCraftingMaterials();
            }
            else
            {
                card.GetComponent<CardController>().SetCard(LootController.loot.GetUnlockedCard(), false);
                card.SetEquipment(null);
                card.SetCard(card.GetComponent<CardController>());
                totalMaterials = card.GetComponent<CardController>().GetCard().GetCraftingMaterials();
            }
            card.seed = seed;

            Dictionary<StoryModeController.RewardsType, int> discountedMaterials = new Dictionary<StoryModeController.RewardsType, int>();
            foreach (StoryModeController.RewardsType m in totalMaterials.Keys)
                discountedMaterials[m] = (int)Mathf.Max(1, Mathf.Floor(totalMaterials[m] * discountMultiplier));            //Half off materials, but always costs at least 1
            card.SetMaterials(discountedMaterials);
            i++;
        }
    }

    public void RollDailyCards(bool isReroll)
    {
        if (isReroll)
        {
            InformationLogger.infoLogger.SetDailyRerollsLeft(InformationLogger.infoLogger.GetDailyRerollsLeft() - 1);
            InformationLogger.infoLogger.SavePlayerPreferences();
            dailyReroll.transform.GetChild(0).GetComponent<Text>().text = "Reroll x" + InformationLogger.infoLogger.GetDailyRerollsLeft();
            if (InformationLogger.infoLogger.GetDailyRerollsLeft() <= 0)
                dailyReroll.gameObject.SetActive(false);
        }

        int day = InformationLogger.infoLogger.GetDailySeed();
        float equipmentChance = 0.3f;

        //Initialize the daily cards
        PopulateCards(DailyDeals, day, equipmentChance, 0.5f, true);
    }

    public void RollWeeklyCards(bool isReroll)
    {
        if (isReroll)
        {
            InformationLogger.infoLogger.SetWeeklyRerollsLeft(InformationLogger.infoLogger.GetWeeklyRerollsLeft() - 1);
            InformationLogger.infoLogger.SavePlayerPreferences();
            weeklyReroll.transform.GetChild(0).GetComponent<Text>().text = "Reroll x" + InformationLogger.infoLogger.GetWeeklyRerollsLeft();
            if (InformationLogger.infoLogger.GetWeeklyRerollsLeft() <= 0)
                weeklyReroll.gameObject.SetActive(false);
        }

        int week = InformationLogger.infoLogger.GetWeeklySeed();
        float equipmentChance = 0.3f;

        //Iniitialize the weekly cards
        PopulateCards(WeeklyWares, week, equipmentChance, 0.75f, false);
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
        duplicateBar.transform.localScale = new Vector2(Mathf.Min(1f, (float)duplicate / 4.0f), 1);

        if (craftable && !bought)
            craftButton.color = buttonEnabledColor;
        else
            craftButton.color = buttonDisabledColor;

        craftButtonSelectable = craftable && !bought;

        StoryModeController.RewardsType rarity = StoryModeController.RewardsType.BlankCard;
        if (card.GetEquipment() == null)
        {
            if (card.GetCardController().GetCard().rarity == Card.Rarity.Common && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.CommonWildCard))
                rarity = StoryModeController.RewardsType.CommonWildCard;
            else if (card.GetCardController().GetCard().rarity == Card.Rarity.Rare && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.RareWildCard))
                rarity = StoryModeController.RewardsType.RareWildCard;
            else if (card.GetCardController().GetCard().rarity == Card.Rarity.Legendary && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.LegendaryWildCard))
                rarity = StoryModeController.RewardsType.LegendaryWildCard;
        }
        else
        {
            if (card.GetEquipment().rarity == Card.Rarity.Common && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.CommonWildCard))
                rarity = StoryModeController.RewardsType.CommonWildCard;
            else if (card.GetEquipment().rarity == Card.Rarity.Rare && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.RareWildCard))
                rarity = StoryModeController.RewardsType.RareWildCard;
            else if (card.GetEquipment().rarity == Card.Rarity.Legendary && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.LegendaryWildCard))
                rarity = StoryModeController.RewardsType.LegendaryWildCard;
        }

        int wildCardNum = 0;
        if (rarity != StoryModeController.RewardsType.BlankCard)
        {
            wildCardNum = StoryModeController.story.GetItemsBought()[rarity];

            wildCardButton.transform.GetChild(0).GetComponent<Text>().text = "x" + wildCardNum;
            wildCardButton.transform.GetChild(1).GetComponent<Image>().sprite = StoryModeController.story.GetRewardSprite(rarity, 0);
            wildCardButton.transform.GetChild(1).GetComponent<Image>().color = StoryModeController.story.GetRewardsColor(rarity);
        }
        wildCardButton.gameObject.SetActive(wildCardNum > 0);
        if (wildCardNum > 0)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.WildCardOptionInShop, 1);

        soldOut.gameObject.SetActive(bought);

        CameraController.camera.transform.position = new Vector3(-7, 0, CameraController.camera.transform.position.z);
    }

    public void ReportCraftButtonPressed()
    {
        if (!craftButtonSelectable)
            return;

        BuyCard();
    }

    public void ReportWildCardButtonPressed()
    {
        if (currentCard.GetCardController().GetCard().rarity == Card.Rarity.Common && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.CommonWildCard))
            currentCardMaterials = new Dictionary<StoryModeController.RewardsType, int> { { StoryModeController.RewardsType.CommonWildCard, 1 } };
        else if (currentCard.GetCardController().GetCard().rarity == Card.Rarity.Rare && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.RareWildCard))
            currentCardMaterials = new Dictionary<StoryModeController.RewardsType, int> { { StoryModeController.RewardsType.RareWildCard, 1 } };
        else if (currentCard.GetCardController().GetCard().rarity == Card.Rarity.Legendary && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.LegendaryWildCard))
            currentCardMaterials = new Dictionary<StoryModeController.RewardsType, int> { { StoryModeController.RewardsType.LegendaryWildCard, 1 } };

        BuyCard();
    }

    private void BuyCard()
    {
        if (currentCard.GetEquipment() == null)
            StoryModeController.story.ReportCardBought(currentCard.GetCardController().GetCard().name, currentCardMaterials);
        else
            StoryModeController.story.ReportEquipmentBought(currentCard.GetEquipment().equipmentName, currentCardMaterials);
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
        MusicController.music.PlaySFX(MusicController.music.uiUseHighSFX);
        MusicController.music.SetHighPassFilter(false);
        StoryModeController.story.GoToMapScene();
        StoryModeController.story.SetMenuBar(true);
    }
}
