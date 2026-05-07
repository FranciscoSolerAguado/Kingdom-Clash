using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la apariencia visual, la visualización de estadísticas y el comportamiento interactivo de un espacio de guardado (slot) individual en el menú principal.
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("Configuración")]
    public int slotNumber; 
    public string levelSelectorScene = "LevelSelectorScene";

    [Header("Referencias de UI")]
    public TextMeshProUGUI titleText;      
    public TextMeshProUGUI starsText;      // Gestiona el texto de estrellas (ej: 5/15)
    public TextMeshProUGUI levelsText;     // Gestiona el texto de niveles (ej: 2/5)
    public GameObject statsPanel;          

    /// <summary>
    /// Inicializa la interfaz gráfica del slot comprobando su estado de guardado actual al cargar la escena.
    /// </summary>
    void Start()
    {
        ActualizarInterfazSlot();
    }

    /// <summary>
    /// Verifica la existencia de datos guardados en el almacenamiento local para este slot y, si existen, calcula y muestra las estadísticas de progreso acumulado.
    /// </summary>
    public void ActualizarInterfazSlot()
    {
        if (SaveSystem.DoesSaveExist(slotNumber))
        {
            PlayerData data = SaveSystem.LoadGame(slotNumber);
            
            titleText.text = "Partida " + slotNumber;
            
            int estrellasObtenidas = 0;
            int nivelesCompletados = 0;

            // Recorremos el array para sumar estrellas y contar niveles con éxito
            foreach (int estrellas in data.levelStars)
            {
                if (estrellas > 0)
                {
                    estrellasObtenidas += estrellas;
                    nivelesCompletados++;
                }
            }
            
            int estrellasTotalesPosibles = data.levelStars.Length * 3;
            int nivelesTotalesPosibles = data.levelStars.Length;

            // Asignación de textos
            if (starsText != null) starsText.text = $"{estrellasObtenidas}/{estrellasTotalesPosibles}";
            if (levelsText != null) levelsText.text = $"{nivelesCompletados}/{nivelesTotalesPosibles}";
            
            if (statsPanel != null) statsPanel.SetActive(true);
        }
        else
        {
            titleText.text = "Partida Nueva";
            if (statsPanel != null) statsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Elimina el archivo de guardado asociado a este slot de forma permanente y actualiza instantáneamente la interfaz para reflejar que el espacio vuelve a estar vacío.
    /// </summary>
    public void OnClick_BorrarPartida()
    {
        if (SaveSystem.DoesSaveExist(slotNumber))
        {
            SaveSystem.DeleteGame(slotNumber);
            ActualizarInterfazSlot(); // Actualiza los textos y oculta el panel al instante
        }
    }

    /// <summary>
    /// Procesa la selección principal del jugador sobre el slot, cargando la partida existente o inicializando una nueva si está vacío, para luego transicionar a la escena del mapa.
    /// </summary>
    public void OnClick_InteractuarConSlot()
    {
        if (SaveSystem.DoesSaveExist(slotNumber))
        {
            SaveSystem.LoadGame(slotNumber);
        }
        else
        {
            SaveSystem.CreateNewGame(slotNumber);
        }

        SceneManager.LoadScene(levelSelectorScene);
    }
}