using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RelicDisplayController : MonoBehaviour
{
    public static RelicDisplayController relicDisplay;

    [Header("Icons")]
    public List<Image> relicImages;

    [Header("Information Menu")]
    public Image back;
    public Text relicMenuText;
    public List<Image> relicIcons;
    public List<Text> relicTitles;
    public List<Text> relicDescriptions;

    private Vector2 originalRelicScale;

    // Start is called before the first frame update
    void Awake()
    {
        if (RelicDisplayController.relicDisplay == null)
            RelicDisplayController.relicDisplay = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        RefreshRelicDisplays();
        HideRelicDescriptionMenu();

        originalRelicScale = relicImages[0].transform.localScale;
    }

    public void RefreshRelicDisplays()
    {
        for (int i = 0; i < relicImages.Count; i++)
        {
            if (i < RelicController.relic.relics.Count)
            {
                relicImages[i].sprite = RelicController.relic.relics[i].art;
                relicImages[i].color = RelicController.relic.relics[i].color;
            }
            else
                relicImages[i].color = Color.clear;
        }
    }

    public void ShowRelicDescriptionMenu()
    {
        back.enabled = true;
        relicMenuText.enabled = true;
        for (int i = 0; i < relicIcons.Count; i++)
        {
            if (i < RelicController.relic.relics.Count)
            {
                relicIcons[i].sprite = RelicController.relic.relics[i].art;
                relicIcons[i].color = RelicController.relic.relics[i].color;
                relicIcons[i].enabled = true;
                relicTitles[i].text = RelicController.relic.relics[i].relicName;
                relicTitles[i].enabled = true;
                relicDescriptions[i].text = RelicController.relic.relics[i].description;
                relicDescriptions[i].enabled = true;
            }
        }
    }

    public void HideRelicDescriptionMenu()
    {
        back.enabled = false;
        relicMenuText.enabled = false;
        for (int i = 0; i < relicIcons.Count; i++)
        {
            relicIcons[i].enabled = false;
            relicTitles[i].enabled = false;
            relicDescriptions[i].enabled = false;
        }
    }

    public IEnumerator ShowRelicTrigger(int index)
    {
        relicImages[index].transform.localScale = originalRelicScale * 1.2f;
        relicImages[index].GetComponent<Outline>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        relicImages[index].transform.localScale = originalRelicScale;
        relicImages[index].GetComponent<Outline>().enabled = false;
    }
}
