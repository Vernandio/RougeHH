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

    // Start is called before the first frame update
    void Start()
    {
        imageName = skillImage.sprite.name;
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
}
