using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameDataRepository
{
    private string filePath;

    public GameDataRepository()
    {
        filePath = Application.persistentDataPath + "/gameData.dat";
    }

    public void SaveGame(GameData data)
    {
        try
        {
            // Utilizamos "using" para asegurarnos de que el archivo se cierra automáticamente
            using (FileStream file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(file, data);
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Error al guardar el archivo: " + e.Message);
        }
    }

    public GameData LoadGame()
    {
        if (File.Exists(filePath))
        {
            try
            {
                // Usamos "using" también aquí para asegurar el cierre del archivo
                using (FileStream file = File.OpenRead(filePath))
                {
                    var formatter = new BinaryFormatter();
                    return (GameData)formatter.Deserialize(file);
                }
            }
            catch (IOException e)
            {
                Debug.LogError("Error al cargar el archivo: " + e.Message);
                return null; // O devolver un estado por defecto si lo prefieres
            }
        }
        else
        {
            // Si no existe el archivo, devolvemos un nuevo estado por defecto
            return new GameData();
        }
    }
}
