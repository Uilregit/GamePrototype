using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController: MonoBehaviour
{
    public static UIController ui;

    public Text drawPileCount;
    public Text discardPileCount;

    // Start is called before the first frame update
    void Awake()
    {
        if (UIController.ui == null)
            UIController.ui = this;
        else
            Destroy(this.gameObject);
    }

    public void ResetPileCounts(int drawPile, int discardPile)
    {
        drawPileCount.text = drawPile.ToString();
        discardPileCount.text = discardPile.ToString();
    }
}
