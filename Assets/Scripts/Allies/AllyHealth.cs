using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gestiona la vida, el daño recibido, la curación y la lógica de muerte de las unidades aliadas, actualizando la interfaz y notificando a las torres correspondientes.
/// </summary>
public class AllyHealth : MonoBehaviour
{
    [Header("UI")] public Slider healthBar;

    [Header("Estadísticas")] public int health = 50;
    public int maxHealth;
    [HideInInspector] public int baseHealth = 0;
    public bool isOccupied = false;

    [Header("Audio")] [Tooltip("Añade aquí 1 o varios sonidos. Si pones varios, se elegirá uno al azar.")]
    public AudioClip[] deathSounds;

    private AudioSource audioSource;

    private Animator anim;
    public bool isDead = false;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private BarracksTower myBarracksTower;
    private FixedArcherTower myArcherTower;
    private int myIndexInTower;
    private bool isInitializedByTower = false;

    private bool canShowHurtEffect = true;
    private float hurtEffectCooldown = 0.3f;

    // --- REFERENCIA AL MOVIMIENTO ---
    private AllyMovement movementScript;

    /// <summary>
    /// Inicializa los componentes necesarios y configura los valores máximos y actuales de la barra de vida de la unidad.
    /// </summary>
    void Start()
    {
        maxHealth = health;
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();

        // Cogemos el script de movimiento
        movementScript = GetComponent<AllyMovement>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
    }

    /// <summary>
    /// Vincula esta unidad a la torre de cuartel que la generó para poder notificarle su estado al morir.
    /// </summary>
    public void Initialize(BarracksTower tower, int index)
    {
        myBarracksTower = tower;
        myIndexInTower = index;
        isInitializedByTower = true;
    }

    /// <summary>
    /// Vincula esta unidad a la torre de arqueros fijos que la generó para poder notificarle su estado al morir.
    /// </summary>
    public void Initialize(FixedArcherTower tower, int index)
    {
        myArcherTower = tower;
        myIndexInTower = index;
        isInitializedByTower = true;
    }

    /// <summary>
    /// Aplica daño a la unidad, actualiza la interfaz, detiene la regeneración de vida y desencadena la muerte si la vida llega a cero o menos.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (healthBar != null) healthBar.value = health;

        // --- CORTAR CURACIÓN AL RECIBIR DAÑO ---
        if (movementScript != null)
        {
            movementScript.OnReceivedDamage();
        }

        if (canShowHurtEffect && health > 0)
        {
            StartCoroutine(HurtEffectRoutine());
        }

        if (health <= 0) Die();
    }

    /// <summary>
    /// Aumenta la vida de la unidad asegurando un mínimo de curación, limitando el valor a la vida máxima permitida y actualizando la interfaz.
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead || health >= maxHealth) return;

        // Aseguramos que al menos cure 1 punto para evitar que decimales bajos redondeen a 0
        int healAmount = Mathf.Max(1, Mathf.RoundToInt(amount));

        health = Mathf.Min(health + healAmount, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = health;
        }
    }

    /// <summary>
    /// Corrutina que cambia temporalmente el color del sprite a rojo para dar retroalimentación visual al jugador tras recibir un impacto.
    /// </summary>
    IEnumerator HurtEffectRoutine()
    {
        canShowHurtEffect = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }

        yield return new WaitForSeconds(hurtEffectCooldown - 0.1f);
        canShowHurtEffect = true;
    }

    /// <summary>
    /// Ejecuta la secuencia de muerte de la unidad, reproduciendo sonido, desactivando colisiones y movimiento, y notificando a la torre de origen para liberar el espacio.
    /// </summary>
    void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayDeathSound();

        if (col != null) col.enabled = false;

        if (movementScript != null) movementScript.enabled = false;

        if (healthBar != null) healthBar.gameObject.SetActive(false);

        if (isInitializedByTower)
        {
            if (myBarracksTower != null)
            {
                myBarracksTower.OnWarriorDied(myIndexInTower);
            }
            else if (myArcherTower != null)
            {
                myArcherTower.OnArcherDied(myIndexInTower);
            }
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    /// <summary>
    /// Selecciona de forma aleatoria y reproduce un efecto de sonido del listado de sonidos de muerte disponibles.
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
    /// Corrutina que desvanece gradualmente la opacidad del sprite tras la muerte hasta desaparecer por completo y luego destruye el objeto.
    /// </summary>
    IEnumerator FadeOutAndDestroy()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            float duration = 1.0f;
            float timer = 0f;
            Color startColor = spriteRenderer.color;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, timer / duration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(gameObject);
    }
}