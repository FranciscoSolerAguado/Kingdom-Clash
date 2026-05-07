using UnityEngine;

/// <summary>
/// Define una ruta de navegación compuesta por una serie de puntos de control (waypoints) que los enemigos utilizarán para desplazarse a través del nivel, incluyendo visualización en el editor para facilitar el diseño.
/// </summary>
public class Path : MonoBehaviour
{
    [Tooltip("Lista ordenada de Transform que definen los nodos del camino.")]
    public Transform[] points;

    /// <summary>
    /// Callback nativo de Unity utilizado para dibujar guías visuales en la ventana de Escena. 
    /// Dibuja líneas rojas entre los puntos consecutivos de la ruta para previsualizar el recorrido de los enemigos.
    /// </summary>
    void OnDrawGizmos()
    {
        // Esto sirve para ver el camino en el Editor de Unity
        if (points == null) return;
        for (int i = 0; i < points.Length - 1; i++)
        {
            if (points[i] == null || points[i+1] == null) continue;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(points[i].position, points[i+1].position);
        }
    }
}