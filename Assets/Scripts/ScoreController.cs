using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public static ScoreController score;

    public int scorePerOverkill;
    public int scorePerDamage;
    public int scorePerDamageShielded;
    public int scorePerDamageOverhealedProtected;
    public int scorePerDamageAvoided;
    public int scorePerEnemiesBroken;
    public int scorePerGoldUsed;
    public int scorePerBossesDefeated;
    public int scorePerSecondsInGame;

    private int overkill;
    private int damage;
    private int damageShielded;
    private int damageOverhealedProtected;
    private int damageAvoided;
    private int enemiesBroken;
    private int goldUsed;
    private int bossesDefeated;
    private int secondsInGame;

    private float gameStartTime;

    // Start is called before the first frame update
    void Start()
    {
        if (ScoreController.score == null)
            ScoreController.score = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        gameStartTime = Time.time;
    }

    public void UpdateOverkill(int value)
    {
        overkill += value;
    }

    public int GetOverKill()
    {
        return overkill;
    }

    public void UpdateDamage(int value)
    {
        damage += value;
    }

    public int GetDamage()
    {
        return damage;
    }

    public void UpdateDamageShielded(int value)
    {
        damageShielded += value;
    }

    public int GetDamageShielded()
    {
        return damageShielded;
    }

    public void UpdateDamageOverhealProtected(int value)
    {
        damageOverhealedProtected += value;
    }

    public int GetDamageOverhealProtected()
    {
        return damageOverhealedProtected;
    }

    public void UpdateDamageAvoided(int value)
    {
        damageAvoided += value;
    }

    public int GetDamageAvoided()
    {
        return damageAvoided;
    }

    public void UpdateEnemiesBroken()
    {
        enemiesBroken += 1;
    }

    public int GetEnemiesBroken()
    {
        return enemiesBroken;
    }

    public void UpdateGoldUsed(int value)
    {
        goldUsed += value;
    }

    public int GetGoldUsed()
    {
        return goldUsed;
    }

    public void UpdateBossesDefeated()
    {
        bossesDefeated += 1;
    }

    public int GetBossesDefeated()
    {
        return bossesDefeated;
    }

    public float GetMinutesInGame()
    {
        return (Time.time - gameStartTime) / 60.0f;
    }
}
