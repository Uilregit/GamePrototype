using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> target, Card card, int effectIndex)
    {
        try
        {
            caster.GetComponent<PlayerMoveController>().TeleportTo(target[0]);
            caster.transform.position = target[0];
        }
        catch { }

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
