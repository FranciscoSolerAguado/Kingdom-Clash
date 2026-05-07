using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sincroniza dinámicamente el orden de renderizado (Sorting Order) de un Canvas en el espacio del mundo con la profundidad calculada por el sistema YSorter del personaje propietario, asegurando que la UI de salud se visualice siempre por encima de su sprite correspondiente.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasYSorter : MonoBehaviour
{
    private Canvas canvas;
    private YSorter parentYSorter;

    [Header("Configuración")]
    [Tooltip("Ajuste manual para que la UI esté x capas por delante del sprite.")]
    public int uiOffset = 1;

    /// <summary>
    /// Inicializa las referencias al componente Canvas y al gestor de profundidad YSorter del padre, configurando el Canvas para permitir la sobrescritura del orden de renderizado independiente.
    /// </summary>
    void Start()
    {
        canvas = GetComponent<Canvas>();

        // Buscamos el YSorter en los padres (el personaje propietario)
        parentYSorter = GetComponentInParent<YSorter>();

        if (parentYSorter == null)
        {
            Debug.LogWarning("[CanvasYSorter] No se encontró un YSorter en los padres de: " + gameObject.name + ". Asegúrate de que este Canvas es hijo de un personaje con YSorter.");
            enabled = false; 
            return;
        }

        // Forzamos la configuración del Canvas para que esto funcione
        canvas.overrideSorting = true; // Fundamental: permite que este Canvas tenga su propio orden independiente
        
        UpdateCanvasSorting();
    }

    /// <summary>
    /// Actualiza el orden de renderizado en cada frame después de que todos los cálculos de movimiento y profundidad hayan finalizado, garantizando una sincronización perfecta.
    /// </summary>
    void LateUpdate()
    {
        UpdateCanvasSorting();
    }

    /// <summary>
    /// Asigna al Canvas el valor de orden actual del personaje sumándole un pequeño desfase positivo, posicionando la interfaz visualmente por delante del cuerpo del personaje.
    /// </summary>
    private void UpdateCanvasSorting()
    {
        if (parentYSorter != null)
        {
            // El Canvas tendrá el mismo orden que su personaje, pero ligeramente superior
            canvas.sortingOrder = parentYSorter.CurrentSortingOrder + uiOffset;
        }
    }
}