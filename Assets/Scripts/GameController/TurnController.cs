using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    private bool playerTurn = true;
    public static TurnController turnController;

    [SerializeField] private int maxMana;
    private int currentMana;

    [SerializeField] private Text maxManaText;
    [SerializeField] private Text currentManaText;

    public Text turnText;
    public float turnChangeDuration;
    public float turnGracePeriod;
    public float enemyExecutionStagger;

    private List<EnemyController> enemies;

    private List<Card> cardsPlayed = new List<Card>();
    private List<Card> cardsPlayedThisTurn = new List<Card>();

    // Start is called before the first frame update
    void Awake()
    {
        if (turnController == null)
            turnController = this;
        else
            Destroy(this.gameObject);

        enemies = new List<EnemyController>();

        currentMana = maxMana;

        ResetManaDisplay();
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
            StartCoroutine(EnemyTurn());
            //Clear the entire hand
            HandController.handController.ClearHand();
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
                player.GetComponent<PlayerMoveController>().ResetTurn();
        }
    }

    public void ReportEnemy(EnemyController newEnemy)
    {
        enemies.Add(newEnemy);
    }

    public void RemoveEnemy(EnemyController thisEnemy)
    {
        enemies.Remove(thisEnemy);
        if (enemies.Count == 0)
            StartCoroutine(GameController.gameController.Victory());
    }

    private IEnumerator EnemyTurn()
    {
        cardsPlayedThisTurn = new List<Card>();

        //Disable Player and card movement, trigger all end of turn effects
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerMoveController>().SetMoveable(false);
            player.GetComponent<HealthController>().AtEndOfTurn();
        }

        //Enemy turn
        turnText.text = "Enemy Turn";
        turnText.enabled = true;
        yield return new WaitForSeconds(turnChangeDuration);
        turnText.enabled = false;
        yield return new WaitForSeconds(turnGracePeriod);

        //Trigger all begining of turn effects
        foreach (EnemyController thisEnemy in enemies)
            thisEnemy.GetComponent<HealthController>().AtStartOfTurn();

        //Execute the turn for each enemy
        foreach (EnemyController thisEnemy in enemies)
        {
            yield return StartCoroutine(thisEnemy.ExecuteTurn());
            yield return new WaitForSeconds(enemyExecutionStagger);
        }

        //Trigger all enemy end of turn effects
        foreach (EnemyController thisEnemy in enemies)
            thisEnemy.GetComponent<HealthController>().AtEndOfTurn();

        //Player turn
        yield return new WaitForSeconds(turnGracePeriod);
        turnText.text = "Your Turn";
        turnText.enabled = true;
        yield return new WaitForSeconds(turnChangeDuration);
        turnText.enabled = false;
        yield return new WaitForSeconds(turnGracePeriod);

        //Allow players to move, reset mana, and draw a full hand
        SetPlayerTurn(true);
        currentMana = maxMana;
        ResetManaDisplay();
        HandController.handController.DrawFullHand();

        //Trigger all player start of turn effects
        foreach (GameObject player in players)
            player.GetComponent<HealthController>().AtStartOfTurn();
    }

    public void ResetManaDisplay()
    {
        maxManaText.text = maxMana.ToString();
        currentManaText.text = currentMana.ToString();
    }

    public void UseMana(int value)
    {
        currentMana -= value;
        ResetManaDisplay();
        HandController.handController.ResetCardPlayability(currentMana);
    }

    public bool HasEnoughMana(int value)
    {
        return value <= currentMana;
    }

    public int GetCurrentMana()
    {
        return currentMana;
    }

    public void ReportPlayedCard(Card card)
    {
        cardsPlayedThisTurn.Add(card);
        cardsPlayed.Add(card);
    }

    public int GetNumerOfCardsPlayedInTurn()
    {
        return cardsPlayedThisTurn.Count;
    }
}
