using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public EnemyDataSO enemyData; // Reference to the ScriptableObject
    private int currentHP;
    public TextMeshPro nameText;
    private Animator _animator;
    public GameObject hpBar;

    private void Awake() {
        _animator = GetComponent<Animator>();
    }

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
        UpdateHPBar();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        _animator.SetTrigger("Death");
        Debug.Log($"{enemyData.enemyName} has been defeated.");
        Destroy(gameObject, 2.5f); // Destroy the enemy after 2 seconds
    }

    private void UpdateHPBar()
    {
        if (hpBar != null)
        {
            // Set the HP bar width based on the current HP (scale in X-axis)
            float healthPercentage = (float)currentHP / (float)enemyData.maxHP;
            Vector3 newScale = hpBar.transform.localScale;
            newScale.x = healthPercentage;  // Scale the HP bar along the X-axis
            hpBar.transform.localScale = newScale;

            float healthXOffset = (1 - healthPercentage) * 0.04f;
            hpBar.transform.position = new Vector3(healthXOffset, 2.053f, 0);
        }
    }
}
