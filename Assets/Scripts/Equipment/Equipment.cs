using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Equipment : ScriptableObject
{
    public Sprite art;
    public string equipmentName;
    [TextArea]
    public string equipmentDescription;
    public bool isWeapon;

    public int numOfCardSlots;
    public int atkChange;
    public int armorChange;
    public int healthChange;
    public int moveRangeChange;
    public int castRangeChange;
    public int handSizeChange;
    public equipmentEffects[] effects;
    public int[] effectValues;

    public StoryModeController.RewardsType[] materialTypes;
    public int[] materialAmounts;

    public enum equipmentEffects
    {
        None = 0,

        EnergyDiscount = 1,
        EnergyGain = 5,
        ManaDiscount = 10,
        ManaGain = 15,

        CardDraw = 20,

        TriggerCardEffect = 100,
    }
}
