using UnityEngine;

/// <summary>
/// Gestiona la serialización, lectura y escritura de los datos de progreso del jugador utilizando PlayerPrefs, lo cual asegura la persistencia en plataformas WebGL.
/// </summary>
public static class SaveSystem
{
    public static PlayerData currentData; 

    /// <summary>
    /// Construye y devuelve la clave (key) única para identificar el slot de guardado en PlayerPrefs.
    /// </summary>
    /// <param name="slot">El identificador numérico del espacio de guardado (slot).</param>
    private static string GetKey(int slot)
    {
        return "saveSlot_" + slot;
    }

    /// <summary>
    /// Serializa el objeto de datos del jugador a formato JSON y lo guarda instantáneamente en el almacenamiento del navegador/dispositivo.
    /// </summary>
    /// <param name="data">El objeto PlayerData que contiene el progreso actual a guardar.</param>
    public static void SaveGame(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true); 
        PlayerPrefs.SetString(GetKey(data.saveSlot), json); 
        PlayerPrefs.Save(); // <- Esto es vital en WebGL, fuerza el guardado en el navegador en este instante
        
        currentData = data;
        Debug.Log("Partida guardada en el slot: " + data.saveSlot);
    }

    /// <summary>
    /// Localiza la clave correspondiente al slot solicitado, lee su contenido y lo deserializa en un objeto PlayerData utilizable en memoria.
    /// </summary>
    /// <param name="slot">El número del slot de guardado a cargar.</param>
    /// <returns>El objeto PlayerData con los datos restaurados, o nulo si la partida no existe.</returns>
    public static PlayerData LoadGame(int slot)
    {
        string key = GetKey(slot);
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json); 
            currentData = data;
            return data;
        }
        else
        {
            Debug.LogWarning("No hay partida en el slot " + slot);
            return null;
        }
    }

    /// <summary>
    /// Instancia un nuevo objeto de progreso con valores por defecto para inicializar una campaña desde cero y lo guarda inmediatamente.
    /// </summary>
    /// <param name="slot">El número del slot donde se creará y guardará la nueva partida.</param>
    public static void CreateNewGame(int slot)
    {
        PlayerData newData = new PlayerData(slot);
        SaveGame(newData);
    }

    /// <summary>
    /// Verifica de forma segura la existencia de datos guardados para el slot especificado, útil para condicionar la lógica de la interfaz de usuario.
    /// </summary>
    /// <param name="slot">El número del slot a comprobar.</param>
    /// <returns>Verdadero si los datos existen; falso en caso contrario.</returns>
    public static bool DoesSaveExist(int slot)
    {
        return PlayerPrefs.HasKey(GetKey(slot));
    }
    
    /// <summary>
    /// Elimina permanentemente los datos guardados de PlayerPrefs y limpia la referencia estática en memoria si la partida eliminada era la que estaba en uso.
    /// </summary>
    /// <param name="slot">El identificador numérico del slot de guardado a borrar.</param>
    public static void DeleteGame(int slot)
    {
        string key = GetKey(slot);
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save(); // Forzamos a que el navegador registre el borrado
            Debug.Log("Partida borrada en el slot: " + slot);

            // Si borramos la partida que teníamos cargada en memoria, la limpiamos
            if (currentData != null && currentData.saveSlot == slot)
            {
                currentData = null;
            }
        }
    }
}