using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyDataSO enemyData; // Reference to the ScriptableObject
    private int currentHP;
    public TextMeshPro nameText;
    private Animator _animator;
    public Slider enemyHPBar;

    private void Awake() {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        enemyHPBar.value = 1;
        enemyHPBar.interactable = false;
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
        enemyHPBar.value = (float)currentHP/(float)enemyData.maxHP;
    }
}
