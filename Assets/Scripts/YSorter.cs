using UnityEngine;

/// <summary>
/// Gestiona dinámicamente el orden de renderizado (Sorting Order) de los sprites basándose en su posición vertical en el mundo. 
/// Permite simular profundidad en entornos 2D, asegurando que las entidades se solapen correctamente según su proximidad a la cámara.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class YSorter : MonoBehaviour
{
    private SpriteRenderer mainSpriteRenderer;
    private SpriteRenderer[] childSpriteRenderers;

    [Header("Configuración de Ordenación")]
    [Tooltip("Ajuste manual por si quieres forzar que algo se vea por encima/debajo de su posición real.")]
    public int sortingOffset = 0;

    [Tooltip("Si el objeto no se mueve nunca (ej: torre), marca esto para ahorrar rendimiento.")]
    public bool isStatic = false;

    private const int BASE_SORTING_ORDER = 5000;

    /// <summary>
    /// Expone el orden de renderizado calculado actualmente para que otros componentes (como la UI de salud) puedan sincronizarse.
    /// </summary>
    public int CurrentSortingOrder { get; private set; }

    /// <summary>
    /// Inicializa las referencias de los renderizadores del objeto principal y sus hijos, realizando el primer cálculo de profundidad.
    /// </summary>
    void Start()
    {
        mainSpriteRenderer = GetComponent<SpriteRenderer>();
        
        // Identificamos todos los SpriteRenderer en la jerarquía para aplicarles la profundidad relativa.
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        System.Collections.Generic.List<SpriteRenderer> childrenList = new System.Collections.Generic.List<SpriteRenderer>();
        
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr != mainSpriteRenderer)
            {
                childrenList.Add(sr);
            }
        }
        childSpriteRenderers = childrenList.ToArray();

        UpdateSorting();

        // Si el objeto es estático, desactivamos el script tras el primer cálculo para optimizar CPU.
        if (isStatic)
        {
            enabled = false; 
        }
    }

    /// <summary>
    /// Actualiza el orden de renderizado después de que se hayan procesado todos los movimientos del frame.
    /// </summary>
    void LateUpdate()
    {
        UpdateSorting();
    }

    /// <summary>
    /// Calcula el nuevo valor de Sorting Order multiplicando la posición Y por un factor negativo. 
    /// Aplica el resultado al sprite principal y asigna un orden inferior (-1) a los sprites hijos para que actúen como base.
    /// </summary>
    private void UpdateSorting()
    {
        if (mainSpriteRenderer != null)
        {
            // El cálculo utiliza la posición Y invertida para que valores más bajos (cerca del suelo) tengan mayor prioridad.
            CurrentSortingOrder = Mathf.RoundToInt(transform.position.y * -100f) + sortingOffset + BASE_SORTING_ORDER;
            mainSpriteRenderer.sortingOrder = CurrentSortingOrder;

            // Sincronizamos los hijos (como sombras o círculos de selección).
            if (childSpriteRenderers != null)
            {
                foreach (SpriteRenderer childSr in childSpriteRenderers)
                {
                    if (childSr != null)
                    {
                        // Se resta 1 para garantizar que el hijo se dibuje justo detrás del cuerpo principal.
                        childSr.sortingOrder = CurrentSortingOrder - 1; 
                    }
                }
            }
        }
    }
}