using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager
{
    private static readonly string savePath = Application.persistentDataPath + "/save.dat";

    public static void SaveGame(SaveData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log($"Game saved to {savePath}");
    }

    public static SaveData LoadGame()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            Debug.Log($"Game loaded from {savePath}");

            return data;
        }
        else
        {
            Debug.LogWarning("Save file not found. Creating new save.");
            return new SaveData(); // Return a fresh SaveData if none exists
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted.");
        }
    }
}