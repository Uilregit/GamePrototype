using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TavernController : MonoBehaviour
{
    public Image[] party;
    public Image[] reserves;
    public Text partyLevel;

    public Text sandText;
    public Text shardText;
    public Text contractText;

    public Canvas recruitCanvas;
    public Image recruitButton;
    public Sprite orangeCharacterSprite;
    public Sprite whiteCharacterSprite;
    public Sprite blackCharacterSprite;
    public Card[] orangeStartingCards;
    public Card[] whiteStartingCards;
    public Card[] blackStartingCards;
    private Dictionary<Card.CasterColor, Sprite> characterSprites;
    private Dictionary<Card.CasterColor, Card[]> characterCards;
    private Dictionary<Card.CasterColor, CardController[]> cardControllers;
    public Image characterSprite;
    public CardDisplay[] cards;
    public Image[] buttons;

    private int selectedIndex = -1;
    private Card.CasterColor recruitColor;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Image img in reserves)
        {
            img.enabled = false;
            img.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }

        for (int i = 0; i < PartyController.party.partyColors.Length; i++)
        {
            party[i].color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);
            party[i].transform.GetChild(0).GetComponent<Text>().text = "Lv." + PartyController.party.GetPartyLevelInfo(PartyController.party.partyColors[i])[0].ToString();
        }
        partyLevel.text = "Lv." + ScoreController.score.teamLevel.ToString();

        Unlocks unlocked = UnlocksController.unlock.GetUnlocks();
        if (unlocked.sand > 0)
        {
            sandText.text = "Sand: " + unlocked.sand;
            sandText.transform.parent.GetComponent<Image>().enabled = true;
        }
        else
        {
            sandText.text = "";
            sandText.transform.parent.GetComponent<Image>().enabled = false;
        }
        if (unlocked.shards > 0)
        {
            shardText.text = "Shards: " + unlocked.shards;
            shardText.transform.parent.GetComponent<Image>().enabled = true;
        }
        else
        {
            shardText.text = "";
            shardText.transform.parent.GetComponent<Image>().enabled = false;
        }
        if (unlocked.tavernContracts > 0)
        {
            contractText.text = "Contracts: " + unlocked.tavernContracts;
            contractText.transform.parent.GetComponent<Image>().enabled = true;
            recruitButton.enabled = true;
            recruitButton.GetComponent<Collider2D>().enabled = true;
            recruitButton.transform.GetChild(0).GetComponent<Text>().enabled = true;
        }
        else
        {
            contractText.text = "";
            contractText.transform.parent.GetComponent<Image>().enabled = false;
            recruitButton.enabled = false;
            recruitButton.GetComponent<Collider2D>().enabled = false;
            recruitButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }

        characterSprites = new Dictionary<Card.CasterColor, Sprite>();
        characterCards = new Dictionary<Card.CasterColor, Card[]>();
        characterSprites[Card.CasterColor.Orange] = orangeCharacterSprite;
        characterSprites[Card.CasterColor.White] = whiteCharacterSprite;
        characterSprites[Card.CasterColor.Black] = blackCharacterSprite;
        characterCards[Card.CasterColor.Orange] = orangeStartingCards;
        characterCards[Card.CasterColor.White] = whiteStartingCards;
        characterCards[Card.CasterColor.Black] = blackStartingCards;
        cardControllers = new Dictionary<Card.CasterColor, CardController[]>();
    }

    public void StartEditing(int index)
    {
        selectedIndex = index;

        int i = 0;
        foreach (Card.CasterColor c in PartyController.party.unlockedPlayerColors)
            if (!PartyController.party.partyColors.Contains(c))
            {
                reserves[i].color = PartyController.party.GetPlayerColor(c);
                reserves[i].GetComponent<TavernIconsController>().SetColor(c);
                reserves[i].enabled = true;
                reserves[i].transform.GetChild(0).GetComponent<Text>().text = "Lv." + PartyController.party.GetPartyLevelInfo(c)[0].ToString();
                reserves[i].transform.GetChild(0).GetComponent<Text>().enabled = true;
                i++;
            }
    }

    public void ReportSelected(Card.CasterColor newColor)
    {
        party[selectedIndex].color = PartyController.party.GetPlayerColor(newColor);
        party[selectedIndex].transform.GetChild(0).GetComponent<Text>().text = "Lv." + PartyController.party.GetPartyLevelInfo(newColor)[0].ToString();
        PartyController.party.partyColors[selectedIndex] = newColor;

        foreach (Image img in reserves)
        {
            img.enabled = false;
            img.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
    }

    public void ReportRecruitSelected(Card.CasterColor newColor)
    {
        characterSprite.sprite = characterSprites[newColor];
        cardControllers[newColor] = new CardController[3];
        for (int j = 0; j < characterCards[newColor].Length; j++)
        {
            cardControllers[newColor][j] = new CardController();
            cardControllers[newColor][j].SetCardDisplay(cards[j]);
            cardControllers[newColor][j].SetCard(characterCards[newColor][j], false);
            cards[j].SetCard(cardControllers[newColor][j], false);
        }

        recruitColor = newColor;
    }

    public void GoToRecruitingMenu()
    {
        GetComponent<Canvas>().enabled = false;
        recruitCanvas.enabled = true;
        recruitCanvas.GetComponent<CanvasScaler>().enabled = false;
        recruitCanvas.GetComponent<CanvasScaler>().enabled = true;

        int i = 0;
        foreach (Card.CasterColor c in PartyController.party.potentialPlayerColors)
            if (!PartyController.party.unlockedPlayerColors.Contains(c))
            {
                buttons[i].GetComponent<RecruitButtonController>().SetEnable(true);
                buttons[i].GetComponent<RecruitButtonController>().SetColor(c);
                characterSprite.sprite = characterSprites[c];
                cardControllers[c] = new CardController[3];
                for (int j = 0; j < characterCards[c].Length; j++)
                {
                    cardControllers[c][j] = new CardController();
                    cardControllers[c][j].SetCardDisplay(cards[j]);
                    cardControllers[c][j].SetCard(characterCards[c][j], false);
                    cards[j].SetCard(cardControllers[c][j], false);
                }
                recruitColor = c;
                i++;
            }
        for (int j = i; j < 3; j++)
            buttons[i].GetComponent<RecruitButtonController>().SetEnable(false);
    }

    public void GoToTavernMenu()
    {
        recruitCanvas.enabled = false;
        GetComponent<Canvas>().enabled = true;

        PartyController.party.unlockedPlayerColors.Add(recruitColor);

        Unlocks unlocked = UnlocksController.unlock.GetUnlocks();
        unlocked.tavernContracts -= 1;
        switch (recruitColor)
        {
            case Card.CasterColor.Orange:
                unlocked.orangeUnlocked = true;
                break;
            case Card.CasterColor.White:
                unlocked.whiteUnlocked = true;
                break;
            case Card.CasterColor.Black:
                unlocked.blackUnlocked = true;
                break;
        }

        UnlocksController.unlock.SetUnlocks(unlocked);
        InformationLogger.infoLogger.SaveUnlocks();

        if (unlocked.tavernContracts > 0)
        {
            contractText.text = "Contracts: " + unlocked.tavernContracts;
            contractText.transform.parent.GetComponent<Image>().enabled = true;
            recruitButton.enabled = true;
            recruitButton.GetComponent<Collider2D>().enabled = true;
            recruitButton.transform.GetChild(0).GetComponent<Text>().enabled = true;
        }
        else
        {
            contractText.text = "";
            contractText.transform.parent.GetComponent<Image>().enabled = false;
            recruitButton.enabled = false;
            recruitButton.GetComponent<Collider2D>().enabled = false;
            recruitButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
        InformationLogger.infoLogger.SavePlayerPreferences();
    }

    public void ResetLevelInfo()
    {
        foreach (Card.CasterColor color in PartyController.party.potentialPlayerColors)
            PartyController.party.partyLevelInfo[color] = new List<int>() { 0, 0 };

        PartyController.party.SetPlayerColors(new string[] { "Red", "Blue", "Green" });
        for (int i = 0; i < PartyController.party.partyColors.Length; i++)
        {
            party[i].color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);
            party[i].transform.GetChild(0).GetComponent<Text>().text = "Lv.0";
        }

        ScoreController.score.teamLevel = 0;
        ScoreController.score.currentEXP = 0;
        partyLevel.text = "Lv.0";

        sandText.text = "";
        sandText.transform.parent.GetComponent<Image>().enabled = false;
        shardText.text = "";
        shardText.transform.parent.GetComponent<Image>().enabled = false;
        contractText.text = "";
        contractText.transform.parent.GetComponent<Image>().enabled = false;

        UnlocksController.unlock.ResetUnlocks();
        InformationLogger.infoLogger.SavePlayerPreferences();
    }
}
