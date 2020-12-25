using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfForcedMovementEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> target, Card card, int effectIndex)
    {
        yield return caster.GetComponent<HealthController>().StartCoroutine(caster.GetComponent<HealthController>().ForcedMovement(((Vector2)caster.transform.position - target[0])* card.effectValue[effectIndex], card.effectValue[effectIndex], false));
        yield return new WaitForSeconds(0);
    }

    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        throw new System.NotImplementedException();
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
