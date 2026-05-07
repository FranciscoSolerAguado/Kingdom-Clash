using UnityEngine;

/// <summary>
/// Gestiona el sistema interactivo de posicionamiento de puntos de reunión (Rally Points), validando la proximidad a los caminos y el rango de la torre, y proporcionando retroalimentación visual al jugador.
/// </summary>
public class RallyPointManager : MonoBehaviour
{
    public static RallyPointManager Instance;

    [Header("Referencias")]
    public GameObject rallyCursorPrefab;

    private Path[] allPathsInLevel;

    [Header("Configuración")]
    public float placedFlagDuration = 0.5f;
    public float pathProximityThreshold = 1.0f;
    public GameObject rallyPlacedPrefab;
    public GameObject rallyInvalidPrefab;

    private bool isSelecting;
    private GameObject rallyCursorInstance;
    private IMoveableTower currentTower;
    private Camera mainCamera;

    /// <summary>
    /// Configura el patrón Singleton y asegura la persistencia o destrucción de instancias duplicadas del gestor.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Inicializa las referencias a la cámara, localiza dinámicamente todos los caminos disponibles en el nivel e instancia el cursor de selección oculto.
    /// </summary>
    private void Start()
    {
        mainCamera = Camera.main;
        isSelecting = false;

        // Recopilamos todos los caminos del nivel al empezar
        allPathsInLevel = FindObjectsOfType<Path>();

        if (rallyCursorPrefab != null)
        {
            rallyCursorInstance = Instantiate(rallyCursorPrefab);
            rallyCursorInstance.SetActive(false);
        }
    }

    /// <summary>
    /// Activa el modo de selección de punto de reunión para una torre específica, ocultando el cursor del sistema y habilitando el cursor visual del juego.
    /// </summary>
    /// <param name="tower">La torre que implementa la interfaz IMoveableTower que solicita el cambio.</param>
    public void StartRallyPointSelection(IMoveableTower tower)
    {
        if (isSelecting) return;
        currentTower = tower;
        isSelecting = true;

        if (rallyCursorInstance != null) rallyCursorInstance.SetActive(true);
        Cursor.visible = false;
    }

    /// <summary>
    /// Bucle de actualización que sincroniza el cursor con la posición del ratón, evalúa la validez de la posición en tiempo real y procesa la confirmación o cancelación del comando.
    /// </summary>
    private void Update()
    {
        if (!isSelecting) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        rallyCursorInstance.transform.position = mouseWorldPos;

        bool isValidPosition = IsPositionValid(mouseWorldPos);

        SpriteRenderer cursorSprite = rallyCursorInstance.GetComponent<SpriteRenderer>();
        if (cursorSprite != null)
        {
            cursorSprite.color = isValidPosition ? Color.white : Color.red;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isValidPosition)
            {
                currentTower.SetRallyPoint(mouseWorldPos);
                PlaceRallyFlagEffect(mouseWorldPos);
                StopSelection();
            }
            else
            {
                PlaceInvalidCrossEffect(mouseWorldPos);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            StopSelection();
        }
    }

    /// <summary>
    /// Realiza una doble validación: comprueba que el punto seleccionado esté dentro del radio de acción de la torre y que se encuentre a una distancia mínima de algún camino transitable.
    /// </summary>
    /// <param name="position">La coordenada del mundo a evaluar.</param>
    /// <returns>Verdadero si la posición cumple todos los requisitos tácticos; falso en caso contrario.</returns>
    private bool IsPositionValid(Vector3 position)
    {
        if (currentTower == null) return false;

        float distanceToTower = Vector3.Distance(position, currentTower.transform.position);
        if (distanceToTower > currentTower.RallyRange)
        {
            return false;
        }

        // Comprobamos si está cerca de CUALQUIER camino
        if (!IsCloseToAnyPath(position))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Itera a través de todos los trazados de ruta del nivel para determinar si el punto indicado es lo suficientemente cercano a algún segmento de camino.
    /// </summary>
    /// <param name="point">El punto de destino propuesto.</param>
    /// <returns>Verdadero si el punto es adyacente a un camino; falso si está demasiado lejos.</returns>
    private bool IsCloseToAnyPath(Vector3 point)
    {
        if (allPathsInLevel == null || allPathsInLevel.Length == 0) return false;

        foreach (Path currentPath in allPathsInLevel)
        {
            if (currentPath.points == null || currentPath.points.Length < 2) continue;

            for (int i = 0; i < currentPath.points.Length - 1; i++)
            {
                Vector3 p1 = currentPath.points[i].position;
                Vector3 p2 = currentPath.points[i+1].position;
                Vector3 closestPointOnSegment = GetClosestPointOnSegment(p1, p2, point);

                if (Vector3.Distance(point, closestPointOnSegment) <= pathProximityThreshold)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Función matemática que proyecta un punto sobre un segmento definido por dos vectores, limitando el resultado al espacio entre ambos extremos.
    /// </summary>
    private Vector3 GetClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ap = p - a;
        Vector3 ab = b - a;
        float t = Vector3.Dot(ap, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    /// <summary>
    /// Instancia un efecto visual temporal (bandera) para confirmar al jugador que el punto de reunión ha sido establecido con éxito.
    /// </summary>
    private void PlaceRallyFlagEffect(Vector3 position)
    {
        if (rallyPlacedPrefab != null)
        {
            GameObject flagEffect = Instantiate(rallyPlacedPrefab, position, Quaternion.identity);
            Destroy(flagEffect, placedFlagDuration);
        }
    }

    /// <summary>
    /// Instancia un efecto visual de advertencia (aspa) indicando al jugador que la posición seleccionada no es válida para las unidades.
    /// </summary>
    private void PlaceInvalidCrossEffect(Vector3 position)
    {
        if (rallyInvalidPrefab != null)
        {
            GameObject crossEffect = Instantiate(rallyInvalidPrefab, position, Quaternion.identity);
            Destroy(crossEffect, placedFlagDuration);
        }
    }

    /// <summary>
    /// Finaliza el proceso de selección, restaura la visibilidad del cursor del sistema y limpia las referencias temporales de la torre.
    /// </summary>
    private void StopSelection()
    {
        isSelecting = false;
        if (rallyCursorInstance != null) rallyCursorInstance.SetActive(false);
        currentTower = null;
        Cursor.visible = true;

        if (RangeDisplayManager.Instance != null)
        {
            RangeDisplayManager.Instance.HideRange();
        }
    }

    /// <summary>
    /// Traduce las coordenadas de pantalla del ratón a coordenadas del mundo 2D utilizando la cámara principal.
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.nearClipPlane;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePos);
        worldPosition.z = 0;
        return worldPosition;
    }
}