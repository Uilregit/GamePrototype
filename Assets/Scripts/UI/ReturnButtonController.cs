using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnButtonController : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        CharacterInformationController.charInfoController.Hide();
    }
}
