using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveData
{

    public static void SavePlayer(WorldSim.Data currentState)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Path.Combine(Application.persistentDataPath, "player.afg");
        FileStream stream = new FileStream(path, FileMode.Create);
        stream.Seek(0, SeekOrigin.Begin);


        formatter.Serialize(stream, currentState);
    }

    public static WorldSim.Data LoadPlayer()
    {
        string path = Path.Combine(Application.persistentDataPath, "player.afg");

        if (File.Exists(path))
        {
            Debug.Log(path);

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            stream.Seek(0, SeekOrigin.Begin);

            WorldSim.Data data = formatter.Deserialize(stream) as WorldSim.Data;
            stream.Close();

            return data;
        } else
        {
            Debug.Log("PATH DIDNT EXIST!!!");
            WorldSim.Data data = new WorldSim.Data(0, 0, 0);

            SavePlayer(data);

            return data;
        }

    }









}
