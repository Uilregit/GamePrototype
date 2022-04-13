using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LetterBoxer : MonoBehaviour
{
    public float x = 16;
    public float y = 9;
    public bool onAwake = true;
    public bool onUpdate = true;

    private AspectRatioFitter aspecFitter;

    public void Awake()
    {
        aspecFitter = GetComponent<AspectRatioFitter>();

        // perform sizing if onAwake is set
        if (onAwake)
        {
            PerformSizing();
        }
    }

    public void Update()
    {
        // perform sizing if onUpdate is set
        if (onUpdate)
        {
            PerformSizing();
        }
    }

    private void OnValidate()
    {
        x = Mathf.Max(1, x);
        y = Mathf.Max(1, y);
    }

    private void PerformSizing()
    {
        // calc based on aspect ratio
        float targetRatio = x / y;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        if (windowaspect > targetRatio)
            aspecFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        else
            aspecFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
    }
}