using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestiona el comportamiento de la torre de magos, incluyendo la detección automática de enemigos, la orientación visual, la selección aleatoria de animaciones de ataque y la instanciación de proyectiles mágicos con efectos sonoros.
/// </summary>
public class MageTower : MonoBehaviour
{
    [Header("Configuración")]
    public float range = 5f;
    [Tooltip("Disparos por segundo.")]
    public float fireRate = 1f;
    public int damage = 15;
    
    [Tooltip("Capa donde se encuentran los enemigos. (Normalmente 'Enemy' o 'Default'). ¡Asegúrate de configurarla!")]
    public LayerMask enemyLayer; 
    
    private float fireCountdown = 0f;

    [Header("Sincronización (Eventos de Animación)")]
    public bool useAnimationEvents = false;

    [Header("Referencias")]
    public GameObject magicBoltPrefab; 
    public Transform firePoint;
    public Animator anim;

    // --- NUEVO: SISTEMA DE AUDIO ---
    [Header("Audio")]
    [Tooltip("Añade aquí sonidos de disparo mágico. Se elegirá uno al azar.")]
    public AudioClip[] shootSounds;
    private AudioSource audioSource;
    // -------------------------------

    private Transform currentTarget;
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Inicializa las referencias a los componentes visuales y de audio, ajusta la velocidad de las animaciones basándose en la cadencia de fuego y valida la máscara de colisión de los enemigos.
    /// </summary>
    void Start()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim != null) spriteRenderer = anim.GetComponent<SpriteRenderer>();

        if (anim != null)
        {
            anim.speed = fireRate;
        }
        
        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Default"); 
        }

        // Buscamos el AudioSource en la torre
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Bucle principal que evalúa constantemente la validez del objetivo actual, actualiza la orientación geométrica del sprite y administra el temporizador para efectuar los ataques mágicos.
    /// </summary>
    void Update()
    {
        UpdateTarget();
        fireCountdown -= Time.deltaTime;

        if (currentTarget != null)
        {
            FaceTarget();
            if (fireCountdown <= 0f)
            {
                StartAttack();
                fireCountdown = 1f / fireRate;
            }
        }
        else
        {
            if (anim != null)
            {
                anim.ResetTrigger("attack");
                anim.ResetTrigger("attack2");
                anim.ResetTrigger("attack3");
            }
        }
    }

    /// <summary>
    /// Verifica continuamente si el objetivo actual sigue siendo válido (vivo y dentro de rango) o invoca la rutina de escaneo para adquirir un nuevo blanco.
    /// </summary>
    void UpdateTarget()
    {
        if (currentTarget != null && !IsTargetValid(currentTarget))
        {
            currentTarget = null;
            fireCountdown = 0f; 
        }
        
        if (currentTarget == null)
        {
            FindTarget();
        }
    }

    /// <summary>
    /// Ajusta la escala del componente SpriteRenderer en el eje X para asegurar que el arte visual del mago mire siempre hacia la posición del objetivo fijado.
    /// </summary>
    void FaceTarget()
    {
        if (spriteRenderer == null || currentTarget == null) return;
        float directionX = currentTarget.position.x - transform.position.x;
        if (directionX < 0) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
    }

    /// <summary>
    /// Utiliza el motor de físicas 2D para escanear el radio de alcance configurado, filtrando por la capa enemiga y seleccionando de forma automática al objetivo más cercano a la estructura.
    /// </summary>
    void FindTarget()
    {
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider2D col in collidersInRange)
        {
            EnemyHealth health = col.GetComponent<EnemyHealth>();
            if (health != null && !health.isDead)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, col.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = col.gameObject;
                }
            }
        }

        if (nearestEnemy != null)
        {
            currentTarget = nearestEnemy.transform;
        }
    }

    /// <summary>
    /// Evalúa de forma integral si un Transform objetivo cumple con las condiciones necesarias para recibir daño (existencia, distancia y estado vital).
    /// </summary>
    /// <param name="target">El Transform del enemigo a comprobar.</param>
    /// <returns>Verdadero si el objetivo es válido para el ataque; falso si ha muerto o ha escapado del rango.</returns>
    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (Vector2.Distance(transform.position, target.position) > range) return false;
        
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null && health.isDead) return false;

        return true;
    }

    /// <summary>
    /// Inicia la secuencia ofensiva seleccionando aleatoriamente entre varias animaciones disponibles para aportar variedad visual y decide el momento de instanciar el proyectil.
    /// </summary>
    void StartAttack()
    {
        if (anim != null)
        {
            int randomAttack = Random.Range(1, 4); 
            string triggerName = "attack";
            if (randomAttack == 2) triggerName = "attack2";
            else if (randomAttack == 3) triggerName = "attack3";
            anim.SetTrigger(triggerName);
        }
        if (!useAnimationEvents)
        {
            SpawnProjectile();
        }
    }

    /// <summary>
    /// Método público destinado a ser interceptado por eventos de animación (Animation Events) desde Unity, permitiendo sincronizar el disparo exacto con el movimiento del modelo.
    /// </summary>
    public void TriggerAttackEvent()
    {
        if (useAnimationEvents)
        {
            SpawnProjectile();
        }
    }

    /// <summary>
    /// Reproduce el efecto sonoro correspondiente e instancia el prefab del proyectil mágico, asignándole el blanco a perseguir y el daño configurado en la torre.
    /// </summary>
    private void SpawnProjectile()
    {
        if (currentTarget == null) return;

        // --- REPRODUCIR SONIDO DE DISPARO ---
        PlayShootSound();

        if (magicBoltPrefab != null && firePoint != null)
        {
            GameObject magicBolt = Instantiate(magicBoltPrefab, firePoint.position, Quaternion.identity);
            Projectile projectile = magicBolt.GetComponent<Projectile>();
            if (projectile != null) 
            {
                projectile.SetTarget(currentTarget);
                projectile.damage = damage; 
            }
        }
    }

    /// <summary>
    /// Selecciona aleatoriamente y reproduce un clip de audio de la lista configurada, utilizando el componente de audio local o instanciando un origen espacial como respaldo.
    /// </summary>
    private void PlayShootSound()
    {
        if (shootSounds != null && shootSounds.Length > 0)
        {
            AudioClip clipToPlay = shootSounds[Random.Range(0, shootSounds.Length)];
            
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
    /// Dibuja un contorno esférico de color cian en la vista de Escena del editor de Unity para facilitar el ajuste visual del alcance del disparo durante la construcción del nivel.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}