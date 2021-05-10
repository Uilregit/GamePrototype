using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

[CreateAssetMenu]
public class Relic : ScriptableObject
{
    public Card.Rarity rarity;
    public string relicName;

    [TextArea]
    public string description;
    public Sprite art;
    public Color color;

    public Card.TargetType targetType = new Card.TargetType();

    private EffectFactory factory = new EffectFactory();

    public enum NotificationType
    {
        OnCombatStart, OnCombatEnd, OnTurnStart, OnTurnEnd, OnPassiveGoldGain, OnOverkillGoldGain, OnEnemyBroken, OnPlayerBroken, OnNoEnergyCardPlyed, OnNoManaCardPlayed,
        OnTook1VitDamage, OnTempArmorGain, OnTempArmorLoss
    }
    public NotificationType condition = new NotificationType();
    public Card.EffectType effect = new Card.EffectType();
    //public Card.BuffType buff = new Card.BuffType();
    public Buff buff;
    public int effectValue;
    public int effectDuration;

    public void Process(object value, List<Relic> traceList)
    {
        Card.EffectType[] effectList = new Card.EffectType[1];
        effectList[0] = effect;
        Effect thisEffect = factory.GetEffects(effectList)[0];

        List<GameObject> targets = new List<GameObject>();
        
        if (targetType == Card.TargetType.None)
            targets = null;
        else if (targetType == Card.TargetType.AllEnemies)
            targets = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        else if (targetType == Card.TargetType.AllPlayers)
            targets = new List<GameObject>(GameController.gameController.GetLivingPlayers());
        else if (targetType == Card.TargetType.Enemy && ((GameObject) value).tag == "Enemy")
            targets.Add((GameObject) value);
        else if (targetType == Card.TargetType.Player && ((GameObject)value).tag == "Player")
            targets.Add((GameObject)value);
        else if (targetType == Card.TargetType.Any)
            targets.Add((GameObject)value);

        if (traceList == null)
            traceList = new List<Relic>() { this };
        thisEffect.RelicProcess(targets, buff, effectValue, effectDuration, traceList);
    }
}
