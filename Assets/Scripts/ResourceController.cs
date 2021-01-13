using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceController : MonoBehaviour
{
    public static ResourceController resource;

    [Header("Gold Gain Settings")]
    public int goldGainPerCombat;

    [Header("Shop Settings")]
    public int commonCardPrice;
    public int rareCardPrice;

    public float commonShopPercentage;
    public float rareShopPercentage;

    [Header("Gold UI Settings")]
    public Text goldCount;

    [Header("Lives UI Settings")]
    public Text livesCount;
    private int lives = 0;
    //public Text goldText;
    //public Image goldUIBack;
    public Canvas canvas;

    private int gold;
    // Start is called before the first frame update
    void Awake()
    {
        if (ResourceController.resource == null)
            ResourceController.resource = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(canvas);
        //DontDestroyOnLoad(goldText);
        //DontDestroyOnLoad(goldUIBack);
    }

    public void ChangeGold(int value)
    {
        gold += value;
        goldCount.text = gold.ToString();
    }

    public int GetGold()
    {
        return gold;
    }

    public void LoadGold(int value)
    {
        gold = value;
        goldCount.text = gold.ToString();
    }

    public void LoadLives(int value)
    {
        livesCount.text = value.ToString();
        lives = value;
    }

    public int GetLives()
    {
        return lives;
    }
}
