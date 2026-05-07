using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el reclutamiento, posicionamiento espacial y seguimiento de las unidades mineras, limitando la cantidad máxima en el mapa y coordinando la interfaz con el gestor principal.
/// </summary>
public class MinerManager : MonoBehaviour
{
    public static MinerManager Instance;

    [Header("Configuración del Minero")]
    public GameObject minerPrefab;
    public int minerCost = 20;
    public int maxMiners = 3;

    [Header("Referencias")]
    public Transform goldStone; 

    private GameObject[] activeMiners;

    private Vector2[] spawnOffsets = new Vector2[3] {
        new Vector2(-0.5f, 0.75f),
        new Vector2(0.5f, 0.75f),
        new Vector2(-0.4f, 1.325f)
    };

    /// <summary>
    /// Configura el patrón Singleton para garantizar que solo exista una única instancia de este gestor durante toda la partida.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Inicializa la capacidad máxima de mineros, localiza automáticamente el recurso de piedra de oro si no fue asignado en el inspector y actualiza el contador visual.
    /// </summary>
    void Start()
    {
        activeMiners = new GameObject[maxMiners];

        if (goldStone == null)
        {
            GameObject stoneObj = GameObject.FindGameObjectWithTag("goldStone");
            if (stoneObj != null)
            {
                goldStone = stoneObj.transform;
                // DEBUG: Esto nos dirá qué objeto ha encontrado y dónde está
                Debug.Log($"MinerManager: Piedra encontrada por Tag. Nombre: {stoneObj.name}, Posición: {stoneObj.transform.position}");
            }
            else
            {
                Debug.LogError("MinerManager: ¡No se ha encontrado ningún objeto con el Tag 'goldStone'!");
            }
        }

        UpdateUI();
    }

    /// <summary>
    /// Intenta reclutar e instanciar un nuevo minero, asignándole el primer hueco disponible alrededor de la mina de oro y orientando su sprite de forma acorde.
    /// </summary>
    /// <returns>Verdadero si el despliegue fue exitoso; falso si no hay huecos libres o falta la referencia de la piedra.</returns>
    public bool TryDeployMiner()
    {
        int freeIndex = -1;
        for (int i = 0; i < maxMiners; i++)
        {
            if (activeMiners[i] == null)
            {
                freeIndex = i;
                break;
            }
        }

        if (freeIndex == -1) return false;

        // Si la piedra sigue siendo null aquí, algo va mal
        if (goldStone == null) {
            Debug.LogError("MinerManager: Intentando spawnear pero 'goldStone' es NULL.");
            return false;
        }

        if (minerPrefab == null) return false;

        Vector2 offset = spawnOffsets[freeIndex];

        // Calculamos la posición real
        Vector3 spawnPosition = goldStone.position + new Vector3(offset.x, offset.y, 0f);

        // DEBUG: Para ver en consola la coordenada exacta del spawn
        Debug.Log($"MinerManager: Spawneando minero {freeIndex} en {spawnPosition}. La piedra está en {goldStone.position}");

        // Forzamos la Z a 0 o a la misma de la piedra para evitar que se esconda tras el mapa
        spawnPosition.z = goldStone.position.z;

        GameObject newMiner = Instantiate(minerPrefab, spawnPosition, Quaternion.identity);
        newMiner.tag = "ally";

        // Ajuste de escala (flip)
        Vector3 scale = newMiner.transform.localScale;
        scale.x = (freeIndex == 1) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        newMiner.transform.localScale = scale;

        activeMiners[freeIndex] = newMiner;

        UpdateUI();
        return true;
    }

    /// <summary>
    /// Evalúa el arreglo de mineros activos para determinar si queda al menos un espacio libre en la zona de trabajo.
    /// </summary>
    /// <returns>Verdadero si se puede reclutar un nuevo minero; falso en caso contrario.</returns>
    public bool CanDeployMiner()
    {
        for (int i = 0; i < maxMiners; i++)
        {
            if (activeMiners[i] == null) return true;
        }
        return false;
    }

    /// <summary>
    /// Libera la plaza de trabajo ocupada por un minero específico tras su muerte, permitiendo su futuro reemplazo.
    /// </summary>
    /// <param name="miner">La referencia del objeto (minero) que acaba de morir.</param>
    public void OnMinerDied(GameObject miner)
    {
        for (int i = 0; i < activeMiners.Length; i++)
        {
            if (activeMiners[i] == miner)
            {
                activeMiners[i] = null;
                break;
            }
        }
        UpdateUI();
    }
    
    /// <summary>
    /// Calcula el total de trabajadores actuales y envía la actualización al GameManager para reflejar los cambios en la interfaz del jugador.
    /// </summary>
    private void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        int currentCount = 0;
        for (int i = 0; i < maxMiners; i++)
        {
            if (activeMiners[i] != null) currentCount++;
        }

        GameManager.Instance.UpdateMinersText(currentCount, maxMiners);
    }
}