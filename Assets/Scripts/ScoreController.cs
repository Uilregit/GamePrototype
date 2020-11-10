using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    public static ScoreController score;

    public Text timerText;
    public bool timerPaused = true; 

    public int scorePerOverkill;
    public int scorePerDamage;
    public int scorePerDamageArmored;
    public int scorePerDamageOverhealedProtected;
    public int scorePerDamageAvoided;
    public int scorePerEnemiesBroken;
    public int scorePerGoldUsed;
    public int scorePerBossesDefeated;
    public int scorePerSecondsInGame;

    private int overkill;
    private int damage;
    private int damageArmored;
    private int damageOverhealedProtected;
    private int damageAvoided;
    private int enemiesBroken;
    private int goldUsed;
    private int bossesDefeated;
    private float secondsInGame;

    // Start is called before the first frame update
    void Start()
    {
        if (ScoreController.score == null)
        {
            ScoreController.score = this;
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(timerText.gameObject);

            secondsInGame = 0;
        }
        else
            Destroy(this.gameObject);
    }

    private void Update()
    {
        if (!timerPaused)
        {
            secondsInGame += Time.deltaTime;
            timerText.text = "Time: " + TimeSpan.FromSeconds(secondsInGame).ToString("hh':'mm':'ss");
        }
    }

    public void UpdateOverkill(int value)
    {
        overkill += value;
    }

    public int GetOverKill()
    {
        return overkill;
    }

    public void SetOverkill(int value)
    {
        overkill = value;
    }

    public void UpdateDamage(int value)
    {
        damage += value;
    }

    public int GetDamage()
    {
        return damage;
    }

    public void SetDamage(int value)
    {
        damage = value;
    }

    public void UpdateDamageArmored(int value)
    {
        damageArmored += value;
    }

    public int GetDamageArmored()
    {
        return damageArmored;
    }

    public void SetDamageArmored(int value)
    {
        damageArmored = value;
    }

    public void UpdateDamageOverhealProtected(int value)
    {
        damageOverhealedProtected += value;
    }

    public int GetDamageOverhealProtected()
    {
        return damageOverhealedProtected;
    }

    public void SetDamageOverhealProtected(int value)
    {
        damageOverhealedProtected = value;
    }

    public void UpdateDamageAvoided(int value)
    {
        damageAvoided += value;
    }

    public int GetDamageAvoided()
    {
        return damageAvoided;
    }

    public void SetDamageAvoided(int value)
    {
        damageAvoided = value;
    }

    public void UpdateEnemiesBroken()
    {
        enemiesBroken += 1;
    }

    public int GetEnemiesBroken()
    {
        return enemiesBroken;
    }

    public void SetEnemiesBroken(int value)
    {
        enemiesBroken = value;
    }

    public void UpdateGoldUsed(int value)
    {
        goldUsed += value;
    }

    public int GetGoldUsed()
    {
        return goldUsed;
    }

    public void SetGoldUsed(int value)
    {
        goldUsed = value;
    }

    public void UpdateBossesDefeated()
    {
        bossesDefeated += 1;
    }

    public int GetBossesDefeated()
    {
        return bossesDefeated;
    }

    public void SetBossesDefeated(int value)
    {
        bossesDefeated = value;
    }

    public float GetSecondsInGame()
    {
        return secondsInGame;
    }

    public void SetSecondsInGame(float value)
    {
        secondsInGame = value;
    }
}
