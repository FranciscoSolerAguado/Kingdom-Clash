using UnityEngine;

/// <summary>
/// Representa la estructura de datos serializable que almacena el progreso global del jugador en la campaña, diseñada para ser guardada y cargada desde el almacenamiento local.
/// </summary>
[System.Serializable] 
public class PlayerData
{
    public int saveSlot; 
    public int maxLevelUnlocked; 
    public int[] levelStars; 

    /// <summary>
    /// Inicializa una nueva instancia de datos de partida desde cero, asignando el nivel inicial y definiendo el tamaño del registro de puntuaciones para el espacio de guardado especificado.
    /// </summary>
    /// <param name="slot">El identificador numérico del espacio de guardado (slot) asignado a esta partida.</param>
    public PlayerData(int slot)
    {
        saveSlot = slot;
        maxLevelUnlocked = 1; 
        levelStars = new int[5]; 
    }
}