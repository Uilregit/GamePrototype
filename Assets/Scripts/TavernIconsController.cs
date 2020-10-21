using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernIconsController : MonoBehaviour
{
    public Card.CasterColor color;
    public TavernController tavern;
    
    public void SetColor(Card.CasterColor value)
    {
        color = value;
    }

    public void ReportClickeD()
    {
        tavern.ReportSelected(color);
    }
}
