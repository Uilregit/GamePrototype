using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryModeEndItemController : MonoBehaviour
{
    public int index;

    public Image lockedIcon;
    public Text lockedAchievementText;
    public Image soldOutText;
    public Image blackoutImage;
    public Image outline;
    public Text itemName;
    public Image itemIcon;
    public Text costText;
    public Text rewardTypeText;

    public StoryModeEndSceenController endSceneController;

    private int itemCost;
    private bool selected = false;

    private bool selectable = false;

    private StoryModeController.RewardsType thisName;
    private Card thisCard;
    private Equipment thisEquipment;
    private int amount;

    public void SetEnabled(bool state)
    {
        lockedIcon.gameObject.SetActive(!state);
        blackoutImage.enabled = !state;
        soldOutText.gameObject.SetActive(false);

        selectable = state;
    }

    public void SetBought()
    {
        lockedIcon.gameObject.SetActive(false);
        blackoutImage.enabled = true;
        soldOutText.gameObject.SetActive(true);
    }

    public void SetSelected()
    {
        if (!selectable || blackoutImage.enabled)     //Doesn't allow selected if greyed out (most likely due to out of gold)
            return;

        selected = !selected;
        outline.GetComponent<Outline>().enabled = selected;
        endSceneController.ReportItemBought(itemCost, thisName, amount, selected, index);
    }

    public void SetValues(StoryModeController.RewardsType newName, int newAmount, int cost, int index)
    {
        itemIcon.sprite = StoryModeController.story.GetRewardSprite(newName, index);
        itemIcon.color = StoryModeController.story.GetRewardsColor(newName);
        thisName = newName;
        thisCard = StoryModeController.story.GetCurrentRoomSetup().rewardCards[index];
        thisEquipment = StoryModeController.story.GetCurrentRoomSetup().rewardEquipment[index];
        amount = newAmount;

        if (newName == StoryModeController.RewardsType.SpecificCard)
        {
            rewardTypeText.text = "Card";
            itemName.text = thisCard.name + "\nx" + amount.ToString();
        }
        else if (newName == StoryModeController.RewardsType.SpecificEquipment)
        {
            rewardTypeText.text = "Gear";
            itemName.text = thisEquipment.equipmentName + "\nx" + amount.ToString();
        }
        else
        {
            rewardTypeText.text = "Material";
            itemName.text = thisName + "\nx" + amount.ToString();
        }
        costText.text = cost.ToString();
        itemCost = cost;
        if (index < 3)
            lockedAchievementText.text = StoryModeController.story.GetCurrentRoomSetup().GetChallengeText(StoryModeController.story.GetCurrentRoomID(), index);
    }

    public void SetGreyout(bool value)
    {
        if (selected && value == true)      //Always show item as selectable if the item is selected
            return;

        if (selectable)
            blackoutImage.enabled = value;
    }

    public StoryModeController.RewardsType GetName()
    {
        return thisName;
    }

    public Card GetCard()
    {
        return thisCard;
    }

    public Equipment GetEquipment()
    {
        return thisEquipment;
    }

    public int GetAmount()
    {
        return amount;
    }

    public bool GetSelected()
    {
        return selected;
    }
}
