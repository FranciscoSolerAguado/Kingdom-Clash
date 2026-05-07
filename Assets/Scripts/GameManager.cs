using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controlador maestro del nivel. Administra el estado global de la partida (recursos, vidas, oleadas), 
/// la persistencia de datos a través del sistema de guardado, la lógica de victoria/derrota 
/// y la sincronización de la interfaz de usuario con los eventos del juego.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración del Nivel")]
    public int levelIndex = 0;

    [Header("Atributos del Jugador")]
    public int lives = 20;
    public int gold = 0;

    [Header("Progreso de Construcción")]
    public int archerTowersBuilt = 0;
    public int barracksTowersBuilt = 0;
    public int mageTowersBuilt = 0;
    public int fixedArcherTowersBuilt = 0;

    [Header("Referencias de Interfaz (UI)")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI minersText;
    public TextMeshProUGUI waveText;
    public GameObject nextWaveButton;
    public Image nextWaveButtonOverlay;
    public TextMeshProUGUI countdownText;

    [Header("Menús de Fin de Partida")]
    public GameObject victoryMenuUI;
    public GameObject defeatMenuUI;
    public GameObject[] victoryStars;

    [Header("Criterios de Estrellas")]
    public int livesForThreeStars = 18;
    public int livesForTwoStars = 10;
    public int livesForOneStar = 1;

    [Header("Menús y Escenas")]
    public GameObject settingsMenuUI;

    [Tooltip("Arrastra aquí el panel que contiene la información del nivel.")]
    public GameObject levelInfoPanel;

    [Header("Control de Audio (UI)")]
    [Tooltip("La imagen de la cruz roja que indica que el sonido está silenciado.")]
    public GameObject muteCrossImage;
    private bool isSceneMuted = false;

    [Header("Control de Audio (Oleadas)")]
    [Tooltip("AudioSource para los sonidos de las oleadas. Si está vacío, se creará uno automáticamente.")]
    public AudioSource waveAudioSource;
    [Tooltip("Sonido que se reproducirá únicamente cuando el temporizador llegue a 0.")]
    public AudioClip autoWaveStartSound;

    public string levelSelectorSceneName = "LevelSelectorScene";
    public string titleScreenSceneName = "TitleScreen";

    [Header("Animaciones UI")]
    public Animator goldUIAnimator;
    public Animator livesUIAnimator;
    public Animator waveUIAnimator;
    public Animator minersUIAnimator;

    [Header("Referencias de Escena")]
    public EnemySpawner enemySpawner;

    public bool hasGameStarted { get; private set; } = false;
    public bool isGamePaused { get; private set; } = false;
    private bool isGameOver = false;

    /// <summary>
    /// Escanea la escena en busca de cualquier objeto con la etiqueta "Enemy" que no haya muerto,
    /// utilizado para determinar si se puede dar por finalizada una oleada.
    /// </summary>
    /// <returns>Verdadero si hay enemigos activos en el campo de batalla.</returns>
    public bool AreEnemiesAlive()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null && !health.isDead) return true;
        }
        return false;
    }

    /// <summary>
    /// Implementa el patrón Singleton para asegurar que solo exista una instancia del gestor de juego activa.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Inicialización segura del AudioSource si se nos olvida asignarlo
        if (waveAudioSource == null)
        {
            waveAudioSource = GetComponent<AudioSource>();
            if (waveAudioSource == null)
            {
                waveAudioSource = gameObject.AddComponent<AudioSource>();
                waveAudioSource.playOnAwake = false;
            }
        }
    }

    /// <summary>
    /// Inicializa la interfaz, oculta los menús de fin de partida y gestiona la visualización
    /// del panel de información inicial pausando el flujo temporal del juego.
    /// </summary>
    void Start()
    {
        UpdateUI();
        ShowCountdown(false);

        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (victoryMenuUI != null) victoryMenuUI.SetActive(false);
        if (defeatMenuUI != null) defeatMenuUI.SetActive(false);

        if (muteCrossImage != null) muteCrossImage.SetActive(false);
        isSceneMuted = false;

        if (victoryStars != null)
        {
            foreach (GameObject star in victoryStars)
            {
                if (star != null) star.SetActive(false);
            }
        }

        if (levelInfoPanel != null) ShowLevelInfo();
        else Time.timeScale = 1f;
    }

    /// <summary>
    /// Alterna el estado de silencio de la música de fondo a través del MusicManager
    /// y actualiza el indicador visual en el menú de ajustes.
    /// </summary>
    public void ToggleSceneMusic()
    {
        isSceneMuted = !isSceneMuted;
        if (muteCrossImage != null) muteCrossImage.SetActive(isSceneMuted);
        if (MusicManager.Instance != null) MusicManager.Instance.SetSceneMute(isSceneMuted);
    }

    /// <summary>
    /// Detiene el tiempo y despliega el panel de información sobre el nivel actual.
    /// </summary>
    public void ShowLevelInfo()
    {
        if (isGameOver) return;
        isGamePaused = true;
        if (levelInfoPanel != null) levelInfoPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Cierra el panel de información y restaura la velocidad normal del juego.
    /// </summary>
    public void HideLevelInfo()
    {
        if (isGameOver) return;
        isGamePaused = false;
        if (levelInfoPanel != null) levelInfoPanel.SetActive(false);
        if (settingsMenuUI == null || !settingsMenuUI.activeSelf) Time.timeScale = 1f;
    }

    /// <summary>
    /// Pausa la lógica del juego y activa el menú de ajustes/opciones.
    /// </summary>
    public void PauseGame()
    {
        if (isGameOver) return;
        isGamePaused = true;
        if (settingsMenuUI != null) settingsMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Desactiva el menú de pausa y reanuda el tiempo de simulación.
    /// </summary>
    public void ResumeGame()
    {
        if (isGameOver) return;
        isGamePaused = false;
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (levelInfoPanel == null || !levelInfoPanel.activeSelf) Time.timeScale = 1f;
    }

    public void GoToLevelSelector() { Time.timeScale = 1f; SceneManager.LoadScene(levelSelectorSceneName); }
    public void GoToTitleScreen() { Time.timeScale = 1f; SceneManager.LoadScene(titleScreenSceneName); }
    public void RestartLevel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void StartGame() { hasGameStarted = true; }

    /// <summary>
    /// Controla la visibilidad del botón para llamar manualmente a la siguiente oleada.
    /// </summary>
    public void ShowNextWaveButton(bool show)
    {
        if (nextWaveButton != null && !isGameOver) nextWaveButton.SetActive(show);
    }

    /// <summary>
    /// Cambia la interactividad del botón de oleada y su capa visual de bloqueo, 
    /// evitando que se pulse durante el transcurso de una oleada activa.
    /// </summary>
    public void SetNextWaveButtonState(bool isInteractable)
    {
        if (isGameOver) return;
        if (nextWaveButton != null)
        {
            Button btn = nextWaveButton.GetComponent<Button>();
            if (btn != null) btn.interactable = isInteractable;
        }
        if (nextWaveButtonOverlay != null) nextWaveButtonOverlay.gameObject.SetActive(!isInteractable);
    }

    public void SetNextWaveButtonText(string text)
    {
        if (nextWaveButton != null)
        {
            TextMeshProUGUI btnText = nextWaveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = text;
        }
    }

    /// <summary>
    /// Muestra u oculta el texto de cuenta atrás para el inicio automático de la siguiente oleada.
    /// </summary>
    /// <param name="show">Estado de activación deseado.</param>
    public void ShowCountdown(bool show)
    {
        if (countdownText != null && countdownText.gameObject.activeSelf != show && !isGameOver)
        {
            countdownText.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// Actualiza el texto de la interfaz con el tiempo restante para la llegada de enemigos.
    /// </summary>
    /// <param name="time">Segundos restantes.</param>
    public void UpdateCountdown(float time)
    {
        if (countdownText != null && !isGameOver)
        {
            time = Mathf.Max(0, time);
            countdownText.text = $"Siguiente oleada en: {Mathf.CeilToInt(time)}s";
        }
    }

    /// <summary>
    /// Actualiza el marcador de oleadas y dispara una animación de pulso visual en la interfaz.
    /// </summary>
    public void UpdateWaveCounter(int current, int total)
    {
        if (waveText != null) waveText.text = $"{current}/{total}";
        if (waveUIAnimator != null)
        {
            if (waveUIAnimator.GetCurrentAnimatorStateInfo(0).IsName("Pulse")) return;
            waveUIAnimator.ResetTrigger("Pulse");
            waveUIAnimator.SetTrigger("Pulse");
        }
    }

    /// <summary>
    /// Procesa la pulsación del botón de siguiente oleada, iniciando el spawner si es necesario.
    /// Al usar este método, NO se reproduce el sonido de oleada automática.
    /// </summary>
    public void OnClick_StartNextWave()
    {
        if (enemySpawner != null && !isGameOver)
        {
            if (!hasGameStarted) StartGame();
            SetNextWaveButtonState(false);
            enemySpawner.StartNextWave(); 
        }
    }

    /// <summary>
    /// Sincroniza los valores de vidas y oro con los elementos de texto de la UI.
    /// </summary>
    public void UpdateUI()
    {
        if (livesText != null) livesText.text = lives.ToString();
        if (goldText != null) goldText.text = gold.ToString();
    }

    /// <summary>
    /// Actualiza el contador de mineros y ejecuta la animación de notificación en la UI.
    /// </summary>
    public void UpdateMinersText(int currentCount, int maxCount)
    {
        if (minersText != null) minersText.text = $"{currentCount}/{maxCount}";
        if (minersUIAnimator != null)
        {
            if (minersUIAnimator.GetCurrentAnimatorStateInfo(0).IsName("Pulse")) return;
            minersUIAnimator.ResetTrigger("Pulse");
            minersUIAnimator.SetTrigger("Pulse");
        }
    }

    /// <summary>
    /// Resta vidas al jugador y dispara una animación de alerta. Activa el Game Over si la salud llega a cero.
    /// </summary>
    /// <param name="amount">Cantidad de daño recibido.</param>
    public void LoseLives(int amount)
    {
        if (isGameOver) return;
        lives -= amount;
        if (lives < 0) lives = 0;
        UpdateUI();
        if (livesUIAnimator != null) livesUIAnimator.SetTrigger("Pulse");
        if (lives <= 0) GameOver();
    }

    /// <summary>
    /// Incrementa las reservas de oro y notifica visualmente el cambio en la interfaz.
    /// </summary>
    /// <param name="amount">Cantidad de oro a añadir.</param>
    public void AddGold(int amount)
    {
        if (isGameOver) return;
        gold += amount;
        UpdateUI();
        if (goldUIAnimator != null) goldUIAnimator.SetTrigger("Pulse");
    }

    /// <summary>
    /// Incrementa los contadores internos de torres según el nombre del prefab, 
    /// utilizado para desbloquear unidades que requieren estructuras específicas.
    /// </summary>
    public void RegisterTowerBuilt(GameObject towerPrefab)
    {
        if (towerPrefab.name.Contains("FixedArcher")) fixedArcherTowersBuilt++;
        else if (towerPrefab.name.Contains("Archer")) archerTowersBuilt++;
        else if (towerPrefab.name.Contains("Barracks")) barracksTowersBuilt++;
        else if (towerPrefab.name.Contains("Mage")) mageTowersBuilt++;
    }

    /// <summary>
    /// Reproduce un efecto de sonido específico indicando que la oleada ha iniciado de forma automática
    /// por haber finalizado el temporizador.
    /// </summary>
    public void PlayTimerWaveSound()
    {
        if (waveAudioSource != null && autoWaveStartSound != null && !isGameOver)
        {
            waveAudioSource.PlayOneShot(autoWaveStartSound);
        }
    }

    /// <summary>
    /// Método invocado por el EnemySpawner cuando se ha generado el último enemigo del nivel.
    /// </summary>
    public void NotifyLastWaveSpawned()
    {
        if (!isGameOver) StartCoroutine(CheckForVictoryRoutine());
    }

    /// <summary>
    /// Comprueba periódicamente si se han eliminado todos los enemigos tras el despliegue de la última oleada.
    /// </summary>
    private IEnumerator CheckForVictoryRoutine()
    {
        while (AreEnemiesAlive()) yield return new WaitForSeconds(0.5f);
        if (!isGameOver && lives > 0) Victory();
    }

    /// <summary>
    /// Declara la victoria, detiene el juego, calcula las estrellas según las vidas 
    /// y guarda el progreso en el SaveSystem desbloqueando el siguiente nivel.
    /// </summary>
    private void Victory()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
        ShowNextWaveButton(false);
        ShowCountdown(false);
        if (victoryMenuUI != null) victoryMenuUI.SetActive(true);
        int earnedStars = CalculateStars();
        DisplayStars(earnedStars);
        SaveProgress(earnedStars);
    }

    /// <summary>
    /// Persiste el resultado del nivel y actualiza el nivel máximo desbloqueado en los datos guardados.
    /// </summary>
    private void SaveProgress(int stars)
    {
        if (SaveSystem.currentData == null) return;
        if (stars > SaveSystem.currentData.levelStars[levelIndex]) 
            SaveSystem.currentData.levelStars[levelIndex] = stars;
        
        int nextLevelNumber = levelIndex + 2;
        if (SaveSystem.currentData.maxLevelUnlocked < nextLevelNumber) 
            SaveSystem.currentData.maxLevelUnlocked = nextLevelNumber;
        
        SaveSystem.SaveGame(SaveSystem.currentData);
    }

    /// <summary>
    /// Detiene la partida y muestra la pantalla de derrota al agotarse las vidas.
    /// </summary>
    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
        ShowNextWaveButton(false);
        ShowCountdown(false);
        if (defeatMenuUI != null) defeatMenuUI.SetActive(true);
    }

    /// <summary>
    /// Calcula la puntuación por estrellas comparando las vidas restantes con los umbrales configurados.
    /// </summary>
    private int CalculateStars()
    {
        if (lives >= livesForThreeStars) return 3;
        if (lives >= livesForTwoStars) return 2;
        if (lives >= livesForOneStar) return 1;
        return 0;
    }

    /// <summary>
    /// Activa las imágenes de estrellas en el menú de victoria basándose en el resultado obtenido.
    /// </summary>
    private void DisplayStars(int count)
    {
        if (victoryStars == null) return;
        foreach (GameObject star in victoryStars) if (star != null) star.SetActive(false);
        for (int i = 0; i < count; i++) 
            if (i < victoryStars.Length && victoryStars[i] != null) victoryStars[i].SetActive(true);
    }
}