using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public abstract class Effect
{
    public string effectName = "default";
    public CardController chosenCard = null;

    private List<Card.EffectType> clientEffects = new List<Card.EffectType> {
    Card.EffectType.DrawCards,
    Card.EffectType.Resurrect,
    Card.EffectType.ManifestANYEnergyCardEffect,
    Card.EffectType.ManifestDiscardCards,
    Card.EffectType.ManifestDrawCards,
    Card.EffectType.EnergyGain,
    Card.EffectType.ManaGain,
    Card.EffectType.GetStarterCardEffect,
    Card.EffectType.DrawLastPlayedCardEffect
    };

    public IEnumerator ProcessCard(GameObject caster, CardEffectsController effectController, List<Vector2> location, Card card, int effectIndex, float waitTimeMultiplier = 1)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        if (!GetIsLocationEffect())
            yield return GameController.gameController.StartCoroutine(ProcessCard(caster, effectController, target, card, effectIndex, waitTimeMultiplier));
        else
            yield return GameController.gameController.StartCoroutine(Process(caster, effectController, location, card, effectIndex, waitTimeMultiplier));
    }

    public IEnumerator ProcessCard(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier = 1)
    {
        target = CheckImmunity(caster, effectController, target, card, effectIndex, waitTimeMultiplier);
        yield return GameController.gameController.StartCoroutine(Process(caster, effectController, target, card, effectIndex, waitTimeMultiplier));
    }

    //effect controller used to store temp values for effects that use get
    protected virtual IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> location, Card card, int effectIndex, float waitTimeMultiplier = 1)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        if (GameController.gameController != null)
            yield return GameController.gameController.StartCoroutine(Process(caster, effectController, target, card, effectIndex, waitTimeMultiplier));
        else
        {
            if (clientEffects.Contains(card.cardEffectName[effectIndex]) && ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == 0 && TurnController.turnController.multiplayerTurnPlayer != 0)   //If multiplayer and effect should trigger only on client
                ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ClientCardProcess(caster.GetComponent<NetworkIdentity>().netId.ToString(), location, card.name, effectIndex, card.GetTempEffectValue(), card.GetTempDuration());
            else
                yield return MultiplayerGameController.gameController.StartCoroutine(Process(caster, effectController, target, card, effectIndex, waitTimeMultiplier));
        }
    }

    protected abstract IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier = 1);

    public List<GameObject> CheckImmunity(GameObject caster, CardEffectsController effectsController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier = 1)
    {
        List<GameObject> viableCastTargets = new List<GameObject>();
        foreach (HealthController hlth in target.Select(x => x.GetComponent<HealthController>()))
        {
            if (hlth.gameObject == caster)                      //Never be immune to your own cards
                viableCastTargets.Add(hlth.gameObject);
            else if (hlth.GetImmuneToEnergy() && card.manaCost == 0)
            {
                if (waitTimeMultiplier != 0)
                    hlth.SetStatusText("Immune", Color.yellow);
            }
            else if (hlth.GetImmuneToMana() && card.manaCost > 0)
            {
                if (waitTimeMultiplier != 0)
                    hlth.SetStatusText("Immune", Color.yellow);
            }
            else
                viableCastTargets.Add(hlth.gameObject);
        }
        return viableCastTargets;
    }

    public int GetSimulatedVitDamage(GameObject caster, CardEffectsController effectController, Vector2 location, Card card, int effectIndex)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return GetSimulatedVitDamage(caster, effectController, target, card, effectIndex);
    }

    public int GetSimulatedVitDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int output = 0;
        /*
        foreach (GameObject obj in target)
        {
            HealthController hlthController = obj.GetComponent<HealthController>();
            HealthController simulation = GameController.gameController.GetSimulationCharacter(hlthController);

            GameController.gameController.StartCoroutine(effectController.TriggerEffect(caster, new List<GameObject> { simulation.gameObject }, new List<Vector2> { simulation.transform.position }, false, true));

            output += hlthController.GetVit() - simulation.GetVit();
            GameController.gameController.ReportSimulationFinished(simulation);
        }
        */
        return output;
    }

    public int GetSimulatedArmorDamage(GameObject caster, CardEffectsController effectController, Vector2 location, Card card, int effectIndex)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return GetSimulatedArmorDamage(caster, effectController, target, card, effectIndex);
    }

    public int GetSimulatedArmorDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int output = 0;
        /*
        foreach (GameObject obj in target)
        {
            HealthController hlthController = obj.GetComponent<HealthController>();
            HealthController simulation = GameController.gameController.GetSimulationCharacter(hlthController);

            GameController.gameController.StartCoroutine(effectController.TriggerEffect(caster, new List<GameObject> { simulation.gameObject }, new List<Vector2> { simulation.transform.position }, false, true));

            while (!effectController.GetFinished()) { }

            output += hlthController.GetArmor() - simulation.GetArmor();
            Debug.Log(hlthController.GetAttack() + "|" + hlthController.GetArmor() + "|" + hlthController.GetVit());
            Debug.Log(simulation.GetAttack() + "|" + simulation.GetArmor() + "|" + simulation.GetVit());
            Debug.Log(hlthController.GetArmor() + " - " + simulation.GetArmor() + " = " + output);
            GameController.gameController.ReportSimulationFinished(simulation);
        }

        Debug.Log(output);
        */
        return output;
    }

    public abstract SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH);

    public virtual void RelicProcess(List<GameObject> targets, Buff buff, int effectValue, int effectDuration, List<Relic> traceList)
    {
        throw new System.NotImplementedException();
    }
    public virtual bool GetIsLocationEffect()
    {
        return false;
    }
}

