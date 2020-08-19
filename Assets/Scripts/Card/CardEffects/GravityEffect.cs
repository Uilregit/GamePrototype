using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        Vector2 center = card.GetCenter();
        foreach (GameObject obj in target)
        {
            try
            {
                obj.GetComponent<PlayerMoveController>().TeleportTo(center);
            }
            catch { }
            GridController.gridController.RemoveFromPosition(obj, obj.transform.position);
            obj.transform.position = center;
            GridController.gridController.ReportPosition(obj, center);
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
