using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ¡NUEVO! Vital para acceder a 'Image' y otros elementos del Canvas

public class MapLevelNode : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [Tooltip("El número de este nivel (1, 2, 3...).")]
    public int levelNumber;
    [Tooltip("El nombre exacto de la escena que debe cargar.")]
    public string sceneToLoad;

    [Header("Referencias Visuales")]
    [Tooltip("Arrastra aquí todas las imágenes de UI (Image) que forman este nodo.")]
    public Image[] nodeSprites; // CAMBIADO: Ahora usa Image en lugar de SpriteRenderer

    [Tooltip("Color que tomarán si el nivel está bloqueado.")]
    public Color lockedColor = Color.gray;
    private Color[] originalColors;

    [Tooltip("Icono de candado (Opcional).")]
    public GameObject lockedIcon;
    [Tooltip("Array con las 3 estrellas (Opcional).")]
    public GameObject[] stars;

    private bool isUnlocked = false;

    /// <summary>
    /// Almacena los colores originales de los componentes visuales del nodo al inicializarse y evalúa inmediatamente su estado de progreso.
    /// </summary>
    void Start()
    {
        // Guardamos el color original de cada una de las piezas del nodo
        if (nodeSprites != null && nodeSprites.Length > 0)
        {
            originalColors = new Color[nodeSprites.Length];
            for (int i = 0; i < nodeSprites.Length; i++)
            {
                if (nodeSprites[i] != null)
                {
                    originalColors[i] = nodeSprites[i].color;
                }
            }
        }

        UpdateNodeStatus();
    }

    /// <summary>
    /// Consulta los datos de progreso guardados del jugador para determinar si este nivel es accesible, restaurando sus colores originales y mostrando las estrellas obtenidas, o aplicando un tinte de bloqueo en caso contrario.
    /// </summary>
    public void UpdateNodeStatus()
    {
        int maxUnlocked = (SaveSystem.currentData != null) ? SaveSystem.currentData.maxLevelUnlocked : 1;

        if (levelNumber <= maxUnlocked)
        {
            // --- NIVEL DESBLOQUEADO ---
            isUnlocked = true;

            // Devolver todas las piezas a su color original
            if (nodeSprites != null)
            {
                for (int i = 0; i < nodeSprites.Length; i++)
                {
                    if (nodeSprites[i] != null && originalColors != null && i < originalColors.Length)
                    {
                        nodeSprites[i].color = originalColors[i];
                    }
                }
            }

            if (lockedIcon != null) lockedIcon.SetActive(false);

            if (SaveSystem.currentData != null && stars != null && stars.Length > 0)
            {
                int index = levelNumber - 1;
                int starsEarned = SaveSystem.currentData.levelStars[index];

                for (int i = 0; i < stars.Length; i++)
                {
                    if (stars[i] != null) stars[i].SetActive(i < starsEarned);
                }
            }
        }
        else
        {
            // --- NIVEL BLOQUEADO ---
            isUnlocked = false;

            // Teñir todas las piezas del color bloqueado
            if (nodeSprites != null)
            {
                for (int i = 0; i < nodeSprites.Length; i++)
                {
                    if (nodeSprites[i] != null)
                    {
                        nodeSprites[i].color = lockedColor;
                    }
                }
            }

            if (lockedIcon != null) lockedIcon.SetActive(true);

            if (stars != null)
            {
                foreach (GameObject star in stars)
                {
                    if (star != null) star.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Inicia la transición a la escena asignada si el nivel está desbloqueado. Preparado para enlazarse al evento OnClick de un Button de la UI.
    /// </summary>
    public void OnLevelClicked() // CAMBIADO: Antes era OnMouseDown, ahora es un método público estándar
    {
        if (isUnlocked)
        {
            Debug.Log("Cargando nivel: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("El nivel " + levelNumber + " está bloqueado. Gana el nivel anterior primero.");
        }
    }
}