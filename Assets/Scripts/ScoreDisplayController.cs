using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayController : MonoBehaviour
{
    public Text overkill;
    public Text damage;
    public Text damageShielded;
    public Text overhealProtected;
    public Text damageAvoided;
    public Text enemiesBroken;
    public Text goldUsed;
    public Text bossesDefeated;
    public Text secondsInGame;
    public Text Total;

    public Text overkillScore;
    public Text damageScore;
    public Text damageShieldedScore;
    public Text overhealProtectedScore;
    public Text damageAvoidedScore;
    public Text enemiesBrokenScore;
    public Text goldUsedScore;
    public Text bossesDefeatedScore;
    public Text secondsInGameScore;
    public Text TotalScore;

    // Start is called before the first frame update
    void Start()
    {
        overkill.text = "Overkill (" + ScoreController.score.GetOverKill().ToString() + ")";
        damage.text = "Damage Dealt (" + ScoreController.score.GetDamage().ToString() + ")";
        damageShielded.text = "Shield Protected (" + ScoreController.score.GetDamageShielded().ToString() + ")";
        overhealProtected.text = "Overheal Protected (" + ScoreController.score.GetDamageOverhealProtected().ToString() + ")";
        damageAvoided.text = "Damage Avoided (" + ScoreController.score.GetDamageAvoided().ToString() + ")";
        enemiesBroken.text = "Enemies Broken (" + ScoreController.score.GetEnemiesBroken().ToString() + ")";
        goldUsed.text = "Gold Used (" + ScoreController.score.GetGoldUsed().ToString() + ")";
        if (ScoreController.score.GetBossesDefeated() > 0)
            bossesDefeated.text = "Bosses Defeated (" + ScoreController.score.GetBossesDefeated().ToString() + ")";
        if (ScoreController.score.GetBossesDefeated() > 0 && (int)ScoreController.score.GetSecondsInGame() / 60 < 30)
            secondsInGame.text = "Speed Bonus (" + ((int)ScoreController.score.GetSecondsInGame() / 60).ToString() + " mins)";

        int scoreTotal = 0;
        overkillScore.text = (ScoreController.score.GetOverKill() * ScoreController.score.scorePerOverkill).ToString();
        scoreTotal += ScoreController.score.GetOverKill() * ScoreController.score.scorePerOverkill;
        damageScore.text = (ScoreController.score.GetDamage() * ScoreController.score.scorePerDamage).ToString();
        scoreTotal += ScoreController.score.GetDamage() * ScoreController.score.scorePerDamage;
        damageShieldedScore.text = (ScoreController.score.GetDamageShielded() * ScoreController.score.scorePerDamageShielded).ToString();
        scoreTotal += ScoreController.score.GetDamageShielded() * ScoreController.score.scorePerDamageShielded;
        overhealProtectedScore.text = (ScoreController.score.GetDamageOverhealProtected() * ScoreController.score.scorePerDamageOverhealedProtected).ToString();
        scoreTotal += ScoreController.score.GetDamageOverhealProtected() * ScoreController.score.scorePerDamageOverhealedProtected;
        damageAvoidedScore.text = (ScoreController.score.GetDamageAvoided() * ScoreController.score.scorePerDamageAvoided).ToString();
        scoreTotal += ScoreController.score.GetDamageAvoided() * ScoreController.score.scorePerDamageAvoided;
        enemiesBrokenScore.text = (ScoreController.score.GetEnemiesBroken() * ScoreController.score.scorePerEnemiesBroken).ToString();
        scoreTotal += ScoreController.score.GetEnemiesBroken() * ScoreController.score.scorePerEnemiesBroken;
        goldUsedScore.text = (ScoreController.score.GetGoldUsed() * ScoreController.score.scorePerGoldUsed).ToString();
        scoreTotal += ScoreController.score.GetGoldUsed() * ScoreController.score.scorePerGoldUsed;
        if (ScoreController.score.GetBossesDefeated() > 0)
        {
            bossesDefeatedScore.text = (ScoreController.score.GetBossesDefeated() * ScoreController.score.scorePerBossesDefeated).ToString();
            scoreTotal += ScoreController.score.GetBossesDefeated() * ScoreController.score.scorePerBossesDefeated;
        }
        if (ScoreController.score.GetBossesDefeated() > 0 && (int)ScoreController.score.GetSecondsInGame() / 60 < 30)
        {
            secondsInGameScore.text = ((int)ScoreController.score.GetSecondsInGame() / 60 * ScoreController.score.scorePerSecondsInGame).ToString();
            scoreTotal += Mathf.Max(0, (30 - (int)ScoreController.score.GetSecondsInGame() / 60)) * ScoreController.score.scorePerSecondsInGame;
        }

        TotalScore.text = scoreTotal.ToString();

        InformationLogger.infoLogger.SaveGame(true);
    }
}
