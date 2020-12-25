using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardsMenuController : MonoBehaviour
{
    public static RewardsMenuController rewardsMenu;

    public Image menuBack;
    public List<Image> menuItemBack;
    public Image relicRewardMenu;

    public Sprite goldRewardSprite;
    public Color goldColor;
    public Sprite cardRewardSprite;
    public Color cardColor;

    private RewardType[] rewardsTypes;
    private Sprite[] rewardsImages;
    private int[] rewardsValues;
    private int numRewards = 0;
    private int numRewardsTaken = 0;
    private int deckId;
    private Relic thisRelic;

    public enum RewardType { PassiveGold, OverkillGold, Card, Relic };

    // Start is called before the first frame update
    void Awake()
    {
        if (RewardsMenuController.rewardsMenu == null)
            RewardsMenuController.rewardsMenu = this;
        else
            Destroy(this.gameObject);

        rewardsTypes = new RewardType[menuItemBack.Count];
        rewardsImages = new Sprite[menuItemBack.Count];
        rewardsValues = new int[menuItemBack.Count];

        HideMenu();
        HideRelicRewardMenu();
    }

    public void AddReward(RewardType type, Sprite overrideImage, int value)
    {
        rewardsTypes[numRewards] = type;
        rewardsImages[numRewards] = overrideImage;
        rewardsValues[numRewards] = value;
        numRewards += 1;
    }

    public void ShowMenu()
    {
        menuBack.enabled = true;
        for (int i = 0; i < numRewards; i++)
        {
            menuItemBack[i].GetComponent<RewardsItemController>().SetValues(i, rewardsValues[i], rewardsTypes[i]);

            menuItemBack[i].enabled = true;
            menuItemBack[i].GetComponent<Collider2D>().enabled = true;

            string description = "";

            if (rewardsImages[i] != null)
            {
                menuItemBack[i].transform.GetChild(0).GetComponent<Image>().sprite = rewardsImages[i];
            }
            else if (rewardsTypes[i] == RewardType.OverkillGold || rewardsTypes[i] == RewardType.PassiveGold)
            {
                menuItemBack[i].transform.GetChild(0).GetComponent<Image>().sprite = goldRewardSprite;
                menuItemBack[i].transform.GetChild(0).GetComponent<Image>().color = goldColor;
                if (rewardsTypes[i] == RewardType.OverkillGold)
                    description = rewardsValues[i] + " Overkill Gold";
                else
                    description = rewardsValues[i] + " Gold";
            }
            else if (rewardsTypes[i] == RewardType.Card)
            {
                menuItemBack[i].transform.GetChild(0).GetComponent<Image>().sprite = cardRewardSprite;
                menuItemBack[i].transform.GetChild(0).GetComponent<Image>().color = cardColor;
                description = "Choose 1 Card";
            }
            else if (rewardsTypes[i] == RewardType.Relic)
            {
                Relic thisRelic = RelicController.relic.GetRandomRelic();
                menuItemBack[i].transform.GetChild(0).GetComponent<Image>().sprite = thisRelic.art;
                menuItemBack[i].transform.GetChild(0).GetComponent<Image>().color = thisRelic.color;
                description = "Obtain a Relic";

                menuItemBack[i].GetComponent<RewardsItemController>().SetRelic(thisRelic);
            }
            menuItemBack[i].transform.GetChild(1).GetComponent<Text>().text = description;

            menuItemBack[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
            menuItemBack[i].transform.GetChild(1).GetComponent<Text>().enabled = true;
        }
    }

    public void HideMenu()
    {
        menuBack.enabled = false;
        for (int i = 0; i < menuItemBack.Count; i++)
        {
            menuItemBack[i].enabled = false;
            menuItemBack[i].GetComponent<Collider2D>().enabled = false;

            menuItemBack[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
            menuItemBack[i].transform.GetChild(1).GetComponent<Text>().enabled = false;
        }
    }

    public void SetItemsClickable(bool state)
    {
        for (int i = 0; i < menuItemBack.Count; i++)
            menuItemBack[i].GetComponent<Collider2D>().enabled = state;
    }

    public void SetDeckID(int value)
    {
        deckId = value;
    }

    public void ReportRewardTaken(int index)
    {
        numRewardsTaken += 1;
        if (numRewards == numRewardsTaken)
        {
            if (RoomController.roomController.GetWorldLevel() != 1 && RoomController.roomController.GetCurrentRoomSetup().isBossRoom)
                RoomController.roomController.LoadNewWorld(RoomController.roomController.GetWorldLevel() + 1);
            GameController.gameController.LoadScene("OverworldScene", true, deckId);
        }
    }

    public void ReportRelicTaken()
    {
        RelicController.relic.AddRelic(thisRelic);
        HideRelicRewardMenu();
        SetItemsClickable(true);
        ReportRewardTaken(0);
    }

    public void HideRewardCards()
    {
        for (int i = 0; i < GameController.gameController.rewardCards.Length; i++)
        {
            GameController.gameController.rewardCards[i].Hide();
            GameController.gameController.rewardCards[i].transform.parent.GetComponent<Collider2D>().enabled = false;
        }
    }

    public void ShowRelicRewardMenu(Relic relic)
    {
        thisRelic = relic;
        relicRewardMenu.enabled = true;
        relicRewardMenu.transform.GetChild(0).GetComponent<Image>().enabled = true;
        relicRewardMenu.transform.GetChild(1).GetComponent<Image>().enabled = true;
        relicRewardMenu.transform.GetChild(1).GetComponent<Image>().sprite = relic.art;
        relicRewardMenu.transform.GetChild(1).GetComponent<Image>().color = relic.color;
        relicRewardMenu.transform.GetChild(2).GetComponent<Text>().enabled = true;
        relicRewardMenu.transform.GetChild(2).GetComponent<Text>().text = relic.relicName;
        relicRewardMenu.transform.GetChild(3).GetComponent<Text>().enabled = true;
        relicRewardMenu.transform.GetChild(3).GetComponent<Text>().text = relic.description;
        relicRewardMenu.transform.GetChild(4).GetComponent<Image>().enabled = true;
        relicRewardMenu.transform.GetChild(4).GetComponent<Collider2D>().enabled = true;
        relicRewardMenu.transform.GetChild(5).GetComponent<Text>().enabled = true;
    }

    public void HideRelicRewardMenu()
    {
        relicRewardMenu.enabled = false;
        relicRewardMenu.transform.GetChild(0).GetComponent<Image>().enabled = false;
        relicRewardMenu.transform.GetChild(1).GetComponent<Image>().enabled = false;
        relicRewardMenu.transform.GetChild(2).GetComponent<Text>().enabled = false;
        relicRewardMenu.transform.GetChild(3).GetComponent<Text>().enabled = false;
        relicRewardMenu.transform.GetChild(4).GetComponent<Image>().enabled = false;
        relicRewardMenu.transform.GetChild(4).GetComponent<Collider2D>().enabled = false;
        relicRewardMenu.transform.GetChild(5).GetComponent<Text>().enabled = false;
    }
}
