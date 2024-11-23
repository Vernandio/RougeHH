using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveLoadSystem : MonoBehaviour
{
    [SerializeField] PlayerDataSO playerData;
    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/gameSave.dat";
    }

    // Save game data di encrypt
    public void SaveGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Create(saveFilePath);
        
        SerializablePlayerData serializableData = new SerializablePlayerData(playerData);
        formatter.Serialize(file, serializableData);
        file.Close();
    }

    // Load game data di decrpyt
    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(saveFilePath, FileMode.Open);
            SerializablePlayerData serializableData = (SerializablePlayerData)formatter.Deserialize(file);
            file.Close();

            serializableData.CopyTo(playerData);
        }
    }

    // Check apakah file save ada
    public bool SaveFileExists()
    {
        return File.Exists(saveFilePath);
    }

    // Class untuk menyimpan data yang akan di encrypt
    [System.Serializable]
    public class SerializablePlayerData
    {
        public int playerLevel;
        public int playerExp;
        public int floorLevel;
        public int currentZhen;
        public int healPotionLevel;
        public int swordLevel;
        public int armorLevel;
        public int defenseLevel;
        public int magicLevel;

        public SerializablePlayerData(PlayerDataSO data)
        {
            playerLevel = data.playerLevel;
            playerExp = data.playerExp;
            floorLevel = data.floorLevel;
            currentZhen = data.currentZhen;
            healPotionLevel = data.healPotion.itemLevel;
            swordLevel = data.sword.itemLevel;
            armorLevel = data.armor.itemLevel;
            defenseLevel = data.defense.itemLevel;
            magicLevel = data.magic.itemLevel;
        }

        public void CopyTo(PlayerDataSO data)
        {
            data.playerLevel = playerLevel;
            data.playerExp = playerExp;
            data.floorLevel = floorLevel;
            data.currentZhen = currentZhen;
            data.healPotion.itemLevel = healPotionLevel;
            data.sword.itemLevel = swordLevel;
            data.armor.itemLevel = armorLevel;
            data.defense.itemLevel = defenseLevel;
            data.magic.itemLevel = magicLevel;
        }
    }
}
