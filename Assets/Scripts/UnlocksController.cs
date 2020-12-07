using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlocksController : MonoBehaviour
{
    public class UnlockedQueue
    {
        public Card.CasterColor casterColor;
        public UnlockTypes type;
        public int level;
    }

    public static UnlocksController unlock;

    private Unlocks unlocks;

    public UnlockTypes[] teamUnlockRewards = new UnlockTypes[20];
    public UnlockTypes[] heroUnlockRewards = new UnlockTypes[20];

    public Sprite livesSprite;
    public Sprite replaceSprite;
    public Sprite holdSprite;
    public Sprite ascentionSprite;
    public Sprite TalentSprite;
    public Sprite customizeableCardSprite;
    public Sprite contractSprite;
    public Sprite errorSprite;

    public List<UnlockedQueue> queue = new List<UnlockedQueue>();

    public int[] sandMinRange;
    public int[] sandMaxRange;
    public int[] shardsMinRange;
    public int[] shardsMaxRange;
    public float legendaryLootChance;
    public float epicLootChance;

    public enum UnlockTypes
    {
        None = 0,
        Lives = 1,
        Replace = 10,
        Hold = 15,
        Relics = 20,
        Ascention = 30,

        Talent = 40,
        CustomizableCardSlot = 50,
        Armor = 60,
        Weapons = 65,
        EpicCardPack = 70,
        LegendaryCardPack = 80,
        HeroSkin = 100,

        Currency = 1000,
        CardPack = 2000,
        Contract = 3000
    }

    private void Awake()
    {
        if (UnlocksController.unlock == null)
            UnlocksController.unlock = this;
        else
            Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);

        InformationLogger.infoLogger.LoadUnlocks();
        PartyController.party.ResolveUnlockedColors();
    }

    public void SetUnlocks(Unlocks value)
    {
        unlocks = value;
        InformationController.infoController.ChangeCombatInfo(unlocks.livesUnlocked, 0, 0, 0);
    }

    public Unlocks GetUnlocks()
    {
        if (unlocks == null)
        {
            Unlocks newUnlock = new Unlocks();
            newUnlock.orangeUnlocked = false;
            newUnlock.whiteUnlocked = false;
            newUnlock.blackUnlocked = false;

            newUnlock.tavernContracts = 0;
            newUnlock.largestBoss = 0;

            newUnlock.sand = 0;
            newUnlock.shards = 0;

            newUnlock.redGoldCardNum = 0;
            newUnlock.blueGoldCardNum = 0;
            newUnlock.greenGoldCardNum = 0;
            newUnlock.orangeGoldCardNum = 0;
            newUnlock.whiteGoldCardNum = 0;
            newUnlock.blackGoldCardNum = 0;

            newUnlock.unlockedCards = new string[0];
            newUnlock.unlockedCardsNumber = new int[0];
            newUnlock.unlockedRelics = new string[0];
            newUnlock.unlockedWeapons = new string[0];
            newUnlock.unlockedArmor = new string[0];
            newUnlock.unlockedSkins = new string[0];

            newUnlock.holdUnlocked = false;
            newUnlock.replaceUnlocked = 1;
            newUnlock.livesUnlocked = 0;

            newUnlock.redTalentUnlocked = new bool[0];
            newUnlock.blueTalentUnlocked = new bool[0];
            newUnlock.greenTalentUnlocked = new bool[0];
            newUnlock.orangeTalentUnlocked = new bool[0];
            newUnlock.whtieTalentUnlocked = new bool[0];
            newUnlock.blackTalentUnlocked = new bool[0];

            newUnlock.redCustomizableCardUnlocked = 0;
            newUnlock.blueCustomizableCardUnlocked = 0;
            newUnlock.greenCustomizableCardUnlocked = 0;
            newUnlock.orangeCustomizableCardUnlocked = 0;
            newUnlock.whiteCustomizableCardUnlocked = 0;
            newUnlock.blackCustomizableCardUnlocked = 0;

            newUnlock.ascentionTierUnlocked = 0;
            newUnlock.totalEXP = 0;
            return newUnlock;
        }
        return unlocks;
    }

    public void ReportLevelUp(int level, bool isTeam, Card.CasterColor castercolor)
    {
        if (isTeam)
        {
            UnlockedQueue q = new UnlockedQueue();
            q.casterColor = castercolor;
            q.level = level;
            q.type = teamUnlockRewards[level - 1];
            queue.Add(q);
            UnlockedQueue q2 = new UnlockedQueue();
            q2.casterColor = Card.CasterColor.Enemy;
            q2.level = level;
            q2.type = UnlockTypes.Currency;
            queue.Add(q2);
        }
        else
        {
            UnlockedQueue q = new UnlockedQueue();
            q.casterColor = castercolor;
            q.level = level;
            q.type = heroUnlockRewards[level - 1];
            queue.Add(q);
            UnlockedQueue q2 = new UnlockedQueue();
            q2.casterColor = castercolor;
            q2.level = level;
            q2.type = UnlockTypes.CardPack;
            queue.Add(q2);
        }
    }

    public Sprite GetRewardArt(UnlockTypes type)
    {
        switch (type)
        {
            case UnlockTypes.Lives:
                return livesSprite;
            case UnlockTypes.Replace:
                return replaceSprite;
            case UnlockTypes.Hold:
                return holdSprite;
            case UnlockTypes.Ascention:
                return ascentionSprite;
            case UnlockTypes.Talent:
                return TalentSprite;
            case UnlockTypes.CustomizableCardSlot:
                return customizeableCardSprite;
            case UnlockTypes.Contract:
                return contractSprite;
            default:
                return errorSprite;
        }
    }

    public void ResetUnlocks()
    {
        Unlocks newUnlock = new Unlocks();
        newUnlock.orangeUnlocked = false;
        newUnlock.whiteUnlocked = false;
        newUnlock.blackUnlocked = false;

        newUnlock.tavernContracts = 0;
        newUnlock.largestBoss = 0;

        newUnlock.sand = 0;
        newUnlock.shards = 0;

        newUnlock.redGoldCardNum = 0;
        newUnlock.blueGoldCardNum = 0;
        newUnlock.greenGoldCardNum = 0;
        newUnlock.orangeGoldCardNum = 0;
        newUnlock.whiteGoldCardNum = 0;
        newUnlock.blackGoldCardNum = 0;

        newUnlock.unlockedCards = new string[0];
        newUnlock.unlockedCardsNumber = new int[0];
        newUnlock.unlockedRelics = new string[0];
        newUnlock.unlockedWeapons = new string[0];
        newUnlock.unlockedArmor = new string[0];
        newUnlock.unlockedSkins = new string[0];

        newUnlock.holdUnlocked = false;
        newUnlock.replaceUnlocked = 1;
        newUnlock.livesUnlocked = 0;

        newUnlock.redTalentUnlocked = new bool[0];
        newUnlock.blueTalentUnlocked = new bool[0];
        newUnlock.greenTalentUnlocked = new bool[0];
        newUnlock.orangeTalentUnlocked = new bool[0];
        newUnlock.whtieTalentUnlocked = new bool[0];
        newUnlock.blackTalentUnlocked = new bool[0];

        newUnlock.redCustomizableCardUnlocked = 0;
        newUnlock.blueCustomizableCardUnlocked = 0;
        newUnlock.greenCustomizableCardUnlocked = 0;
        newUnlock.orangeCustomizableCardUnlocked = 0;
        newUnlock.whiteCustomizableCardUnlocked = 0;
        newUnlock.blackCustomizableCardUnlocked = 0;

        newUnlock.ascentionTierUnlocked = 0;
        newUnlock.totalEXP = 0;

        unlocks = newUnlock;

        InformationLogger.infoLogger.SaveUnlocks();
    }
}
