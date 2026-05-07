using UnityEngine;

/// <summary>
/// Gestiona la interfaz gráfica y la lógica principal para la construcción, mejora, venta e interacción táctica de las torres mediante menús contextuales dinámicos.
/// </summary>
public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Interfaz General")] public GameObject buildMenuUI;

    [Header("Elementos del Menú")] public GameObject buildButtonsContainer;
    public GameObject towerButtonsContainer;
    public GameObject moveUnitsButton;

    [Header("UI de Mejoras")] [Tooltip("La imagen de la X que indica que la mejora está bloqueada en este nivel")]
    public GameObject upgradeLockedIcon;

    [Tooltip("El texto donde sale el precio de la mejora")]
    public TMPro.TextMeshProUGUI upgradeCostText;

    [Header("Escala del Menú Dinámica")] public bool scaleWithCameraZoom = true;
    public float baseOrthographicSize = 5f;
    public float menuScaleMultiplier = 1f;

    private Vector3 originalMenuScale;
    private Camera mainCamera;

    [Header("Prefabs de Torres")] public GameObject archerTowerPrefab;
    public GameObject barracksTowerPrefab;
    public GameObject mageTowerPrefab;
    public GameObject fixedArcherTowerPrefab;

    [Header("Costes de Torres")] public int archerCost = 80;
    public int barracksCost = 70;
    public int mageCost = 90;
    public int fixedArcherCost = 70;

    [Header("Audio")] [Tooltip("Añade aquí 1 o varios sonidos de construcción (martillazos, madera, etc).")]
    public AudioClip[] buildSounds;

    private AudioSource audioSource;

    private TowerSlot selectedSlot;
    private System.Collections.Generic.Dictionary<string, int> towerCosts;

    /// <summary>
    /// Configura el patrón Singleton para garantizar un acceso global único y enlaza el componente de audio asociado al gestor.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Inicializa las referencias a la cámara principal, oculta el menú al iniciar y construye el diccionario de costes base utilizado posteriormente para el sistema de venta y reembolsos.
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;
        if (buildMenuUI != null)
        {
            originalMenuScale = buildMenuUI.transform.localScale;
            buildMenuUI.SetActive(false);
        }

        towerCosts = new System.Collections.Generic.Dictionary<string, int>();
        if (archerTowerPrefab != null) towerCosts[archerTowerPrefab.name + "(Clone)"] = archerCost;
        if (barracksTowerPrefab != null) towerCosts[barracksTowerPrefab.name + "(Clone)"] = barracksCost;
        if (mageTowerPrefab != null) towerCosts[mageTowerPrefab.name + "(Clone)"] = mageCost;
        if (fixedArcherTowerPrefab != null) towerCosts[fixedArcherTowerPrefab.name + "(Clone)"] = fixedArcherCost;
    }

    /// <summary>
    /// Ajusta dinámicamente la escala visual del menú contextual en tiempo real para mantener proporciones legibles cuando el jugador acerca o aleja la cámara.
    /// </summary>
    void Update()
    {
        if (scaleWithCameraZoom && buildMenuUI != null && buildMenuUI.activeSelf && mainCamera != null)
        {
            float zoomRatio = mainCamera.orthographicSize / baseOrthographicSize;
            buildMenuUI.transform.localScale = originalMenuScale * zoomRatio * menuScaleMultiplier;
        }
    }

    /// <summary>
    /// Despliega el menú de construcción básico sobre una ranura vacía, ocultando indicadores previos y mostrando únicamente las opciones de compra de nuevas estructuras.
    /// </summary>
    /// <param name="slot">La ranura de construcción seleccionada por el jugador.</param>
    public void OpenBuildMenu(TowerSlot slot)
    {
        if (buildMenuUI != null) buildMenuUI.SetActive(false);
        if (RangeDisplayManager.Instance != null) RangeDisplayManager.Instance.HideRange();

        PrepareMenu(slot);
        if (buildButtonsContainer != null) buildButtonsContainer.SetActive(true);
        if (towerButtonsContainer != null) towerButtonsContainer.SetActive(false);
    }

    /// <summary>
    /// Activa el menú avanzado sobre una torre ya construida, evaluando si existen mejoras disponibles permitidas por el nivel, actualizando los costes y habilitando botones tácticos específicos como el movimiento de tropas.
    /// </summary>
    /// <param name="slot">La ranura que contiene la torre seleccionada.</param>
    public void OpenTowerMenu(TowerSlot slot)
    {
        if (buildMenuUI != null) buildMenuUI.SetActive(false);

        if (slot.currentTower == null) return;

        if (RangeDisplayManager.Instance != null) RangeDisplayManager.Instance.HideRange();

        if (moveUnitsButton != null)
        {
            bool isBarracks = slot.currentTower.GetComponent<BarracksTower>() != null;
            bool isFixedArcher = slot.currentTower.GetComponent<FixedArcherTower>() != null;
            moveUnitsButton.SetActive(isBarracks || isFixedArcher);
        }

        TowerUpgrade upgradeManager = slot.currentTower.GetComponent<TowerUpgrade>();
        if (upgradeManager != null)
        {
            bool isAllowed = LevelUpgradeSettings.Instance != null &&
                             LevelUpgradeSettings.Instance.IsUpgradeAllowed(upgradeManager.towerType,
                                 upgradeManager.currentUpgradeLevel);

            bool hasMore = upgradeManager.HasMoreUpgrades();

            if (upgradeLockedIcon != null)
            {
                upgradeLockedIcon.SetActive(!isAllowed || !hasMore);
            }

            if (upgradeCostText != null)
            {
                if (!hasMore)
                {
                    upgradeCostText.text = "MAX";
                }
                else if (!isAllowed)
                {
                    upgradeCostText.text = "---";
                }
                else
                {
                    upgradeCostText.text = upgradeManager.GetNextUpgrade().cost.ToString();
                }
            }
        }

        PrepareMenu(slot);

        if (buildButtonsContainer != null) buildButtonsContainer.SetActive(false);
        if (towerButtonsContainer != null) towerButtonsContainer.SetActive(true);

        float range = 0f;
        if (slot.currentTower.GetComponent<ArcherTower>()) range = slot.currentTower.GetComponent<ArcherTower>().range;
        if (slot.currentTower.GetComponent<BarracksTower>())
            range = slot.currentTower.GetComponent<BarracksTower>().rallyRange;
        if (slot.currentTower.GetComponent<MageTower>()) range = slot.currentTower.GetComponent<MageTower>().range;
        if (slot.currentTower.GetComponent<FixedArcherTower>())
            range = slot.currentTower.GetComponent<FixedArcherTower>().rallyRange;

        if (range > 0f && RangeDisplayManager.Instance != null)
        {
            RangeDisplayManager.Instance.ToggleRange(slot.currentTower, slot.transform.position, range);
        }
    }

    /// <summary>
    /// Centraliza la lógica de posicionamiento, escala y ordenación de capas (Sorting Layer) del menú en el espacio del mundo, garantizando que la interfaz nunca quede oculta tras otros elementos del juego.
    /// </summary>
    /// <param name="slot">La ranura que anclará la posición del menú.</param>
    private void PrepareMenu(TowerSlot slot)
    {
        if (selectedSlot != null && selectedSlot != slot)
        {
            selectedSlot.OnMenuClosed();
        }

        selectedSlot = slot;

        if (buildMenuUI == null) return;
        buildMenuUI.SetActive(true);
        buildMenuUI.transform.position = new Vector3(slot.transform.position.x, slot.transform.position.y, -5f);

        // --- SOLUCIÓN DEL SORTING LAYER (VERSIÓN DEFINITIVA) ---
        Canvas menuCanvas = buildMenuUI.GetComponent<Canvas>();
        if (menuCanvas != null)
        {
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 30000;
        }
        else
        {
            UnityEngine.Rendering.SortingGroup sortGroup =
                buildMenuUI.GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (sortGroup != null)
            {
                sortGroup.sortingOrder = 30000;
            }
        }
        // --------------------------------------------------------

        if (scaleWithCameraZoom && mainCamera != null)
        {
            float zoomRatio = mainCamera.orthographicSize / baseOrthographicSize;
            buildMenuUI.transform.localScale = originalMenuScale * zoomRatio * menuScaleMultiplier;
        }
        else
        {
            buildMenuUI.transform.localScale = originalMenuScale;
        }
    }

    /// <summary>
    /// Verifica si el menú contextual está actualmente abierto y focalizado en una ranura específica.
    /// </summary>
    /// <param name="slot">La ranura a consultar.</param>
    /// <returns>Verdadero si el menú corresponde a la ranura indicada; falso en caso contrario.</returns>
    public bool IsMenuOpenFor(TowerSlot slot)
    {
        return (buildMenuUI != null && buildMenuUI.activeSelf && selectedSlot == slot);
    }

    /// <summary>
    /// Cierra y oculta el menú contextual actual, liberando las referencias a las ranuras y apagando los indicadores de rango en el mapa.
    /// </summary>
    public void CloseBuildMenu()
    {
        if (buildMenuUI != null) buildMenuUI.SetActive(false);
        if (selectedSlot != null)
        {
            selectedSlot.OnMenuClosed();
            selectedSlot = null;
        }

        if (RangeDisplayManager.Instance != null) RangeDisplayManager.Instance.HideRange();
    }

    /// <summary>
    /// Verifica si el menú contextual está actualmente abierto y focalizado en una ranura específica.
    /// </summary>
    /// <param name="slot">La ranura a consultar.</param>
    /// <returns>Verdadero si el menú corresponde a la ranura indicada; falso en caso contrario.</returns>
    public void BuildArcher()
    {
        BuildTower(archerTowerPrefab, archerCost);
    }

    public void BuildBarracks()
    {
        BuildTower(barracksTowerPrefab, barracksCost);
    }

    public void BuildMage()
    {
        BuildTower(mageTowerPrefab, mageCost);
    }

    public void BuildFixedArcher()
    {
        BuildTower(fixedArcherTowerPrefab, fixedArcherCost);
    }

    /// <summary>
    /// Ejecuta la validación económica y la instanciación de un nuevo prefab de torre, deduciendo el coste del oro del jugador y registrando la estructura construida.
    /// </summary>
    /// <param name="towerPrefab">El GameObject correspondiente a la torre deseada.</param>
    /// <param name="cost">El coste en oro de la torre.</param>
    private void BuildTower(GameObject towerPrefab, int cost)
    {
        if (selectedSlot == null || towerPrefab == null || GameManager.Instance == null ||
            GameManager.Instance.gold < cost)
        {
            CloseBuildMenu();
            return;
        }

        GameManager.Instance.AddGold(-cost);
        GameManager.Instance.RegisterTowerBuilt(towerPrefab);

        GameObject newTower = Instantiate(towerPrefab, selectedSlot.transform.position, Quaternion.identity);
        selectedSlot.currentTower = newTower;
        selectedSlot.ClearHover();

        PlayBuildSound();

        CloseBuildMenu();
    }

    /// <summary>
    /// Selecciona aleatoriamente y reproduce un efecto sonoro de construcción, utilizando el emisor del gestor o uno espacial temporal si es necesario.
    /// </summary>
    private void PlayBuildSound()
    {
        if (buildSounds != null && buildSounds.Length > 0)
        {
            AudioClip clipToPlay = buildSounds[Random.Range(0, buildSounds.Length)];

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
            else
            {
                if (mainCamera != null)
                {
                    AudioSource.PlayClipAtPoint(clipToPlay, mainCamera.transform.position);
                }
            }
        }
    }

    /// <summary>
    /// Solicita al componente gestor de la torre actual que aplique el siguiente nivel de mejora si el jugador cuenta con el oro requerido y el nivel en curso lo permite.
    /// </summary>
    public void UpgradeTower()
    {
        if (selectedSlot == null || selectedSlot.currentTower == null) return;

        TowerUpgrade upgradeManager = selectedSlot.currentTower.GetComponent<TowerUpgrade>();
        if (upgradeManager != null && upgradeManager.HasMoreUpgrades())
        {
            bool isAllowed = LevelUpgradeSettings.Instance == null ||
                             LevelUpgradeSettings.Instance.IsUpgradeAllowed(upgradeManager.towerType,
                                 upgradeManager.currentUpgradeLevel);

            if (!isAllowed)
            {
                Debug.Log("Mejora bloqueada en este nivel.");
                return;
            }

            TowerUpgrade.UpgradeData nextUpgrade = upgradeManager.GetNextUpgrade();
            if (GameManager.Instance.gold >= nextUpgrade.cost)
            {
                GameManager.Instance.AddGold(-nextUpgrade.cost);
                upgradeManager.ApplyUpgrade();

                CloseBuildMenu();
            }
        }
    }

    /// <summary>
    /// Desmantela la torre seleccionada devolviendo un porcentaje fraccionado del coste original al jugador mediante la consulta del diccionario de valores.
    /// </summary>
    public void SellTower()
    {
        if (selectedSlot != null && selectedSlot.currentTower != null)
        {
            int returnGold = 0;
            if (towerCosts.ContainsKey(selectedSlot.currentTower.name))
            {
                returnGold = Mathf.RoundToInt(towerCosts[selectedSlot.currentTower.name] * 0.75f);
            }

            if (GameManager.Instance != null && returnGold > 0)
            {
                GameManager.Instance.AddGold(returnGold);
            }

            Destroy(selectedSlot.currentTower);
            selectedSlot.currentTower = null;
            if (RangeDisplayManager.Instance != null)
            {
                RangeDisplayManager.Instance.HideRange();
            }
        }

        CloseBuildMenu();
    }

    /// <summary>
    /// Intercepta el comando táctico para reubicar unidades y transfiere el control de interacción al gestor global de puntos de reunión (RallyPointManager).
    /// </summary>
    public void SetRallyPoint()
    {
        if (selectedSlot == null || selectedSlot.currentTower == null) return;

        IMoveableTower moveableTower = selectedSlot.currentTower.GetComponent<IMoveableTower>();
        if (moveableTower != null)
        {
            if (buildMenuUI != null)
            {
                buildMenuUI.SetActive(false);
            }

            RallyPointManager.Instance.StartRallyPointSelection(moveableTower);
        }
    }
}