using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManager
{
    private static string GetSavePath(int slot)
    {
        // Saves in a subfolder inside the persistent data path
        string folder = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return Path.Combine(folder, $"save{slot}.dat");
    }

    public static void SaveGame(SaveData data, int slot)
    {
        string path = GetSavePath(slot);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData LoadGame(int slot)
    {
        string path = GetSavePath(slot);

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogWarning($"Save file not found at {path}");
            return null;
        }
    }

    public static bool SaveExists(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public static void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }
}