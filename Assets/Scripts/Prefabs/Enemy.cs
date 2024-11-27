using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public EnemyDataSO enemyData;
    public int currentHP;
    public TextMeshPro nameText;
    private Animator _animator;
    public Slider enemyHPBar;
    public PlayerDataSO playerData;
    public Text message;
    private bool idleState = false;
    private bool aggroState = false;
    public SoundManager soundManager;

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
            currentHP = enemyData.maxHP;
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

    private int defenseScalingFactor;
    public void TakeDamage(int damage)
    {
        int defense = enemyData.defense;
        if(damage <= 10){
            defenseScalingFactor = Random.Range(100, 201);
        }else if(damage < 50){
            defenseScalingFactor = Random.Range(50, 101);
        }else if(damage >= 50){
            defenseScalingFactor = Random.Range(20, 51);
        }

        float defenseFactor = 1 - (defense / (defense + defenseScalingFactor));
        float damageOutput = damage * defenseFactor;
        
        damage = (int)damageOutput;

        float critChance = playerData.defense.itemPoint * 0.01f;
        float critDamage = playerData.sword.itemPoint * (1 + playerData.magic.itemPoint * 0.01f);
        float realDamage = damage;

        if (Random.value < critChance) 
        {
            realDamage = critDamage;
        }

        if(realDamage == damage){
            message.color = Color.white;
        }else{
            message.color = new Color(1f, 0.647f, 0f);
        }
        
        message.text = realDamage.ToString();
        message.gameObject.SetActive(true);

        currentHP -= (int)realDamage;
        Debug.Log($"{enemyData.enemyName} took {realDamage} damage. Remaining HP: {currentHP}");

        StartCoroutine(swordAnimation());

        StartCoroutine(resetMessage());

        if (currentHP <= 0)
        {
            StartCoroutine(swordAnimation2());
            return;
        }
    }

    private IEnumerator swordAnimation(){
        yield return new WaitForSeconds(1f);
        updateHPBar();
    }

    private IEnumerator swordAnimation2(){
        yield return new WaitForSeconds(1.5f);
        if(enemyData.maxHP == 10){
            playerData.playerExp += 1000;
            playerData.currentZhen += 2;
        }else if(enemyData.maxHP == 50){
            playerData.playerExp += 500;
            playerData.currentZhen += 15;
        }else if(enemyData.maxHP == 200){
            playerData.playerExp += 1000;
            playerData.currentZhen += 50;
        }
        Die();
    }

    public void alert(){
        if(idleState){
            return;
        }
        idleState = true;
        message.color = Color.white;
        message.text = "??";
        message.gameObject.SetActive(true);
        StartCoroutine(resetMessage());
    }

    public void aggro(){
        // soundManager.combatSound();
        if(aggroState){
            return;
        }
        idleState = true;
        aggroState = true;
        message.color = Color.red;
        message.text = "!!";
        message.gameObject.SetActive(true);
        StartCoroutine(resetMessage());
    }

    private IEnumerator resetMessage()
    {
        yield return new WaitForSeconds(1f);

        message.gameObject.SetActive(false);
    }

    void Die()
    {
        StartCoroutine(deathAnimation());
        Debug.Log($"{enemyData.enemyName} has been defeated.");
        Destroy(gameObject, 2.5f);
    }

    private IEnumerator deathAnimation(){
        yield return new WaitForSeconds(1f);
        _animator.SetTrigger("Death");
    }

    private void updateHPBar()
    {
        enemyHPBar.value = (float)currentHP/(float)enemyData.maxHP;
    }

    public void DeathSFX(){
        soundManager.deathSound();
    }

    public void PunchSFX(){
        soundManager.punchSound();
    }
}
