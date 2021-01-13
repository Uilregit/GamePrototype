using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalizeButtonController : MonoBehaviour
{
    public Color enableColor;
    public Color disableColor;

    private void OnMouseDown()
    {
        CollectionController.collectionController.FinalizeDeck();
        try
        {
            CollectionController.collectionController.LogInformation();
        }
        catch { }
        CameraController.camera.transform.position = new Vector3(0, 0, -10);
    }

    public void Enable(bool state)
    {
        if (state)
            GetComponent<Image>().color = enableColor;
        else
            GetComponent<Image>().color = disableColor;
        GetComponent<Collider2D>().enabled = state;
    }
}