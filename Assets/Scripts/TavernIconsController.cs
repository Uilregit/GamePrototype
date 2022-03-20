using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TavernIconsController : MonoBehaviour
{
    public Card.CasterColor color;
    public TavernController tavern;

    public void SetColor(Card.CasterColor value)
    {
        color = value;
        GetComponent<Image>().enabled = true;
    }

    public virtual void ReportClickeD()
    {
        tavern.ReportSelected(color);
    }
}
