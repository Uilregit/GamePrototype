using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SwapEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        if (!target.All(x => x.GetComponent<HealthController>().size >1))
        {
            Vector2 newLoc = target[0].transform.position;
            Vector2 castLoc = caster.transform.position;
            //Move the caster to the new position
            GridController.gridController.RemoveFromPosition(caster, caster.transform.position);
            caster.transform.position = newLoc;
            GridController.gridController.ReportPosition(caster, caster.transform.position);
            try
            {
                caster.GetComponent<PlayerMoveController>().TeleportTo(caster.transform.position);
            }
            catch { }
            //Move all objects at the new position to the caster's location
            foreach (GameObject obj in target)
            {
                if (obj.GetComponent<HealthController>().size == 1)
                {
                    GridController.gridController.RemoveFromPosition(obj, obj.transform.position);
                    obj.transform.position = castLoc;
                    GridController.gridController.ReportPosition(obj, obj.transform.position);
                    try
                    {
                        obj.GetComponent<PlayerMoveController>().TeleportTo(obj.transform.position);
                    }
                    catch { }
                }
            }
            //Update the caster's origin after the swap
            try
            {
                caster.GetComponent<PlayerMoveController>().UpdateOrigin(caster.transform.position);
            }
            catch { }
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
