using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Text playerZhen;
    public Text currentFloor;
    public Text enemyCount;
    public Text playerLevel;
    public Text playerHP;
    public Text playerExp;
    public Slider playerHPBar;
    public Slider playerExpBar;
    public PlayerDataSO playerData;
    public GameObject escapeMenu;
    public GameObject gameOverMenu;
    public Text gameOverText;
    public Text active_passive1;
    public Text active_passive2;
    public GameObject holder_passive1;
    public GameObject holder_passive2;
    public GameObject holder_active;
    public Text cooldown_passive1;
    public Text cooldown_passive2;
    public Text cooldown_active;
    int currentHealth;
    int maxExp;
    public int activePassive1;
    public int activePassive2;
    public int cooldownPassive1;
    public int cooldownPassive2;
    public int cooldownActive;

    void Awake()
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

    void Start()
    {
        currentHealth = playerData.healPotion.itemPoint;
        maxExp = playerData.playerLevel * 1000;
        playerStats();
        active_passive1.fontSize = 20;
        active_passive2.fontSize = 20;
    }

    void Update()
    {
        checkActive();
        if (Input.GetKeyDown(KeyCode.Escape)){
            bool isActive = escapeMenu.activeSelf;
            escapeMenu.SetActive(!isActive); 
        }else if(Input.GetKeyDown(KeyCode.Alpha1) && playerData.playerLevel >= 3 && !holder_passive1.activeSelf){
            TogglePassive("Passive_1");
            cooldownPassive1 = 8;
            holder_passive1.SetActive(true);
        }else if(Input.GetKeyDown(KeyCode.Alpha2) && playerData.playerLevel >= 4 && !holder_active.activeSelf){
            TogglePassive("Physical_1");
        }else if(Input.GetKeyDown(KeyCode.Alpha3) && playerData.playerLevel >= 5 && !holder_passive2.activeSelf){
            TogglePassive("Passive_2");
            cooldownPassive2 = 12;
            holder_passive2.SetActive(true);
        }

        if (escapeMenu.activeSelf)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        playerStats();
        checkOver();
    }

     private void TogglePassive(string passiveName)
    {
        GameObject parent = GameObject.FindGameObjectWithTag("Player");
        if(passiveName == "Physical_1"){
            parent = GameObject.FindGameObjectWithTag("Skill2");
        }
        GameObject passive = FindChildWithTag(parent.transform, passiveName);

        if (passive.activeSelf && passiveName == "Physical_1")
        {
            passive.SetActive(false);
        }
        else
        {
            passive.SetActive(true);
            if(passiveName == "Physical_1"){
                return;
            }else if(passiveName == "Passive_1"){
                GameObject holder = GameObject.FindGameObjectWithTag("ActiveSkill");
                GameObject active = FindChildWithTag(holder.transform, "Skill1");
                activePassive1 = 5;
                active.SetActive(true);
            }else if(passiveName == "Passive_2"){
                GameObject holder = GameObject.FindGameObjectWithTag("ActiveSkill");
                GameObject active = FindChildWithTag(holder.transform, "Skill3");
                activePassive2 = 4;
                active.SetActive(true);
            }
            // StartCoroutine(DeactivatePassiveAfterTime(passive, 10f, passiveName));
        }
    }

    private IEnumerator DeactivatePassiveAfterTime(GameObject passive, float time, string passiveName)
    {
        yield return new WaitForSeconds(time);
        passive.SetActive(false);
        if(passiveName == "Passive_1"){
            GameObject holder = GameObject.FindGameObjectWithTag("ActiveSkill");
            GameObject active = FindChildWithTag(holder.transform, "Skill1");
            active.SetActive(false);
        }else if(passiveName == "Passive_2"){
            GameObject holder = GameObject.FindGameObjectWithTag("ActiveSkill");
            GameObject active = FindChildWithTag(holder.transform, "Skill3");
            active.SetActive(false);
        }
    }

    public void checkActive(){
        if(activePassive1 <= 0){
            GameObject parent = GameObject.FindGameObjectWithTag("Player");
            GameObject passive = FindChildWithTag(parent.transform, "Passive_1");
            passive.SetActive(false);
            GameObject holder = GameObject.FindGameObjectWithTag("ActiveSkill");
            GameObject active = FindChildWithTag(holder.transform, "Skill1");
            active.SetActive(false);
        }else if(activePassive2 <= 0){
            GameObject parent = GameObject.FindGameObjectWithTag("Player");
            GameObject passive = FindChildWithTag(parent.transform, "Passive_2");
            passive.SetActive(false);
            GameObject holder = GameObject.FindGameObjectWithTag("ActiveSkill");
            GameObject active = FindChildWithTag(holder.transform, "Skill3");
            active.SetActive(false);
        }else if(cooldownPassive1 <= 0){
            holder_passive1.SetActive(false);
        }else if(cooldownPassive2 <= 0){
            holder_passive2.SetActive(false);
        }else if(cooldownActive <= 0){
            holder_active.SetActive(false);
        }

        active_passive1.text = activePassive1.ToString();
        active_passive2.text = activePassive2.ToString();
        cooldown_passive1.text = cooldownPassive1.ToString();
        cooldown_passive2.text = cooldownPassive2.ToString();
        cooldown_active.text = cooldownActive.ToString();
    }

    public void setActiveCooldown(){
        cooldownActive = 2;
        cooldown_active.text = cooldownActive.ToString();
        holder_active.SetActive(true);
    }

    public void action(){
        activePassive1 -= 1;
        activePassive2 -= 1;
        cooldownPassive1 -= 1;
        cooldownPassive2 -= 1;
        cooldownActive -= 1;
    }

    private GameObject FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }

            GameObject found = FindChildWithTag(child, tag);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    public void PlayerTakeDamage(int damage){
        currentHealth -= damage;
        MovementPlayer playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<MovementPlayer>();
        playerMovement.getDamage(damage);
        playerStats();
    }

    public void checkOver(){
        int enemyLeft = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if(enemyLeft <= 0){
            gameOverText.text = "Floor Cleared";
            gameOverMenu.SetActive(true);
        }else if(currentHealth <= 0){
            StartCoroutine(PlayDeathAnimation());
        }
    }

    private IEnumerator PlayDeathAnimation()
    {
        Animator animator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        animator.SetTrigger("Death"); 

        yield return new WaitForSeconds(2f);

        gameOverText.text = "Game Over";
        gameOverMenu.SetActive(true);
    }

    public void playerStats(){

        if(playerData.playerExp >= playerData.playerLevel * 1000){
            playerData.playerExp -= playerData.playerLevel * 1000;
            playerData.playerLevel += 1;
            playerData.healPotion.itemPoint += playerData.playerLevel * 2;
            playerData.sword.itemPoint += (int)(playerData.playerLevel * 1.5);
            playerData.defense.itemPoint += (int)(playerData.playerLevel * 1.2);
            MovementPlayer playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<MovementPlayer>();
            playerMovement.levelUp();
        }

        maxExp = playerData.playerLevel * 1000;
        playerZhen.text = playerData.currentZhen.ToString();
        if(playerData.selectedFloor == -30){
            currentFloor.text = "BOSS";
        }else{
            currentFloor.text = "Floor " + playerData.selectedFloor; 
        }
        enemyCount.text = "Enemy left: 0";
        playerLevel.text = "Level " + playerData.playerLevel.ToString();
        playerHP.text = currentHealth + "/" + playerData.healPotion.itemPoint.ToString();
        playerExp.text = playerData.playerExp.ToString() + "/" + maxExp.ToString();

        playerHPBar.value = (float)currentHealth/(float)playerData.healPotion.itemPoint;
        playerExpBar.value = (float)playerData.playerExp/(float)maxExp;
        updateEnemyCount();
    }

    void updateEnemyCount()
    {
        int enemyLeft = GameObject.FindGameObjectsWithTag("Enemy").Length;
        enemyCount.text = "Enemy left: " + enemyLeft;
    }

    public void lifeSteal(int health){
        currentHealth += health;
        playerStats();
    }

    public void resume(){
        escapeMenu.SetActive(false);
    }

    public void backToUpgradeMenu(){
        SceneManager.LoadScene("Upgrade Menu");
    }

    public void backToMainMenu(){
        SceneManager.LoadScene("Main Menu");
    }
}
