using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PatchController : MonoBehaviour
{
    private string[] oldPatchVersions = new string[] { "0.5.0.4", "0.5.1", "0.5.1.1", "0.5.1.3" };
    public GameObject[] worlds;

    // Start is called before the first frame update
    void Awake()
    {
        string saveFilePatchID = InformationLogger.infoLogger.GetStoryModeSaveFilePatchID();
        if (saveFilePatchID == null || saveFilePatchID != InformationLogger.infoLogger.patchID)
            for (int i = 0; i < oldPatchVersions.Length; i++)
                if (PatchIsOlderThan(saveFilePatchID, oldPatchVersions[i]))
                    Patch(oldPatchVersions[i]);
    }
    /*
    public void PatchButton()
    {
        Debug.Log("patched");
        //Reset achievements for room 10 as they've been updated
        Dictionary<int, int[]> challengeValues = StoryModeController.story.GetChallengeValues();
        if (challengeValues.ContainsKey(10))
        {
            challengeValues[10] = new int[] { challengeValues[10][0], -1, -1 };
            StoryModeController.story.SetChallengeValues(challengeValues);
            Dictionary<int, bool[]> challengeBought = StoryModeController.story.GetChallengeItemsBought();
            challengeBought[10] = new bool[] { challengeBought[10][0], false, false };
            StoryModeController.story.SetChallengeItemsBought(challengeBought);
        }

        //Rollback number of blank cards due to previous bug
        int numOFCardsBought = 0;
        foreach (string cardName in CollectionController.collectionController.GetCompleteDeckDict().Keys)
            if (LootController.loot.GetCardWithName(cardName).rarity != Card.Rarity.Starter && LootController.loot.GetCardWithName(cardName).rarity != Card.Rarity.StarterAttack)
                numOFCardsBought += CollectionController.collectionController.GetCompleteDeckDict()[cardName];
        Dictionary<StoryModeController.RewardsType, int> itemsBought = StoryModeController.story.GetItemsBought();
        itemsBought[StoryModeController.RewardsType.BlankCard] = 10 - numOFCardsBought;
        StoryModeController.story.SetItemsBought(itemsBought);

        //Assign currnent party colors as the colors completed for all currently completed rooms
        foreach (int roomID in StoryModeController.story.GetCompletedRooms())
            StoryModeController.story.ReportColorsCompleted(roomID, new List<Card.CasterColor>(PartyController.party.GetPlayerCasterColors()));

        StoryModeSceneController.story.RefreshWorldRooms();
    }
    */
    private void Patch(string version)
    {
        Debug.Log("######## Patching to version: " + version + " ########");
        switch (version)
        {
            case "0.5.0.4":
                //Reset achievements for room 10 as they've been updated
                RevertAchievements(10, false, true, true);

                //Rollback number of blank cards due to previous bug
                int numOFCardsBought = 0;
                foreach (string cardName in CollectionController.collectionController.GetCompleteDeckDict().Keys)
                    if (!new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(LootController.loot.GetCardWithName(cardName).rarity))
                        numOFCardsBought += CollectionController.collectionController.GetCompleteDeckDict()[cardName];

                int totalBlankCardsGotten = 0;
                List<StoryRoomController> rooms = new List<StoryRoomController>();
                for (int i = 0; i < worlds.Length; i++)
                    rooms.AddRange(worlds[i].GetComponentsInChildren<StoryRoomController>().ToList());
                foreach (StoryRoomController setup in rooms)
                {
                    if (StoryModeController.story.GetChallengeItemsBought().ContainsKey(setup.roomId))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (setup.setup.rewardTypes[i] == StoryModeController.RewardsType.BlankCard && StoryModeController.story.GetChallengeItemsBought()[setup.roomId][i])
                                totalBlankCardsGotten += setup.setup.rewardAmounts[i];
                            if (setup.setup.rewardTypes[i] == StoryModeController.RewardsType.SpecificCard && StoryModeController.story.GetChallengeItemsBought()[setup.roomId][i])
                                numOFCardsBought -= setup.setup.rewardAmounts[i];
                        }
                    }
                }

                Dictionary<StoryModeController.RewardsType, int> itemsBought = StoryModeController.story.GetItemsBought();
                itemsBought[StoryModeController.RewardsType.BlankCard] = totalBlankCardsGotten - numOFCardsBought;
                StoryModeController.story.SetItemsBought(itemsBought);

                //Assign currnent party colors as the colors completed for all currently completed rooms
                foreach (int roomID in StoryModeController.story.GetCompletedRooms())
                    StoryModeController.story.ReportColorsCompleted(roomID, new List<Card.CasterColor>(PartyController.party.GetPlayerCasterColors()));

                //StoryModeSceneController.story.RefreshWorldRooms();
                break;
            case "0.5.1":
                //Reset achievements for room 51 (secret room 4) as they've been updated
                RevertAchievements(51, false, true, true);
                break;
            case "0.5.1.1":
                //Rollback number of blank cards due to previous bug
                numOFCardsBought = 0;
                foreach (string cardName in CollectionController.collectionController.GetCompleteDeckDict().Keys)
                    if (!new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(LootController.loot.GetCardWithName(cardName).rarity))
                        numOFCardsBought += CollectionController.collectionController.GetCompleteDeckDict()[cardName];

                totalBlankCardsGotten = 0;
                rooms = new List<StoryRoomController>();
                for (int i = 0; i < worlds.Length; i++)
                    rooms.AddRange(worlds[i].GetComponentsInChildren<StoryRoomController>().ToList());
                foreach (StoryRoomController setup in rooms)
                {
                    if (StoryModeController.story.GetChallengeItemsBought().ContainsKey(setup.roomId))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (setup.setup.rewardTypes[i] == StoryModeController.RewardsType.BlankCard && StoryModeController.story.GetChallengeItemsBought()[setup.roomId][i])
                                totalBlankCardsGotten += setup.setup.rewardAmounts[i];
                            if (setup.setup.rewardTypes[i] == StoryModeController.RewardsType.SpecificCard && StoryModeController.story.GetChallengeItemsBought()[setup.roomId][i])
                                numOFCardsBought -= setup.setup.rewardAmounts[i];
                        }
                    }
                }

                itemsBought = StoryModeController.story.GetItemsBought();
                itemsBought[StoryModeController.RewardsType.BlankCard] = totalBlankCardsGotten - numOFCardsBought;
                StoryModeController.story.SetItemsBought(itemsBought);
                break;
            case "0.5.1.3":
                //Reset achievements for room 5 and room 61 (secret boss room 1) for the new boss
                RevertAchievements(5, false, true, true);
                RevertAchievements(10, false, false, true);
                RevertAchievements(62, false, true, false);
                RevertAchievements(61, false, true, true);
                break;
        }
        Debug.Log("######## Patching successful ########");
        InformationLogger.infoLogger.SaveStoryModeGame();
        //InformationLogger.infoLogger.LoadStoryModeGame();
    }

    private void RevertAchievements(int id, bool revert1, bool revert2, bool revert3)
    {
        Dictionary<int, int[]> challengeValues = StoryModeController.story.GetChallengeValues();
        Dictionary<int, bool[]> challengeBought = StoryModeController.story.GetChallengeItemsBought();

        if (challengeValues.ContainsKey(id))
        {
            int[] newAchieveValues = new int[] { challengeValues[id][0], challengeValues[id][1], challengeValues[id][2] };
            bool[] newChallegeBought = new bool[] { challengeBought[id][0], challengeBought[id][1], challengeBought[id][2] };
            if (revert1)
            {
                newAchieveValues[0] = -1;
                newChallegeBought[0] = false;
            }
            if (revert2)
            {
                newAchieveValues[1] = -1;
                newChallegeBought[1] = false;
            }
            if (revert3)
            {
                newAchieveValues[2] = -1;
                newChallegeBought[2] = false;
            }

            foreach (StoryRoomController room in worlds[0].GetComponentsInChildren<StoryRoomController>())
                if (room.roomId == id)
                {

                    room.setup.bestChallengeValues = newAchieveValues;
                    room.setup.challengeRewardBought = newChallegeBought;
                }
            challengeValues[id] = newAchieveValues;
            StoryModeController.story.SetChallengeValues(challengeValues);

            challengeBought[id] = newChallegeBought;
            StoryModeController.story.SetChallengeItemsBought(challengeBought);
        }
    }

    private bool PatchIsOlderThan(string currentID, string checkID)
    {
        //Always patch files that don't have any patchIDs as they're from before 0.5.0.4
        if (currentID == null)
            return true;

        currentID = currentID + ".";
        checkID = checkID + ".";

        while (currentID.IndexOf(".") != -1 && checkID.IndexOf(".") != -1)
        {
            int current = int.Parse(currentID.Substring(0, currentID.IndexOf(".")));
            int check = int.Parse(checkID.Substring(0, checkID.IndexOf(".")));
            if (current < check)
                return true;
            //Don't patch if current patch is ahead of the check patch
            else if (current > check)
                return false;
            else
            {
                currentID = currentID.Substring(currentID.IndexOf(".") + 1, currentID.Length - currentID.IndexOf(".") - 1);
                checkID = checkID.Substring(checkID.IndexOf(".") + 1, checkID.Length - checkID.IndexOf(".") - 1);
            }

            //If the check ID has a higher subversion than the current ID, then the current ID is older
            //ie. 0.5.1 vs 0.5.1.1
            if (currentID.IndexOf(".") == -1 && checkID.IndexOf(".") != -1)
                return true;
        }
        //All else being equal, don't patch
        return false;
    }
}
