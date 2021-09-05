using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SacrificeEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        foreach (GameObject targ in target)
        {
            if (targ == caster)
                continue;
            targ.GetComponent<AbilitiesController>().TriggerAbilities(AbilitiesController.TriggerType.OnSacrifice);
            if (targ.tag == "Enemy")
            {
                targ.GetComponent<EnemyController>().SetSacrificed(true);
                GridController.gridController.RemoveFromPosition(targ, targ.transform.position);
                targ.transform.position = new Vector2(99, 99);
            }
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
