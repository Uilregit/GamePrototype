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
        int armorDamage = 0;
        string targetNames = "|";

        int bonusCast = 0;
        if (card.GetCard().casterColor == Card.CasterColor.Enemy)
            bonusCast += TurnController.turnController.GetEnemyBonusCast();
        else
            bonusCast += TurnController.turnController.GetPlayerBonusCast();

        //If there are bonus casts, then process the card that many more times
        for (int j = 0; j < 1 + bonusCast; j++)
        {
            //Trigger each of the effects on the card
            for (int i = 0; i < effects.Length; i++)
            {
                if (ConditionsMet(caster, targets, card.GetCard().conditionType[i], card.GetCard().conditionValue[i]))
                {
                    card.GetCard().SetPreviousConditionTrue(true);

                    List<GameObject> t = new List<GameObject>();
                    List<Vector2> locs = new List<Vector2>();

                    if (caster.GetComponent<HealthController>().GetTauntedTarget() != null)
                        t = GridController.gridController.GetObjectAtLocation(targets, new string[] { caster.GetComponent<HealthController>().GetTauntedTarget().tag });    //If the caster is taunted, then it casts to the target's tag instead
                    else
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

                    if (card.GetCard().targetType[i] == Card.TargetType.None || card.GetCard().castType == Card.CastType.EmptySpace)
                        locs = targets;
                    else
                        foreach (GameObject obj in t)
                            locs.Add(obj.transform.position);
                    int damage = effects[i].GetSimulatedVitDamage(caster, this, t, card.GetCard(), i);
                    vitDamage += damage;
                    armorDamage += effects[i].GetSimulatedArmorDamage(caster, this, t, card.GetCard(), i);

                    float minHealthPercentage = 1;
                    bool hasPlayer = false;
                    foreach (Vector2 targ in locs)
                        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(targ))
                        {
                            if (card.GetCard().hitEffect[i] == Card.HitEffect.PlayerAttack)
                            {
                                if (damage <= 5)
                                    obj.GetComponent<HealthController>().charDisplay.hitEffectAnim.SetTrigger("PlayerAttack");
                                else if (damage <= 20)
                                    obj.GetComponent<HealthController>().charDisplay.hitEffectAnim.SetTrigger("MediumImpact");
                                else
                                    obj.GetComponent<HealthController>().charDisplay.hitEffectAnim.SetTrigger("LargeImpact");
                            }
                            else if (card.GetCard().hitEffect[i] == Card.HitEffect.MagicAttack)
                            {
                                if (damage <= 5)
                                    obj.GetComponent<HealthController>().charDisplay.hitEffectAnim.SetTrigger("SmallMagic");
                                else if (damage <= 20)
                                    obj.GetComponent<HealthController>().charDisplay.hitEffectAnim.SetTrigger("MediumMagic");
                                else
                                    obj.GetComponent<HealthController>().charDisplay.hitEffectAnim.SetTrigger("LargeMagic");
                            }
                            else
                                obj.GetComponent<HealthController>().charDisplay.hitEffectAnim.SetTrigger(card.GetCard().hitEffect[i].ToString());

                            if (obj.GetComponent<PlayerController>() != null)
                            {
                                hasPlayer = true;
                                if (effects[i].GetSimulatedVitDamage(caster, this, new List<GameObject>() { obj }, card.GetCard(), i) > 0)
                                {
                                    float healthPercentage = (float)(obj.GetComponent<HealthController>().GetCurrentVit() - damage) / (float)obj.GetComponent<HealthController>().GetMaxVit();
                                    if (healthPercentage < minHealthPercentage)
                                        minHealthPercentage = healthPercentage;
                                }
                            }
                        }

                    if (hasPlayer && damage > 0)
                    {
                        GetComponent<PlayerController>();
                        GameController.gameController.SetDamageOverlay(minHealthPercentage);
                    }

                    switch (card.GetCard().hitEffect[i])
                    {
                        case Card.HitEffect.PlayerAttack:
                            CameraController.camera.ScreenShake(Mathf.Lerp(0.03f, 0.5f, 0.04f * vitDamage - 0.2f), 0.4f);
                            break;
                        case Card.HitEffect.EnemyAttack:
                            CameraController.camera.ScreenShake(Mathf.Lerp(0.03f, 0.5f, 0.04f * vitDamage - 0.2f), 0.4f);
                            break;
                        case Card.HitEffect.None:
                            break;
                        default:
                            CameraController.camera.ScreenShake(0.06f, 0.3f);
                            break;
                    }

                    yield return new WaitForSeconds(0.3f);

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
                                                    armorDamage.ToString(),
                                                    card.GetCard().energyCost.ToString(),
                                                    card.GetCard().manaCost.ToString(),
                                                    "0");
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
                return GridController.gridController.GetObjectAtLocation(targets).Any(x => x.GetComponent<HealthController>().GetCurrentArmor() == 0);
            case Card.ConditionType.TargetNotBroken:
                return GridController.gridController.GetObjectAtLocation(targets).Any(x => x.GetComponent<HealthController>().GetCurrentArmor() > 0);
            case Card.ConditionType.TargetAttackingCaster:
                try
                {
                    foreach (GameObject targ in GridController.gridController.GetObjectAtLocation(targets))
                        if (targ.GetComponent<EnemyController>().GetCurrentTarget() == caster)
                            return true;
                }
                catch { }
                return false;
            case Card.ConditionType.TargetNotAttackingCaster:
                try
                {
                    foreach (GameObject targ in GridController.gridController.GetObjectAtLocation(targets))
                        if (targ.GetComponent<EnemyController>().GetCurrentTarget() == caster)
                            return false;
                }
                catch { }
                return true;
            case Card.ConditionType.CasterBroken:
                return caster.GetComponent<HealthController>().GetCurrentArmor() == 0;
            case Card.ConditionType.CasterNotBroken:
                return caster.GetComponent<HealthController>().GetCurrentArmor() > 0;
            case Card.ConditionType.PreviousEffectSuccessful:
                return card.GetCard().GetPreviousEffectSuccessful();
            case Card.ConditionType.CasterHasHigherArmor:
                targs = GridController.gridController.GetObjectAtLocation(targets);
                int minArmor = 9999999;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetArmor() < minArmor)
                        minArmor = obj.GetComponent<HealthController>().GetArmor();
                return caster.GetComponent<HealthController>().GetArmor() > minArmor;
            case Card.ConditionType.CasterHasLowerArmor:
                targs = GridController.gridController.GetObjectAtLocation(targets);
                int maxarmor = 0;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetArmor() > maxarmor)
                        maxarmor = obj.GetComponent<HealthController>().GetArmor();
                return caster.GetComponent<HealthController>().GetArmor() < maxarmor;
            case Card.ConditionType.CasterHasBonusATK:
                return caster.GetComponent<HealthController>().GetBonusAttack() != 0;
            case Card.ConditionType.CasterHasNoBonusATK:
                return caster.GetComponent<HealthController>().GetBonusAttack() == 0;
            case Card.ConditionType.CasterHasHigherATK:
                targs = GridController.gridController.GetObjectAtLocation(targets);
                int minATK = 999999;
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
