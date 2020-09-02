using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditDeckButtonController : MonoBehaviour
{
    public void OnMouseDown()
    {
        CameraController.camera.transform.position = new Vector3(7, 0, -10);
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
