using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemLevel : MonoBehaviour
{
    public Text healthPotionLevel;
    public Text swordLevel;
    public Text armorLevel;
    public Text defenseLevel;
    public Text magicLevel;
    public PlayerDataSO playerData;

    void Start()
    {
        Debug.Log("Player Level: " + playerData.playerLevel);
        healthPotionLevel.text = "Lvl. " + playerData.healPotion.itemLevel.ToString() + "/50";
        swordLevel.text = "Lvl. " + playerData.sword.itemLevel.ToString() + "/50";
        armorLevel.text = "Lvl. " + playerData.armor.itemLevel.ToString() + "/50";
        defenseLevel.text = "Lvl. " + playerData.defense.itemLevel.ToString() + "/50";
        magicLevel.text = "Lvl. " + playerData.magic.itemLevel.ToString() + "/50";
    }

    void Update()
    {
        healthPotionLevel.text = "Lvl. " + playerData.healPotion.itemLevel.ToString() + "/50";
        swordLevel.text = "Lvl. " + playerData.sword.itemLevel.ToString() + "/50";
        armorLevel.text = "Lvl. " + playerData.armor.itemLevel.ToString() + "/50";
        defenseLevel.text = "Lvl. " + playerData.defense.itemLevel.ToString() + "/50";
        magicLevel.text = "Lvl. " + playerData.magic.itemLevel.ToString() + "/50";
    }
}
