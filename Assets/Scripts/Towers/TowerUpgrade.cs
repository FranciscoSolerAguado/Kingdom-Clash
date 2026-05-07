using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el sistema de progresión y niveles de mejora para todas las torres del juego, permitiendo incrementar estadísticas como daño, rango y salud mediante el consumo de recursos, además de reproducir efectos de sonido.
/// </summary>
public class TowerUpgrade : MonoBehaviour
{
    /// <summary>
    /// Contenedor de datos que define los parámetros de una mejora específica, incluyendo su coste y los multiplicadores porcentuales de atributos.
    /// </summary>
    [System.Serializable]
    public class UpgradeData
    {
        public string upgradeName = "Nivel 2";
        public int cost = 100;

        [Header("Mejoras (%)")]
        public float damageIncreasePercent = 20f;
        public float rangeIncreasePercent = 10f;
        [Tooltip("Aplica solo a Barracas y Fixed Archers")]
        public float healthIncreasePercent = 20f;
    }

    [Header("Configuración de la Torre")]
    [Tooltip("Escribe: Archer, Mage, Barracks o FixedArcher para que el LevelManager lo reconozca")]
    public string towerType = "Archer";

    [Header("Niveles de Mejora")]
    public List<UpgradeData> upgrades;

    [Header("Configuración de Sonido")]
    [Tooltip("Asigna aquí el AudioSource que reproducirá los sonidos. Si está vacío, el script creará uno automáticamente.")]
    public AudioSource audioSource;

    [Tooltip("Lista de sonidos. Se elegirá uno al azar cada vez que la torre suba de nivel.")]
    public AudioClip[] upgradeSounds;

    [HideInInspector] public int currentUpgradeLevel = 0;

    /// <summary>
    /// Configura el componente de audio al iniciar, creándolo automáticamente si la torre no dispone de uno.
    /// </summary>
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    /// <summary>
    /// Comprueba si la torre aún dispone de niveles de mejora superiores según la lista configurada en el inspector.
    /// </summary>
    /// <returns>Verdadero si existe al menos una mejora pendiente; falso si se ha alcanzado el nivel máximo.</returns>
    public bool HasMoreUpgrades()
    {
        return currentUpgradeLevel < upgrades.Count;
    }

    /// <summary>
    /// Recupera los datos correspondientes al siguiente nivel de mejora disponible.
    /// </summary>
    /// <returns>Un objeto UpgradeData con la información del siguiente nivel, o nulo si no hay más mejoras.</returns>
    public UpgradeData GetNextUpgrade()
    {
        if (HasMoreUpgrades()) return upgrades[currentUpgradeLevel];
        return null;
    }

    /// <summary>
    /// Ejecuta la lógica de mejora: calcula los nuevos multiplicadores, identifica el tipo de torre actual mediante la detección de componentes, aplica las nuevas estadísticas, actualiza los visuales y reproduce el sonido de mejora.
    /// </summary>
    public void ApplyUpgrade()
    {
        if (!HasMoreUpgrades()) return;

        UpgradeData upgrade = upgrades[currentUpgradeLevel];
        float damageMult = 1f + (upgrade.damageIncreasePercent / 100f);
        float rangeMult = 1f + (upgrade.rangeIncreasePercent / 100f);
        float healthMult = 1f + (upgrade.healthIncreasePercent / 100f);

        // --- Aplicación de mejoras según el componente específico de la torre ---

        ArcherTower archer = GetComponent<ArcherTower>();
        if (archer != null)
        {
            archer.damage = Mathf.RoundToInt(archer.damage * damageMult);
            archer.range *= rangeMult;
        }

        MageTower mage = GetComponent<MageTower>();
        if (mage != null)
        {
            mage.damage = Mathf.RoundToInt(mage.damage * damageMult);
            mage.range *= rangeMult;
        }

        BarracksTower barracks = GetComponent<BarracksTower>();
        if (barracks != null)
        {
            barracks.UpgradeUnitStats(damageMult, healthMult);
        }

        FixedArcherTower fixedArcher = GetComponent<FixedArcherTower>();
        if (fixedArcher != null)
        {
            fixedArcher.UpgradeUnitStats(damageMult, healthMult);
        }

        // Llamada al script visual (si lo tiene)
        GetComponent<TowerVisualUpgrade>()?.UpdateVisuals(currentUpgradeLevel);
        
        // Reproducir sonido aleatorio
        PlayRandomUpgradeSound();

        currentUpgradeLevel++;
        Debug.Log($"{gameObject.name} mejorada al nivel {currentUpgradeLevel}");
    }

    /// <summary>
    /// Selecciona un clip de audio al azar de la lista y lo reproduce sin interrumpir otros sonidos en curso.
    /// </summary>
    private void PlayRandomUpgradeSound()
    {
        if (upgradeSounds != null && upgradeSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, upgradeSounds.Length);
            AudioClip clipToPlay = upgradeSounds[randomIndex];

            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
    }
}