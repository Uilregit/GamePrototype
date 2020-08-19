using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObjectEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> location, Card card, int effectIndex)
    {
        if (location.Count != 1)
            throw new KeyNotFoundException();

        Vector2 loc = location[0];
        GameObject obj;
        for (int i = 0; i < card.effectValue[effectIndex]; i++)
        {
            if (card.castType == Card.CastType.AoE || card.castType == Card.CastType.TargetedAoE)
            {
                List<Vector2> viableLocations = GridController.gridController.GetEmptyLocationsInAoE(location[0], card.radius);

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
                obj.GetComponent<TrapController>().SetValues(caster, card, effectIndex + 1);
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