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

    public BattlePassController teamBattlePass;
    public BattlePassController heroBattlePass;

    public Image background;

    public Text selectedColorClass;
    public Text selectedColorName;
    public Text recruitName;

    public Text selectedColorATK;
    public Text selectedColorDef;
    public Text selectedColorHlth;

    public SpriteRenderer selectedColorSprite;

    public GameObject reserveObject;

    public Canvas recruitCanvas;
    public Image recruitButton;
    public GameObject recruitInfo;
    public Image recruitBackground;
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

    private int selectedCardIndex = 0;
    private Vector2 selectedCardOriginalPosition;
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
            recruitButton.gameObject.SetActive(true);
        }
        else
        {
            contractText.text = "";
            contractText.transform.parent.GetComponent<Image>().enabled = false;
            recruitButton.gameObject.SetActive(false);
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

        teamBattlePass.SetBattlePass(Card.CasterColor.Enemy);
        heroBattlePass.SetBattlePass(PartyController.party.partyColors[0]);


        int counter = 0;
        foreach (Card.CasterColor c in PartyController.party.unlockedPlayerColors)
            if (!PartyController.party.partyColors.Contains(c))
            {
                reserves[counter].gameObject.SetActive(false);
                counter++;
            }

        reserveObject.SetActive(false);

        SetSelectedChar(PartyController.party.partyColors[0]);
    }

    public void StartEditing(int index)
    {
        selectedIndex = index;

        int reserveNumber = PartyController.party.unlockedPlayerColors.Count - 3;
        if (reserveNumber > 0)
        {
            int i = 0;
            foreach (Card.CasterColor c in PartyController.party.unlockedPlayerColors)
                if (!PartyController.party.partyColors.Contains(c))
                {
                    reserves[i].color = PartyController.party.GetPlayerColor(c);
                    reserves[i].GetComponent<TavernIconsController>().SetColor(c);
                    reserves[i].gameObject.SetActive(true);

                    float reserveSize = 5.0f / (reserveNumber);
                    reserves[i].transform.localScale = new Vector3(reserveSize, reserves[i].transform.localScale.y, 1);
                    reserves[i].transform.position = new Vector2(-reserveSize * (reserveNumber - 1) / 2 + reserveSize * i, reserves[i].transform.position.y);

                    i++;
                }

            if (heroBattlePass != null)
                heroBattlePass.SetBattlePass(PartyController.party.partyColors[index]);

            reserveObject.GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[index]);
            reserveObject.SetActive(true);
        }
        SetSelectedChar(PartyController.party.partyColors[index]);
    }

    public void ReportSelected(Card.CasterColor newColor)
    {
        party[selectedIndex].color = PartyController.party.GetPlayerColor(newColor);
        //party[selectedIndex].transform.GetChild(0).GetComponent<Text>().text = "Lv." + PartyController.party.GetPartyLevelInfo(newColor)[0].ToString();
        PartyController.party.partyColors[selectedIndex] = newColor;

        foreach (Image img in reserves)
        {
            img.gameObject.SetActive(false);
            img.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }

        if (heroBattlePass != null)
            heroBattlePass.SetBattlePass(newColor);

        reserveObject.SetActive(false);

        SetSelectedChar(newColor);
    }

    private void SetSelectedChar(Card.CasterColor color)
    {
        selectedColorName.text = PartyController.party.GetPlayerName(color);

        selectedColorSprite.sprite = PartyController.party.GetPlayerSprite(color);

        selectedColorATK.text = PartyController.party.GetStartingAttack(color).ToString();
        selectedColorDef.text = PartyController.party.GetStartingArmor(color).ToString();
        selectedColorHlth.text = PartyController.party.GetStartingHealth(color).ToString();

        background.color = PartyController.party.GetPlayerColor(color);
    }

    public void ReportRecruitSelected(Card.CasterColor newColor)
    {
        recruitName.text = PartyController.party.GetPlayerName(newColor);
        characterSprite.sprite = characterSprites[newColor];
        cardControllers[newColor] = new CardController[3];
        for (int j = 0; j < characterCards[newColor].Length; j++)
        {
            cardControllers[newColor][j] = new CardController();
            cardControllers[newColor][j].SetCardDisplay(cards[j]);
            cardControllers[newColor][j].SetCard(characterCards[newColor][j], false);
            cards[j].SetCard(cardControllers[newColor][j], false);
        }

        recruitBackground.color = PartyController.party.GetPlayerColor(newColor);
        recruitInfo.SetActive(true);

        recruitColor = newColor;
    }

    public void GoToRecruitingMenu()
    {
        GetComponent<Canvas>().enabled = false;
        selectedColorSprite.enabled = false;

        recruitCanvas.gameObject.SetActive(true);
        recruitCanvas.GetComponent<CanvasScaler>().enabled = false;
        recruitCanvas.GetComponent<CanvasScaler>().enabled = true;

        teamBattlePass.GetComponent<Canvas>().enabled = false;
        heroBattlePass.GetComponent<Canvas>().enabled = false;

        int i = 0;
        int recruitNumber = PartyController.party.potentialPlayerColors.Length - PartyController.party.unlockedPlayerColors.Count;
        Debug.Log(recruitNumber);
        foreach (Card.CasterColor c in PartyController.party.potentialPlayerColors)
            if (!PartyController.party.unlockedPlayerColors.Contains(c))
            {
                buttons[i].GetComponent<RecruitButtonController>().SetEnable(true);
                buttons[i].GetComponent<RecruitButtonController>().SetColor(c);

                float recruitSize = 5.0f / (recruitNumber);
                buttons[i].transform.localScale = new Vector3(recruitSize, buttons[i].transform.localScale.y, 1);
                buttons[i].transform.position = new Vector2(-recruitSize * (recruitNumber - 1) / 2 + recruitSize * i, buttons[i].transform.position.y);

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
        recruitInfo.SetActive(false);
        recruitCanvas.gameObject.SetActive(false);
        GetComponent<Canvas>().enabled = true;
        selectedColorSprite.enabled = true;

        PartyController.party.unlockedPlayerColors.Add(recruitColor);

        //teamBattlePass.GetComponent<Canvas>().enabled = true;
        //heroBattlePass.GetComponent<Canvas>().enabled = true;

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
            recruitButton.gameObject.SetActive(true);
        }
        else
        {
            contractText.text = "";
            contractText.transform.parent.GetComponent<Image>().enabled = false;
            recruitButton.gameObject.SetActive(false);
        }
    }

    public void GoToMainMenu()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(false);
        if (StoryModeController.story != null)
            SceneManager.LoadScene("StoryModeScene", LoadSceneMode.Single);
        else
            SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
        InformationLogger.infoLogger.SavePlayerPreferences();
    }

    public void ResetLevelInfo()
    {
        UnlocksController.unlock.ResetUnlocks();
        InformationLogger.infoLogger.SavePlayerPreferences();

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

        if (UnlocksController.unlock.GetUnlocks().tavernContracts <= 0)
        {
            recruitButton.enabled = false;
            recruitButton.GetComponent<Collider2D>().enabled = false;
            recruitButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }

        teamBattlePass.SetBattlePass(Card.CasterColor.Enemy);
        heroBattlePass.SetBattlePass(PartyController.party.partyColors[0]);

        InformationLogger.infoLogger.SaveGame(true);
    }

    public void SelectCardForHighlight(int index)
    {
        selectedCardIndex = index;
        selectedCardOriginalPosition = cards[index].transform.position;

        cards[index].transform.position = new Vector3(0, 1, 0);
        cards[index].transform.localScale = new Vector3(2, 2, 2);
    }

    public void DeselectCardForHighlight()
    {
        cards[selectedCardIndex].transform.position = selectedCardOriginalPosition;
        cards[selectedCardIndex].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    }
}
