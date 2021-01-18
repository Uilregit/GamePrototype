﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;

public class TurnController : MonoBehaviour
{
    private bool playerTurn = true;
    public static TurnController turnController;

    [SerializeField] private int maxEnergy;
    private int currentEnergy;

    private int maxMana = 10;
    private int currentMana = 0;

    private List<int> manaCostCap;
    private List<int> energyCostCap;
    private List<int> manaReduction;
    private List<int> energyReduction;

    private int cardDrawChange = 0;

    private int playerBonusCast = 0;
    private int enemyBonusCast = 0;

    [SerializeField] private Text maxEnergyText;
    [SerializeField] private Text currentEnergyText;

    public Image endTurnButton;
    [SerializeField] private Color turnEnabledColor, turnDisabledColor;
    public Text turnText;
    public Image turnTextBack;
    public float turnChangeDuration;
    public float turnGracePeriod;
    public float enemyExecutionStagger;

    public int turnID = 1;

    private List<EnemyController> enemies;
    private List<EnemyController> queuedEnemies;

    private List<Card> cardsPlayed = new List<Card>();
    private List<Card> cardsPlayedThisTurn = new List<Card>();
    private List<int> manaSpent = new List<int>();
    private List<int> energySpent = new List<int>();

    // Start is called before the first frame update
    void Awake()
    {
        if (turnController == null)
            turnController = this;
        else
            Destroy(this.gameObject);

        enemies = new List<EnemyController>();
        queuedEnemies = new List<EnemyController>();

        manaCostCap = new List<int>();
        energyCostCap = new List<int>();
        manaReduction = new List<int>();
        energyReduction = new List<int>();

        currentEnergy = maxEnergy;

        ResetEnergyDisplay();
        UIController.ui.ResetManaBar(currentMana);
        turnID = 0;

        HandController.handController.ResetReplaceCounter();
    }

    public bool GetIsPlayerTurn()
    {
        return playerTurn;
    }

    public void SetPlayerTurn(bool newTurn)
    {
        playerTurn = newTurn;
        if (!playerTurn)
        {
            //Clear the entire hand
            HandController.handController.ClearHand();
            StartCoroutine(EnemyTurn());
        }
        else
        {
            List<GameObject> players = GameController.gameController.GetLivingPlayers();
            foreach (GameObject player in players)
                player.GetComponent<PlayerMoveController>().ResetTurn();
        }
    }

    public void ReportEnemy(EnemyController newEnemy)
    {
        if (playerTurn)
            enemies.Add(newEnemy);
        else
            queuedEnemies.Add(newEnemy);
    }

    public void RemoveEnemy(EnemyController thisEnemy)
    {
        if (enemies.Contains(thisEnemy))
            enemies.Remove(thisEnemy);
        else
            queuedEnemies.Remove(thisEnemy);
    }

    public List<EnemyController> GetEnemies()
    {
        return enemies;
    }

    public IEnumerator EnemyTurn()
    {
        turnID += 1;

        int manaCardsPlayed = 0;
        int energyCardsPlayed = 0;
        foreach (Card c in cardsPlayedThisTurn)
            if (c.manaCost == 0)
                energyCardsPlayed += 1;
            else
                manaCardsPlayed += 1;
        if (manaCardsPlayed == 0)
            RelicController.relic.OnNotify(this, Relic.NotificationType.OnNoManaCardPlayed, null);
        if (energyCardsPlayed == 0)
            RelicController.relic.OnNotify(this, Relic.NotificationType.OnNoEnergyCardPlyed, null);

        cardsPlayedThisTurn = new List<Card>();

        GridController.gridController.ResolveOverlap();
        GridController.gridController.DebugGrid();

        //Disable Player and card movement, trigger all end of turn effects
        List<GameObject> players = GameController.gameController.GetLivingPlayers();
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerMoveController>().SetMoveable(false);
            player.GetComponent<PlayerMoveController>().CommitMove();
        }

        //Trigger all end of turn buff effects
        foreach (GameObject characters in players)
        {
            characters.GetComponent<AbilitiesController>().TriggerAbilities(AbilitiesController.TriggerType.AtEndOfTurn);
            yield return StartCoroutine(characters.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtEndOfTurn, characters.GetComponent<HealthController>(), 0));
        }
        foreach (EnemyController thisEnemy in enemies)
            yield return StartCoroutine(thisEnemy.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtStartOfTurn, thisEnemy.GetComponent<HealthController>(), 0));


        RelicController.relic.OnNotify(this, Relic.NotificationType.OnTurnEnd, null);

        if ((object)HandController.handController.GetHeldCard() != null)
            HandController.handController.GetHeldCard().GetComponent<Collider2D>().enabled = false;

        yield return StartCoroutine(GridController.gridController.CheckDeath());

        //Enemy turn
        CameraController.camera.ScreenShake(0.03f, 0.05f);
        turnText.text = "Enemy Turn";
        turnText.enabled = true;
        turnTextBack.enabled = true;
        yield return new WaitForSeconds(TimeController.time.turnChangeDuration * TimeController.time.timerMultiplier);
        turnText.enabled = false;
        turnTextBack.enabled = false;
        yield return new WaitForSeconds(TimeController.time.turnGracePeriod * TimeController.time.timerMultiplier);

        //Trigger all begining of turn effects
        foreach (EnemyController thisEnemy in enemies)
            thisEnemy.GetComponent<HealthController>().AtStartOfTurn();

        //Order all the enemies by their card execution priority. Solve ties by closest enemies to their target
        enemies = enemies.OrderBy(x => x.GetCard()[0].GetCard().executionPriority).ThenBy(x => GridController.gridController.GetManhattanDistance(x.GetTarget().transform.position, x.transform.position)).ToList();

        //Execute the turn for each enemy
        foreach (EnemyController thisEnemy in enemies)
        {
            yield return StartCoroutine(thisEnemy.ExecuteTurn());
            yield return new WaitForSeconds(TimeController.time.enemyExecutionStagger * TimeController.time.timerMultiplier);
        }

        GridController.gridController.ResolveOverlap();
        enemies.AddRange(queuedEnemies);
        queuedEnemies = new List<EnemyController>();

        cardsPlayedThisTurn = new List<Card>();
        manaSpent = new List<int>();
        energySpent = new List<int>();

        if ((object)HandController.handController.GetHeldCard() != null)
            HandController.handController.GetHeldCard().GetComponent<Collider2D>().enabled = false;

        //Player turn
        yield return new WaitForSeconds(TimeController.time.turnGracePeriod * TimeController.time.timerMultiplier);

        CameraController.camera.ScreenShake(0.06f, 0.05f);
        turnText.text = "Your Turn";
        turnText.enabled = true;
        turnTextBack.enabled = true;
        yield return new WaitForSeconds(TimeController.time.turnChangeDuration * TimeController.time.timerMultiplier);
        turnText.enabled = false;
        turnTextBack.enabled = false;
        yield return new WaitForSeconds(TimeController.time.turnGracePeriod * TimeController.time.timerMultiplier);

        //Allow players to move, reset mana, and draw a full hand
        currentEnergy = maxEnergy;
        ResetEnergyDisplay();
        HandController.handController.UnholdCard(true);
        HandController.handController.ResetReplaceCounter();
        yield return HandController.handController.StartCoroutine(HandController.handController.DrawFullHand()); //Must be called after unholdcard

        RelicController.relic.OnNotify(this, Relic.NotificationType.OnTurnStart, null);

        players = GameController.gameController.GetLivingPlayers();
        //Trigger all start of turn buff effects
        foreach (GameObject characters in players)
            yield return StartCoroutine(characters.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtStartOfTurn, characters.GetComponent<HealthController>(), 0));

        foreach (EnemyController thisEnemy in enemies)
        {
            thisEnemy.GetComponent<AbilitiesController>().TriggerAbilities(AbilitiesController.TriggerType.AtEndOfTurn);
            yield return StartCoroutine(thisEnemy.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtEndOfTurn, thisEnemy.GetComponent<HealthController>(), 0));
        }

        yield return StartCoroutine(GridController.gridController.CheckDeath());

        //Resolve broken
        foreach (GameObject player in players)
            player.GetComponent<HealthController>().ResolveBroken();
        foreach (EnemyController thisEnemy in enemies)
            thisEnemy.GetComponent<HealthController>().ResolveBroken();

        //Refresh the intents fo all enemies
        foreach (EnemyController thisEnemy in enemies)
            if (!thisEnemy.GetSacrificed())
                thisEnemy.GetComponent<EnemyController>().RefreshIntent();

        SetPlayerTurn(true); //Trigger all player start of turn effects
    }

    public void MultiplayerEndTurnButtonPressed()
    {
        if (playerTurn)
            ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportEndTurn();
    }

    public void SetEndTurnButtonEnabled(bool state)
    {
        if (state)
            endTurnButton.color = turnEnabledColor;
        else
            endTurnButton.color = turnDisabledColor;
    }

    public IEnumerator SetMultiplayerTurn(int playerNumber)
    {
        if (ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() != playerNumber)
        {
            int manaCardsPlayed = 0;
            int energyCardsPlayed = 0;
            foreach (Card c in cardsPlayedThisTurn)
                if (c.manaCost == 0)
                    energyCardsPlayed += 1;
                else
                    manaCardsPlayed += 1;
            if (manaCardsPlayed == 0)
                RelicController.relic.OnNotify(this, Relic.NotificationType.OnNoManaCardPlayed, null);
            if (energyCardsPlayed == 0)
                RelicController.relic.OnNotify(this, Relic.NotificationType.OnNoEnergyCardPlyed, null);

            cardsPlayedThisTurn = new List<Card>();

            GridController.gridController.ResolveOverlap();
            GridController.gridController.DebugGrid();

            //Disable Player and card movement, trigger all end of turn effects
            List<GameObject> players = MultiplayerGameController.gameController.GetLivingPlayers();
            foreach (GameObject player in players)
            {
                player.GetComponent<MultiplayerPlayerMoveController>().SetMoveable(false);
                player.GetComponent<MultiplayerPlayerMoveController>().CommitMove();
            }

            //Trigger all end of turn buff effects
            foreach (GameObject characters in players)
            {
                characters.GetComponent<AbilitiesController>().TriggerAbilities(AbilitiesController.TriggerType.AtEndOfTurn);
                yield return StartCoroutine(characters.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtEndOfTurn, characters.GetComponent<HealthController>(), 0));
            }
            foreach (GameObject thisEnemy in MultiplayerGameController.gameController.GetLivingPlayers(1 - playerNumber))
                yield return StartCoroutine(thisEnemy.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtStartOfTurn, thisEnemy.GetComponent<HealthController>(), 0));


            RelicController.relic.OnNotify(this, Relic.NotificationType.OnTurnEnd, null);

            if ((object)HandController.handController.GetHeldCard() != null)
                HandController.handController.GetHeldCard().GetComponent<Collider2D>().enabled = false;

            yield return StartCoroutine(GridController.gridController.CheckDeath());
        }

        CameraController.camera.ScreenShake(0.06f, 0.05f);
        if (ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == playerNumber)
            turnText.text = "Your Turn";
        else
            turnText.text = "Enemy Turn";
        turnText.enabled = true;
        turnTextBack.enabled = true;
        yield return new WaitForSeconds(TimeController.time.turnChangeDuration * TimeController.time.timerMultiplier);
        turnText.enabled = false;
        turnTextBack.enabled = false;
        yield return new WaitForSeconds(TimeController.time.turnGracePeriod * TimeController.time.timerMultiplier);

        playerTurn = ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == playerNumber;

        if (ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == playerNumber)
        {
            currentEnergy = maxEnergy;
            ResetEnergyDisplay();
            HandController.handController.UnholdCard(true);
            HandController.handController.ResetReplaceCounter();
            if (turnID > 1)
                yield return HandController.handController.StartCoroutine(HandController.handController.DrawFullHand()); //Must be called after unholdcard

            RelicController.relic.OnNotify(this, Relic.NotificationType.OnTurnStart, null);

            List<GameObject> players = MultiplayerGameController.gameController.GetLivingPlayers();
            //Trigger all start of turn buff effects
            foreach (GameObject characters in players)
                yield return StartCoroutine(characters.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtStartOfTurn, characters.GetComponent<HealthController>(), 0));

            foreach (GameObject thisEnemy in MultiplayerGameController.gameController.GetLivingPlayers(1 - playerNumber))
            {
                thisEnemy.GetComponent<AbilitiesController>().TriggerAbilities(AbilitiesController.TriggerType.AtEndOfTurn);
                yield return StartCoroutine(thisEnemy.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtEndOfTurn, thisEnemy.GetComponent<HealthController>(), 0));
            }

            yield return StartCoroutine(GridController.gridController.CheckDeath());

            //Resolve broken
            foreach (GameObject player in players)
                player.GetComponent<HealthController>().ResolveBroken();
            foreach (GameObject thisEnemy in MultiplayerGameController.gameController.GetLivingPlayers(1 - playerNumber))
                thisEnemy.GetComponent<HealthController>().ResolveBroken();

            players = MultiplayerGameController.gameController.GetLivingPlayers();
            foreach (GameObject player in players)
                player.GetComponent<MultiplayerPlayerMoveController>().ResetTurn();
        }
        else
        {
            //Trigger all begining of turn effects
            foreach (GameObject thisEnemy in MultiplayerGameController.gameController.GetLivingPlayers(1 - playerNumber))
                thisEnemy.GetComponent<HealthController>().AtStartOfTurn();

            GridController.gridController.ResolveOverlap();

            cardsPlayedThisTurn = new List<Card>();
            manaSpent = new List<int>();
            energySpent = new List<int>();

            if ((object)HandController.handController.GetHeldCard() != null)
                HandController.handController.GetHeldCard().GetComponent<Collider2D>().enabled = false;

            //Clear the entire hand
            HandController.handController.ClearHand();
        }
        turnID += 1;
    }

    public void ResetEnergyDisplay()
    {
        maxEnergyText.text = maxEnergy.ToString();
        currentEnergyText.text = currentEnergy.ToString();
    }

    public void UseResources(int energyValue, int manaValue)
    {
        currentEnergy -= energyValue;
        currentMana += energyValue;
        currentMana = Mathf.Clamp(currentMana - manaValue, 0, maxMana);

        ResetEnergyDisplay();
        UIController.ui.ResetManaBar(currentMana);

        HandController.handController.ResetCardPlayability(currentEnergy, currentMana);
    }

    public void GainMana(int manaValue)
    {
        currentMana = Mathf.Clamp(currentMana + manaValue, 0, maxMana);
        UIController.ui.ResetManaBar(currentMana);

        HandController.handController.ResetCardPlayability(currentEnergy, currentMana);
    }

    public void GainEnergy(int energyValue)
    {
        currentEnergy += energyValue;
        ResetEnergyDisplay();

        HandController.handController.ResetCardPlayability(currentEnergy, currentMana);
    }

    public bool HasEnoughResources(int energy, int mana)
    {
        return energy <= currentEnergy && mana <= currentMana;
    }

    public int GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public void ResetCurrentEnergy()
    {
        currentEnergy = maxEnergy;
    }

    public int GetCurrentMana()
    {
        return currentMana;
    }

    public void ReportPlayedCard(Card card, int energy, int mana)
    {
        cardsPlayedThisTurn.Add(card);
        cardsPlayed.Add(card);
        energySpent.Add(energy);
        manaSpent.Add(mana);
    }

    public List<Card> GetCardsPlayedThisTurn()
    {
        return cardsPlayedThisTurn;
    }

    public int GetManaSpent()
    {
        return manaSpent.Sum();
    }

    public int GetEnergySpent()
    {
        return energySpent.Sum();
    }

    public int GetNumerOfCardsPlayedInTurn()
    {
        return cardsPlayedThisTurn.Count;
    }

    public int GetNumberOfEnemies()
    {
        return enemies.Count;
    }

    public void SetManaCostCap(int value)
    {
        manaCostCap.Add(value);
    }

    public void SetEnergyCostCap(int value)
    {
        energyCostCap.Add(value);
    }

    public void RemoveManaCostCap(int value)
    {
        manaCostCap.Remove(value);
    }

    public void RemoveEnergyCostCap(int value)
    {
        energyCostCap.Remove(value);
    }

    public int GetManaCostCap()
    {
        try
        {
            return manaCostCap.Min();
        }
        catch
        {
            return 999;
        }
    }

    public int GetEnergyCostCap()
    {
        try
        {
            return energyCostCap.Min();
        }
        catch
        {
            return 999;
        }
    }

    public void SetManaReduction(int value)
    {
        manaReduction.Add(value);
    }

    public void SetEnergyReduction(int value)
    {
        energyReduction.Add(value);
    }

    public void RemoveManaReduction(int value)
    {
        manaReduction.Remove(value);
    }

    public void RemoveEnergyReduction(int value)
    {
        energyReduction.Remove(value);
    }

    public int GetManaReduction()
    {
        try
        {
            return manaReduction.Sum();
        }
        catch
        {
            return 0;
        }
    }

    public int GetEnergyReduction()
    {
        try
        {
            return energyReduction.Sum();
        }
        catch
        {
            return 0;
        }
    }

    public void SetCardDrawChange(int value)
    {
        cardDrawChange += value;
    }

    public int GetCardDrawChange()
    {
        return cardDrawChange;
    }

    public void SetPlayerBonusCast(int value)
    {
        playerBonusCast += value;
    }

    public int GetPlayerBonusCast()
    {
        return playerBonusCast;
    }

    public void SetEnemyBonusCast(int value)
    {
        enemyBonusCast += value;
    }

    public int GetEnemyBonusCast()
    {
        return enemyBonusCast;
    }
}