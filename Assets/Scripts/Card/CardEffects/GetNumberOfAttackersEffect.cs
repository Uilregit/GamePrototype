using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GetNumberOfAttackersEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int count = 0;
        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
            if (enemy.desiredTarget.Any(o => o == caster))
                count++;
        card.SetTempEffectValue(count);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
