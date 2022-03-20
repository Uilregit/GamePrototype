using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceMovementFromCastLocation : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        List<GameObject> deepCopy = new List<GameObject>();
        foreach (GameObject obj in target) //Prevent list modify error
            deepCopy.Add(obj);
        for (int i = 0; i < deepCopy.Count; i++)

            if (GridController.gridController.GetRoundedVector(deepCopy[i].transform.position, 1) != effectController.GetCastLocation())
            {
                if (i == deepCopy.Count - 1)
                    yield return deepCopy[i].GetComponent<HealthController>().StartCoroutine(deepCopy[i].GetComponent<HealthController>().ForcedMovement(effectController.GetCastLocation(), ((Vector2)deepCopy[i].transform.position - effectController.GetCastLocation()).normalized, card.effectValue[effectIndex]));
                else
                    deepCopy[i].GetComponent<HealthController>().StartCoroutine(deepCopy[i].GetComponent<HealthController>().ForcedMovement(effectController.GetCastLocation(), ((Vector2)deepCopy[i].transform.position - effectController.GetCastLocation()).normalized, card.effectValue[effectIndex]));
            }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
