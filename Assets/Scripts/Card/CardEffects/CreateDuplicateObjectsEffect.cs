using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateDuplicateObjectsEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> location, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        List<GameObject> duplicatedObjs = GridController.gridController.GetObjectAtLocation(location, new string[] { "Enemy" });

        for (int i = 0; i < card.effectValue[effectIndex]; i++)
        {
            foreach (GameObject o in duplicatedObjs)
            {
                List<Vector2> viableLocations = new List<Vector2>();
                for (int j = 1; j <= 4; j++)
                {
                    viableLocations = GridController.gridController.GetEmptyLocationsInAoE(location[0], j);
                    if (viableLocations.Count != 0)
                        break;
                }

                if (viableLocations.Count == 0)
                    break;

                Vector2 loc = viableLocations[Random.Range(0, viableLocations.Count)];
                GameObject obj = GameObject.Instantiate(RoomController.roomController.GetObjectPrefab(o), loc, Quaternion.identity);

                obj.transform.parent = CanvasController.canvasController.boardCanvas.transform;
                try
                {
                    foreach (TrapController t in GridController.gridController.traps)
                        if (t.transform.position == obj.transform.position)
                        {
                            obj.gameObject.SetActive(false);
                            break;
                        }
                    obj.GetComponent<TrapController>().SetValues(caster, card, effectIndex + 1);
                    GridController.gridController.traps.Add(obj.GetComponent<TrapController>());
                }
                catch
                {
                    obj.GetComponent<EnemyController>().Spawn(loc);
                    obj.GetComponent<HealthController>().SetCreator(caster);
                    obj.GetComponent<EnemyController>().SetSkipInitialIntent(true);


                    obj.GetComponent<HealthController>().SetCurrentAttack(o.GetComponent<HealthController>().GetCurrentAttack());
                    obj.GetComponent<HealthController>().SetCurrentArmor(o.GetComponent<HealthController>().GetCurrentArmor(), false);
                    obj.GetComponent<HealthController>().SetMaxVit(o.GetComponent<HealthController>().GetMaxVit());
                    obj.GetComponent<HealthController>().SetCurrentVit(o.GetComponent<HealthController>().GetCurrentVit());

                }
            }
        }
        yield return new WaitForSeconds(0);
    }

    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

            throw new System.NotImplementedException();
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}