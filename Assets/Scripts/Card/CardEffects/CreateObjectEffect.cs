using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObjectEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, Vector2 location, Card card, int effectIndex)
    {
        GameObject trap = GameObject.Instantiate(card.spawnObject[effectIndex], location, Quaternion.identity);
        trap.transform.parent = CanvasController.canvasController.boardCanvas.transform;
        trap.GetComponent<TrapController>().SetValues(card, effectIndex + 1);
    }

    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        throw new System.NotImplementedException();
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}