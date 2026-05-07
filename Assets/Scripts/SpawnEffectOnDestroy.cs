using UnityEngine;

/// <summary>
/// Gestiona la instanciación de efectos visuales (como explosiones o partículas) en el momento en que el objeto es destruido, incluyendo salvaguardas para evitar errores durante el cierre de la aplicación o el cambio de escenas.
/// </summary>
public class SpawnEffectOnDestroy : MonoBehaviour
{
    [Tooltip("El prefab de la explosión que aparecerá cuando este proyectil muera")]
    public GameObject explosionPrefab;
    
    private bool isQuitting = false;
    
    /// <summary>
    /// Registra si la aplicación se está cerrando para evitar la creación de nuevos objetos durante el proceso de limpieza de Unity.
    /// </summary>
    void OnApplicationQuit() 
    { 
        isQuitting = true; 
    }

    /// <summary>
    /// Verifica si la escena sigue cargada cuando el objeto se desactiva, actuando como una capa de seguridad adicional para prevenir instanciaciones huérfanas.
    /// </summary>
    void OnDisable()
    {
        if (gameObject.scene.isLoaded == false)
        {
            isQuitting = true;
        }
    }

    /// <summary>
    /// Dispara la creación del efecto visual en la posición actual del objeto antes de su eliminación definitiva, asegurando que el efecto tenga un tiempo de vida limitado para optimizar el rendimiento.
    /// </summary>
    void OnDestroy()
    {
        if (!isQuitting && explosionPrefab != null)
        {
            // Instancia la explosión justo en la posición de este proyectil antes de desaparecer.
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            // Garantiza que el efecto visual se elimine de la memoria tras 2 segundos.
            Destroy(explosion, 2f);
        }
    }
}