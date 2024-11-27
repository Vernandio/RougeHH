using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skills : MonoBehaviour
{
    public GameObject locked;
    public Text slot;
    public PlayerDataSO playerData;
    public Image skillImage;
    string imageName;
    public GameObject hover;
    public Text hoverText;

    // Start is called before the first frame update
    void Start()
    {
        imageName = skillImage.sprite.name;
        hover.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(slot.text == "-"){
            locked.SetActive(false);
            return;
        }
        
        if((playerData.playerLevel - 2) >= int.Parse(slot.text) || imageName == "Mini_frame0"){
            locked.SetActive(false);
        }
    }

    // Method to handle mouse enter event for hover effect
    public void OnMouseEnter(string type)
    {
        hover.SetActive(true);
        if(type == "Locked"){
            switch(imageName){
                case "16_life_drain":
                    hoverText.text = "Unlocked at level 3";
                    break;
                case "09_Melee_slash":
                    hoverText.text = "Unlocked at level 4";
                    break;
                case "13_Healing_spell_2":
                    hoverText.text = "Unlocked at level 5";
                    break;
                default:
                    hover.SetActive(false);
                    break;
            }
        }else{
            switch(imageName){
                case "16_life_drain":
                    hoverText.text = "Life Steal - For each successfull hit, increase your health by 20% of your attack";
                    break;
                case "09_Melee_slash":
                    hoverText.text = "Bash - Unleash powerful strike to the head, dealing 150% of your attack as physical damage";
                    break;
                case "13_Healing_spell_2":
                    hoverText.text = "+1000 Aura - Increase your aura by 100, Improve your attack and defend 20$";
                    break;
                default:
                    hover.SetActive(false);
                    break;
            }
        }
    }

    // Method to handle mouse exit event for hover effect
    public void OnMouseExit()
    {
        hover.SetActive(false);
    }
}
