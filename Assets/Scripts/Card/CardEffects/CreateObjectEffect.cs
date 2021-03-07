using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObjectEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> location, Card card, int effectIndex)
    {
        if (location.Count != 1)
        {
            Debug.Log("too many locations passed through to create object");
            foreach (Vector2 l in location)
                Debug.Log(l);
            throw new KeyNotFoundException();
        }

        Vector2 loc = location[0];
        GameObject obj;
        for (int i = 0; i < card.effectValue[effectIndex]; i++)
        {
            if (card.castType == Card.CastType.AoE || card.castType == Card.CastType.TargetedAoE)
            {
                List<Vector2> viableLocations = GridController.gridController.GetEmptyLocationsInAoE(location[0], card.radius);

                if ((Object)card.spawnObject[effectIndex].GetComponent<TrapController>() != null)
                {
                    viableLocations = GridController.gridController.GetEmptyTrapLocationsInAoE(location[0], card.radius);
                    foreach (Vector2 l in GridController.gridController.GetLocationsInAoE(location[0], card.radius, new string[] { caster.tag }))
                        viableLocations.Remove(l);          //Ensure caster never traps themselves or allies
                }

                if (viableLocations.Count == 0)
                    break;

                loc = viableLocations[Random.Range(0, viableLocations.Count)];
                obj = GameObject.Instantiate(card.spawnObject[effectIndex], loc, Quaternion.identity);
            }
            else
                obj = GameObject.Instantiate(card.spawnObject[effectIndex], loc, Quaternion.identity);
            obj.transform.parent = CanvasController.canvasController.boardCanvas.transform;
            try
            {
                foreach (TrapController t in GridController.gridController.traps)
                    if (t.transform.position == obj.transform.position)                                     //Check if there is an existing trap on that location
                    {
                        if (t.GetComponent<TrapController>().GetDuration() > card.effectDuration[effectIndex] * 2 + 1)        //Create the trap only if it's duration is higher than the existing traps
                        {
                            Debug.Log(t.GetComponent<TrapController>().GetDuration());
                            Debug.Log(card.effectDuration[effectIndex] * 2 + 1);
                            Debug.Log("old trap stays");
                            obj.gameObject.SetActive(false);
                        }
                        else
                        {
                            Debug.Log("old trap disabled");
                            t.gameObject.SetActive(false);
                            GridController.gridController.traps.Remove(t.GetComponent<TrapController>());
                        }
                        break;
                    }

                if (obj.activeSelf)
                {
                    obj.GetComponent<TrapController>().SetValues(caster, card, effectIndex + 1);
                    GridController.gridController.traps.Add(obj.GetComponent<TrapController>());
                    Debug.Log("added trap to global list");
                }
            }
            catch
            {
                obj.GetComponent<EnemyController>().Spawn(loc);
                obj.GetComponent<HealthController>().SetCreator(caster);
                obj.GetComponent<EnemyController>().SetSkipInitialIntent(true);
            }
        }
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