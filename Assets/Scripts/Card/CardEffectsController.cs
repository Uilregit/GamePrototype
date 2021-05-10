using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class CardEffectsController : MonoBehaviour
{
    private CardController card;
    private Effect[] effects;
    private Vector2 castLocation;
    private List<Card.EffectType> nonPauseEffects = new List<Card.EffectType>()
    {
        Card.EffectType.SetDuration,
        Card.EffectType.ModifyTempValue,
        Card.EffectType.GetMissingHealth,
        Card.EffectType.GetBonusHealth,
        Card.EffectType.GetCurrentAttack,
        Card.EffectType.GetCurrentArmor,
        Card.EffectType.GetBonusArmor,
        Card.EffectType.GetDistanceMoved,
        Card.EffectType.GetDamageDoneEffect,
        Card.EffectType.GetNumberOfTargetsInRangeEffect,
        Card.EffectType.GetNumberOfCardsPlayedInTurn,
        Card.EffectType.GetNumberOfAttackers,
        Card.EffectType.GetNumberInStack,
        Card.EffectType.GetNumberOfBuffsOnTarget,
        Card.EffectType.GetDrawnCardEnergy,
        Card.EffectType.GetDrawnCardMana,
        Card.EffectType.GetNumberOfCardsInHand,
        Card.EffectType.GetHighestHealthAlly,
        Card.EffectType.GetManaSpentTurn,
        Card.EffectType.GetEnergySpentTurn
    };

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

    public IEnumerator TriggerEffect(GameObject caster, List<GameObject> targets, List<Vector2> targetLocs, bool propogateOverServer = true, bool isSimulation = false)
    {
        /*
        if (MultiplayerGameController.gameController != null) //Multiplayer component
        {
            if (propogateOverServer)
                ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportCardUsed(caster.GetComponent<NetworkIdentity>().netId.ToString(), card.GetCard().name, targets, ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());
            if (ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == 1)    //Will never play cards on client, always on server
            {
                DeckController.deckController.ReportUsedCard(card);     //Client should still add card to discard pile even if the effect isn't triggered
                yield break;
            }
        }
        */
        int vitDamage = 0;
        int armorDamage = 0;
        string targetNames = "|";

        int bonusCast = 0;
        if (card.GetCard().casterColor == Card.CasterColor.Enemy)
            bonusCast += TurnController.turnController.GetEnemyBonusCast();
        else
            bonusCast += TurnController.turnController.GetPlayerBonusCast();

        List<GameObject> movedObjects = new List<GameObject>();
        //If there are bonus casts, then process the card that many more times
        for (int j = 0; j < 1 + bonusCast; j++)
        {
            //Trigger each of the effects on the card
            for (int i = 0; i < effects.Length; i++)
            {
                if (ConditionsMet(caster, targets, card.GetCard().conditionType[i], card.GetCard().conditionValue[i]))
                {
                    card.GetCard().SetPreviousConditionTrue(true);

                    HealthController simulationCharacter = null;
                    List<GameObject> t = new List<GameObject>();
                    List<Vector2> locs = new List<Vector2>();

                    if (caster.GetComponent<HealthController>().GetTauntedTarget() != null && card.GetCard().cardEffectName[i] != Card.EffectType.CreateObject && card.GetCard().cardEffectName[i] != Card.EffectType.Sacrifice)
                        // t = GridController.gridController.GetObjectAtLocation(targets, new string[] { caster.GetComponent<HealthController>().GetTauntedTarget().tag });    //If the caster is taunted, then it casts to the target's tag instead
                        t = targets.Where(x => x.tag == caster.GetComponent<HealthController>().GetTauntedTarget().tag).ToList();
                    else
                        switch (card.GetCard().targetType[i])
                        {
                            //If the target of the effect is the self
                            case Card.TargetType.Self:
                                if (isSimulation)
                                {
                                    simulationCharacter = GameController.gameController.GetSimulationCharacter(caster.GetComponent<HealthController>(), false);
                                    t.Add(simulationCharacter.gameObject);
                                }
                                else
                                    t.Add(caster);
                                break;
                            //If the target of the effect is not the self
                            case Card.TargetType.AllEnemies:
                                t = targets.Where(x => x.tag == "Enemy").ToList();
                                break;
                            case Card.TargetType.AllPlayers:
                                t = targets.Where(x => x.tag == "Player").ToList();
                                break;
                            case Card.TargetType.Any:
                                t = targets;
                                break;
                            case Card.TargetType.Enemy:
                                t = targets.Where(x => x.tag == "Enemy").ToList();
                                break;
                            case Card.TargetType.Player:
                                t = targets.Where(x => x.tag == "Player").ToList();
                                break;
                            default:
                                t = targets;
                                break;
                        }

                    t.AddRange(movedObjects);       //Add force moved objects to list so they're still affected after the position change

                    if (card.GetCard().cardEffectName[i] == Card.EffectType.ForcedMovement)
                        movedObjects.AddRange(t);

                    foreach (GameObject obj in t)
                        targetNames += obj.name + "|";

                    if (card.GetCard().targetType[i] == Card.TargetType.None || card.GetCard().castType == Card.CastType.EmptySpace)
                        locs = targetLocs;
                    else if (new List<Card.CastType>() { Card.CastType.EmptyTargetedAoE, Card.CastType.TargetedAoE, Card.CastType.AoE }.Contains(card.GetCard().castType) && card.GetCard().targetType[i] == Card.TargetType.Center)
                    {
                        locs = new List<Vector2>() { card.GetCard().GetCenter() };

                        List<GameObject> temp = new List<GameObject>();
                        foreach (GameObject o in t)
                            if ((Vector2)o.transform.position == card.GetCard().GetCenter())
                                temp.Add(o);
                        t = temp;
                    }
                    else if (new List<Card.CastType>() { Card.CastType.EmptyTargetedAoE, Card.CastType.TargetedAoE, Card.CastType.AoE }.Contains(card.GetCard().castType) && card.GetCard().targetType[i] == Card.TargetType.Peripherals)
                    {
                        foreach (Vector2 l in targetLocs)
                            if (l != card.GetCard().GetCenter())
                                locs.Add(l);

                        List<GameObject> temp = new List<GameObject>();
                        foreach (GameObject o in t)
                            if ((Vector2)o.transform.position != card.GetCard().GetCenter())
                                temp.Add(o);
                        t = temp;
                    }
                    else
                    {
                        foreach (GameObject obj in t)
                            locs.Add(obj.transform.position);
                        locs = locs.Distinct().ToList();
                    }
                    int damage = effects[i].GetSimulatedVitDamage(caster, this, t, card.GetCard(), i);
                    vitDamage += damage;
                    armorDamage += effects[i].GetSimulatedArmorDamage(caster, this, t, card.GetCard(), i);

                    float minHealthPercentage = 1;
                    bool hasPlayer = false;
                    foreach (Vector2 targ in locs)
                        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(targ, new string[] { "Player", "Enemy" }))
                        {
                            if (!isSimulation)
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

                    if (hasPlayer && damage > 0 && !isSimulation)
                    {
                        GetComponent<PlayerController>();
                        GameController.gameController.SetDamageOverlay(minHealthPercentage);
                    }

                    if (!isSimulation)
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

                    if (!nonPauseEffects.Contains(card.GetCard().cardEffectName[i]) && !isSimulation)
                        yield return new WaitForSeconds(0.5f);

                    if (isSimulation)
                    {
                        if (simulationCharacter != null)
                            GameController.gameController.ReportSimulationFinished(simulationCharacter);
                        yield return StartCoroutine(effects[i].Process(caster, this, t, card.GetCard(), i, 0));
                    }
                    else
                    {
                        if (card.GetCard().cardEffectName.Length > i + 1 && card.GetCard().cardEffectName[i + 1] == Card.EffectType.ForcedMovement)
                            StartCoroutine(effects[i].Process(caster, this, locs, card.GetCard(), i));
                        else
                            yield return StartCoroutine(effects[i].Process(caster, this, locs, card.GetCard(), i));
                    }

                    if (card.GetCard().cardEffectName[i] == Card.EffectType.CreateObject)
                        i += 1;

                    if (MultiplayerGameController.gameController != null) //Multiplayer component
                        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(locs))
                            ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportHealthController(obj.GetComponent<NetworkIdentity>().netId.ToString(), obj.GetComponent<HealthController>().GetHealthInformation(), ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());
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

        try
        {
            caster.GetComponent<EnemyInformationController>().CardTriggerFinished();        //Used to sync enemy turns with card effect trigger times
        }
        catch { }

        try //Singleplayer
        {
            if (card.GetCard().casterColor != Card.CasterColor.Enemy)       //Only trigger on card played if it's a player card
                foreach (GameObject player in GameController.gameController.GetLivingPlayers())
                    player.GetComponent<BuffController>().StartCoroutine(player.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardPlayed, player.GetComponent<HealthController>(), 1));
        }
        catch  //Multiplayer
        {
            if (card.GetCard().casterColor != Card.CasterColor.Enemy)       //Only trigger on card played if it's a player card
                foreach (GameObject player in MultiplayerGameController.gameController.GetLivingPlayers())
                    player.GetComponent<BuffController>().StartCoroutine(player.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardPlayed, player.GetComponent<HealthController>(), 1));
        }

        if (MultiplayerGameController.gameController != null) //Multiplayer component
        {
            if (TurnController.turnController.GetIsPlayerTurn() && ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == 0)
                DeckController.deckController.ReportUsedCard(card);     //Only allow card to shuffle into discard pile if server played it on server's turn
        }
        else if (card.GetCard().casterColor != Card.CasterColor.Enemy && !isSimulation)  //Singleplayer component
        {
            DeckController.deckController.ReportUsedCard(card);
            AchievementSystem.achieve.OnNotify(vitDamage, StoryRoomSetup.ChallengeType.DamageDealtWithSingeCard);
        }

        try
        {
            if (!isSimulation)
                InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                                            InformationLogger.infoLogger.gameID,
                                                            RoomController.roomController.worldLevel.ToString(),
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
        catch { }
    }

    public IEnumerator TriggerEffect(GameObject caster, List<Vector2> targets, bool propogateOverServer = true, bool hasEffectDelay = false)
    {
        yield return StartCoroutine(TriggerEffect(caster, GridController.gridController.GetObjectAtLocation(targets), targets, propogateOverServer, hasEffectDelay));
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

    private bool ConditionsMet(GameObject caster, List<GameObject> targets, Card.ConditionType condition, int value)
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
                return targets.Any(x => x.GetComponent<HealthController>().GetCurrentArmor() == 0);
            case Card.ConditionType.TargetNotBroken:
                return targets.Any(x => x.GetComponent<HealthController>().GetCurrentArmor() > 0);
            case Card.ConditionType.TargetAttackingCaster:
                try
                {
                    foreach (GameObject targ in targets)
                        if (targ.GetComponent<EnemyController>().GetCurrentTarget() == caster)
                            return true;
                }
                catch { }
                return false;
            case Card.ConditionType.TargetNotAttackingCaster:
                try
                {
                    foreach (GameObject targ in targets)
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
                targs = targets;
                int minArmor = 9999999;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetArmor() < minArmor)
                        minArmor = obj.GetComponent<HealthController>().GetArmor();
                return caster.GetComponent<HealthController>().GetArmor() > minArmor;
            case Card.ConditionType.CasterHasLowerArmor:
                targs = targets;
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
                targs = targets;
                int minATK = 999999;
                foreach (GameObject obj in targs)
                    if (obj.GetComponent<HealthController>().GetAttack() < minATK)
                        minATK = obj.GetComponent<HealthController>().GetAttack();
                return caster.GetComponent<HealthController>().GetAttack() > minATK;
            case Card.ConditionType.CasterHasLowerATK:
                targs = targets;
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

    private bool ConditionsMet(GameObject caster, List<Vector2> targets, Card.ConditionType condition, int value)
    {
        return ConditionsMet(caster, GridController.gridController.GetObjectAtLocation(targets), condition, value);
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
