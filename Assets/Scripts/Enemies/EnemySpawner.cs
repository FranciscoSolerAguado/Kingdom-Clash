using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Define un grupo específico de enemigos dentro de una oleada, configurando su cantidad, frecuencia de aparición y permitiendo sobrescribir opcionalmente su ruta o punto de origen.
/// </summary>
[System.Serializable]
public class EnemySequence {
    public GameObject enemyPrefab;
    public int count;
    public float rate;
    
    [Tooltip("Opcional: Si se asigna, los enemigos de esta secuencia saldrán de aquí en lugar del spawn general de la oleada/spawner.")]
    public Transform specificSpawnPoint;
    [Tooltip("Opcional: Si se asigna, los enemigos de esta secuencia seguirán esta ruta en lugar de la general.")]
    public Path specificPathToFollow;
}

/// <summary>
/// Representa una oleada completa del nivel compuesta por múltiples secuencias de enemigos, definiendo su duración de descanso y estableciendo puntos de aparición o rutas por defecto para todo el conjunto.
/// </summary>
[System.Serializable]
public class Wave {
    public string name; 
    public List<EnemySequence> sequences;
    public float waveDuration = 120f;
    
    [Tooltip("Opcional: Si se asigna, TODOS los enemigos de esta oleada saldrán de aquí por defecto.")]
    public Transform waveSpawnPoint;
    [Tooltip("Opcional: Si se asigna, TODOS los enemigos de esta oleada seguirán esta ruta por defecto.")]
    public Path wavePathToFollow;
}

/// <summary>
/// Motor principal que orquesta la generación de enemigos a lo largo del nivel, gestionando el flujo de las oleadas, los tiempos de descanso, las bonificaciones de oro por anticipación y la sincronización con la interfaz.
/// </summary>
public class EnemySpawner : MonoBehaviour {
    [Header("Configuración de Posicionamiento General")]
    [Tooltip("Punto de aparición por defecto si la oleada o secuencia no especifica otro.")]
    public Transform defaultSpawnPoint;
    [Tooltip("Ruta por defecto si la oleada o secuencia no especifica otra.")]
    public Path defaultPathToFollow;

    [Header("Control de Oleadas")]
    public List<Wave> waves;

    public int TotalWaves => waves != null ? waves.Count : 0;

    private int waveIndex = 0;
    private bool isWaitingForPlayer = false;
    private bool gameStarted = false;

    /// <summary>
    /// Prepara el estado inicial del nivel e invoca la corrutina de espera hasta que el jugador decida comenzar la partida.
    /// </summary>
    void Start() {
        StartCoroutine(WaitForStart());
    }

    /// <summary>
    /// Desencadena el inicio de la partida o interrumpe el temporizador de descanso actual para forzar la llegada inmediata de la siguiente oleada.
    /// </summary>
    public void StartNextWave()
    {
        if (!gameStarted) {
            gameStarted = true;
        } else if (isWaitingForPlayer) {
            isWaitingForPlayer = false;
        }
    }

    /// <summary>
    /// Corrutina que detiene la generación de enemigos al principio del nivel hasta que el jugador interactúa con el botón de inicio, configurando la interfaz gráfica a través del GameManager.
    /// </summary>
    IEnumerator WaitForStart()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.SetNextWaveButtonText("Empezar Defensa");
            GameManager.Instance.ShowNextWaveButton(true);
            GameManager.Instance.SetNextWaveButtonState(true);
            GameManager.Instance.ShowCountdown(false);
            GameManager.Instance.UpdateWaveCounter(0, TotalWaves);
        }

        while (!gameStarted) {
            yield return null;
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.SetNextWaveButtonText("Siguiente Oleada");
        }

        StartCoroutine(SpawnLevel());
    }

    /// <summary>
    /// Corrutina principal que itera secuencialmente a través de todas las oleadas configuradas en el nivel, notificando al sistema global una vez que la última ronda ha finalizado.
    /// </summary>
    IEnumerator SpawnLevel() {
        for (waveIndex = 0; waveIndex < waves.Count; waveIndex++) {
            Wave currentWave = waves[waveIndex];
            Debug.Log("Iniciando Oleada: " + currentWave.name);
            yield return StartCoroutine(RunWave(currentWave));
        }
        Debug.Log("¡Todas las oleadas han sido completadas!");
        
        if (GameManager.Instance != null)
        {
            Debug.Log("[EnemySpawner] Notificando al GameManager que la última oleada ha terminado de spawnear.");
            GameManager.Instance.NotifyLastWaveSpawned();
        }
    }

    /// <summary>
    /// Gestiona el ciclo de vida de una oleada individual, ejecutando la aparición de unidades y controlando el temporizador de descanso posterior junto con la actualización visual de la cuenta atrás.
    /// </summary>
    /// <param name="wave">La estructura de datos de la oleada a procesar.</param>
    IEnumerator RunWave(Wave wave) {
        isWaitingForPlayer = false;
        if (GameManager.Instance != null) {
            GameManager.Instance.ShowCountdown(false);
            GameManager.Instance.SetNextWaveButtonState(false);
            GameManager.Instance.UpdateWaveCounter(waveIndex + 1, TotalWaves);
        }

        yield return StartCoroutine(SpawnEnemiesInWave(wave));

        if (waveIndex < waves.Count - 1) {
            isWaitingForPlayer = true;
            if (GameManager.Instance != null) {
                GameManager.Instance.ShowNextWaveButton(true);
                GameManager.Instance.SetNextWaveButtonState(true);
            }

            float timer = wave.waveDuration;
            while (timer > 0 && isWaitingForPlayer) {
                timer -= Time.deltaTime;
                if (timer <= 30f && GameManager.Instance != null) {
                    GameManager.Instance.ShowCountdown(true);
                    GameManager.Instance.UpdateCountdown(timer);
                }
                yield return null;
            }

            // --- AQUI ESTÁ EL CAMBIO ---
            if (!isWaitingForPlayer) {
                // El jugador pulsó el botón antes de tiempo
                CalculateAndAwardGoldBonus(timer);
            }
            else {
                // El temporizador llegó a 0 (el jugador esperó la cuenta atrás)
                if (GameManager.Instance != null) {
                    GameManager.Instance.PlayTimerWaveSound();
                }
            }
            // ---------------------------
        }
        else {
            // --- CAMBIO: Ya no esperamos aquí. La espera la gestionará el GameManager ---
            // yield return new WaitForSeconds(wave.waveDuration);
            if (GameManager.Instance != null) {
                GameManager.Instance.ShowNextWaveButton(false);
                GameManager.Instance.ShowCountdown(false);
            }
        }
    }

    /// <summary>
    /// Evalúa el tiempo sobrante al invocar una oleada anticipadamente y recompensa al jugador con una cantidad de oro extra proporcional al tiempo ahorrado.
    /// </summary>
    /// <param name="timeLeft">Los segundos restantes en el temporizador de descanso al momento de pulsar el botón.</param>
    private void CalculateAndAwardGoldBonus(float timeLeft)
    {
        int goldBonus = 0;
        if (timeLeft >= 30f) goldBonus = 10;
        else if (timeLeft >= 20f) goldBonus = 7;
        else if (timeLeft >= 10f) goldBonus = 5;
        else if (timeLeft >= 5f) goldBonus = 4;
        else if (timeLeft > 0f) goldBonus = 3;

        if (goldBonus > 0 && GameManager.Instance != null) {
            Debug.Log($"Bonificación de oro por tiempo restante ({timeLeft:F1}s): {goldBonus} de oro.");
            GameManager.Instance.AddGold(goldBonus);
        }
    }

    /// <summary>
    /// Desglosa y ejecuta cada secuencia de la oleada actual, determinando qué punto de aparición y ruta deben aplicarse según la jerarquía de prioridades (Secuencia > Oleada > General).
    /// </summary>
    /// <param name="wave">La oleada de la que extraer y procesar las secuencias de enemigos.</param>
    IEnumerator SpawnEnemiesInWave(Wave wave) {
        yield return null;

        Transform baseSpawnForWave = wave.waveSpawnPoint != null ? wave.waveSpawnPoint : defaultSpawnPoint;
        Path basePathForWave = wave.wavePathToFollow != null ? wave.wavePathToFollow : defaultPathToFollow;

        if (wave.sequences != null) {
            foreach (EnemySequence sequence in wave.sequences) {
                
                Transform finalSpawn = sequence.specificSpawnPoint != null ? sequence.specificSpawnPoint : baseSpawnForWave;
                Path finalPath = sequence.specificPathToFollow != null ? sequence.specificPathToFollow : basePathForWave;
                
                for (int i = 0; i < sequence.count; i++) {
                    SpawnEnemy(sequence.enemyPrefab, finalSpawn, finalPath);
                    yield return new WaitForSeconds(sequence.rate);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Instancia físicamente el prefab del enemigo en las coordenadas indicadas y le asigna la ruta matemática que debe seguir a través de su componente de movimiento.
    /// </summary>
    /// <param name="prefab">El GameObject del enemigo a generar.</param>
    /// <param name="spawnLocation">El transform del punto de aparición origen.</param>
    /// <param name="path">El objeto ruta compuesto de waypoints que el enemigo recorrerá.</param>
    void SpawnEnemy(GameObject prefab, Transform spawnLocation, Path path) {
        if (prefab == null || spawnLocation == null) {
            Debug.LogWarning("Falta asignar el Prefab o el Spawn Point en el Inspector.");
            return;
        }
        GameObject enemy = Instantiate(prefab, spawnLocation.position, Quaternion.identity);
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null) {
            movement.SetPath(path);
        }
    }
}