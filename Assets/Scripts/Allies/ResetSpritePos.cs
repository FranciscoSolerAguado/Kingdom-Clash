using UnityEngine;

/// <summary>
/// Fuerza al objeto a mantener una posición local exacta en el origen (0,0,0) con respecto a su padre, operando de forma continua tanto en el editor de Unity como durante la ejecución de la partida.
/// </summary>
[ExecuteInEditMode] 
public class ResetSpritePos : MonoBehaviour
{
    /// <summary>
    /// Se ejecuta al final de cada frame, garantizando que después de aplicarse todas las demás lógicas de movimiento o animaciones, se corrija instantáneamente cualquier desviación de la posición local.
    /// </summary>
    void LateUpdate() 
    {
        if (transform.localPosition != Vector3.zero)
        {
            transform.localPosition = Vector3.zero;
        }
    }
}