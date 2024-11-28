using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    public TurnState currentTurn = TurnState.PlayerTurn;

    // Events for starting turns, can be hooked to UI or other behaviors
    public delegate void TurnChangedHandler();
    public event TurnChangedHandler OnTurnChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Switch to the next turn
    public void EndTurn()
    {
        if (currentTurn == TurnState.PlayerTurn)
        {
            currentTurn = TurnState.EnemyTurn;
        }
        else
        {
            currentTurn = TurnState.PlayerTurn;
        }

        OnTurnChanged?.Invoke();  // Notify other classes when the turn changes
    }
    
    // Return true if it's the player's turn
    public bool IsPlayerTurn()
    {
        return currentTurn == TurnState.PlayerTurn;
    }

    // Return true if it's the enemy's turn
    public bool IsEnemyTurn()
    {
        return currentTurn == TurnState.EnemyTurn;
    }
}
