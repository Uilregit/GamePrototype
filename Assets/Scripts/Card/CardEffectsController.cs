﻿using System.Collections;
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
    private bool finished = false;

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

    public IEnumerator TriggerEffect(GameObject caster, List<GameObject> targets, List<Vector2> targetLocs, bool propogateOverServer = true, bool isSimulation = false, GameObject simulatedSelf = null)
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
        int numEnemiesAlive = 0;
        string targetNames = "|";

        bool blockedByImmune = false;

        //Stats tracking for achievement purposes
        bool playerDifferentThanCaster = false;
        int enemiesTotalVit = 0;
        int enemiesTotalArmor = 0;
        List<GameObject> allEnemies = TurnController.turnController.GetEnemies().Select(x => x.gameObject).ToList();
        foreach (GameObject o in allEnemies)
        {
            enemiesTotalVit += o.GetComponent<HealthController>().GetVit();
            enemiesTotalArmor += o.GetComponent<HealthController>().GetArmor();
            if (o.GetComponent<HealthController>().GetVit() > 0)
                numEnemiesAlive++;
        }

        int playerTotalVit = 0;
        int playerTotalArmor = 0;
        List<GameObject> allPlayers = GameController.gameController.GetLivingPlayers();
        foreach (GameObject o in allPlayers)
        {
            playerTotalVit += o.GetComponent<HealthController>().GetVit();
            playerTotalArmor += o.GetComponent<HealthController>().GetArmor();
        }

        int bonusCast = 0;
        if (card.GetCard().casterColor == Card.CasterColor.Enemy)
            bonusCast += TurnController.turnController.GetEnemyBonusCast();
        else
            bonusCast += TurnController.turnController.GetPlayerBonusCast();

        if (!isSimulation && card.GetCard().casterColor != Card.CasterColor.Enemy)
        {
            List<GameObject> allChars = GameController.gameController.GetLivingPlayers();
            allChars.AddRange(TurnController.turnController.GetEnemies().Select(x => x.gameObject));
            foreach (AbilitiesController abilities in allChars.Select(x => x.GetComponent<AbilitiesController>()))
                abilities.TriggerAbilities(AbilitiesController.TriggerType.BeforePlayerCardCast);
        }

        List<GameObject> movedObjects = new List<GameObject>();

        //Triggers equipment effects before trigger if there are any
        if (card.GetAttachedEquipment() != null && card.GetAttachedEquipment().beforeTriggerCard != null)
            yield return StartCoroutine(TriggerEquipmentEffect(caster, targets, targetLocs, movedObjects, isSimulation, true, simulatedSelf));

        //If there are bonus casts, then process the card that many more times
        for (int j = 0; j < 1 + bonusCast; j++)
        {
            //Trigger each of the effects on the card
            for (int i = 0; i < effects.Length; i++)
            {
                if (ConditionsMet(caster, targets, card.GetCard().conditionType[i], card.GetCard().conditionValue[i]))
                {
                    card.GetCard().SetPreviousConditionTrue(true);

                    int targetTotalVit = 0;

                    //HealthController simulationCharacter = null;
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
                                    t.Add(simulatedSelf);
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

                    if (!nonPauseEffects.Contains(card.GetCard().cardEffectName[i]) && !isSimulation)
                        yield return new WaitForSeconds(0.5f);

                    //Used to find out exactly how much damage was dealt
                    List<GameObject> targetedObjects = new List<GameObject>();
                    foreach (Vector2 l in locs)
                    {
                        foreach (GameObject o in GridController.gridController.GetObjectAtLocation(l, new string[] { "Player", "Enemy" }))
                        {
                            targetedObjects.Add(o);
                            targetTotalVit += o.GetComponent<HealthController>().GetVit();
                        }
                    }

                    if (isSimulation)
                        yield return StartCoroutine(effects[i].ProcessCard(caster, this, t, card.GetCard(), i, 0));
                    else
                    {
                        foreach (GameObject obj in movedObjects)
                            locs.Add(obj.transform.position);
                        locs = locs.Distinct().ToList();

                        //Used for achievement based on card blocked
                        if (card.GetCard().casterColor != Card.CasterColor.Enemy && !blockedByImmune)
                        {
                            List<GameObject> objs = GridController.gridController.GetObjectAtLocation(locs, new string[] { "Player", "Enemy" });
                            if (objs.Count != effects[i].CheckImmunity(caster, this, objs, card.GetCard(), i, 0).Count)
                                blockedByImmune = true;
                        }

                        if (effects[i].CheckImmunity(caster, this, GridController.gridController.GetObjectAtLocation(locs), card.GetCard(), i, 0).Count != 0)
                            caster.GetComponent<HealthController>().charDisplay.onHitSoundController.PlaySound(card.GetCard().soundEffect[i]);

                        if (card.GetCard().cardEffectName.Length > i + 1 && card.GetCard().cardEffectName[i + 1] == Card.EffectType.ForcedMovement)
                            StartCoroutine(effects[i].ProcessCard(caster, this, locs, card.GetCard(), i));
                        else
                            yield return StartCoroutine(effects[i].ProcessCard(caster, this, locs, card.GetCard(), i));
                    }

                    int damage = 0;
                    if (!isSimulation)
                    {
                        int newTotalArmor = 0;
                        int newTotalVit = 0;
                        foreach (GameObject o in targetedObjects)
                        {
                            newTotalArmor += o.GetComponent<HealthController>().GetArmor();
                            newTotalVit += o.GetComponent<HealthController>().GetVit();
                        }

                        damage = targetTotalVit - newTotalVit;
                        vitDamage += damage;
                    }

                    if (!isSimulation)
                        switch (card.GetCard().hitEffect[i])
                        {
                            case Card.HitEffect.PlayerAttack:
                                CameraController.camera.ScreenShake(Mathf.Lerp(0.03f, 0.5f, 0.04f * damage - 0.2f), 0.4f);
                                break;
                            case Card.HitEffect.EnemyAttack:
                                CameraController.camera.ScreenShake(Mathf.Lerp(0.03f, 0.5f, 0.04f * damage - 0.2f), 0.4f);
                                break;
                            case Card.HitEffect.None:
                                break;
                            default:
                                CameraController.camera.ScreenShake(0.06f, 0.3f);
                                break;
                        }

                    float minHealthPercentage = 1;
                    bool hasPlayer = false;
                    foreach (Vector2 targ in locs)
                        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(targ, new string[] { "Player", "Enemy" }))
                        {
                            if (!isSimulation)
                            {

                                if (card.GetCard().hitEffect[i] == Card.HitEffect.PlayerAttack)
                                {
                                    if (damage <= 5)
                                        obj.GetComponent<HealthController>().charDisplay.TriggerOnHitEffect(Card.HitEffect.PlayerAttack, "PlayerAttack");
                                    else if (damage <= 20)
                                        obj.GetComponent<HealthController>().charDisplay.TriggerOnHitEffect(Card.HitEffect.PlayerAttack, "MediumImpact");
                                    else
                                        obj.GetComponent<HealthController>().charDisplay.TriggerOnHitEffect(Card.HitEffect.PlayerAttack, "LargeImpact");
                                }
                                else if (card.GetCard().hitEffect[i] == Card.HitEffect.MagicAttack)
                                {
                                    if (damage <= 5)
                                        obj.GetComponent<HealthController>().charDisplay.TriggerOnHitEffect(Card.HitEffect.MagicAttack, "SmallMagic");
                                    else if (damage <= 20)
                                        obj.GetComponent<HealthController>().charDisplay.TriggerOnHitEffect(Card.HitEffect.MagicAttack, "MediumMagic");
                                    else
                                        obj.GetComponent<HealthController>().charDisplay.TriggerOnHitEffect(Card.HitEffect.MagicAttack, "LargeMagic");
                                }
                                else
                                    obj.GetComponent<HealthController>().charDisplay.TriggerOnHitEffect(card.GetCard().hitEffect[i]);

                                if (obj.GetComponent<PlayerController>() != null)
                                {
                                    hasPlayer = true;
                                    if (obj.GetComponent<PlayerController>().GetColorTag() != card.GetCard().casterColor)
                                        playerDifferentThanCaster = true;
                                    if (damage > 0)
                                    {
                                        float healthPercentage = (float)(obj.GetComponent<HealthController>().GetCurrentVit() - damage) / (float)obj.GetComponent<HealthController>().GetMaxVit();
                                        if (healthPercentage < minHealthPercentage)
                                            minHealthPercentage = healthPercentage;
                                    }
                                }
                            }
                        }

                    if (hasPlayer && damage > 0 && !isSimulation)
                    {
                        GetComponent<PlayerController>();
                        GameController.gameController.SetDamageOverlay(minHealthPercentage);
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

        //Triggers equipment effects before trigger if there are any
        if (card.GetAttachedEquipment() != null && card.GetAttachedEquipment().afterTriggerCard != null)
            yield return StartCoroutine(TriggerEquipmentEffect(caster, targets, targetLocs, movedObjects, isSimulation, false, simulatedSelf));

        //Calculate final damage for achievement purposes
        int enemiesFinalTotalVit = 0;
        int enemiesFinalTotalArmor = 0;
        int finalNumEneiesAlive = 0;
        foreach (GameObject o in allEnemies)
        {
            enemiesFinalTotalVit += o.GetComponent<HealthController>().GetVit();
            enemiesFinalTotalArmor += o.GetComponent<HealthController>().GetArmor();
            if (o.GetComponent<HealthController>().GetVit() > 0)
                finalNumEneiesAlive++;
        }

        int playerFinalTotalVit = 0;
        int playerFinalTotalArmor = 0;
        foreach (GameObject o in allPlayers)
        {
            playerFinalTotalVit += o.GetComponent<HealthController>().GetVit();
            playerFinalTotalArmor += o.GetComponent<HealthController>().GetArmor();
        }

        if (!isSimulation && card.GetCard().casterColor != Card.CasterColor.Enemy)
        {
            List<GameObject> allChars = GameController.gameController.GetLivingPlayers();
            allChars.AddRange(TurnController.turnController.GetEnemies().Select(x => x.gameObject));
            foreach (AbilitiesController abilities in allChars.Select(x => x.GetComponent<AbilitiesController>()))
                abilities.TriggerAbilities(AbilitiesController.TriggerType.AfterPlayerCardCast);

            AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.UseLessThanXCards);
            if (playerTotalVit + enemiesTotalVit - playerFinalTotalVit - enemiesFinalTotalVit > 0)
                AchievementSystem.achieve.OnNotify(playerTotalVit + enemiesTotalVit - playerFinalTotalVit - enemiesFinalTotalVit, StoryRoomSetup.ChallengeType.DamageDealtWithSingeCard);
            if (enemiesTotalArmor - enemiesFinalTotalArmor > 0)
            {
                AchievementSystem.achieve.OnNotify(enemiesTotalArmor - enemiesFinalTotalArmor, StoryRoomSetup.ChallengeType.AromrRemovedWithSingleCard);
                AchievementSystem.achieve.OnNotify(enemiesTotalArmor - enemiesFinalTotalArmor, StoryRoomSetup.ChallengeType.ArmorRemovedFromEnemy);
            }
            if (numEnemiesAlive - finalNumEneiesAlive > 0)
                AchievementSystem.achieve.OnNotify(numEnemiesAlive - finalNumEneiesAlive, StoryRoomSetup.ChallengeType.KillWithSingleCard);
            if (playerDifferentThanCaster)
                AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.CastOnAnotherally);
            if (blockedByImmune)
                AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.XNumOfImmunedCards);
        }

        try
        {
            caster.GetComponent<EnemyInformationController>().CardTriggerFinished();        //Used to sync enemy turns with card effect trigger times
        }
        catch
        {
            if (!isSimulation)
                caster.GetComponent<PlayerMoveController>().ReportCast();                   //If not a simulation cast, report for achievement purposes
        }

        try //Singleplayer
        {
            if (card.GetCard().casterColor != Card.CasterColor.Enemy && !isSimulation)       //Only trigger on card played if it's a player card
                foreach (GameObject player in GameController.gameController.GetLivingPlayers())
                    player.GetComponent<BuffController>().StartCoroutine(player.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardPlayed, player.GetComponent<HealthController>(), 1));
        }
        catch  //Multiplayer
        {
            if (card.GetCard().casterColor != Card.CasterColor.Enemy && !isSimulation)       //Only trigger on card played if it's a player card
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
        }

        finished = true;

        try
        {
            string equipmentName = "";
            if (card.GetAttachedEquipment() != null)
                equipmentName = card.GetAttachedEquipment().equipmentName;
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
                                                            (playerTotalVit + enemiesTotalVit - playerFinalTotalVit - enemiesFinalTotalVit).ToString(),
                                                            (playerTotalArmor + enemiesTotalArmor - playerFinalTotalArmor - enemiesFinalTotalArmor).ToString(),
                                                            card.GetCard().energyCost.ToString(),
                                                            card.GetCard().manaCost.ToString(),
                                                            "0",
                                                            equipmentName);
        }
        catch { }
    }

    public IEnumerator TriggerEquipmentEffect(GameObject caster, List<GameObject> targets, List<Vector2> targetLocs, List<GameObject> movedObjects, bool isSimulation, bool isBeforeCard, GameObject simulatedSelf)
    {
        if (card.isResurrectCard)
            yield break;

        //HealthController simulationCharacter = null;
        EffectFactory factory = new EffectFactory();
        Card equipmentCard = card.GetAttachedEquipment().beforeTriggerCard;
        if (!isBeforeCard)
            equipmentCard = card.GetAttachedEquipment().afterTriggerCard;
        Effect[] weaponEffects = factory.GetEffects(equipmentCard.cardEffectName);
        for (int i = 0; i < equipmentCard.cardEffectName.Length; i++)
        {
            List<Vector2> locs = new List<Vector2>();
            List<GameObject> t = new List<GameObject>();
            if (equipmentCard.targetType[i] == Card.TargetType.Self)
            {
                if (isSimulation)
                    t.Add(simulatedSelf);
                else
                {
                    locs.Add(caster.transform.position);
                    t.Add(caster);
                }
            }
            else
            {
                locs = targetLocs;
                t.AddRange(targets);
            }

            foreach (GameObject obj in movedObjects)
                locs.Add(obj.transform.position);
            locs = locs.Distinct().ToList();

            if (equipmentCard.cardEffectName[i] == Card.EffectType.ForcedMovement)
                movedObjects.AddRange(t);
            if (!isSimulation)
            {
                yield return StartCoroutine(weaponEffects[i].ProcessCard(caster, this, locs, equipmentCard, i));
            }
            else
            {
                yield return StartCoroutine(weaponEffects[i].ProcessCard(caster, this, t, equipmentCard, i, 0));
            }
        }
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
                        effects[i].ProcessCard(caster, this, new List<Vector2>() { caster.transform.position }, card.GetCard(), i);
                        break;
                    //If the target of the effect is not the self
                    default:
                        effects[i].ProcessCard(caster, this, targets, card.GetCard(), i);
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

    public bool GetFinished()
    {
        return finished;
    }
}
