using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UpgradeMenuManager : MonoBehaviour
{
    public Image itemImage;
    public Text itemName;
    public Text itemDescription;
    public Text itemPrice;
    public Text currentZhen;
    public Text errorText;
    public InputField inputField; 
    public GameObject detailsPanel; 
    public PlayerDataSO playerData;
    public AudioSource audioSource;
    public AudioClip purchaseSFX;
    public AudioClip errorSFX;
    public AudioClip cheatCodeSound;
    public SaveLoadSystem saveLoadSystem;

    void Start()
    {
        resetItemDetails();
        inputField.onValueChanged.AddListener(OnInputFieldChanged);
    }

    private void Update() {
        currentZhen.text = playerData.currentZhen.ToString();
    }

    public void updateItemDetails(string name)
    {
        detailsPanel.SetActive(true);
        switch (name)
        {
            case "heal":
                itemName.text = "Health Up";
                itemDescription.text = "Improve your maximum health. A better heart means better health.\n\nCurrent: " + 
                                        playerData.healPotion.itemPoint.ToString() + " hp\nUpgrade: +10 hp";
                itemPrice.text = playerData.healPotion.itemPrice + " To upgrade";
                break;
                
            case "sword":
                itemName.text = "Attack Up";
                itemDescription.text = "A proper muscle training will allow you yo really strengthen your muscles.\n\nCurrent: " +
                                        playerData.sword.itemPoint.ToString() + " atk\nUpgrade: +2 atk";
                itemPrice.text = playerData.sword.itemPrice + " To upgrade";
                break;

            case "armor":
                itemName.text = "Defense Up";
                itemDescription.text = "Resistance training will improve your body toughness. Tougher body means more TPA work :D\n\nCurrent: " +
                                        playerData.armor.itemPoint.ToString() + " def\nUpgrade: +2 def";
                itemPrice.text = playerData.armor.itemPrice + " To upgrade";
                break;

            case "defense":
                itemName.text = "Luck Up";
                itemDescription.text = "When luck's on your side, even the smallest nudge can turn into a devastating blow! Increases your chance to strike a critical hit.\n\nCurrent: " + 
                                        playerData.defense.itemPoint.ToString() + "% rate\nUpgrade: +5% rate";   
                itemPrice.text = playerData.defense.itemPrice + " To upgrade";
                break;

            case "magic":
                itemName.text = "Crit Dmg Up";
                itemDescription.text = "Why just hit hard when you can hit harder? Boost your critical damage to make those lucky hits hurt even more.\n\nCurrent: " +
                                        playerData.magic.itemPoint.ToString() + "% dmg\nUpgrade: +5% dmg";
                itemPrice.text = playerData.magic.itemPrice + " To upgrade";
                break;
        }
    }

    public void buyItem(){
        switch(itemName.text){
            case "Health Up":
                if(playerData.currentZhen >= playerData.healPotion.itemPrice && playerData.healPotion.itemLevel < 50){
                    // playerData.currentZhen -= playerData.healPotion.itemPrice;
                    // playerData.healPotion.itemPrice += 50;
                    // playerData.sword.itemPrice += 10;
                    // playerData.armor.itemPrice += 10;
                    // playerData.defense.itemPrice += 10;
                    // playerData.magic.itemPrice += 10;
                    // playerData.healPotion.itemPoint += 10;
                    // playerData.healPotion.itemLevel += 1;

                    //Tambahan Event Channel
                    playerData.UpdateItem(ref playerData.healPotion, playerData.healPotion.itemLevel+1, playerData.healPotion.itemPrice+50, playerData.healPotion.itemPoint+10);
                    playerData.UpdateZhen(-playerData.healPotion.itemPrice);
                    //Testing

                    currentZhen.text = playerData.currentZhen.ToString();
                    updateItemDetails("heal");
                    audioSource.PlayOneShot(purchaseSFX);
                    saveLoadSystem.SaveGame();
                    errorText.gameObject.SetActive(false);
                }else{
                    if(playerData.currentZhen < playerData.healPotion.itemPrice){
                        errorText.text = "You don't have enough Zhen.";
                    }else{
                        errorText.text = "Max level reached.";
                    }
                    errorText.gameObject.SetActive(true);
                    audioSource.PlayOneShot(errorSFX);
                }
                break;
            case "Attack Up":
                if(playerData.currentZhen >= playerData.sword.itemPrice && playerData.sword.itemLevel < 50){
                    // playerData.currentZhen -= playerData.sword.itemPrice;
                    // playerData.sword.itemPrice += 50;
                    // playerData.healPotion.itemPrice += 10;
                    // playerData.armor.itemPrice += 10;
                    // playerData.defense.itemPrice += 10;
                    // playerData.magic.itemPrice += 10;
                    // playerData.sword.itemPoint += 2;
                    // playerData.sword.itemLevel += 1;

                    //Tambahan Event Channel
                    playerData.UpdateItem(ref playerData.sword, playerData.sword.itemLevel+1, playerData.sword.itemPrice+50, playerData.sword.itemPoint+2);
                    playerData.UpdateZhen(-playerData.sword.itemPrice);
                    //Testing

                    currentZhen.text = playerData.currentZhen.ToString();
                    updateItemDetails("sword");
                    audioSource.PlayOneShot(purchaseSFX);
                    saveLoadSystem.SaveGame();
                    errorText.gameObject.SetActive(false);
                }else{
                    if(playerData.currentZhen < playerData.sword.itemPrice){
                        errorText.text = "You don't have enough Zhen.";
                    }else{
                        errorText.text = "Max level reached.";
                    }
                    errorText.gameObject.SetActive(true);
                    audioSource.PlayOneShot(errorSFX);
                }
                    break;
            case "Defense Up":
                if(playerData.currentZhen >= playerData.armor.itemPrice && playerData.armor.itemLevel < 50){
                    // playerData.currentZhen -= playerData.armor.itemPrice;
                    // playerData.armor.itemPrice += 50;
                    // playerData.healPotion.itemPrice += 10;
                    // playerData.sword.itemPrice += 10;
                    // playerData.defense.itemPrice += 10;
                    // playerData.magic.itemPrice += 10;
                    // playerData.armor.itemPoint += 2;
                    // playerData.armor.itemLevel += 1;

                    //Tambahan Event Channel
                    playerData.UpdateItem(ref playerData.armor, playerData.armor.itemLevel+1, playerData.armor.itemPrice+50, playerData.armor.itemPoint+2);
                    playerData.UpdateZhen(-playerData.armor.itemPrice);
                    //Testing

                    currentZhen.text = playerData.currentZhen.ToString();
                    updateItemDetails("armor");
                    audioSource.PlayOneShot(purchaseSFX);
                    saveLoadSystem.SaveGame();
                    errorText.gameObject.SetActive(false);
                }else{
                    if(playerData.currentZhen < playerData.armor.itemPrice){
                        errorText.text = "You don't have enough Zhen.";
                    }else{
                        errorText.text = "Max level reached.";
                    }
                    errorText.gameObject.SetActive(true);
                    audioSource.PlayOneShot(errorSFX);
                }
                    break;
            case "Luck Up":
                if(playerData.currentZhen >= playerData.defense.itemPrice && playerData.defense.itemLevel < 50){
                        // playerData.currentZhen -= playerData.defense.itemPrice;
                        // playerData.defense.itemPrice += 50;
                        // playerData.healPotion.itemPrice += 10;
                        // playerData.sword.itemPrice += 10;
                        // playerData.armor.itemPrice += 10;
                        // playerData.magic.itemPrice += 10;
                        // playerData.defense.itemPoint += 5;
                        // playerData.defense.itemLevel += 1;

                        //Tambahan Event Channel
                        playerData.UpdateItem(ref playerData.defense, playerData.defense.itemLevel+1, playerData.defense.itemPrice+50, playerData.defense.itemPoint+5);
                        playerData.UpdateZhen(-playerData.defense.itemPrice);
                        //Testing

                        currentZhen.text = playerData.currentZhen.ToString();
                        updateItemDetails("defense");
                        audioSource.PlayOneShot(purchaseSFX);
                        saveLoadSystem.SaveGame();
                        errorText.gameObject.SetActive(false);
                }else{
                    if(playerData.currentZhen < playerData.defense.itemPrice){
                        errorText.text = "You don't have enough Zhen.";
                    }else{
                        errorText.text = "Max level reached.";
                    }
                    errorText.gameObject.SetActive(true);
                    audioSource.PlayOneShot(errorSFX);
                }
                break;
            case "Crit Dmg Up":
                if(playerData.currentZhen >= playerData.magic.itemPrice && playerData.magic.itemLevel < 50){
                        // playerData.currentZhen -= playerData.magic.itemPrice;
                        // playerData.magic.itemPrice += 50;
                        // playerData.healPotion.itemPrice += 10;
                        // playerData.sword.itemPrice += 10;
                        // playerData.armor.itemPrice += 10;
                        // playerData.defense.itemPrice += 10;
                        // playerData.magic.itemPoint += 5;
                        // playerData.magic.itemLevel += 1;

                        //Tambahan Event Channel
                        playerData.UpdateItem(ref playerData.magic, playerData.magic.itemLevel+1, playerData.magic.itemPrice+50, playerData.magic.itemPoint+5);
                        playerData.UpdateZhen(-playerData.magic.itemPrice);
                        //Testing

                        currentZhen.text = playerData.currentZhen.ToString();
                        updateItemDetails("magic");
                        audioSource.PlayOneShot(purchaseSFX);
                        saveLoadSystem.SaveGame();
                        errorText.gameObject.SetActive(false);
                }else{
                    if(playerData.currentZhen < playerData.magic.itemPrice){
                        errorText.text = "You don't have enough Zhen.";
                    }else{
                        errorText.text = "Max level reached.";
                    }
                    errorText.gameObject.SetActive(true);
                    audioSource.PlayOneShot(errorSFX);
                }
                break;
        }
    }

    private void OnInputFieldChanged(string text)
    {
        if (text == "hesoyam")
        {
            playerData.UpdatePlayerExp(500);
            // playerData.playerExp += 500;
            if(playerData.playerExp >= playerData.playerLevel * 1000){
                playerData.playerExp -= playerData.playerLevel * 1000;
                playerData.playerLevel += 1;
                playerData.healPotion.itemPoint += playerData.playerLevel * 2;
                playerData.sword.itemPoint += (int)(playerData.playerLevel * 1.5);
                playerData.defense.itemPoint += (int)(playerData.playerLevel * 1.2);
            }
            saveLoadSystem.SaveGame();
            audioSource.PlayOneShot(cheatCodeSound);
            inputField.text = "";
        }else if(text == "tpagamegampang"){
            // playerData.currentZhen += 20000;
            playerData.UpdateZhen(20000);
            saveLoadSystem.SaveGame();
            audioSource.PlayOneShot(cheatCodeSound);
            inputField.text = "";
        }else if(text == "opensesame"){
            // playerData.floorLevel = 101;
            playerData.UpdateFloor(101);
            saveLoadSystem.SaveGame();
            audioSource.PlayOneShot(cheatCodeSound);
            inputField.text = "";
        }
    }

    public void startGame(){
        saveLoadSystem.SaveGame();
        SceneManager.LoadScene("Game");
    }

    public void exit(){
        SceneManager.LoadScene("Main Menu");
    }

    public void getImage(Sprite image){
        itemImage.sprite = image;
    }
    public void resetItemDetails()
    {
        detailsPanel.SetActive(false); 
    }
}
