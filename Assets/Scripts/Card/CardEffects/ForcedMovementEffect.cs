using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcedMovementEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        List<GameObject> deepCopy = new List<GameObject>();
        foreach (GameObject obj in target) //Prevent list modify error
            deepCopy.Add(obj);
        foreach (GameObject targ in deepCopy)
            yield return targ.GetComponent<HealthController>().StartCoroutine(targ.GetComponent<HealthController>().ForcedMovement(caster.transform.position, card.effectValue[effectIndex]));
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
