using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffDescriptionController : MonoBehaviour
{
    public Image background;
    public Image icon;
    public Text text;

    public void SetBuff(Buff buff, int duration)
    {
        icon.color = buff.GetIconColor();
        Debug.Log(buff.GetDescription());
        if (duration == 1)
            text.text = buff.GetDescription().Replace("{d}", duration.ToString()).Replace("(s)", "");
        else
            text.text = buff.GetDescription().Replace("{d}", duration.ToString()).Replace("(s)", "s");
    }

    public void Show()
    {
        background.enabled = true;
        icon.enabled = true;
        text.enabled = true;
    }

    public void Hide()
    {
        background.enabled = false;
        icon.enabled = false;
        text.enabled = false;
    }
}
