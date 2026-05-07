using UnityEngine;
using UnityEngine.EventSystems; // Necesario para la interfaz de clic
using UnityEngine.UI; // Necesario para acceder al componente Button

/// <summary>
/// Gestiona la reproducción de un efecto de sonido al interactuar con elementos de la interfaz de usuario, ofreciendo soporte automático tanto para componentes Button estándar como para otros elementos clicables genéricos mediante eventos de puntero.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ClickSound : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración de Sonido")]
    [Tooltip("El clip de audio que sonará al hacer clic.")]
    public AudioClip clickSound;

    private AudioSource audioSource;
    private Button button; // Referencia al botón

    /// <summary>
    /// Obtiene las referencias necesarias y configura el componente de audio optimizándolo para elementos de interfaz (sonido 2D puro sin reproducción automática).
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<Button>(); // Intentamos obtener el componente Button

        // --- Pequeñas optimizaciones para sonidos de UI ---
        audioSource.spatialBlend = 0; 
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Suscribe de forma automática la función de reproducción de sonido al evento nativo del botón en caso de que este componente esté presente en el objeto.
    /// </summary>
    private void OnEnable()
    {
        // Si encontramos un componente Button, nos suscribimos a su evento onClick
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    /// <summary>
    /// Retira la suscripción del evento nativo del botón de manera segura para prevenir errores de referencia o fugas de memoria cuando el objeto se desactiva o destruye.
    /// </summary>
    private void OnDisable()
    {
        // Es buena práctica desuscribirse para evitar errores
        if (button != null)
        {
            button.onClick.RemoveListener(PlaySound);
        }
    }

    /// <summary>
    /// Valida la existencia de los componentes necesarios y ejecuta el clip de audio asignado permitiendo que los sonidos se superpongan si se clica rápidamente.
    /// </summary>
    private void PlaySound()
    {
        // Comprobamos que tenemos un sonido asignado para evitar errores
        if (clickSound != null && audioSource != null)
        {
            // PlayOneShot es perfecto para efectos de sonido rápidos.
            audioSource.PlayOneShot(clickSound);
        }
        else
        {
            Debug.LogWarning($"[ClickSound] No hay 'clickSound' o 'AudioSource' asignado en el objeto {gameObject.name}");
        }
    }

    /// <summary>
    /// Intercepta los eventos directos del ratón o pantalla táctil sobre la zona visual del objeto, sirviendo como alternativa de reproducción si el elemento no dispone de un componente Button estándar.
    /// </summary>
    /// <param name="eventData">Información detallada sobre la interacción del puntero proporcionada por el EventSystem de Unity.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Si el objeto NO es un botón (ya que los botones se gestionan por el evento onClick),
        // reproducimos el sonido aquí.
        if (button == null)
        {
            PlaySound();
        }
    }
}