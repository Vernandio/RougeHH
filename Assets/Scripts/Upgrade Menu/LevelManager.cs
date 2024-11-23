using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public TMP_Dropdown levelDropdown; // Assign your Dropdown component in the Inspector
    public PlayerDataSO playerData; // Reference to your PlayerDataSO


    public int temp;
    private void Start()
    {
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
        // Clear existing options
        levelDropdown.ClearOptions();

        // Generate a list of floor options based on the player's floorLevel
        List<string> options = new List<string>();
        options.Add("Boss");
        for (int i = 1; i <= playerData.floorLevel; i++)
        {
            options.Add($"Floor {i}");
        }

        // Add the options to the dropdown
        levelDropdown.AddOptions(options);

        // Set the default selected value (optional)
        levelDropdown.value = 0;
    }

    public void OnDropdownValueChanged()
    {
        // Example: Log the selected floor
        Debug.Log($"Selected: {levelDropdown.options[levelDropdown.value].text}");
    }
}
