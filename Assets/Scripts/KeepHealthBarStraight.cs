using UnityEngine;

/// <summary>
/// Asegura que la interfaz de usuario anclada a un personaje (como barras de vida) mantenga su orientación horizontal correcta, compensando automáticamente las inversiones de escala que ocurren cuando el personaje cambia de dirección.
/// </summary>
public class KeepHealthBarStraight : MonoBehaviour
{
    private Vector3 originalScale;

    /// <summary>
    /// Almacena la escala inicial del objeto para usarla como referencia de magnitud durante las correcciones de orientación posteriores.
    /// </summary>
    void Start() 
    { 
        originalScale = transform.localScale; 
    }

    /// <summary>
    /// Ajusta la escala del objeto en cada frame tras los cálculos de movimiento del padre. Multiplica la escala original por el signo de la escala del progenitor para neutralizar el efecto de espejo en el eje X.
    /// </summary>
    void LateUpdate()
    {
        // Obligamos a que la escala X del Canvas sea siempre positiva 
        // sin importar hacia dónde mire el padre, evitando que la barra o los textos se vean al revés.
        Vector3 parentScale = transform.parent.localScale;
        
        transform.localScale = new Vector3(
            Mathf.Sign(parentScale.x) * originalScale.x, 
            originalScale.y, 
            originalScale.z
        );
    }
}