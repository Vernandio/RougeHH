using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatLogic : MonoBehaviour
{
    public InputField inputField; 
    public AudioSource audioSource;
    public AudioClip cheatCodeSound;
    public PlayerDataSO playerData;
    public SaveLoadSystem saveLoadSystem;

    private void Start()
    {
        inputField.onValueChanged.AddListener(OnInputFieldChanged);
    }

    private void OnInputFieldChanged(string text)
    {
        if (text == "hesoyam")
        {
            playerData.playerExp += 1000;
            saveLoadSystem.SaveGame();
            playCheatSound();
            clearInputField();
        }else if(text == "tpagamegampang"){
            playerData.currentZhen += 20000;
            saveLoadSystem.SaveGame();
            playCheatSound();
            clearInputField();
        }else if(text == "opensesame"){
            playerData.floorLevel = 101;
            saveLoadSystem.SaveGame();
            playCheatSound();
            clearInputField();
        }
    }

    private void playCheatSound()
    {
        if (audioSource != null && cheatCodeSound != null)
        {
            audioSource.PlayOneShot(cheatCodeSound);
        }
    }

    private void clearInputField()
    {
        inputField.text = "";
    }
}
