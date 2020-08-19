using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController time;

    public float timerMultiplier;

    [Header("Enemy Time Settings")]
    public float enemyMoveHangTime;
    public float enemyMoveStepTime;
    public float enemyStunnedTurnTime;
    public float enemyAttackCardHangTime;

    [Header("Enemy Character Information Settings")]
    public float timeTillCardDisplay;

    [Header("Turn Change Timer Settings")]
    public float turnChangeDuration;
    public float turnGracePeriod;
    public float enemyExecutionStagger;
    public float victoryTextDuration;

    [Header("Card Effect Timer Settings")]
    public float attackBufferTime;

    [Header("Health Bar Timer Settings")]
    public float barShownDuration;
    public float numberExpandDuration;
    public float numberShownDuration;

    [Header("Mana Bar Timer Settings")]
    public float anticipatedGainFlickerPeriod;
    public float manaGainFlickerPeriod;

    // Start is called before the first frame update
    void Start()
    {
        if (TimeController.time == null)
            TimeController.time = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
