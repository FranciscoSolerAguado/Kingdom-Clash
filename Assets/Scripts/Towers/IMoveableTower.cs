using UnityEngine;

/// <summary>
/// Interfaz para cualquier torre que permita mover sus unidades a un punto de reunión.
/// </summary>
public interface IMoveableTower
{
    /// <summary>
    /// Establece el nuevo punto de reunión para las unidades de la torre.
    /// </summary>
    void SetRallyPoint(Vector3 position);

    /// <summary>
    /// El rango máximo desde la torre donde se puede establecer el punto de reunión.
    /// </summary>
    float RallyRange { get; }

    /// <summary>
    /// La posición de la torre en el mundo.
    /// </summary>
    Transform transform { get; }
}
