using UnityEngine;

/// <summary>
/// Gestiona la eliminación automática de objetos temporales al finalizar sus animaciones, proporcionando métodos para eventos de animación y un sistema de seguridad basado en tiempo para garantizar la limpieza de la memoria.
/// </summary>
public class DestroyAfterAnimation : MonoBehaviour
{
    [Tooltip("Tiempo máximo de seguridad antes de destruirse, por si falla el evento de animación.")]
    public float fallbackDestroyTime = 2f;

    /// <summary>
    /// Configura un temporizador de destrucción automática al iniciarse el objeto para prevenir que errores en la configuración de la animación dejen objetos huérfanos en la escena.
    /// </summary>
    void Start()
    {
        // Seguro de vida: Garantiza la destrucción del objeto incluso si el evento de animación no se dispara.
        Destroy(gameObject, fallbackDestroyTime);
        
        if (transform.parent != null)
        {
            // Si el objeto está anidado, se programa la destrucción de la jerarquía completa.
            Destroy(transform.root.gameObject, fallbackDestroyTime);
        }
    }

    /// <summary>
    /// Elimina el objeto de la escena de forma inmediata. Este método está diseñado para ser invocado mediante un Animation Event al final de un clip de animación.
    /// </summary>
    public void DestroyMe()
    {
        // Destrucción inmediata del objeto y su raíz jerárquica si existe un contenedor.
        if (transform.parent != null)
        {
            Destroy(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Sobrecarga del método de destrucción para permitir la recepción de parámetros desde el sistema de eventos de animación de Unity sin interrumpir el flujo.
    /// </summary>
    /// <param name="param">Parámetro de cadena opcional ignorado por la lógica de destrucción.</param>
    public void DestroyMe(string param)
    {
        DestroyMe();
    }
}