using UnityEngine;

/// <summary>
/// Interfaz para cualquier torre que pueda tener un punto de reunión (Rally Point)
/// para sus unidades.
/// </summary>
public interface IRallyPointTower
{
    /// <summary>
    /// Establece la nueva posición del punto de reunión.
    /// </summary>
    /// <param name="newPoint">La nueva coordenada en el mundo.</param>
    void SetRallyPoint(Vector3 newPoint);

    /// <summary>
    /// Obtiene el rango máximo desde la torre donde se puede establecer el punto de reunión.
    /// </summary>
    /// <returns>El radio del rango.</returns>
    float GetRange();

    /// <summary>
    /// Obtiene la posición de la torre en el mundo.
    /// </summary>
    Transform transform { get; }
}
