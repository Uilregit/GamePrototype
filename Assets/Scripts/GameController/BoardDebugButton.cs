using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardDebugButton : MonoBehaviour
{
    private Image image;
    private BoxCollider2D bc2d;

    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<Image>();
        bc2d = GetComponent<BoxCollider2D>();

        image.enabled = InformationLogger.infoLogger.debug;
        bc2d.enabled = InformationLogger.infoLogger.debug;
    }

    private void OnMouseDown()
    {
        GridController.gridController.DebugGrid();
    }
}
