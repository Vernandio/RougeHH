using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public EnemyDataSO enemyData; // Reference to the ScriptableObject
    private int currentHP;
    public TextMeshPro nameText;

    void Start()
    {
        InitializeEnemy();
    }

    void InitializeEnemy()
    {
        if (enemyData != null)
        {
            nameText = GetComponentInChildren<TextMeshPro>();
            nameText.text = GetRandomEnemyName();
            currentHP = enemyData.maxHP; // Initialize current HP
            Debug.Log($"{enemyData.enemyName} spawned with {currentHP} HP and {enemyData.damage} damage.");
        }
        else
        {
            Debug.LogError("EnemyData is not assigned!");
        }
    }

    private static readonly List<string> enemyNames = new List<string>
    {
        "AC", "AS", "BD", "BT", "CG", "CT", "CV", "DD", "DO", "FO", 
        "FR", "FW", "GN", "GY", "HO", "JK", "KH", "MJ", "MM", "MR", 
        "MV", "NB", "NE", "NS", "NT", "OV", "PL", "RU", "SC", "TI", 
        "VD", "VM", "VX", "WS", "WW", "YD"
    };

    public static string GetRandomEnemyName()
    {
        int randomIndex = Random.Range(0, enemyNames.Count);
        return enemyNames[randomIndex];
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{enemyData.enemyName} took {damage} damage. Remaining HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{enemyData.enemyName} has been defeated.");
        Destroy(gameObject);
    }
}
