using UnityEngine;
using TMPro; // Para usar TextMeshPro

/// <summary>
/// Gestiona la visualización del progreso global del jugador en la interfaz de selección de niveles, calculando y mostrando el total de estrellas obtenidas frente al máximo posible.
/// </summary>
public class LevelSelectorUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TextMeshProUGUI totalStarsText;

    /// <summary>
    /// Inicializa la interfaz al arrancar la escena ejecutando la actualización del progreso global.
    /// </summary>
    void Start()
    {
        ActualizarProgresoGlobal();
    }

    /// <summary>
    /// Recupera los datos del sistema de guardado, suma las estrellas conseguidas en todos los niveles y actualiza el componente de texto con el formato actual/total.
    /// </summary>
    public void ActualizarProgresoGlobal()
    {
        // 1. Verificamos que haya datos cargados
        if (SaveSystem.currentData == null)
        {
            // Intentamos cargar el último slot usado (por defecto el 1 si no hay otro)
            SaveSystem.LoadGame(1);
        }

        if (SaveSystem.currentData != null)
        {
            int estrellasObtenidas = 0;
            int estrellasTotalesPosibles = SaveSystem.currentData.levelStars.Length * 3;

            // 2. Sumamos las estrellas de cada nivel
            foreach (int estrellasEnNivel in SaveSystem.currentData.levelStars)
            {
                estrellasObtenidas += estrellasEnNivel;
            }

            // 3. Mostramos el resultado en el formato solicitado
            if (totalStarsText != null)
            {
                totalStarsText.text = $"{estrellasObtenidas}/{estrellasTotalesPosibles}";
            }
        }
    }
}