using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

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
    public CardDisplay displayCard;

    public StoryModeEndSceenController endSceneController;

    private int itemCost;
    private bool selected = false;

    public bool selectable = false;

    private StoryModeController.RewardsType thisName;
    private Card thisCard;
    private Equipment thisEquipment;
    private int amount;

    private bool held = false;
    private float heldTimer = 0;
    private Coroutine holdRoutine;

    private void Update()
    {
        if (held)
            heldTimer += Time.deltaTime;
    }

    public void SetEnabled(bool state)
    {
        if (index < 3 && SceneManager.GetActiveScene().name == "StoryModeEndScene")
            lockedIcon.gameObject.SetActive(!state);
        else
            lockedIcon.gameObject.SetActive(false);
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

    public void OnMouseDown()
    {
        held = true;
        heldTimer = 0;
        if (thisName == StoryModeController.RewardsType.SpecificCard || thisName == StoryModeController.RewardsType.SpecificEquipment)
            holdRoutine = StartCoroutine(ShowDisplayCard());
    }

    public void OnMouseUp()
    {
        displayCard.gameObject.SetActive(false);
        if (holdRoutine != null)
            StopCoroutine(holdRoutine);
        held = false;
        if (heldTimer < 0.3f)
            SetSelected();
    }

    private IEnumerator ShowDisplayCard()
    {
        yield return new WaitForSeconds(0.3f);

        if (thisName == StoryModeController.RewardsType.SpecificCard)
        {
            CardController c = this.gameObject.AddComponent<CardController>();
            c.SetCardDisplay(displayCard);
            c.SetCard(thisCard, false, true, false);
            displayCard.SetCard(c);
        }
        else if (thisName == StoryModeController.RewardsType.SpecificEquipment)
            displayCard.SetEquipment(thisEquipment, Card.CasterColor.Gray);
        displayCard.SetHighLight(true);
        displayCard.gameObject.SetActive(true);
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
        if (SceneManager.GetActiveScene().name == "StoryModeEndScene")
        {
            itemIcon.sprite = StoryModeController.story.GetRewardSprite(newName, index);
            itemIcon.color = StoryModeController.story.GetRewardsColor(newName);
        }
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
            if (SceneManager.GetActiveScene().name == "StoryModeEndScene")
            {
                itemName.text = thisName + "\nx" + amount.ToString();
                rewardTypeText.text = "Material";
            }
            else                    //If it's a secret shop item, use different rules for setting item name
            {
                itemName.text = thisName.ToString().Replace("Plus", "+").Replace("X", amount.ToString());
                itemName.text = string.Concat(itemName.text.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '); ;
                rewardTypeText.text = "Passive";
            }
        }
        costText.text = cost.ToString();
        itemCost = cost;
        if (index < 3 && SceneManager.GetActiveScene().name == "StoryModeEndScene")
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
