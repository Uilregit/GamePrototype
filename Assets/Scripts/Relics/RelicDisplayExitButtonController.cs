using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicDisplayExitButtonController : MonoBehaviour
{
    public void OnMouseDown()
    {
        RelicDisplayController.relicDisplay.HideRelicDescriptionMenu();
    }
}
