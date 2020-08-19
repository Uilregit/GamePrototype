using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNumberOfTargetsInRangeEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        string[] targetTag = new string[] { "Player", "Enemy" };
        int size = caster.GetComponent<HealthController>().size;
        switch (card.targetType[effectIndex])
        {
            case Card.TargetType.Enemy:
                targetTag = new string[] { "Enemy" };
                break;
            case Card.TargetType.AllEnemies:
                targetTag = new string[] { "Enemy" };
                break;
            case Card.TargetType.Player:
                targetTag = new string[] { "Player" };
                break;
            case Card.TargetType.AllPlayers:
                targetTag = new string[] { "Player" };
                break;
            case Card.TargetType.Any:
                targetTag = new string[] { "Player", "Enemy" };
                break;
            case Card.TargetType.Self:
                try
                {
                    caster.GetComponent<EnemyController>();
                    targetTag = new string[] { "Enemy" };
                }
                catch
                {
                    targetTag = new string[] { "Player" };
                }
                break;
            default:
                targetTag = new string[] { "xxxxxxxxxxxxxxxx" };
                break;
        }
        List<GameObject> objects = GridController.gridController.GetObjectsInAoE(caster.transform.position, card.radius, targetTag);
        if (objects.Contains(caster))
            objects.Remove(caster);
        effectController.GetCard().GetCard().SetTempEffectValue(objects.Count);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
