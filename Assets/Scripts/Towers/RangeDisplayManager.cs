using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Administra la representación visual de los radios de alcance de las torres en el mapa, utilizando un indicador gráfico en un Canvas de espacio de mundo (World Space) con escalado dinámico.
/// </summary>
public class RangeDisplayManager : MonoBehaviour
{
    public static RangeDisplayManager Instance;

    [Header("Referencias")]
    [Tooltip("El RectTransform de la Imagen. DEBE estar en un Canvas en modo 'World Space'.")]
    public RectTransform rangeIndicator;

    [Header("Ajuste Visual")]
    public float visualScalePadding = 1.0f;

    private GameObject currentTowerShowingRange;
    private Vector3 currentTargetWorldPosition;
    private float currentTargetRadius;

    /// <summary>
    /// Configura el patrón Singleton para permitir el acceso global al gestor de rangos desde cualquier punto del juego.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Asegura que el indicador de rango esté oculto al iniciar la partida.
    /// </summary>
    private void Start()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Mantiene la posición y el tamaño del indicador actualizados en cada frame, permitiendo que el círculo de rango siga a la torre o responda a cambios dinámicos.
    /// </summary>
    private void Update()
    {
        if (rangeIndicator != null && rangeIndicator.gameObject.activeSelf)
        {
            UpdateIndicatorPositionAndScale();
        }
    }

    /// <summary>
    /// Activa o desactiva la visualización del radio de alcance para una torre específica. Si la torre ya está mostrando su rango, este se oculta.
    /// </summary>
    /// <param name="tower">El GameObject de la torre que solicita mostrar su rango.</param>
    /// <param name="worldPosition">La posición central donde se debe situar el indicador.</param>
    /// <param name="radius">El radio matemático de alcance de la torre.</param>
    public void ToggleRange(GameObject tower, Vector3 worldPosition, float radius)
    {
        if (rangeIndicator == null) return;

        if (currentTowerShowingRange == tower)
        {
            HideRange();
            return;
        }

        rangeIndicator.gameObject.SetActive(true);

        currentTowerShowingRange = tower;
        currentTargetWorldPosition = worldPosition;
        currentTargetRadius = radius;

        UpdateIndicatorPositionAndScale();
    }

    /// <summary>
    /// Calcula y aplica la posición y dimensiones físicas del indicador de rango. Ajusta la coordenada Z para garantizar que el gráfico se renderice entre el suelo y los menús de interfaz.
    /// </summary>
    private void UpdateIndicatorPositionAndScale()
    {
        // 1. Posición: Situamos el indicador en la coordenada de la torre con un offset en Z.
        // Se establece en -2f para quedar por encima del escenario (0f) y por debajo del menú (-5f).
        Vector3 newPos = currentTargetWorldPosition;
        newPos.z = -2f; 
        rangeIndicator.position = newPos;

        // 2. Tamaño: Al usar un Canvas en World Space, convertimos el radio matemático 
        // en dimensiones de diámetro para el RectTransform.
        float worldDiameter = currentTargetRadius * 2f * visualScalePadding;
        rangeIndicator.sizeDelta = new Vector2(worldDiameter, worldDiameter);
    }

    /// <summary>
    /// Desactiva visualmente el indicador de rango y limpia la referencia de la torre actual.
    /// </summary>
    public void HideRange()
    {
        if (rangeIndicator == null) return;

        rangeIndicator.gameObject.SetActive(false);
        currentTowerShowingRange = null;
    }
}