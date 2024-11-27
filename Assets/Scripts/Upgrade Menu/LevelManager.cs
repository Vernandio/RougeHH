using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public TMP_Dropdown levelDropdown;
    public PlayerDataSO playerData;


    public int temp;
    private void Start()
    {
        playerData.selectedFloor = -30; 
        temp = playerData.floorLevel;
        PopulateDropdown();
    }

    private void Update() {
        if(temp != playerData.floorLevel){
            temp = playerData.floorLevel;
            PopulateDropdown();
        }
    }

    private void PopulateDropdown()
    {
        levelDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("Boss");
        for (int i = 1; i <= playerData.floorLevel; i++)
        {
            options.Add($"Floor {i}");
        }

        levelDropdown.AddOptions(options);

        levelDropdown.value = 0;
    }

    public void OnDropdownValueChanged()
    {
        string selectedOption = levelDropdown.options[levelDropdown.value].text;

        if (selectedOption == "Boss")
        {
            playerData.selectedFloor = -30;
        }
        else
        {
            playerData.selectedFloor = int.Parse(selectedOption.Replace("Floor ", ""));
        }
    }
}
