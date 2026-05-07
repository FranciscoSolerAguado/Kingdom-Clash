using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
/// <summary>
/// Gestiona la reproducción de música en segundo plano de forma persistente entre escenas mediante el patrón Singleton, controlando dinámicamente en qué niveles debe sonar y evitando cortes de audio al transicionar.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Configuración de Música")]
    [Tooltip("Lista de nombres exactos de las escenas donde debe sonar esta música.")]
    public List<string> scenesToPlayIn;

    private AudioSource audioSource;

    /// <summary>
    /// Configura el patrón Singleton persistente. Comprueba si ya existe un gestor musical y, si ambos comparten la misma pista, se autodestruye para no reiniciar la canción; de lo contrario, reemplaza al anterior.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (Instance != null && Instance != this)
        {
            AudioSource existingAudioSource = Instance.GetComponent<AudioSource>();

            if (existingAudioSource != null && audioSource != null && existingAudioSource.clip == audioSource.clip)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Destroy(Instance.gameObject);
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;

        CheckMusicForScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Suscribe el método de comprobación al evento nativo de carga de escenas de Unity para asegurar que el sistema reaccione cada vez que el jugador cambia de pantalla.
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Elimina la suscripción al evento de carga de escenas para prevenir posibles errores de referencia o fugas de memoria si este objeto llegara a destruirse.
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Método delegado que se ejecuta automáticamente al completarse la carga de una nueva escena, restaurando el sonido si estaba silenciado y reevaluando si la música debe continuar.
    /// </summary>
    /// <param name="scene">La información de la nueva escena recién cargada.</param>
    /// <param name="mode">El modo en el que se ha cargado la escena (Single o Additive).</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cargar una nueva escena, nos aseguramos de quitar el mute
        if (audioSource != null)
        {
            audioSource.mute = false;
        }

        CheckMusicForScene(scene.name);
    }

    /// <summary>
    /// Compara el nombre de la escena actual con la lista blanca configurada en el inspector, iniciando o deteniendo el componente de audio según corresponda.
    /// </summary>
    /// <param name="sceneName">El identificador en texto de la escena a evaluar.</param>
    private void CheckMusicForScene(string sceneName)
    {
        if (scenesToPlayIn.Contains(sceneName))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    /// <summary>
    /// Activa o desactiva temporalmente el silencio del canal de audio principal, siendo especialmente útil durante pantallas de pausa, anuncios o transiciones cortas.
    /// </summary>
    /// <param name="isMuted">Verdadero para silenciar la pista de audio; falso para restaurar su volumen normal.</param>
    public void SetSceneMute(bool isMuted)
    {
        if (audioSource != null)
        {
            audioSource.mute = isMuted;
        }
    }
}