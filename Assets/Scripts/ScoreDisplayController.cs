using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayController : MonoBehaviour
{
    public Text overkill;
    public Text damage;
    public Text damageArmored;
    public Text overhealProtected;
    public Text damageAvoided;
    public Text enemiesBroken;
    public Text goldUsed;
    public Text bossesDefeated;
    public Text secondsInGame;
    public Text Total;

    public Text overkillScore;
    public Text damageScore;
    public Text damageArmoredScore;
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
        ScoreController.score.timerPaused = true;

        overkill.text = "Overkill (" + ScoreController.score.GetOverKill().ToString() + ")";
        damage.text = "Damage Dealt (" + ScoreController.score.GetDamage().ToString() + ")";
        damageArmored.text = "Armor Protected (" + ScoreController.score.GetDamageArmored().ToString() + ")";
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
        damageArmoredScore.text = (ScoreController.score.GetDamageArmored() * ScoreController.score.scorePerDamageArmored).ToString();
        scoreTotal += ScoreController.score.GetDamageArmored() * ScoreController.score.scorePerDamageArmored;
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
        if (ScoreController.score.GetBossesDefeated() > 0 && (int)ScoreController.score.GetSecondsInGame() / 60.0f < 30)
        {
            secondsInGameScore.text = ((int)ScoreController.score.GetSecondsInGame() / 60 * ScoreController.score.scorePerSecondsInGame).ToString();
            scoreTotal += Mathf.Max(0, (30 - (int)ScoreController.score.GetSecondsInGame() / 60)) * ScoreController.score.scorePerSecondsInGame;
        }

        TotalScore.text = scoreTotal.ToString();

        InformationLogger.infoLogger.SaveGame(true);

        InformationLogger.infoLogger.SaveGameScoreInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.selectedLevel.ToString(),
                                RoomController.roomController.roomName,
                                (ScoreController.score.GetBossesDefeated() == 1).ToString(),
                                (ScoreController.score.GetBossesDefeated() != 1).ToString(),
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
