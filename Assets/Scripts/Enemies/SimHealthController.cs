using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimHealthController
{
    public int currentVit;
    public int currentArmor;
    public int maxVit;
    public int currentAttack;

    public void SetValues(SimHealthController info)
    {
        currentVit = info.currentVit;
        currentArmor = info.currentArmor;
        maxVit = info.maxVit;
    }
}
