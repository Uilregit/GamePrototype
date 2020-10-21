using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernPartyController : MonoBehaviour
{
    public int index;
    public TavernController tavern;

    public void OnClick()
    {
        tavern.StartEditing(index);
    }
}
