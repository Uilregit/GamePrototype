using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreDisplayController : MonoBehaviour
{
    public Canvas expBarCanvas;
    public EXPBarController teamEXPBar;
    public Color teamEXPBarColor;
    public EXPBarController[] heroEXPBar;
    public Image continueButton;
    public Image mainMenuButton;

    public Canvas cardPackCanvas;
    public Canvas cardSingleCanvas;
    public Canvas relicRewardCanvas;
    public Canvas individualUnlockCanvas;
    public Canvas currencyCanvas;

    public Color normalScoreColor;
    public Color highScoreColor;

    private bool isOnMenu = false;
    private Canvas currentCanvas;

    public List<string> scoreItems = new List<string>()
    {   "Overkill (" ,
        "Damage Dealt (",
        "Armor Protected (",
        "Overheal Protected (",
        "Damage Avoided (",
        "Enemies Broken (",
        "Gold Used (",
        "Bosses Defeated (",
        "Speed Bonus ("
    };

    public List<int> scores = new List<int>();

    public List<Text> scoreItemTexts = new List<Text>();
    public Text Total;

    public List<Text> scoreTexts = new List<Text>();
    public Text TotalScore;

    public List<Text> highScoreTexts = new List<Text>();
    public Text TotalHighScore;

    public List<Text> newBestTexts = new List<Text>();
    public Text TotalNewBest;

    private int scoreTotal;

    // Start is called before the first frame update
    void Start()
    {
        ScoreController.score.timerPaused = true;
        scoreTotal = 0;

        scores = new List<int>()
        {
            ScoreController.score.GetOverKill(),
            ScoreController.score.GetDamage(),
            ScoreController.score.GetDamageArmored(),
            ScoreController.score.GetDamageOverhealProtected(),
            ScoreController.score.GetDamageAvoided(),
            ScoreController.score.GetEnemiesBroken(),
            ScoreController.score.GetGoldUsed(),
            ScoreController.score.GetBossesDefeated(),
            (int)ScoreController.score.GetSecondsInGame()
        };

        List<int> scoreMultiplier = new List<int>()
        {
            ScoreController.score.scorePerOverkill,
            ScoreController.score.scorePerDamage,
            ScoreController.score.scorePerDamageArmored,
            ScoreController.score.scorePerDamageOverhealedProtected,
            ScoreController.score.scorePerDamageAvoided,
            ScoreController.score.scorePerEnemiesBroken,
            ScoreController.score.scorePerGoldUsed,
            ScoreController.score.scorePerBossesDefeated,
            ScoreController.score.scorePerSecondsInGame
        };

        for (int i = 0; i < scoreItems.Count; i++)
        {
            if (i != 8)
                scoreItemTexts[i].text = scoreItems[i] + scores[i].ToString() + ")";
            else
                scoreItemTexts[i].text = scoreItems[i] + (scores[i] / 60).ToString() + " mins)";

            scoreTexts[i].text = (scores[i] * scoreMultiplier[i]).ToString();
            highScoreTexts[i].text = ScoreController.score.highestScores[i].ToString();
            if (ScoreController.score.highestScores[i] == 0)
                highScoreTexts[i].enabled = false;
            scoreTotal += scores[i] * scoreMultiplier[i];

            if (scores[i] > ScoreController.score.highestScores[i])
            {
                scoreTexts[i].color = highScoreColor;
                highScoreTexts[i].color = normalScoreColor;
                newBestTexts[i].enabled = true;
                if (i != 8 || (scores[7] == 2 && scores[8] / 60 >= 30))
                    ScoreController.score.highestScores[i] = scores[i];
            }
            else
            {
                scoreTexts[i].color = normalScoreColor;
                newBestTexts[i].enabled = false;
            }

            if (i == 7 && scores[7] == 0)
            {
                scoreItemTexts[i].text = "";
                scoreTexts[i].text = "";
                scoreTotal -= scores[i] * scoreMultiplier[i];
                newBestTexts[i].enabled = false;
                highScoreTexts[i].enabled = false;
            }
            else if (i == 7)
            {
                scoreTexts[i].text = (ScoreController.score.GetBossesDefeated() * ScoreController.score.scorePerBossesDefeated).ToString();
            }
            if (i == 8 && (scores[7] < 2 || scores[8] / 60 >= 30))
            {
                scoreItemTexts[i].text = "";
                scoreTexts[i].text = "";
                scoreTotal -= scores[i] * scoreMultiplier[i];
                newBestTexts[i].enabled = false;
                highScoreTexts[i].enabled = false;
            }
            else if (i == 8)
            {
                scoreTotal -= scores[i] * scoreMultiplier[i];
                scoreTexts[i].text = (Mathf.Max(0, (30 - (int)ScoreController.score.GetSecondsInGame() / 60)) * ScoreController.score.scorePerSecondsInGame).ToString();
                scoreTotal += Mathf.Max(0, (30 - (int)ScoreController.score.GetSecondsInGame() / 60)) * ScoreController.score.scorePerSecondsInGame;
            }
        }
        TotalScore.text = scoreTotal.ToString();
        if (ScoreController.score.highestTotalScore == 0)
            TotalHighScore.enabled = false;
        if (scoreTotal > ScoreController.score.highestTotalScore)
        {
            TotalScore.color = highScoreColor;
            TotalNewBest.enabled = true;
            ScoreController.score.highestTotalScore = scoreTotal;
        }
        else
        {
            TotalScore.color = normalScoreColor;
            TotalHighScore.text = ScoreController.score.highestTotalScore.ToString();
            TotalNewBest.enabled = false;
        }

        InformationLogger.infoLogger.SaveGame(true);

        for (int i = UnlocksController.unlock.GetUnlocks().largestBoss; i < ScoreController.score.GetBossesDefeated(); i++)
        {
            UnlocksController.UnlockedQueue q = new UnlocksController.UnlockedQueue();
            q.level = i + 1;
            q.casterColor = Card.CasterColor.Enemy;
            q.type = UnlocksController.UnlockTypes.Contract;
            UnlocksController.unlock.queue.Add(q);
        }

        Unlocks unlock = UnlocksController.unlock.GetUnlocks();
        unlock.totalEXP += scoreTotal;
        unlock.largestBoss = ScoreController.score.GetBossesDefeated();
        UnlocksController.unlock.SetUnlocks(unlock);
        InformationLogger.infoLogger.SaveUnlocks();

        if (!InformationLogger.infoLogger.debug)
        {
            InformationLogger.infoLogger.SavePlayerPreferences();
            InformationLogger.infoLogger.SaveGameScoreInfo(InformationLogger.infoLogger.patchID,
                                    InformationLogger.infoLogger.gameID,
                                    RoomController.roomController.selectedLevel.ToString(),
                                    RoomController.roomController.roomName,
                                    (ScoreController.score.GetBossesDefeated() == 2).ToString(),
                                    (ScoreController.score.GetBossesDefeated() != 2).ToString(),
                                    scoreTotal.ToString(),
                                    ScoreController.score.GetOverKill().ToString(),
                                    ScoreController.score.GetDamage().ToString(),
                                    ScoreController.score.GetDamageArmored().ToString(),
                                    ScoreController.score.GetDamageOverhealProtected().ToString(),
                                    ScoreController.score.GetDamageAvoided().ToString(),
                                    ScoreController.score.GetEnemiesBroken().ToString(),
                                    ScoreController.score.GetGoldUsed().ToString(),
                                    ScoreController.score.GetBossesDefeated().ToString(),
                                    ((int)ScoreController.score.GetSecondsInGame()).ToString());
        }
    }

    public void ShowHeroExpBars()
    {
        continueButton.enabled = false;
        continueButton.GetComponent<Collider2D>().enabled = false;
        continueButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        mainMenuButton.enabled = false;
        mainMenuButton.GetComponent<Collider2D>().enabled = false;
        mainMenuButton.transform.GetChild(0).GetComponent<Text>().enabled = false;

        GetComponent<Canvas>().enabled = false;
        expBarCanvas.enabled = true;
        expBarCanvas.GetComponent<CanvasScaler>().enabled = false;
        expBarCanvas.GetComponent<CanvasScaler>().enabled = true;
        for (int i = 0; i < PartyController.party.partyColors.Length; i++)
        {
            heroEXPBar[i].SetEnabled(true);
            heroEXPBar[i].SetScoreDisplayController(this);
            heroEXPBar[i].SetValues(PartyController.party.GetPartyLevelInfo(PartyController.party.partyColors[i])[0], PartyController.party.GetPartyLevelInfo(PartyController.party.partyColors[i])[1], false, PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]), PartyController.party.partyColors[i]);
            heroEXPBar[i].StartCoroutine(heroEXPBar[i].GainEXP(scoreTotal));
            heroEXPBar[i].SetEnabled(true);
        }
        teamEXPBar.SetEnabled(false);
    }

    public void ReportBarDone(bool isTeamBar)
    {
        if (isTeamBar)
        {
            continueButton.enabled = true;
            continueButton.GetComponent<Collider2D>().enabled = true;
            continueButton.transform.GetChild(0).GetComponent<Text>().enabled = true;
            mainMenuButton.enabled = false;
            mainMenuButton.GetComponent<Collider2D>().enabled = false;
            mainMenuButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
        else
        {
            continueButton.enabled = false;
            continueButton.GetComponent<Collider2D>().enabled = false;
            continueButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
            mainMenuButton.enabled = true;
            mainMenuButton.GetComponent<Collider2D>().enabled = true;
            mainMenuButton.transform.GetChild(0).GetComponent<Text>().enabled = true;
        }
    }

    public void ShowTeamEXPBars()
    {
        continueButton.enabled = false;
        continueButton.GetComponent<Collider2D>().enabled = false;
        continueButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        mainMenuButton.enabled = false;
        mainMenuButton.GetComponent<Collider2D>().enabled = false;
        mainMenuButton.transform.GetChild(0).GetComponent<Text>().enabled = false;

        GetComponent<Canvas>().enabled = false;
        expBarCanvas.enabled = true;
        expBarCanvas.GetComponent<CanvasScaler>().enabled = false;
        expBarCanvas.GetComponent<CanvasScaler>().enabled = true;
        teamEXPBar.SetScoreDisplayController(this);
        teamEXPBar.SetValues(ScoreController.score.teamLevel, ScoreController.score.currentEXP, true, teamEXPBarColor, Card.CasterColor.Enemy);
        teamEXPBar.StartCoroutine(teamEXPBar.GainEXP(scoreTotal));
        teamEXPBar.SetEnabled(true);
        for (int i = 0; i < PartyController.party.partyColors.Length; i++)
            heroEXPBar[i].SetEnabled(false);
    }

    public void ShowUnlocks(bool isTeam)
    {
        continueButton.enabled = false;
        continueButton.GetComponent<Collider2D>().enabled = false;
        continueButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        mainMenuButton.enabled = false;
        mainMenuButton.GetComponent<Collider2D>().enabled = false;
        mainMenuButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        StartCoroutine(ShowUnlockedCanvases(isTeam));
    }

    public IEnumerator ShowUnlockedCanvases(bool isTeam)
    {
        for (int i = 0; i < UnlocksController.unlock.queue.Count; i++)
        //foreach (UnlocksController.UnlockedQueue queue in UnlocksController.unlock.queue)
        {
            UnlocksController.UnlockedQueue queue = UnlocksController.unlock.queue[i];
            switch (queue.type)
            {
                case UnlocksController.UnlockTypes.Lives:
                    isOnMenu = true;

                    string rewardsCondition = "";
                    if (isTeam)
                        rewardsCondition += "Team Rank ";
                    else
                        rewardsCondition += "Hero Level ";
                    rewardsCondition += queue.level;
                    individualUnlockCanvas.transform.GetChild(1).GetComponent<Text>().text = rewardsCondition;
                    individualUnlockCanvas.transform.GetChild(2).GetComponent<Image>().sprite = UnlocksController.unlock.GetRewardArt(queue.type);
                    individualUnlockCanvas.transform.GetChild(3).GetComponent<Text>().text = "Bottled Breath";
                    individualUnlockCanvas.transform.GetChild(4).GetComponent<Text>().text = "+1 Life For All Future Runs";

                    if (!isTeam)
                    {
                        individualUnlockCanvas.transform.GetChild(1).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                        individualUnlockCanvas.transform.GetChild(3).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                    }

                    Debug.Log("###################");
                    UnlocksController.unlock.GetUnlocks().livesUnlocked += 1;
                    UnlocksController.unlock.SetUnlocks(UnlocksController.unlock.GetUnlocks());
                    Debug.Log(UnlocksController.unlock.GetUnlocks().livesUnlocked);
                    Debug.Log("###################");

                    individualUnlockCanvas.enabled = true;
                    currentCanvas = individualUnlockCanvas;
                    break;
                case UnlocksController.UnlockTypes.Replace:
                    isOnMenu = true;

                    rewardsCondition = "";
                    if (isTeam)
                        rewardsCondition += "Team Rank ";
                    else
                        rewardsCondition += "Hero Level ";
                    rewardsCondition += queue.level;
                    individualUnlockCanvas.transform.GetChild(1).GetComponent<Text>().text = rewardsCondition;
                    individualUnlockCanvas.transform.GetChild(2).GetComponent<Image>().sprite = UnlocksController.unlock.GetRewardArt(queue.type);
                    individualUnlockCanvas.transform.GetChild(3).GetComponent<Text>().text = "Glimpse Of The Future";
                    individualUnlockCanvas.transform.GetChild(4).GetComponent<Text>().text = "+1 Replace Per Turn For All Future Runs";

                    if (!isTeam)
                    {
                        individualUnlockCanvas.transform.GetChild(1).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                        individualUnlockCanvas.transform.GetChild(3).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                    }

                    UnlocksController.unlock.GetUnlocks().replaceUnlocked += 1;
                    UnlocksController.unlock.SetUnlocks(UnlocksController.unlock.GetUnlocks());

                    individualUnlockCanvas.enabled = true;
                    currentCanvas = individualUnlockCanvas;
                    break;
                case UnlocksController.UnlockTypes.Contract:
                    isOnMenu = true;

                    individualUnlockCanvas.transform.GetChild(1).GetComponent<Text>().text = "Defeated The World " + queue.level + " Boss";
                    individualUnlockCanvas.transform.GetChild(2).GetComponent<Image>().sprite = UnlocksController.unlock.GetRewardArt(queue.type);
                    individualUnlockCanvas.transform.GetChild(3).GetComponent<Text>().text = "Tavern Contract";
                    individualUnlockCanvas.transform.GetChild(4).GetComponent<Text>().text = "Use In Tavern To Unlock A New Party Member";

                    UnlocksController.unlock.GetUnlocks().tavernContracts += 1;
                    UnlocksController.unlock.SetUnlocks(UnlocksController.unlock.GetUnlocks());

                    individualUnlockCanvas.enabled = true;
                    currentCanvas = individualUnlockCanvas;
                    break;
                case UnlocksController.UnlockTypes.CardPack:
                    isOnMenu = true;

                    cardPackCanvas.transform.GetChild(1).GetComponent<Text>().text = "Hero Level " + queue.level;
                    cardPackCanvas.transform.GetChild(2).GetComponent<Image>().sprite = UnlocksController.unlock.GetRewardArt(queue.type);
                    cardPackCanvas.transform.GetChild(6).GetComponent<Text>().text = "Card Pack";
                    cardPackCanvas.transform.GetChild(7).GetComponent<Text>().text = "Pretend these work kk thnx";

                    if (!isTeam)
                    {
                        cardPackCanvas.transform.GetChild(1).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                        cardPackCanvas.transform.GetChild(6).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                    }

                    cardPackCanvas.enabled = true;
                    currentCanvas = cardPackCanvas;
                    break;
                case UnlocksController.UnlockTypes.Currency:
                    isOnMenu = true;

                    int index = 0;
                    float chance = Random.Range(0f, 1f);
                    if (chance < UnlocksController.unlock.legendaryLootChance)
                        index = 2;
                    else if (chance < UnlocksController.unlock.legendaryLootChance + UnlocksController.unlock.epicLootChance)
                        index = 1;

                    int sand = Random.Range(UnlocksController.unlock.sandMinRange[index], UnlocksController.unlock.sandMaxRange[index]);
                    int shards = Random.Range(UnlocksController.unlock.shardsMinRange[index], UnlocksController.unlock.shardsMaxRange[index]);

                    currencyCanvas.transform.GetChild(1).GetComponent<Text>().text = "Team Level " + queue.level;
                    switch (index)
                    {
                        case 0:
                            currencyCanvas.transform.GetChild(2).GetComponent<Image>().color = Color.white;
                            currencyCanvas.transform.GetChild(3).GetComponent<Image>().color = Color.white;
                            currencyCanvas.transform.GetChild(4).GetComponent<Text>().color = Color.white;
                            currencyCanvas.transform.GetChild(5).GetComponent<Text>().color = Color.white;
                            break;
                        case 1:
                            currencyCanvas.transform.GetChild(2).GetComponent<Image>().color = Color.blue;
                            currencyCanvas.transform.GetChild(3).GetComponent<Image>().color = Color.blue;
                            currencyCanvas.transform.GetChild(4).GetComponent<Text>().color = Color.blue;
                            currencyCanvas.transform.GetChild(5).GetComponent<Text>().color = Color.blue;
                            break;
                        case 2:
                            currencyCanvas.transform.GetChild(2).GetComponent<Image>().color = Color.yellow;
                            currencyCanvas.transform.GetChild(3).GetComponent<Image>().color = Color.yellow;
                            currencyCanvas.transform.GetChild(4).GetComponent<Text>().color = Color.yellow;
                            currencyCanvas.transform.GetChild(5).GetComponent<Text>().color = Color.yellow;
                            break;
                    }
                    currencyCanvas.transform.GetChild(4).GetComponent<Text>().text = sand + " Sand";
                    currencyCanvas.transform.GetChild(5).GetComponent<Text>().text = shards + " Shards";
                    currencyCanvas.transform.GetChild(6).GetComponent<Text>().text = "Rewards";
                    currencyCanvas.transform.GetChild(7).GetComponent<Text>().text = "Used For Card Synthesis And Skins";

                    if (!isTeam)
                    {
                        currencyCanvas.transform.GetChild(1).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                        currencyCanvas.transform.GetChild(6).GetComponent<Text>().color = PartyController.party.GetPlayerColor(queue.casterColor);
                    }

                    UnlocksController.unlock.GetUnlocks().sand += sand;
                    UnlocksController.unlock.GetUnlocks().shards += shards;
                    UnlocksController.unlock.SetUnlocks(UnlocksController.unlock.GetUnlocks());

                    currencyCanvas.enabled = true;
                    currentCanvas = currencyCanvas;
                    break;
            }
            while (isOnMenu)
            {
                yield return null;
            }

            InformationLogger.infoLogger.SaveUnlocks();
            Debug.Log("end of loop");
            Debug.Log(i);
            if (i == UnlocksController.unlock.queue.Count - 1)
                break;
        }

        Debug.Log("out of loop");
        UnlocksController.unlock.queue = new List<UnlocksController.UnlockedQueue>();
        Debug.Log(UnlocksController.unlock.queue.Count);
        if (isTeam)
            ShowHeroExpBars();
        else
            MainMenuButton();

        yield break;
    }

    public void ContinueButtonPress()
    {
        isOnMenu = false;
        currentCanvas.enabled = false;
    }

    public void MainMenuButton()
    {
        //GameController.gameController.RestartGame();
        GameObject sacrificialLamb = new GameObject();
        DontDestroyOnLoad(sacrificialLamb);

        foreach (GameObject obj in sacrificialLamb.scene.GetRootGameObjects())  //Destroy all don't destroy on load objects to start anew
            Destroy(obj);

        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
