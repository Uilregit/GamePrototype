using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public string effectName = "default";

    //effect controller used to store temp values for effects that use get
    public virtual void Process(GameObject caster, CardEffectsController effectController, Vector2 location, Card card, int effectIndex)
    {
        GameObject target = GridController.gridController.GetObjectAtLocation(location);
        Process(caster, effectController, target, card, effectIndex);
    }

    public abstract void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex);
    public abstract SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH);
}

