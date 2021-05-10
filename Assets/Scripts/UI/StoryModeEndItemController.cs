using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryModeEndItemController : MonoBehaviour
{
    public Image lockedIcon;
    public Text soldOutText;
    public Image blackoutImage;
    public Image outline;
    public Text itemName;
    public Text costText;

    public StoryModeEndSceenController endSceneController;

    private int itemCost;
    private bool selected = false;

    private bool selectable = false;

    public void SetEnabled(bool state)
    {
        lockedIcon.enabled = !state;
        blackoutImage.enabled = !state;
        soldOutText.enabled = false;

        selectable = state;
    }

    public void SetBought()
    {
        lockedIcon.enabled = false;
        blackoutImage.enabled = true;
        soldOutText.enabled = true;
    }

    public void SetSelected()
    {
        if (!selectable || blackoutImage.enabled)     //Doesn't allow selected if greyed out (most likely due to out of gold)
            return;

        selected = !selected;
        outline.GetComponent<Outline>().enabled = selected;
        endSceneController.ReportItemBought(itemCost, selected);
    }

    public void SetValues(string name, int cost)
    {
        itemName.text = name;
        costText.text = cost.ToString();
        itemCost = cost;
    }

    public void SetGreyout(bool value)
    {
        if (selected && value == true)      //Always show item as selectable if the item is selected
            return;

        if (selectable)
            blackoutImage.enabled = value;
    }
}
