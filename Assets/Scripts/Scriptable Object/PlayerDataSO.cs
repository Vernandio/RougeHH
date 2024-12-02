using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public int itemLevel;
    public int itemPrice;
    public int itemPoint;
}

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "ScriptableObjects/PlayerDataSO", order = 1)]
public class PlayerDataSO : ScriptableObject
{
    public int playerLevel;
    public int playerExp;
    public int floorLevel;
    public int selectedFloor;
    public int currentZhen;
    public Item healPotion;
    public Item sword;
    public Item armor;
    public Item defense;
    public Item magic;

    //Tambahan Event Channel
    public EventChannelSO_Int OnPlayerExpUpdated;
    public EventChannelSO_Int OnZhenUpdated;
    public EventChannelSO_Item OnItemUpdated;
    public EventChannelSO_Int OnFloorUpdated;
    
    public void UpdatePlayerExp(int amount)
    {
        playerExp += amount;
        OnPlayerExpUpdated.RaiseEvent(playerExp); 
    }

    public void UpdateZhen(int amount)
    {
        currentZhen += amount;
        OnZhenUpdated.RaiseEvent(currentZhen);
    }

    public void UpdateItem(ref Item item, int newLevel, int newPrice, int newPoint)
    {
        item.itemLevel = newLevel;
        item.itemPrice = newPrice;
        item.itemPoint = newPoint;

        List<Item> allItems = new List<Item> { healPotion, sword, armor, defense, magic };

        foreach (var otherItem in allItems)
        {
            if (otherItem != item)
            {
                otherItem.itemPrice += 10;
            }
        }

        OnItemUpdated.RaiseEvent(item);
    }

    public void UpdateFloor(int floor){
        floorLevel = floor;
        OnFloorUpdated.RaiseEvent(floorLevel);
    }
}   
