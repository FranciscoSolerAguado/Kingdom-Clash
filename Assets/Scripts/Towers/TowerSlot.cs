using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gestiona la interacción del usuario con las ranuras de construcción del mapa, controlando los estados de selección, los efectos visuales de resaltado (hover) y la comunicación con el gestor de construcción.
/// </summary>
public class TowerSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Estado de la Casilla")]
    public GameObject currentTower;

    [Header("UI Visual (Hover)")]
    public GameObject hoverCornersPrefab;
    [Range(0.1f, 3f)]
    public float cornersScale = 1f;

    private GameObject activeHoverCorners;
    private bool isHovered = false;

    /// <summary>
    /// Detecta cuando el puntero entra en el área de la ranura, activando el resaltado visual y actualizando el icono del cursor si la ranura está vacía.
    /// </summary>
    /// <param name="eventData">Datos del evento proporcionados por el EventSystem.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ShowHover();
    }

    /// <summary>
    /// Detecta cuando el puntero sale del área de la ranura, restaurando el cursor y limpiando los efectos visuales, a menos que el menú de construcción permanezca abierto.
    /// </summary>
    /// <param name="eventData">Datos del evento proporcionados por el EventSystem.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (BuildManager.Instance != null && BuildManager.Instance.IsMenuOpenFor(this))
        {
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetDefaultCursor();
            return;
        }
        ClearHover();
    }

    /// <summary>
    /// Procesa el clic del jugador para alternar entre abrir o cerrar los menús contextuales de construcción o mejora, dependiendo de si la ranura contiene ya una torre o está vacía.
    /// </summary>
    /// <param name="eventData">Datos del evento que incluyen la identificación del botón del ratón pulsado.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Solo respondemos al clic izquierdo
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (currentTower != null)
        {
            if (BuildManager.Instance != null)
            {
                if (BuildManager.Instance.IsMenuOpenFor(this))
                {
                    BuildManager.Instance.CloseBuildMenu();
                }
                else
                {
                    BuildManager.Instance.OpenTowerMenu(this);
                }
            }
        }
        else if (BuildManager.Instance != null)
        {
            if (RangeDisplayManager.Instance != null) RangeDisplayManager.Instance.HideRange();

            if (BuildManager.Instance.IsMenuOpenFor(this))
            {
                BuildManager.Instance.CloseBuildMenu();
            }
            else
            {
                BuildManager.Instance.OpenBuildMenu(this);
            }
        }
    }

    /// <summary>
    /// Notifica a la ranura que su menú asociado se ha cerrado, permitiendo limpiar indicadores visuales persistentes o restaurar el estado del cursor.
    /// </summary>
    public void OnMenuClosed()
    {
        if (!isHovered)
        {
            ClearHover();
        }
        else if (currentTower == null && CursorManager.Instance != null)
        {
            CursorManager.Instance.SetHoverCursor();
        }
        
        if (RangeDisplayManager.Instance != null)
        {
            RangeDisplayManager.Instance.HideRange();
        }
    }

    /// <summary>
    /// Instancia y configura el efecto visual de las esquinas de selección (hover), ajustando su escala y forzando un orden de renderizado alto para que sea visible sobre las unidades.
    /// </summary>
    private void ShowHover()
    {
        if (currentTower == null)
        {
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetHoverCursor();

            if (hoverCornersPrefab != null && activeHoverCorners == null)
            {
                activeHoverCorners = Instantiate(hoverCornersPrefab, transform);
                activeHoverCorners.transform.localPosition = new Vector3(0, 0, -0.1f);
                activeHoverCorners.transform.localRotation = Quaternion.identity;
                activeHoverCorners.transform.localScale = new Vector3(cornersScale, cornersScale, 1f);

                // --- SOLUCIÓN DEL SORTING LAYER PARA EL HOVER ---
                // Forzamos un número muy alto para que tape a los personajes (25000)
                SpriteRenderer[] renderers = activeHoverCorners.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in renderers)
                {
                    sr.sortingOrder = 25000;
                }

                UnityEngine.Rendering.SortingGroup sortGroup = activeHoverCorners.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (sortGroup != null)
                {
                    sortGroup.sortingOrder = 25000;
                }
            }
        }
    }

    /// <summary>
    /// Elimina instantáneamente los efectos visuales de resaltado y restaura el cursor predeterminado del sistema.
    /// </summary>
    public void ClearHover()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetDefaultCursor();

        if (activeHoverCorners != null)
        {
            Destroy(activeHoverCorners);
            activeHoverCorners = null;
        }
    }
}