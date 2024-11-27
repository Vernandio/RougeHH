using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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

    
    int currentHealth;
    int maxExp;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = playerData.healPotion.itemPoint;
        maxExp = playerData.playerLevel * 1000;
        playerStats();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            bool isActive = escapeMenu.activeSelf;
            escapeMenu.SetActive(!isActive); // Toggle active state
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

    public void checkOver(){
        int enemyLeft = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if(enemyLeft <= 0){
            gameOverText.text = "Floor Cleared";
            gameOverMenu.SetActive(true);
        }else if(currentHealth <= 0){
            gameOverText.text = "Game Over";
            gameOverMenu.SetActive(true);
        }
    }

    public void playerStats(){

        if(playerData.playerExp >= playerData.playerLevel * 1000){
            playerData.playerExp -= playerData.playerLevel * 1000;
            playerData.playerLevel += 1;
            playerData.healPotion.itemPoint += playerData.playerLevel * 2;
            playerData.sword.itemPoint += (int)(playerData.playerLevel * 1.5);
            playerData.defense.itemPoint += (int)(playerData.playerLevel * 1.2);
        }

        currentHealth = playerData.healPotion.itemPoint;
        maxExp = playerData.playerLevel * 1000;
        playerZhen.text = playerData.currentZhen.ToString();
        currentFloor.text = "Floor " + playerData.selectedFloor; //Belum ada logicnya
        enemyCount.text = "Enemy left: 0"; //Belum ada logicnya
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
