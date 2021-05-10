using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GetNumberOfAttackersEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        int maxCount = 0;
        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
        {
            int count = 0;
            foreach (GameObject targ in target)
                if (enemy.desiredTarget.Any(o => o == targ))
                    count++;
            maxCount = Mathf.Max(count, maxCount);
        }
        card.SetTempEffectValue(maxCount);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
