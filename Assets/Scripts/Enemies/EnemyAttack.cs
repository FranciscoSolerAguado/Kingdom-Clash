using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona la lógica ofensiva de los enemigos, soportando ataques cuerpo a cuerpo y a distancia, sincronización con animaciones y reproducción de efectos sonoros aleatorios.
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    [Header("Configuración")]
    public int attackDamage = 10;
    public float attackInterval = 1.5f;

    [Header("Sincronización")]
    [Tooltip("Tiempo que tarda en impactar el golpe (o disparar) desde que inicia la animación")]
    public float damageDelay = 0.5f;

    [Header("Ataque a Distancia")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         

    // --- NUEVO: SISTEMA DE AUDIO ---
    [Header("Audio")]
    [Tooltip("Añade aquí 1 o varios sonidos. Si pones varios, se elegirá uno al azar para dar variedad.")]
    public AudioClip[] attackSounds;
    private AudioSource audioSource;
    // -------------------------------

    private Animator anim;
    private Transform currentTarget; 
    
    // Control de tiempo para el último ataque realizado
    private float lastAttackTime = -999f;
    
    // Control de tiempo para aplicación de daño (seguro contra loops)
    private float lastDamageAppliedTime = -999f;

    /// <summary>
    /// Inicializa las referencias a los componentes de animación y audio, e incluye una corrección automática de seguridad para evitar intervalos de ataque perjudiciales para el rendimiento.
    /// </summary>
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        
        // Buscamos si el enemigo tiene un AudioSource propio
        audioSource = GetComponent<AudioSource>();
        
        // CORRECCIÓN AUTOMÁTICA DE SEGURIDAD
        if (attackInterval < 0.1f) 
        {
            attackInterval = 1.0f;
            Debug.LogWarning($"[EnemyAttack] El AttackInterval de {gameObject.name} era demasiado bajo. Se ha ajustado a 1.0f.");
        }
    }

    /// <summary>
    /// Inicia la secuencia de ataque hacia un objetivo específico, verificando los tiempos de enfriamiento principales y activando las animaciones correspondientes.
    /// </summary>
    /// <param name="target">El transform de la unidad aliada a la que se va a atacar.</param>
    public void PerformAttack(Transform target)
    {
        if (target == null) return;
        
        float effectiveInterval = Mathf.Max(attackInterval, 0.5f);

        if (Time.time - lastAttackTime < effectiveInterval) return;
        
        lastAttackTime = Time.time;
        currentTarget = target;

        if (anim != null)
        {
            anim.ResetTrigger("attack");
            anim.SetTrigger("attack");
        }

        if (damageDelay > 0)
        {
            StopAllCoroutines(); 
            StartCoroutine(ApplyDamageRoutine());
        }
    }

    /// <summary>
    /// Método diseñado para ser llamado exclusivamente desde un evento de animación (Animation Event) en el frame exacto del impacto.
    /// </summary>
    public void TriggerAttackEvent()
    {
        ApplyDamage();
    }

    /// <summary>
    /// Corrutina que aplica el daño tras un retraso específico, utilizada como alternativa programada si no se configuran eventos de animación en el modelo.
    /// </summary>
    IEnumerator ApplyDamageRoutine()
    {
        yield return new WaitForSeconds(damageDelay);
        ApplyDamage();
    }

    /// <summary>
    /// Ejecuta la lógica principal del daño, validando el enfriamiento secundario anti-loops, instanciando un proyectil si es un atacante a distancia o aplicando daño directo a las unidades aliadas.
    /// </summary>
    void ApplyDamage()
    {
        float effectiveInterval = Mathf.Max(attackInterval, 0.5f);
        if (Time.time - lastDamageAppliedTime < (effectiveInterval - 0.1f)) return;

        if (currentTarget == null) return;

        lastDamageAppliedTime = Time.time; 

        // --- REPRODUCIR SONIDO JUSTO AL ATACAR ---
        PlayAttackSound();

        // --- LÓGICA DE PROYECTIL (Ranged) ---
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>(); 
            
            if (p != null)
            {
                p.damage = attackDamage;
                p.targetTag = "ally"; 
                p.SetTarget(currentTarget); 
            }
        }
        // --- LÓGICA DE MELEE (Cuerpo a Cuerpo) ---
        else
        {
            AllyHealth targetHealth = currentTarget.GetComponent<AllyHealth>();
            if (targetHealth != null && !targetHealth.isDead)
            {
                targetHealth.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// Selecciona aleatoriamente y reproduce un efecto de sonido de la lista de audios disponibles, utilizando el componente local o instanciando un origen temporal si es necesario.
    /// </summary>
    private void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0)
        {
            // Elegimos un sonido al azar de la lista
            AudioClip clipToPlay = attackSounds[Random.Range(0, attackSounds.Length)];
            
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clipToPlay); // Reproduce sin cortar el sonido anterior
            }
            else
            {
                // Fallback: Si se te olvidó ponerle un AudioSource al enemigo, Unity creará uno temporal
                AudioSource.PlayClipAtPoint(clipToPlay, transform.position);
            }
        }
    }

    /// <summary>
    /// Detiene cualquier corrutina de ataque en curso, limpia el objetivo actual y reinicia el estado del animador para interrumpir la ofensiva de forma segura.
    /// </summary>
    public void StopAttack()
    {
        StopAllCoroutines(); 
        currentTarget = null;

        if (anim != null)
        {
            anim.ResetTrigger("attack");
        }
    }
}