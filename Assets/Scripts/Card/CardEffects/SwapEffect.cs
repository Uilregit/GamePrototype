using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SwapEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        if (!target.All(x => x.GetComponent<HealthController>().size >1))
        {
            Vector2 newLoc = target[0].transform.position;
            foreach(GameObject obj in target)
            {
                if (obj.GetComponent<HealthController>().size == 1)
                {
                    GridController.gridController.RemoveFromPosition(obj, obj.transform.position);
                    obj.transform.position = caster.transform.position;
                    GridController.gridController.ReportPosition(obj, obj.transform.position);
                    try
                    {
                        obj.GetComponent<PlayerMoveController>().TeleportTo(obj.transform.position);
                    }
                    catch { }
                }
            }
            try
            {
                caster.GetComponent<PlayerMoveController>().UpdateOrigin(caster.transform.position);
            }
            catch { }
            GridController.gridController.RemoveFromPosition(caster, caster.transform.position);
            caster.transform.position = newLoc;
            GridController.gridController.ReportPosition(caster, caster.transform.position);
            try
            {
                caster.GetComponent<PlayerMoveController>().TeleportTo(caster.transform.position);
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
