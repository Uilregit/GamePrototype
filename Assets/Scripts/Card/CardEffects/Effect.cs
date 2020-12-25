using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Effect
{
    public string effectName = "default";
    public CardController chosenCard = null;

    //effect controller used to store temp values for effects that use get
    public virtual IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> location, Card card, int effectIndex)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        yield return GameController.gameController.StartCoroutine(Process(caster, effectController, target, card, effectIndex));
    }

    public virtual int GetSimulatedVitDamage(GameObject caster, CardEffectsController effectController, Vector2 location, Card card, int effectIndex)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return GetSimulatedVitDamage(caster, effectController, target, card, effectIndex);
    }

    public virtual int GetSimulatedVitDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        return 0;
    }

    public virtual int GetSimulatedArmorDamage(GameObject caster, CardEffectsController effectController, Vector2 location, Card card, int effectIndex)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return GetSimulatedArmorDamage(caster, effectController, target, card, effectIndex);
    }

    public virtual int GetSimulatedArmorDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        return 0;
    }

    public abstract IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex);
    public abstract SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH);

    public virtual void RelicProcess(List<GameObject> targets, Buff buff, int effectValue, int effectDuration, List<Relic> traceList)
    {
        throw new System.NotImplementedException();
    }
}

