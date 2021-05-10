using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetHighestHealthAlly : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        int highestHealth = -99999;
        GameObject highestHealthAlly = null;
        foreach (GameObject obj in GameController.gameController.GetLivingPlayers())
            if (obj.GetComponent<HealthController>().GetVit() > highestHealth)
            {
                highestHealth = obj.GetComponent<HealthController>().GetVit();
                highestHealthAlly = obj;
            }
        card.SetTempObject(highestHealthAlly);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}