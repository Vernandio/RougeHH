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
    public int currentZhen;
    public Item healPotion;
    public Item sword;
    public Item armor;
    public Item defense;
    public Item magic;
}
