using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTrap : TrapController
{
    public int damage;

    public override void Trigger(GameObject trappedObject)
    {
        trappedObject.GetComponent<HealthController>().TakePiercingDamage(damage, null);
        base.Trigger(trappedObject);
    }
}
