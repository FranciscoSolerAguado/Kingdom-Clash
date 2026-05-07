using UnityEngine;

/// <summary>
/// Define y gestiona las restricciones de progresión de las torres específicas para cada nivel, 
/// permitiendo limitar el número de mejoras disponibles según el diseño de la misión actual.
/// </summary>
public class LevelUpgradeSettings : MonoBehaviour
{
    public static LevelUpgradeSettings Instance;

    [Header("Límites de Mejoras Permitidas en esta Escena")]
    [Tooltip("Nivel máximo al que se puede mejorar la torre. 0 = Bloqueado (solo torre base), 1 = Una mejora, etc.")]
    public int maxArcherUpgrades = 3;
    public int maxMageUpgrades = 3;
    public int maxBarracksUpgrades = 3;
    public int maxFixedArcherUpgrades = 3;

    /// <summary>
    /// Implementa el patrón Singleton para facilitar la consulta de los límites de mejora desde cualquier gestor de interfaz o de construcción en la escena.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Evalúa si una torre de un tipo específico puede realizar una mejora adicional comparando su nivel actual con el límite máximo configurado para el escenario.
    /// </summary>
    /// <param name="towerType">Identificador de texto del tipo de torre (Archer, Mage, Barracks, FixedArcher).</param>
    /// <param name="currentUpgradeLevel">El nivel de mejora actual de la torre que realiza la consulta.</param>
    /// <returns>Verdadero si el nivel actual es inferior al límite permitido; falso en caso contrario.</returns>
    public bool IsUpgradeAllowed(string towerType, int currentUpgradeLevel)
    {
        switch (towerType)
        {
            case "Archer": return currentUpgradeLevel < maxArcherUpgrades;
            case "Mage": return currentUpgradeLevel < maxMageUpgrades;
            case "Barracks": return currentUpgradeLevel < maxBarracksUpgrades;
            case "FixedArcher": return currentUpgradeLevel < maxFixedArcherUpgrades;
            default: return false;
        }
    }
}