using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicDisplayExitButtonController : MonoBehaviour
{
    public void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        RelicDisplayController.relicDisplay.HideRelicDescriptionMenu();
    }
}
