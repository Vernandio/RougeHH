using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    
    public enum Turn
    {
        Player,
    }

    public Turn currentTurn = Turn.Player;
    public MovementPlayer playerMovement;
    public List<Enemy> enemies = new List<Enemy>();
    private List<Turn> turnOrder = new List<Turn>();
    private int turnIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerMovement = playerObject.GetComponent<MovementPlayer>();
        }

        // GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        // foreach (GameObject enemyObject in enemyObjects)
        // {
        //     Enemy enemy = enemyObject.GetComponent<Enemy>();
        //     if (enemy != null)
        //     {
        //         enemies.Add(enemy);
        //     }
        // }

        // InitializeTurnOrder();
        StartPlayerTurn();
    }

    void Update()
    {
        if (currentTurn == Turn.Player && Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn(); 
        }
    }

    public void AddEnemyToTurnOrder(Enemy newEnemy)
    {
        enemies.Add(newEnemy);
        InitializeTurnOrder();
    }

    private void InitializeTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.Add(Turn.Player);

        foreach (var enemy in enemies)
        {
            turnOrder.Add((Turn)System.Enum.GetValues(typeof(Turn)).Length); 
        }
    }

    public void StartPlayerTurn()
    {
        currentTurn = Turn.Player;
        if (playerMovement != null)
        {
            playerMovement.SetPlayerTurn();
        }
    }

    public void StartEnemyTurn()
    {
        if (turnIndex > 0 && turnIndex < turnOrder.Count)
        {
            Enemy currentEnemy = enemies[turnIndex - 1];

            if (currentEnemy.aggroState) 
            {
                StartCoroutine(currentEnemy.EnemyTurn());
            }
            else
            {
                EndTurn();
            }
        }
    }

    public void EndTurn()
    {
        turnIndex++;

        if (turnIndex >= turnOrder.Count)
        {
            turnIndex = 0;
            StartPlayerTurn();
        }
        else
        {
            StartEnemyTurn();
        }
    }
    
    public void RemoveEnemyFromTurnList(Enemy enemyToRemove)
    {
        int enemyIndex = enemies.IndexOf(enemyToRemove);
        if (enemyIndex >= 0)
        {
            turnOrder.RemoveAt(enemyIndex + 1); 

            enemies.RemoveAt(enemyIndex);

            if (turnIndex >= enemyIndex + 1)
            {
                turnIndex--; 
            }
        }
    }

}
