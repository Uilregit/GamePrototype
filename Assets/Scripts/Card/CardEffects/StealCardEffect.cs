using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealCardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach(GameObject targ in target)
        {
            Card c = targ.GetComponent<EnemyController>().GetCard()[0].GetCard().GetCopy();
            c.casterColor = caster.GetComponent<PlayerController>().GetColorTag();
            switch(c.castType)
            {
                case Card.CastType.Player:
                    c.castType = Card.CastType.Any;
                    break;
                case Card.CastType.Enemy:
                    c.castType = Card.CastType.Player;
                    break;
            }
            for (int i = 0; i < c.targetType.Length; i++)
                switch(c.targetType[i])
                {
                    case Card.TargetType.Player:
                        c.targetType[i] = Card.TargetType.Any;
                        break;
                    case Card.TargetType.Enemy:
                        c.targetType[i] = Card.TargetType.Player;
                        break;
                }
            c.exhaust = true;

            CardController cc = HandController.handController.gameObject.AddComponent<CardController>();
            cc.SetCard(c, true, false);
            HandController.handController.CreateSpecificCard(cc);
        }
        yield return HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    /*
    public override void RelicProcess(List<GameObject> targets, Card.BuffType buffType, int effectValue, int effectDuration)
    {
        for (int i = 0; i < effectValue; i++) //Draw effectValue number of random cards
            HandController.handController.DrawAnyCard();
    }
    */
}