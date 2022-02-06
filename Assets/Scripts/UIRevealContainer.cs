using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRevealContainer : MonoBehaviour
{
    public List<UIRevealController.UIElement> elements;
    public List<GameObject> objects;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < elements.Count; i++)
            UIRevealController.UIReveal.ReportElement(elements[i], objects[i]);
    }
}
