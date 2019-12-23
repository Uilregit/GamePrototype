using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectsController : MonoBehaviour
{
    private Card card;
    private Effect[] effects;

    public void SetCard(Card info)
    {
        card = info;
        EffectFactory factory = new EffectFactory();
        effects = factory.GetEffects(info.cardEffectName);
    }

    public Card GetCard()
    {
        return card;
    }

    public void TriggerEffect(GameObject caster, Vector2 targetLocation)
    {
        //Trigger each of the effects on the card
        for (int i = 0; i < effects.Length; i++)
        {
            if (ConditionsMet(card.conditionType[i], card.conditionValue[i]))
            {
                switch (card.targetType[i])
                {
                    //If the target of the effect is the self
                    case Card.TargetType.Self:
                        effects[i].Process(caster, this, caster.transform.position, card, i);
                        break;
                    //If the target of the effect is not the self
                    default:
                        effects[i].Process(caster, this, targetLocation, card, i);
                        break;
                }
                if (card.cardEffectName[i] == Card.EffectType.CreateObject)
                    i += 1;
            }
            else if (card.cardEffectName[i] == Card.EffectType.CreateObject)
            {
                i += 2;
                if (i < effects.Length)
                    break;
            }
        }
        if (card.casterColor != Card.CasterColor.Enemy)
            DeckController.deckController.ReportUsedCard(card);
    }

    public SimHealthController SimulateTriggerEffect(GameObject caster, Vector2 targetLocation, SimHealthController simH)
    {
        SimHealthController output = new SimHealthController();
        //Trigger each of the effects on the card
        for (int i = 0; i < effects.Length; i++)
            switch (card.targetType[i])
            {
                //If the target of the effect is the self
                case Card.TargetType.Self:
                    output.SetValues(effects[i].SimulateProcess(caster, this, caster.transform.position, card.effectValue[i], card.effectDuration[i], simH));
                    break;
                //If the target of the effect is not the self
                default:
                    output.SetValues(effects[i].SimulateProcess(caster, this, targetLocation, card.effectValue[i], card.effectDuration[i], simH));
                    break;
            }
        return output;
    }

    private bool ConditionsMet(Card.ConditionType condition, int value)
    {
        switch (condition)
        {
            case Card.ConditionType.None:
                return true;
            case Card.ConditionType.Even:
                return TurnController.turnController.GetNumerOfCardsPlayedInTurn() % 2 == 0;
            case Card.ConditionType.Odd:
                return TurnController.turnController.GetNumerOfCardsPlayedInTurn() % 2 == 1;
            default:
                return false;
        }
    }
}
