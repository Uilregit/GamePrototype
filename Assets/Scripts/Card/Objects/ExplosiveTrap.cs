using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTrap : TrapController
{
    public int damage;
    /*
    public override void Trigger(List<GameObject> trappedObject)
    {
        foreach (GameObject trappedObj in trappedObject)
            trappedObj.GetComponent<HealthController>().TakePiercingDamage(damage, null);
        base.Trigger(trappedObject);
    }
    */
}
