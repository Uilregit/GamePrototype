using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimHealthController
{
    public int currentVit;
    public int currentShield;
    public int maxVit;
    public int currentAttack;

    public void SetValues(SimHealthController info)
    {
        currentVit = info.currentVit;
        currentShield = info.currentShield;
        maxVit = info.maxVit;
    }
}
