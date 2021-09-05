using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfForcedMovementEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        yield return caster.GetComponent<HealthController>().StartCoroutine(caster.GetComponent<HealthController>().ForcedMovement(((Vector2)caster.transform.position - target[0])* card.effectValue[effectIndex], card.effectValue[effectIndex], false));
        yield return new WaitForSeconds(0);
    }

    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        throw new System.NotImplementedException();
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
