/// <summary>
/// Interfaz para cualquier torre que genera unidades y necesita saber cuándo mueren.
/// </summary>
public interface ITower
{
    /// <summary>
    /// Notifica a la torre que la unidad en un hueco específico ha muerto.
    /// </summary>
    /// <param name="index">El índice del hueco de la unidad que ha muerto.</param>
    void OnUnitDied(int index);
}
