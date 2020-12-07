using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitButtonController : MonoBehaviour
{
    public TavernController tavern;

    private Card.CasterColor color;

    public void OnClick()
    {
        tavern.ReportRecruitSelected(color);
    }

    public void SetColor(Card.CasterColor c)
    {
        color = c;
        GetComponent<Image>().color = PartyController.party.GetPlayerColor(c);
    }

    public void SetEnable(bool state)
    {
        GetComponent<Image>().enabled = state;
        GetComponent<Collider2D>().enabled = state;
    }
}
