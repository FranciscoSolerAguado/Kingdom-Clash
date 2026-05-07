using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Gestiona el sistema de despliegue táctico de unidades, controlando la previsualización visual (fantasmas), la validación de posición respecto a los caminos y la instanciación física de múltiples unidades en formación.
/// </summary>
public class DeployManager : MonoBehaviour
{
    public static DeployManager Instance;

    [Header("Ajustes de Despliegue")]
    public float maxDistanceFromPath = 2.0f;
    public float multiSpawnRadius = 0.3f;

    [Header("Ajuste Visual y de Lógica")]
    public Vector2 cursorOffset = Vector2.zero;
    public Vector2 pathValidationOffset = new Vector2(0f, 0.5f);

    [Header("Restricciones")]
    public LayerMask towerLayer; 
    public float overlapCheckRadius = 0.5f;

    private GameObject currentUnitPrefab;
    private int currentSpawnCount = 1;
    private int currentGoldCost = 0; 
    private DeployButton currentButtonOrigin;
    
    private GameObject ghostContainer;
    private List<SpriteRenderer> ghostRenderers = new List<SpriteRenderer>();
    private Vector2[] currentOffsets;
    private Vector3 prefabSpriteLocalPos;

    private bool isDeploying = false;
    private bool hasSkippedFirstFrame = false;

    private Path[] allPaths;

    public bool IsDeploying => isDeploying;
    public DeployButton GetCurrentButtonOrigin() => currentButtonOrigin;

    /// <summary>
    /// Configura el patrón Singleton para garantizar un punto de acceso único al sistema de despliegue.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Localiza todos los caminos disponibles en la escena al inicio para optimizar los cálculos de validación de posición posteriores.
    /// </summary>
    private void Start()
    {
        allPaths = FindObjectsByType<Path>(FindObjectsSortMode.None);
    }

    /// <summary>
    /// Inicia el modo de despliegue, calculando las posiciones de formación para el número de unidades solicitado y creando las representaciones visuales semitransparentes (ghosts) que seguirán al cursor.
    /// </summary>
    /// <param name="unitPrefab">El prefab de la unidad a desplegar.</param>
    /// <param name="spawnCount">Cantidad de unidades que forman el grupo.</param>
    /// <param name="originButton">Referencia al botón de UI que disparó la acción.</param>
    /// <param name="goldCost">Coste económico del despliegue.</param>
    public void StartDeployment(GameObject unitPrefab, int spawnCount, DeployButton originButton, int goldCost)
    {
        if (isDeploying) CancelDeployment();

        if (GameManager.Instance != null && GameManager.Instance.gold < goldCost) return;

        currentUnitPrefab = unitPrefab;
        currentSpawnCount = spawnCount;
        currentButtonOrigin = originButton;
        currentGoldCost = goldCost;
        isDeploying = true;
        hasSkippedFirstFrame = false;

        // Cálculo de formación: determina los offsets de cada unidad respecto al centro del cursor
        currentOffsets = new Vector2[spawnCount];
        if (spawnCount == 1) { currentOffsets[0] = Vector2.zero; } 
        else if (spawnCount == 2) { currentOffsets[0] = new Vector2(-multiSpawnRadius, 0); currentOffsets[1] = new Vector2(multiSpawnRadius, 0); }
        else if (spawnCount == 3) { currentOffsets[0] = new Vector2(0, multiSpawnRadius); currentOffsets[1] = new Vector2(-multiSpawnRadius, -multiSpawnRadius * 0.5f); currentOffsets[2] = new Vector2(multiSpawnRadius, -multiSpawnRadius * 0.5f); }
        else 
        {
            for (int i = 0; i < spawnCount; i++) 
            {
                float angle = i * (Mathf.PI * 2) / spawnCount;
                angle += Mathf.PI / 2; 
                currentOffsets[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * multiSpawnRadius;
            }
        }

        // Creación del contenedor de previsualización (Ghost)
        ghostContainer = new GameObject("DeployGhostContainer");
        ghostContainer.transform.localScale = Vector3.one;
        ghostRenderers.Clear();
        
        SpriteRenderer prefabSprite = unitPrefab.GetComponentInChildren<SpriteRenderer>();
        prefabSpriteLocalPos = prefabSprite != null ? prefabSprite.transform.localPosition : Vector3.zero;
        
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject ghostObj = new GameObject($"GhostSprite_{i}");
            ghostObj.transform.SetParent(ghostContainer.transform);
            ghostObj.transform.localPosition = new Vector3(currentOffsets[i].x, currentOffsets[i].y, 0f);
            
            if (prefabSprite != null)
            {
                ghostObj.transform.localRotation = prefabSprite.transform.localRotation;
                ghostObj.transform.localScale = prefabSprite.transform.localScale;

                SpriteRenderer sr = ghostObj.AddComponent<SpriteRenderer>();
                sr.sprite = prefabSprite.sprite;
                sr.color = new Color(1f, 1f, 1f, 0.5f);
                sr.sortingOrder = 999 + i;
                
                ghostRenderers.Add(sr);
            }
        }
    }

    /// <summary>
    /// Actualiza la posición de la previsualización y detecta las entradas del usuario para confirmar el despliegue (clic izquierdo) o cancelarlo (clic derecho o Escape).
    /// </summary>
    private void Update()
    {
        if (!isDeploying) return;

        if (!hasSkippedFirstFrame)
        {
            hasSkippedFirstFrame = true;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverButton()) return;
            HandlePlacement();
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelDeployment();
        }

        UpdateGhostVisuals();
    }

    /// <summary>
    /// Valida la posición final, deduce el coste de oro, instancia las unidades reales en el mapa y activa el enfriamiento del botón de origen.
    /// </summary>
    private void HandlePlacement()
    {
        Vector3 checkPathPos = GetValidationPosition();
        bool isValidPosition = IsPositionValid(checkPathPos) && !IsOverTower(GetLogicBasePosition());

        if (isValidPosition)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(-currentGoldCost); 
            }

            Vector3 logicBasePos = GetLogicBasePosition();
            for (int i = 0; i < currentSpawnCount; i++)
            {
                Vector3 spawnPos = logicBasePos + new Vector3(currentOffsets[i].x, currentOffsets[i].y, 0f);
                Instantiate(currentUnitPrefab, spawnPos, Quaternion.identity);
            }

            if (currentButtonOrigin != null)
            {
                currentButtonOrigin.StartCooldown();
            }
        }
        
        CancelDeployment();
    }

    /// <summary>
    /// Sincroniza el contenedor de fantasmas con el ratón y cambia el color de los Sprites (blanco/rojo) para indicar si la posición actual es válida para el despliegue.
    /// </summary>
    private void UpdateGhostVisuals()
    {
        Vector3 rawMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rawMousePos.z = 0f;
        Vector3 visualCenter = rawMousePos + (Vector3)cursorOffset;

        if (ghostContainer != null)
        {
            ghostContainer.transform.position = visualCenter;
        }

        Vector3 checkPathPos = GetValidationPosition();
        bool isValidPosition = IsPositionValid(checkPathPos) && !IsOverTower(GetLogicBasePosition());

        if (GameManager.Instance != null && GameManager.Instance.gold < currentGoldCost)
        {
            CancelDeployment();
            return;
        }

        Color ghostColor = isValidPosition ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        foreach (SpriteRenderer sr in ghostRenderers)
        {
            if (sr != null) sr.color = ghostColor;
        }
    }

    /// <summary>
    /// Finaliza el modo de despliegue de forma segura, destruyendo los objetos de previsualización y limpiando las referencias de datos temporales.
    /// </summary>
    public void CancelDeployment()
    {
        isDeploying = false;
        currentUnitPrefab = null;
        currentButtonOrigin = null; 
        currentGoldCost = 0;
        ghostRenderers.Clear();
        if (ghostContainer != null)
        {
            Destroy(ghostContainer);
        }
    }

    /// <summary>
    /// Utiliza el sistema de eventos de Unity para verificar si el cursor se encuentra sobre un botón de la interfaz y así evitar despliegues accidentales al interactuar con el menú.
    /// </summary>
    private bool IsPointerOverButton()
    {
        if (EventSystem.current == null) return false;
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Button>() != null) return true;
        }
        return false;
    }

    /// <summary>
    /// Comprueba si la posición indicada se encuentra dentro de la distancia de seguridad respecto a cualquier segmento de los caminos del nivel.
    /// </summary>
    private bool IsPositionValid(Vector3 pos)
    {
        if (allPaths == null || allPaths.Length == 0) return true;
        foreach (Path path in allPaths)
        {
            if (path.points == null || path.points.Length < 2) continue;
            for (int i = 0; i < path.points.Length - 1; i++)
            {
                if (DistancePointLine(pos, path.points[i].position, path.points[i+1].position) <= maxDistanceFromPath)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Verifica mediante un círculo de colisión si la posición de despliegue solapa con alguna estructura de torre existente.
    /// </summary>
    private bool IsOverTower(Vector3 pos)
    {
        return towerLayer.value != 0 && Physics2D.OverlapCircle(pos, overlapCheckRadius, towerLayer) != null;
    }

    /// <summary>
    /// Calcula el punto de origen lógico para las unidades en el mundo, ajustando el desfase visual del cursor y del sprite del prefab.
    /// </summary>
    private Vector3 GetLogicBasePosition()
    {
        Vector3 rawMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rawMousePos.z = 0f;
        Vector3 visualCenter = rawMousePos + (Vector3)cursorOffset;
        return visualCenter - prefabSpriteLocalPos;
    }

    /// <summary>
    /// Aplica un offset adicional de validación para determinar el punto exacto que debe ser comprobado contra los caminos.
    /// </summary>
    private Vector3 GetValidationPosition()
    {
        return GetLogicBasePosition() + (Vector3)pathValidationOffset;
    }

    /// <summary>
    /// Función matemática que calcula la distancia más corta entre un punto y un segmento de línea definido por dos vectores.
    /// </summary>
    private float DistancePointLine(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector2 p2 = new Vector2(p.x, p.y);
        Vector2 a2 = new Vector2(a.x, a.y);
        Vector2 b2 = new Vector2(b.x, b.y);
        float l2 = (a2 - b2).sqrMagnitude;
        if (l2 == 0) return Vector2.Distance(p2, a2);
        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p2 - a2, b2 - a2) / l2));
        Vector2 projection = a2 + t * (b2 - a2);
        return Vector2.Distance(p2, projection);
    }
}