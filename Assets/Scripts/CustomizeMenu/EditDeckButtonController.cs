using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditDeckButtonController : MonoBehaviour
{
    public void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        MusicController.music.SetHighPassFilter(true);
        CameraController.camera.transform.position = new Vector3(100, 0, -10);
        CollectionController.collectionController.ResetDeck();
    }

    public void Hide()
    {
        GetComponent<Collider2D>().enabled = false;
    }

    public void Show()
    {
        GetComponent<Collider2D>().enabled = true;
    }
}
