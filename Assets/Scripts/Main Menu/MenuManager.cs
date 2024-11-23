using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button continueButton;
    public Button newGameButton;
    public Button exitButton;
    public Button backButton;
    public Button continueButtonValidation;
    public GameObject validationPanel;
    public SaveLoadSystem saveLoadSystem;
    public PlayerDataSO playerData;

    private void Start()
    {
        //Check file exist atau gak
        continueButton.interactable = saveLoadSystem.SaveFileExists();

        //Main Button
        continueButton.onClick.AddListener(continueGame);
        newGameButton.onClick.AddListener(newGame);
        exitButton.onClick.AddListener(exitGame);

        //Validation New Game
        backButton.onClick.AddListener(backToMainMenu);
        continueButtonValidation.onClick.AddListener(continueGameValidation);
    }

    private void continueGame()
    {
        if (saveLoadSystem.SaveFileExists())
        {
            saveLoadSystem.LoadGame();
            SceneManager.LoadScene("Upgrade Menu");
        }
    }

    private void newGame()
    {
        if(saveLoadSystem.SaveFileExists()){
            validationPanel.SetActive(true);
        }else{
            newFile();
        }
    }

    private void backToMainMenu(){
        validationPanel.SetActive(false);
    }

    private void continueGameValidation(){
        newFile();
    }

    private void newFile(){
        
            playerData.playerLevel = 1;
            playerData.playerExp = 0;
            playerData.floorLevel = 1;
            playerData.currentZhen = 0;
            playerData.healPotion.itemLevel = 1;
            playerData.sword.itemLevel = 1;
            playerData.armor.itemLevel = 1;
            playerData.defense.itemLevel = 1;
            playerData.magic.itemLevel = 1;
            playerData.healPotion.itemPrice = 10;
            playerData.sword.itemPrice = 10;
            playerData.armor.itemPrice = 10;
            playerData.defense.itemPrice = 10;
            playerData.magic.itemPrice = 10;
            playerData.healPotion.itemPoint = 20;
            playerData.sword.itemPoint = 5;
            playerData.armor.itemPoint = 5;
            playerData.defense.itemPoint = 5;
            playerData.magic.itemPoint = 150;

            saveLoadSystem.SaveGame();
            SceneManager.LoadScene("Upgrade Menu");
    }

    private void exitGame(){
        Application.Quit();
    }
}
