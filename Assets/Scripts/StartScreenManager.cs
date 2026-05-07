using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// Gestiona la transición inicial desde la pantalla de presentación hacia el menú principal o el primer nivel del juego, detectando entradas táctiles o de ratón de forma optimizada para dispositivos móviles y escritorio.
/// </summary>
public class StartScreenManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El nombre EXACTO de la escena a la que quieres ir (ej: MainMenu o Level1)")]
    public string nextSceneName = "MainMenu";

    private bool isLoading = false; 

    /// <summary>
    /// Escucha en cada frame la liberación del botón izquierdo del ratón o el levantamiento del dedo de la pantalla táctil para disparar la carga de la siguiente escena.
    /// </summary>
    void Update()
    {
        if (isLoading) return;

        // 1. Detectar si soltamos el clic izquierdo del ratón
        bool isMouseReleased = Input.GetMouseButtonUp(0);
        
        // 2. Detectar si levantamos el dedo de la pantalla (compatibilidad con móviles)
        bool isTouchReleased = false;
        if (Input.touchCount > 0)
        {
            // Comprobamos todos los toques actuales para identificar el final de la pulsación
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Ended)
                {
                    isTouchReleased = true;
                    break;
                }
            }
        }

        // Si se detecta cualquier interacción válida, procedemos a la transición
        if (isMouseReleased || isTouchReleased)
        {
            isLoading = true;
            LoadNextScene();
        }
    }

    /// <summary>
    /// Utiliza el SceneManager de Unity para cargar de forma inmediata la escena especificada en la configuración del inspector.
    /// </summary>
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}