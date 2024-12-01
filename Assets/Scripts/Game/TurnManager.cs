using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    
    // Enum for turn states: Player's turn, and then dynamically for each enemy
    public enum Turn
    {
        Player,
        // Enemies will be added dynamically, no need to list each one explicitly
    }

    public Turn currentTurn = Turn.Player; // Starting with player's turn
    public MovementPlayer playerMovement;  // Reference to player movement script
    public List<Enemy> enemies = new List<Enemy>();  // List of all enemies in the game

    private List<Turn> turnOrder = new List<Turn>();  // List to track the order of turns
    private int turnIndex = 0;  // Index to track current turn

    private void Awake()
    {
        // Singleton pattern: ensure only one TurnManager exists
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
        // Wait for 2-3 seconds before starting the game to ensure player and enemies are spawned
        yield return new WaitForSeconds(0.5f);  // Adjust the delay as needed

        // Dynamically find the player and assign the MovementPlayer component
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerMovement = playerObject.GetComponent<MovementPlayer>();
        }
        else
        {
            Debug.LogError("Player GameObject not found! Make sure the player is tagged as 'Player'.");
        }

        // Dynamically find all enemies in the scene with the "Enemy" tag
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemies.Add(enemy);  // Add enemy to the list
                Debug.Log("Added Enemy to the turn order: " + enemy.name);
            }
            else
            {
                Debug.LogWarning("Enemy GameObject found without Enemy script attached: " + enemyObject.name);
            }
        }

        // Initialize the turn order
        InitializeTurnOrder();
        StartPlayerTurn(); // Start with player's turn
    }

    // Method to add a newly spawned enemy to the turn order
    public void AddEnemyToTurnOrder(Enemy newEnemy)
    {
        enemies.Add(newEnemy);
        if (newEnemy.aggroState)  // Only add the enemy to the turn order if it's in aggro state
        {
            turnOrder.Add((Turn)System.Enum.GetValues(typeof(Turn)).Length);  // Add a dynamic entry for this enemy
            Debug.Log("Added Enemy to Turn Order: " + newEnemy.name);
        }
        else
        {
            Debug.Log("Enemy not in aggroState, skipping turn order.");
        }
    }

    // Initialize the turn order based on the enemies in aggroState
    private void InitializeTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.Add(Turn.Player);  // Add the player first

        // Add enemies that are in aggroState to the turn order dynamically (use AddEnemyToTurnOrder)
        foreach (var enemy in enemies)
        {
            turnOrder.Add((Turn)System.Enum.GetValues(typeof(Turn)).Length);  // Dynamically assign turn
        }

        // Log the turn order for debugging
        Debug.Log("Turn Order: " + string.Join(", ", turnOrder));
    }

    // Start the player's turn (enable player movement, etc.)
    public void StartPlayerTurn()
    {
        currentTurn = Turn.Player;
        if (playerMovement != null)
        {
            playerMovement.SetPlayerTurn();  // Let the player know it's their turn
        }
        Debug.Log("Player's Turn");
    }

    // Start an enemy's turn based on the turnIndex
    public void StartEnemyTurn()
    {
        // Skip the first entry (Player), then check for the enemy in the turn order
        if (turnIndex > 0 && turnIndex < turnOrder.Count)
        {
            Debug.Log("ITS ENEMY TURN (TURNMANAGER)");
            // Get the current enemy based on the turn order
            Enemy currentEnemy = enemies[turnIndex - 1];  // Get the correct enemy from the list

            if (currentEnemy.aggroState)  // Only act if the enemy is in aggroState
            {
                // Perform enemy's turn (either move or attack)
                StartCoroutine(currentEnemy.EnemyTurn());
            }
            else
            {
                EndTurn();  // If not in aggroState, skip this enemy's turn
            }
        }
    }

    // Method to end the current turn and go to the next one
    public void EndTurn()
    {
        turnIndex++;  // Move to the next turn in the list

        if (turnIndex >= turnOrder.Count)
        {
            turnIndex = 0;  // Once we reach the end of the turn order, loop back to the player
            StartPlayerTurn();
        }
        else
        {
            Debug.Log("StartEnemyTurn() (TURN MANAGER)");
            StartEnemyTurn();  // Start the next enemy's turn
        }
    }

    // Method to skip the player's turn by pressing space
    void Update()
    {
        // Allow the player to skip their turn
        if (currentTurn == Turn.Player && Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();  // Skip the player's turn and go to the next turn
        }
    }
    
    public void RemoveEnemyFromTurnList(Enemy enemyToRemove)
    {
        // Find the index of the enemy in the enemies list
        int enemyIndex = enemies.IndexOf(enemyToRemove);
        if (enemyIndex >= 0)
        {
            // Remove the enemy from the turn order (account for Player's turn being at index 0)
            turnOrder.RemoveAt(enemyIndex + 1);  // Add 1 to skip the player's turn

            // Also remove the enemy from the enemies list
            enemies.RemoveAt(enemyIndex);

            Debug.Log("Removed Enemy from Turn List: " + enemyToRemove.name);

            // If the removed enemy's turn was active, end the turn immediately
            if (turnIndex >= enemyIndex + 1)
            {
                turnIndex--;  // Adjust turnIndex to skip the removed enemy
            }
        }
        else
        {
            Debug.LogWarning("Enemy not found in turn list: " + enemyToRemove.name);
        }
    }

}
