using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona la lógica de ataque de las unidades aliadas, abarcando tanto ataques cuerpo a cuerpo como a distancia, sincronización con animaciones y reproducción de efectos de sonido.
/// </summary>
public class AllyAttack : MonoBehaviour
{
    [Header("Configuración")]
    public int attackDamage = 15;
    [HideInInspector] public int baseDamage = 0;
    public float attackInterval = 1.5f;
    
    [Header("Sincronización")]
    [Tooltip("Si usas Animation Events, deja esto en 0. Si no, ajusta el tiempo.")]
    public float damageDelay = 0.0f; 
    
    [Header("Ataque a Distancia")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    
    [Header("Audio")]
    [Tooltip("Añade aquí 1 o varios sonidos. Si pones varios, se elegirá uno al azar para dar variedad.")]
    public AudioClip[] attackSounds;
    private AudioSource audioSource;
    

    private Animator anim;
    private Transform currentTarget; 
    
    private float lastFireTime = -999f;
    
    /// <summary>
    /// Inicializa las referencias a los componentes necesarios para el ataque.
    /// </summary>
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Inicia la secuencia de ataque hacia un objetivo específico, activando la animación correspondiente y preparando la aplicación del daño.
    /// </summary>
    /// <param name="target">El transform del enemigo al que se va a atacar.</param>
    public void PerformAttack(Transform target)
    {
        if (target == null) return;
        
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
    /// Corrutina que aplica el daño tras un retraso específico. Se utiliza como alternativa manual si no se implementan eventos de animación.
    /// </summary>
    IEnumerator ApplyDamageRoutine()
    {
        yield return new WaitForSeconds(damageDelay);
        ApplyDamage();
    }

    /// <summary>
    /// Ejecuta la lógica principal del daño, validando el enfriamiento, instanciando un proyectil si la unidad ataca a distancia, o aplicando el daño directamente si es cuerpo a cuerpo.
    /// </summary>
    void ApplyDamage()
    {
        if (Time.time - lastFireTime < Mathf.Max(0.1f, attackInterval - 0.2f))
        {
            return;
        }

        if (currentTarget == null) return;

        lastFireTime = Time.time;

        EnemyHealth targetHealth = currentTarget.GetComponent<EnemyHealth>();

        if (targetHealth == null || targetHealth.isDead || targetHealth.currentHealth <= 0)
        {
            return;
        }

        // --- REPRODUCIR SONIDO JUSTO AL ATACAR ---
        PlayAttackSound();

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            Projectile p = proj.GetComponent<Projectile>(); 
            if (p != null)
            {
                p.damage = attackDamage;
                p.targetTag = "Enemy"; 
                p.SetTarget(currentTarget);
            }
        }
        else 
        {
            targetHealth.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Selecciona aleatoriamente y reproduce un efecto de sonido de la lista de sonidos de ataque disponibles.
    /// </summary>
    private void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0)
        {
            AudioClip clipToPlay = attackSounds[Random.Range(0, attackSounds.Length)];
            
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
    /// Detiene cualquier ataque en curso, limpia el objetivo actual y reinicia el estado del animador.
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