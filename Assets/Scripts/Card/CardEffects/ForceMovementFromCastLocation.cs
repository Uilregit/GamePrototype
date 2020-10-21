﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceMovementFromCastLocation : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        List<GameObject> deepCopy = new List<GameObject>();
        foreach (GameObject obj in target) //Prevent list modify error
            deepCopy.Add(obj);
        foreach (GameObject targ in deepCopy)
            if (GridController.gridController.GetRoundedVector(targ.transform.position, 1) != effectController.GetCastLocation())
                targ.GetComponent<HealthController>().ForcedMovement(effectController.GetCastLocation(), card.effectValue[effectIndex]);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}