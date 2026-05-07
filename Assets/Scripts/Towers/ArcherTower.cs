using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestiona el comportamiento autónomo de las torres de arqueros, incluyendo la detección de enemigos en un radio definido, la rotación visual, la selección aleatoria de animaciones de ataque y el instanciado de proyectiles con efectos de sonido.
/// </summary>
public class ArcherTower : MonoBehaviour
{
    [Header("Configuración")]
    public float range = 5f;
    [Tooltip("Disparos por segundo.")]
    public float fireRate = 1f;
    public int damage = 10;
    
    [Tooltip("Capa donde se encuentran los enemigos. (Normalmente 'Enemy' o 'Default'). ¡Asegúrate de configurarla!")]
    public LayerMask enemyLayer; 
    
    private float fireCountdown = 0f;

    [Header("Sincronización (Eventos de Animación)")]
    public bool useAnimationEvents = false;

    [Header("Referencias")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public Animator anim;

    // --- NUEVO: SISTEMA DE AUDIO ---
    [Header("Audio")]
    [Tooltip("Añade aquí sonidos de disparo (ej. tensar arco, flecha volando). Se elegirá uno al azar.")]
    public AudioClip[] shootSounds;
    private AudioSource audioSource;
    // -------------------------------

    private Transform currentTarget;
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Inicializa las referencias a los componentes visuales y de audio, ajustando la velocidad de la animación en base a la cadencia de fuego y asegurando una máscara de colisión válida para buscar enemigos.
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
    /// Bucle principal que evalúa constantemente la validez del objetivo actual, actualiza la orientación geométrica del sprite y gestiona el temporizador para ejecutar los ataques.
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
    /// Verifica si el objetivo actual sigue siendo válido (sigue vivo y dentro del rango) o invoca la rutina de búsqueda para fijar un nuevo enemigo prioritario.
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
    /// Ajusta dinámicamente la escala en el eje X del componente SpriteRenderer para que el arte visual del arquero apunte siempre en la dirección del objetivo fijado.
    /// </summary>
    void FaceTarget()
    {
        if (spriteRenderer == null || currentTarget == null) return;
        float directionX = currentTarget.position.x - transform.position.x;
        if (directionX < 0) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
    }

    /// <summary>
    /// Utiliza el motor de físicas 2D para escanear el área de alcance de la torre, filtrando por la capa enemiga y seleccionando automáticamente al objetivo más cercano a la estructura.
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
    /// Evalúa de forma integral si un Transform específico cumple con las condiciones para ser atacado (existencia, distancia y salud).
    /// </summary>
    /// <param name="target">El Transform del enemigo a evaluar.</param>
    /// <returns>Verdadero si el objetivo es válido para recibir daño; falso en caso contrario.</returns>
    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (Vector2.Distance(transform.position, target.position) > range) return false;
        
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null && health.isDead) return false;

        return true;
    }

    /// <summary>
    /// Inicia la secuencia ofensiva seleccionando de forma aleatoria entre varias animaciones de ataque disponibles para añadir variedad visual, y decide cómo y cuándo instanciar el proyectil.
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
    /// Método público diseñado para ser interceptado por eventos de animación (Animation Events) desde Unity, garantizando que la flecha se dispare en el frame exacto de la animación.
    /// </summary>
    public void TriggerAttackEvent()
    {
        if (useAnimationEvents)
        {
            SpawnProjectile();
        }
    }

    /// <summary>
    /// Ejecuta el efecto sonoro correspondiente e instancia el prefab de la flecha, transfiriéndole el valor de daño configurado y referenciando el objetivo al que debe perseguir.
    /// </summary>
    private void SpawnProjectile()
    {
        if (currentTarget == null) return;

        // --- REPRODUCIR SONIDO DE DISPARO ---
        PlayShootSound();

        if (arrowPrefab != null && firePoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
            Projectile projectile = arrow.GetComponent<Projectile>();
            if (projectile != null) 
            {
                projectile.SetTarget(currentTarget);
                projectile.damage = damage; 
            }
        }
    }

    /// <summary>
    /// Selecciona aleatoriamente y reproduce un clip de audio de la lista configurada, utilizando el emisor de la torre o instanciando uno espacial en caso de carecer de él.
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
    /// Dibuja un contorno esférico de color rojo en la vista de Escena del editor de Unity para facilitar la calibración visual del rango de disparo durante el diseño del nivel.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}