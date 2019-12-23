using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public static CanvasController canvasController;
    public Canvas boardCanvas;
    public Canvas uiCanvas;

    // Start is called before the first frame update
    void Awake()
    {
        if (CanvasController.canvasController == null)
            CanvasController.canvasController = this;
        else
            Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
