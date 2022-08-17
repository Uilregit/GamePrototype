using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceController : MonoBehaviour
{
    public static ResourceController resource;

    [Header("Gold Gain Settings")]
    public int goldGainPerCombat;
    public int goldGainPerBoss;

    [Header("Shop Settings")]
    public int commonCardPrice;
    public int rareCardPrice;
    public int commonEquipmentPrice;
    public int rareEquipmentPrice;

    public float commonShopPercentage;
    public float rareShopPercentage;
    public float shopEquipmentPercentage;
    [Header("Gold UI Settings")]
    public Text goldCount;

    [Header("Lives UI Settings")]
    public Text livesCount;
    private int lives = 0;
    private int numberOfRevievesUsed = 0;

//public Text goldText;
//public Image goldUIBack;
[SerializeField] private Canvas canvas;

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

    public void EnableStoryModeRelicsMenu(bool state)
    {
        goldCount.transform.parent.gameObject.SetActive(false);
        livesCount.transform.parent.gameObject.SetActive(false);
        canvas.enabled = state;
        canvas.GetComponent<CanvasScaler>().enabled = false;
        canvas.GetComponent<CanvasScaler>().enabled = true;
        RelicDisplayController.relicDisplay.SetRelicsToStoryMode(state);
    }

    public void ChangeGold(int value)
    {
        gold += value;
        goldCount.text = gold.ToString();

        if (StoryModeController.story != null)
            StoryModeController.story.RefreshGoldValue();
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
        if (StoryModeController.story != null)
            return 1;
        return lives;
    }

    public void ReportReviveUsed()
    {
        numberOfRevievesUsed += 1;
    }

    public int GetNumberOfRevivesUsed()
    {
        return numberOfRevievesUsed;
    }

    public void ResetReviveUsed()
    {
        numberOfRevievesUsed = 0;
    }
}
