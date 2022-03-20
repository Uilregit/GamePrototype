using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EXPBarController : MonoBehaviour
{
    public float barEXPGainTime;

    public Image levelBack;
    public Text levelText;
    public Image levelUPParticle;

    public Image expBar;
    public Text expNumerator;
    public Text expDenominator;

    public Image backgroundLevel;
    public Image backgroundBar;

    private int level;
    private int numerator;
    private int denominator;
    private bool isTeamEXPBar;
    private Color color;
    private Card.CasterColor casterColor;
    private ScoreDisplayController sdController;
    private StoryModeEndSceenController smesController;

    public void SetValues(int newLevel, int newNumerator, bool newIsTeamExpBar, Color newColor, Card.CasterColor newCasterColor)
    {
        level = newLevel;
        numerator = newNumerator;
        isTeamEXPBar = newIsTeamExpBar;
        levelText.text = newLevel.ToString();
        if (isTeamEXPBar)
            denominator = ScoreController.score.GetTeamEXPNeededToLevel(level);
        else
            denominator = ScoreController.score.GetHeroEXPNeededToLevel(level);
        expNumerator.text = newNumerator.ToString();
        expDenominator.text = "/" + denominator.ToString();
        expBar.transform.localScale = new Vector2((float)numerator / denominator, 1);
        color = newColor;
        casterColor = newCasterColor;
        levelBack.color = newColor;
        expBar.color = newColor;
    }

    public IEnumerator GainEXP(int exp)
    {
        yield return new WaitForSeconds(0.5f);
        while (exp > 0)
        {
            if (exp > 7)       //Set to a non multiple of 10 so singles digit changes as well as bar fills up
            {
                numerator += 7;
                exp -= 7;
            }
            else
            {
                numerator += exp;
                exp -= exp;
            }
            if (numerator >= denominator)
            {
                level += 1;
                StartCoroutine(LevelUp());
                //UnlocksController.unlock.ReportLevelUp(level, isTeamEXPBar, casterColor);
                numerator -= denominator;
                if (isTeamEXPBar)
                    denominator = ScoreController.score.GetTeamEXPNeededToLevel(level);
                else
                    denominator = ScoreController.score.GetHeroEXPNeededToLevel(level);
                smesController.ReportLevelUp();
            }
            SetValues(level, numerator, isTeamEXPBar, color, casterColor);
            yield return new WaitForSeconds(barEXPGainTime * (1 + Mathf.Pow((float)numerator / denominator, 4) * 100));
        }
        if (isTeamEXPBar)
        {
            ScoreController.score.teamLevel = level;
            ScoreController.score.currentEXP = numerator;
        }
        else
            PartyController.party.SetPartyLevelInfo(casterColor, level, numerator);
        InformationLogger.infoLogger.SavePlayerPreferences();
        //sdController.ReportBarDone(isTeamEXPBar);
    }

    public IEnumerator LevelUp()
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.3f)
        {
            levelUPParticle.transform.localScale = Vector2.Lerp(new Vector2(1, 1), new Vector2(5, 5), elapsedTime / 0.3f);
            levelUPParticle.color = new Color(levelUPParticle.color.r, levelUPParticle.color.g, levelUPParticle.color.b, Mathf.Clamp(2 - (elapsedTime / 0.3f) * 2, 0, 1));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        levelUPParticle.color = new Color(levelUPParticle.color.r, levelUPParticle.color.g, levelUPParticle.color.b, 0);
    }

    public void SetScoreDisplayController(ScoreDisplayController value)
    {
        sdController = value;
    }

    public void SetStoryModeEndSceneController(StoryModeEndSceenController value)
    {
        smesController = value;
    }    

    public void SetEnabled(bool value)
    {
        levelBack.enabled = value;
        levelText.enabled = value;
        levelUPParticle.enabled = value;

        expBar.enabled = value;
        expNumerator.enabled = value;
        expDenominator.enabled = value;

        backgroundLevel.enabled = value;
        backgroundBar.enabled = value;
    }
}
