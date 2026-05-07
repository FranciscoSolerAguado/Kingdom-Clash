using UnityEngine;
using System.Collections;

/// <summary>
/// Controla la lógica de generación pasiva de recursos (oro), gestionando la animación de minería y su efecto de sonido en bucle en función del estado de la partida y la presencia de enemigos.
/// </summary>
public class GoldMiner : MonoBehaviour
{
    [Header("Configuración de Farmeo")]
    public int goldPerTick = 1;
    public float tickInterval = 5f;

    // --- NUEVO: SISTEMA DE AUDIO ---
    [Header("Audio")]
    [Tooltip("Añade el sonido de picar piedra. El script lo pondrá en bucle automáticamente.")]
    public AudioClip miningSound;
    private AudioSource audioSource;
    // -------------------------------

    private Animator anim;
    private AllyHealth healthScript;
    private bool isMining = false;

    /// <summary>
    /// Inicializa las referencias a los componentes, configura el bucle de audio del pico y arranca la corrutina principal de generación de oro.
    /// </summary>
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        healthScript = GetComponent<AllyHealth>();
        
        // Configuramos el AudioSource automáticamente
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && miningSound != null)
        {
            audioSource.clip = miningSound;
            audioSource.loop = true; // Hacemos que sea un bucle continuo
            audioSource.playOnAwake = false; // Que no suene hasta que empiece a picar
        }

        StartCoroutine(MinerBehaviorRoutine());
    }

    /// <summary>
    /// Supervisa el estado vital del minero para detener su actividad al morir, y sincroniza las animaciones y sonidos de minería dependiendo de si hay enemigos activos en el mapa.
    /// </summary>
    void Update()
    {
        // 1. Si el minero muere
        if (healthScript != null && healthScript.isDead)
        {
            isMining = false;
            StopAllCoroutines();
            
            // Apagamos el sonido inmediatamente al morir
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            if (MinerManager.Instance != null)
            {
                MinerManager.Instance.OnMinerDied(this.gameObject);
            }
            
            enabled = false; 
            return;
        }

        // 2. Comportamiento mientras está vivo
        if (isMining && anim != null)
        {
            if (GameManager.Instance != null && GameManager.Instance.AreEnemiesAlive())
            {
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("Attack")) 
                {
                    anim.SetTrigger("attack");
                }

                // --- REPRODUCIR SONIDO ---
                // Si hay enemigos y no está sonando el pico, lo iniciamos
                if (audioSource != null && !audioSource.isPlaying && miningSound != null)
                {
                    audioSource.Play();
                }
            }
            else
            {
                anim.ResetTrigger("attack");

                // --- DETENER SONIDO ---
                // Si no hay enemigos (deja de picar), pausamos el sonido
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
            }
        }
    }

    /// <summary>
    /// Corrutina principal que espera al inicio del nivel y, posteriormente, inyecta ingresos de oro a la economía del jugador a intervalos regulares siempre que haya oleadas en curso.
    /// </summary>
    IEnumerator MinerBehaviorRoutine()
    {
        while (GameManager.Instance != null && !GameManager.Instance.hasGameStarted)
        {
            yield return null;
        }

        isMining = true;

        while (isMining)
        {
            yield return new WaitForSeconds(tickInterval);
            
            if (isMining && GameManager.Instance != null && GameManager.Instance.AreEnemiesAlive())
            {
                GameManager.Instance.AddGold(goldPerTick);
            }
        }
    }
}