using UnityEngine;

/// <summary>
/// Gestiona exclusivamente el cambio visual (Sprite) de la torre al subir de nivel, 
/// manteniendo su tamaño y transform original intactos.
/// </summary>
public class TowerVisualUpgrade : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("Asigna aquí el SpriteRenderer que muestra la torre. Si está vacío, buscará uno en este mismo objeto.")]
    public SpriteRenderer towerRenderer;

    [Tooltip("Añade aquí los sprites de las mejoras. El elemento 0 será el Nivel 2, el elemento 1 será el Nivel 3, etc.")]
    public Sprite[] upgradeSprites;

    private void Awake()
    {
        // Si se nos olvidó asignarlo en el inspector, lo busca automáticamente
        if (towerRenderer == null)
        {
            towerRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Cambia el sprite de la torre basándose en el nivel de mejora actual.
    /// </summary>
    /// <param name="upgradeIndex">El índice de la mejora (1 para la primera mejora, 2 para la segunda...)</param>
    public void UpdateVisuals(int upgradeIndex)
    {
        if (towerRenderer == null) return;

        // Calculamos la posición en el array (el nivel 1 de mejora corresponde a la posición 0 del array)
        int arrayIndex = upgradeIndex;

        // Comprobamos que no nos salimos de los límites de la lista que has configurado
        if (arrayIndex >= 0 && arrayIndex < upgradeSprites.Length)
        {
            Sprite nextSprite = upgradeSprites[arrayIndex];
            
            if (nextSprite != null)
            {
                // Cambiamos el dibujo sin alterar la escala ni la posición
                towerRenderer.sprite = nextSprite;
            }
        }
        else
        {
            Debug.LogWarning($"[TowerVisualUpgrade] Faltan sprites asignados en {gameObject.name} para la mejora {upgradeIndex}");
        }
    }
}
