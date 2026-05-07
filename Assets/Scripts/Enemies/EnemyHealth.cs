using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gestiona la vida, la recepción de daño, los efectos visuales de impacto y la lógica de muerte de los enemigos, encargándose además de otorgar la recompensa de oro al jugador al ser derrotados.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Estadísticas")]
    public int maxHealth = 10;
    public int goldValue = 10;

    [Header("UI")]
    public Slider healthBar;

    [Header("Estado de Combate")]
    public bool isBeingBlocked = false;
    public bool isDead = false;

    // --- NUEVO: SISTEMA DE AUDIO ---
    [Header("Audio")]
    [Tooltip("Añade aquí 1 o varios sonidos de muerte. Se elegirá uno al azar.")]
    public AudioClip[] deathSounds;
    private AudioSource audioSource;
    // -------------------------------

    public int currentHealth { get; private set; }

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    
    private bool canShowHurtEffect = true;
    private float hurtEffectCooldown = 0.2f;

    /// <summary>
    /// Inicializa los valores de salud al máximo, vincula los componentes gráficos y de audio necesarios, y configura la barra de vida de la interfaz.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }
    }

    /// <summary>
    /// Reduce la salud actual del enemigo según la cantidad de daño recibido, actualiza la interfaz, desencadena el parpadeo rojo de daño y verifica si la unidad debe morir.
    /// </summary>
    /// <param name="amount">La cantidad de puntos de daño a restar de la salud actual.</param>
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (healthBar != null) healthBar.value = currentHealth;

        if (canShowHurtEffect && currentHealth > 0)
        {
            StartCoroutine(HurtEffectRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Corrutina que tiñe temporalmente el sprite del enemigo de color rojo para proporcionar una retroalimentación visual clara al jugador tras recibir un impacto.
    /// </summary>
    IEnumerator HurtEffectRoutine()
    {
        canShowHurtEffect = false;
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        yield return new WaitForSeconds(hurtEffectCooldown - 0.1f);
        canShowHurtEffect = true;
    }

    /// <summary>
    /// Ejecuta la secuencia completa de muerte del enemigo, otorgando el oro correspondiente al jugador, reproduciendo los efectos sonoros, desactivando colisiones y deteniendo sus rutinas de movimiento.
    /// </summary>
    void Die()
    {
        if (isDead) return;
        isDead = true;

        // --- REPRODUCIR SONIDO DE MUERTE ---
        PlayDeathSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldValue);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (healthBar != null) healthBar.gameObject.SetActive(false);

        if (anim != null) anim.SetTrigger("die");

        EnemyMovement moveScript = GetComponent<EnemyMovement>();
        if (moveScript != null) moveScript.enabled = false;

        StartCoroutine(DestroyAfterAnimation());
    }

    /// <summary>
    /// Selecciona de manera aleatoria y reproduce un efecto de sonido del listado de audios de muerte configurados en el inspector.
    /// </summary>
    private void PlayDeathSound()
    {
        if (deathSounds != null && deathSounds.Length > 0)
        {
            AudioClip clipToPlay = deathSounds[Random.Range(0, deathSounds.Length)];
            
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clipToPlay, transform.position);
            }
        }
    }

    /// <summary>
    /// Corrutina que retrasa la destrucción completa del GameObject para asegurar que la animación de muerte tenga tiempo de reproducirse íntegramente.
    /// </summary>
    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}