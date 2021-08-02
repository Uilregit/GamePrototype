using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class Equipment : ScriptableObject
{
    public Sprite art;
    public string equipmentName;
    public Card.Rarity rarity = Card.Rarity.Common;
    [TextArea]
    public string equipmentDescription;
    public bool isWeapon;

    [Header("Passive Attributes")]
    public int numOfCardSlots;
    public int atkChange;
    public int armorChange;
    public int healthChange;
    public int moveRangeChange;
    public int castRangeChange;
    public int handSizeChange;
    public int replaceChange;
    public int energyChange;
    public int manaChange;
    public int attachedCardCastRangeChange;

    [Header("Before Trigger Effects")]
    public Card beforeTriggerCard;

    [Header("After Trigger Effects")]
    public Card afterTriggerCard;

    [Header("Crafting Materials")]
    public StoryModeController.RewardsType[] materialTypes;
    public int[] materialAmounts;

    public Dictionary<StoryModeController.RewardsType, int> GetCraftingMaterials()
    {
        Dictionary<StoryModeController.RewardsType, int> output = new Dictionary<StoryModeController.RewardsType, int>();

        for (int i = 0; i < materialTypes.Length; i++)
            output[materialTypes[i]] = materialAmounts[i];

        return output;
    }

    public bool GetHasCardPassives()
    {
        int totalCardPassives = 0;
        totalCardPassives = Mathf.Abs(energyChange) + Mathf.Abs(manaChange) + Mathf.Abs(attachedCardCastRangeChange);
        return totalCardPassives > 0;
    }

    public bool GetHasPlayerPassives()
    {
        int totalPlayerPassives = 0;
        totalPlayerPassives = Mathf.Abs(atkChange) + Mathf.Abs(armorChange) + Mathf.Abs(healthChange) + Mathf.Abs(moveRangeChange) + Mathf.Abs(castRangeChange) + Mathf.Abs(handSizeChange);
        return totalPlayerPassives > 0;
    }
}
