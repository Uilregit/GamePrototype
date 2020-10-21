using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardEffectsController : MonoBehaviour
{
    private CardController card;
    private Effect[] effects;
    private Vector2 castLocation;

    public void SetCard(CardController info)
    {
        card = info;
        EffectFactory factory = new EffectFactory();
        effects = factory.GetEffects(info.GetCard().cardEffectName);
    }

    public CardController GetCard()
    {
        return card;
    }

    public void SetCastLocation(Vector2 loc)
    {
        castLocation = loc;
    }

    public Vector2 GetCastLocation()
    {
        return castLocation;
    }

    public IEnumerator TriggerEffect(GameObject caster, List<Vector2> targets)
    {
        int vitDamage = 0;
        int shieldDamage = 0;
        string targetNames = "|";

        //Trigger each of the effects on the card
        for (int i = 0; i < effects.Length; i++)
        {
            if (ConditionsMet(caster, targets, card.GetCard().conditionType[i], card.GetCard().conditionValue[i]) && card.GetCard().conditionType[i] != Card.ConditionType.OnPlay)
            {
                card.GetCard().SetPreviousConditionTrue(true);

                List<GameObject> t = new List<GameObject>();
                List<Vector2> locs = new List<Vector2>();

                switch (card.GetCard().targetType[i])
                {
                    //If the target of the effect is the self
                    case Card.TargetType.Self:
                        t.Add(caster);
                        break;
                    //If the target of the effect is not the self
                    case Card.TargetType.AllEnemies:
                        t = GridController.gridController.GetObjectAtLocation(targets, new string[] { "Enemy" });
                        break;
                    case Card.TargetType.AllPlayers:
                        t = GridController.gridController.GetObjectAtLocation(targets, new string[] { "Player" });
                        break;
                    case Card.TargetType.Any:
                        t = GridController.gridController.GetObjectAtLocation(targets);
                        break;
                    case Card.TargetType.Enemy:
                        t = GridController.gridController.GetObjectAtLocation(targets, new string[] { "Enemy" });
                        break;
                    case Card.TargetType.None:
                        break;
                    case Card.TargetType.Player:
                        t = GridController.gridController.GetObjectAtLocation(targets, new string[] { "Player" });
                        break;
                }

                foreach (GameObject obj in t)
                    targetNames += obj.name + "|";

                try
                {
                    if (card.GetCard().targetType[i] == Card.TargetType.Enemy)
                    {
                        GameObject tauntedTarget = caster.GetComponent<EnemyController>().GetTaunt();
                        if (tauntedTarget != null && targets.Contains(tauntedTarget.transform.position))    //Will always cast on the taunted target regardless of cast type
                            t.Add(tauntedTarget);
                    }
                }
                catch { }

                if (card.GetCard().targetType[i] == Card.TargetType.None || card.GetCard().castType == Card.CastType.EmptySpace)
                    locs = targets;
                else
                    foreach (GameObject obj in t)
                        locs.Add(obj.transform.position);
                vitDamage += effects[i].GetSimulatedVitDamage(caster, this, t, card.GetCard(), i);
                shieldDamage += effects[i].GetSimulatedShieldDamage(caster, this, t, card.GetCard(), i);

                if (card.GetCard().cardEffectName.Length > i + 1 && card.GetCard().cardEffectName[i + 1] == Card.EffectType.ForcedMovement)
                    StartCoroutine(effects[i].Process(caster, this, locs, card.GetCard(), i));
                else
                    yield return StartCoroutine(effects[i].Process(caster, this, locs, card.GetCard(), i));

                if (card.GetCard().cardEffectName[i] == Card.EffectType.CreateObject)
                    i += 1;
            }
            else
            {
                card.GetCard().SetPreviousConditionTrue(false);

                if (card.GetCard().cardEffectName[i] == Card.EffectType.CreateObject)
                {
                    i += 2;
                    if (i < effects.Length)
                        break;
                }
            }
        }

        caster.GetComponent<BuffController>().StartCoroutine(caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardPlayed, caster.GetComponent<HealthController>(), 1));

        if (card.GetCard().casterColor != Card.CasterColor.Enemy)
            DeckController.deckController.ReportUsedCard(card);

        InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                                    InformationLogger.infoLogger.gameID,
                                                    RoomController.roomController.selectedLevel.ToString(),
                                                    RoomController.roomController.roomName,
                                                    TurnController.turnController.turnID.ToString(),
                                                    TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                                    card.GetCard().casterColor.ToString(),
                                                    card.GetCard().name,
                                                    "False",
                                                    "False",
                                                    "False",
                                                    "True",
                                                    caster.name,
                                                    targets.Count.ToString(),
                                                    targetNames,
                                                    vitDamage.ToString(),
                                                    shieldDamage.ToString(),
                                                    card.GetCard().energyCost.ToString(),
                                                    card.GetCard().manaCost.ToString());
    }

    public void TriggerOnPlayEffect(GameObject caster, List<Vector2> targets)
    {
        //Trigger each of the effects on the card
        for (int i = 0; i < effects.Length; i++)
        {
            if (card.GetCard().conditionType[i] == Card.ConditionType.OnPlay)
            {
                switch (card.GetCard().targetType[i])
                {
                    //If the target of the effect is the self
                    case Card.TargetType.Self:
                        effects[i].Process(caster, this, new List<Vector2>() { caster.transform.position }, card.GetCard(), i);
                        break;
                    //If the target of the effect is not the self
                    default:
                        effects[i].Process(caster, this, targets, card.GetCard(), i);
                        break;
                }
                if (card.GetCard().cardEffectName[i] == Card.EffectType.CreateObject)
                    i += 1;
            }
        }
    }

    public SimHealthController SimulateTriggerEffect(GameObject caster, Vector2 targetLocation, SimHealthController simH)
    {
        SimHealthController output = new SimHealthController();
        //Trigger each of the effects on the card
        for (int i = 0; i < effects.Length; i++)
            switch (card.GetCard().targetType[i])
            {
                //If the target of the effect is the self
                case Card.TargetType.Self:
                    output.SetValues(effects[i].SimulateProcess(caster, this, caster.transform.position, card.GetCard().effectValue[i], card.GetCard().effectDuration[i], simH));
                    break;
                //If the target of the effect is not the self
                default:
                    output.SetValues(effects[i].SimulateProcess(caster, this, targetLocation, card.GetCard().effectValue[i], card.GetCard().effectDuration[i], simH));
                    break;
            }
        return output;
    }

    private bool ConditionsMet(GameObject caster, List<Vector2> targets, Card.ConditionType condition, int value)
    {
        List<GameObject> targs = new List<GameObject>();
        switch (condition)
        {
            case Card.ConditionType.None:
                return true;
            case Card.ConditionType.Even:
                return TurnController.turnController.GetNumerOfCardsPlayedInTurn() % 2 == 0;
            case Card.ConditionType.Odd:
                return TurnController.turnController.GetNumerOfCardsPlayedInTurn() % 2 == 1;
            case Card.ConditionType.TargetBroken:
                return GridController.gridController.GetObjectAtLocation(targets).Any(x => x.GetComponent<HealthController>().GetCurrentShield() == 0);
            case Card.ConditionType.TargetNotBroken:
                return GridController.gridController.GetObjectAtLocation(targets).Any(x => x.GetComponent<HealthController>().GetCurrentShield() > 0);
            case Card.ConditionType.TargetAttackingCaster:
                try
                {
                    foreach (GameObject targ in GridController.gridController.GetObjectAtLocation(targets))
                        if (targ.GetComponent<EnemyController>().GetTarget() == caster)
                            return true;
                }
                catch { }
                return false;
            case Card.ConditionType.TargetNotAttackingCaster:
                try
                {
                    foreach (GameObject targ in GridController.gridController.GetObjectAtLocation(targets))
                        if (targ.GetComponent<EnemyController>().GetTarget() == caster)
                            return false;
                }
                catch { }
                return true;
            case Card.ConditionType.CasterBroken:
                return caster.GetComponent<HealthController>().GetCurrentShield() == 0;
            case Card.ConditionType.CasterNotBroken:
                return caster.GetComponent<HealthController>().GetCurrentShield() > 0;
            case Card.ConditionType.PreviousEffectSuccessful:
                return card.GetCard().GetPreviousEffectSuccessful();
            case Card.ConditionType.CasterHasHigherShield:
                targs = GridController.gridController.GetObjectAtLocation(targets);
                int minShield = 9999999;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetShield() < minShield)
                        minShield = obj.GetComponent<HealthController>().GetShield();
                return caster.GetComponent<HealthController>().GetShield() > minShield;
            case Card.ConditionType.CasterHasLowerShield:
                targs = GridController.gridController.GetObjectAtLocation(targets);
                int maxshield = 0;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetShield() > maxshield)
                        maxshield = obj.GetComponent<HealthController>().GetShield();
                return caster.GetComponent<HealthController>().GetShield() < maxshield;
            case Card.ConditionType.CasterHasBonusATK:
                return caster.GetComponent<HealthController>().GetBonusAttack() != 0;
            case Card.ConditionType.CasterHasNoBonusATK:
                return caster.GetComponent<HealthController>().GetBonusAttack() == 0;
            case Card.ConditionType.CasterHasHigherATK:
                targs = GridController.gridController.GetObjectAtLocation(targets);
                int minATK = 0;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetAttack() < minATK)
                        minATK = obj.GetComponent<HealthController>().GetAttack();
                return caster.GetComponent<HealthController>().GetAttack() > minATK;
            case Card.ConditionType.CasterHasLowerATK:
                targs = GridController.gridController.GetObjectAtLocation(targets);
                int maxATK = 0;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetAttack() > maxATK)
                        minATK = obj.GetComponent<HealthController>().GetAttack();
                return caster.GetComponent<HealthController>().GetAttack() < maxATK;
            case Card.ConditionType.Else:
                return !card.GetCard().GetPreviousConditionTrue();
            default:
                return false;
        }
    }

    public int GetSimulatedAttackValue(GameObject caster, List<Vector2> targets)
    {
        int totalAttack = 0;
        //Trigger each of the effects on the card
        for (int i = 0; i < effects.Length; i++)
        {
            if (ConditionsMet(caster, targets, card.GetCard().conditionType[i], card.GetCard().conditionValue[i]) && card.GetCard().conditionType[i] != Card.ConditionType.OnPlay)
            {
                List<Card.EffectType> damageTypes = new List<Card.EffectType> { Card.EffectType.PiercingDamage, Card.EffectType.PiercingDamageAll, Card.EffectType.VitDamage, Card.EffectType.VitDamageAll };
                card.GetCard().SetPreviousConditionTrue(true);
                if (damageTypes.Contains(card.GetCard().cardEffectName[i]))
                    totalAttack += Mathf.CeilToInt(caster.GetComponent<HealthController>().GetAttack() * card.GetCard().effectValue[i] / 100.0f);
            }
            else
            {
                card.GetCard().SetPreviousConditionTrue(false);

                if (card.GetCard().cardEffectName[i] == Card.EffectType.CreateObject)
                {
                    i += 2;
                    if (i < effects.Length)
                        break;
                }
            }
        }

        return totalAttack;
    }
}
