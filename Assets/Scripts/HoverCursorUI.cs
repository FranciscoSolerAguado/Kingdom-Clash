using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gestiona dinámicamente la apariencia del cursor al interactuar con elementos de la interfaz de usuario, evaluando en tiempo real las condiciones de economía (oro), tiempos de reutilización (cooldown) y límites de unidades para proporcionar retroalimentación visual inmediata.
/// </summary>
public class HoverCursorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Coste de Oro")]
    [Tooltip("El coste en oro asociado a este botón. Si es 0, no se comprobará el oro.")]
    public int requiredGold = 0;

    /// <summary>
    /// Indica si el puntero del ratón se encuentra actualmente sobre el área de colisión de este elemento de interfaz.
    /// </summary>
    public bool isPointerOver { get; private set; } = false;

    /// <summary>
    /// Ejecuta una actualización continua del estado del cursor mientras el ratón permanezca sobre el elemento, permitiendo reaccionar a cambios externos como la obtención de oro o la finalización de enfriamientos.
    /// </summary>
    private void Update()
    {
        if (isPointerOver)
        {
            ForceCursorUpdate();
        }
    }

    /// <summary>
    /// Intercepta el evento de entrada del puntero para marcar el inicio del estado de interacción y disparar la primera actualización del cursor.
    /// </summary>
    /// <param name="eventData">Datos proporcionados por el EventSystem de Unity.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        ForceCursorUpdate();
    }

    /// <summary>
    /// Intercepta el evento de salida del puntero para limpiar el estado de interacción y restaurar el cursor predeterminado del sistema.
    /// </summary>
    /// <param name="eventData">Datos proporcionados por el EventSystem de Unity.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetDefaultCursor();
        }
    }

    /// <summary>
    /// Evalúa jerárquicamente las condiciones del botón (cooldown, límites de despliegue y coste de oro) para solicitar al CursorManager la textura visual más apropiada según el estado actual del juego.
    /// </summary>
    public void ForceCursorUpdate()
    {
        if (CursorManager.Instance == null || !isPointerOver) return;

        // 1. Obtención del componente DeployButton para validar estados lógicos específicos
        DeployButton deployBtn = GetComponent<DeployButton>();
        
        if (deployBtn != null)
        {
            // 1a. Prioridad: Validación de Cooldown activo
            if (deployBtn.isCooldownActive)
            {
                CursorManager.Instance.SetCooldownCursor();
                return;
            }

            // 1b. Validación de capacidad máxima (específico para mineros)
            if (deployBtn.deployType == DeployButton.DeployType.Miner && MinerManager.Instance != null)
            {
                if (!MinerManager.Instance.CanDeployMiner())
                {
                    CursorManager.Instance.SetNoGoldCursor();
                    return;
                }
            }
        }

        // 2. Validación económica: Determina si el jugador posee el oro suficiente para realizar la acción
        int costToCheck = requiredGold > 0 ? requiredGold : (deployBtn != null ? deployBtn.goldCost : 0);

        if (costToCheck <= 0 || (GameManager.Instance != null && GameManager.Instance.gold >= costToCheck))
        {
            // El jugador cumple los requisitos -> Muestra cursor de interacción disponible
            CursorManager.Instance.SetHoverCursor();
        }
        else
        {
            // Recursos insuficientes -> Muestra cursor de advertencia
            CursorManager.Instance.SetNoGoldCursor();
        }
    }
}