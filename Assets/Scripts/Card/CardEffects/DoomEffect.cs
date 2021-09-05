using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoomEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        GameController.gameController.ChangeDoomCounter(-1);
        if (GameController.gameController.GetDoomCounter() <= 0)
            foreach (GameObject player in GameController.gameController.GetLivingPlayers())
            {
                player.GetComponent<HealthController>().charDisplay.charAnimController.TriggerDeath();
                player.GetComponent<HealthController>().SetInstantKill();
            }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
