using UnityEngine;

/// <summary>
/// Centraliza y gestiona la apariencia visual del puntero del ratón, permitiendo el intercambio dinámico de texturas personalizadas según el contexto de juego (interacción, falta de recursos o tiempos de espera).
/// </summary>
public class CursorManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static CursorManager Instance { get; private set; }

    [Header("Configuración de Cursores")]
    [Tooltip("La imagen (textura) que se usará como cursor por defecto.")]
    public Texture2D defaultCursorTexture;
    [Tooltip("El punto de la imagen por defecto que funciona como 'clic'.")]
    public Vector2 defaultHotspot = Vector2.zero;

    [Space(10)]
    [Tooltip("La imagen (textura) que se usará al pasar sobre un objeto interactivo.")]
    public Texture2D hoverCursorTexture;
    [Tooltip("El punto de la imagen de hover que funciona como 'clic'.")]
    public Vector2 hoverHotspot = Vector2.zero;

    [Space(10)]
    [Tooltip("La imagen (textura) que se usará cuando no hay suficiente oro.")]
    public Texture2D noGoldCursorTexture;
    [Tooltip("El punto de la imagen de 'no gold' que funciona como 'clic'.")]
    public Vector2 noGoldHotspot = Vector2.zero;

    [Space(10)]
    [Tooltip("La imagen (textura) que se usará cuando una habilidad o unidad esté en cooldown.")]
    public Texture2D cooldownCursorTexture; 
    [Tooltip("El punto de la imagen de cooldown que funciona como 'clic'.")]
    public Vector2 cooldownHotspot = Vector2.zero;

    /// <summary>
    /// Implementa el patrón Singleton para garantizar una única instancia global del gestor de cursores y su accesibilidad desde cualquier otro sistema.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Establece el cursor inicial del juego al arrancar la aplicación.
    /// </summary>
    void Start()
    {
        SetDefaultCursor();
    }

    /// <summary>
    /// Ejecuta el cambio físico de la textura del cursor en el sistema operativo, incluyendo una validación de seguridad para evitar punteros nulos y gestionar errores de carga.
    /// </summary>
    /// <param name="tex">La textura 2D que se desea aplicar.</param>
    /// <param name="hotspot">El punto de anclaje (píxel exacto) donde se registra el clic dentro de la textura.</param>
    private void SetCursorSafe(Texture2D tex, Vector2 hotspot)
    {
        if (tex != null)
        {
            try
            {
                Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
            }
            catch (UnityException e)
            {
                Debug.LogWarning($"No se pudo cargar el cursor {tex.name}. Error: {e.Message}. El cursor volverá al por defecto.");
                Cursor.SetCursor(defaultCursorTexture, defaultHotspot, CursorMode.Auto);
            }
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    /// <summary>
    /// Restaura la apariencia visual del cursor a su estado neutro predeterminado.
    /// </summary>
    public void SetDefaultCursor()
    {
        SetCursorSafe(defaultCursorTexture, defaultHotspot);
    }

    /// <summary>
    /// Activa la textura visual de interacción cuando el ratón se encuentra sobre un elemento accionable de la escena.
    /// </summary>
    public void SetHoverCursor()
    {
        if (hoverCursorTexture != null)
        {
            SetCursorSafe(hoverCursorTexture, hoverHotspot);
        }
        else
        {
            SetDefaultCursor();
        }
    }

    /// <summary>
    /// Aplica el cursor de advertencia para indicar visualmente que el jugador no posee los recursos de oro necesarios para completar una acción.
    /// </summary>
    public void SetNoGoldCursor()
    {
        if (noGoldCursorTexture != null)
        {
            SetCursorSafe(noGoldCursorTexture, noGoldHotspot);
        }
        else
        {
            SetDefaultCursor();
            Debug.LogWarning("No se ha asignado una textura para el cursor de 'no gold'.");
        }
    }

    /// <summary>
    /// Cambia el cursor a la apariencia de tiempo de espera, notificando al usuario que la acción solicitada se encuentra actualmente en recarga.
    /// </summary>
    public void SetCooldownCursor()
    {
        if (cooldownCursorTexture != null)
        {
            SetCursorSafe(cooldownCursorTexture, cooldownHotspot);
        }
        else
        {
            SetDefaultCursor();
            Debug.LogWarning("No se ha asignado una textura para el cursor de 'cooldown'.");
        }
    }
}