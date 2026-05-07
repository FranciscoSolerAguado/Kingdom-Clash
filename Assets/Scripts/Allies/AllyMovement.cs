using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona la inteligencia artificial, el movimiento mediante NavMeshAgent, la búsqueda de objetivos y las transiciones entre estados de combate y reposo de las unidades aliadas.
/// </summary>
public class AllyMovement : MonoBehaviour
{
    [Header("Tipo de Unidad")]
    public bool isRanged;

    [Header("Ajustes")]
    public float speed = 2f;
    [Tooltip("Distancia a la que se para para ATACAR")]
    public float stoppingDistance = 0.8f;
    public float attackRange = 1.0f;
    [Tooltip("Mueve el centro del círculo de ataque (Ej: Y=0.5 para subirlo al pecho)")]
    public Vector2 rangeCenterOffset = new Vector2(0f, 0.5f);
    public string enemyTag = "Enemy";

    [Header("Lógica de Torre Defense")]
    [Tooltip("El radio de la bandera. Si el enemigo sale de este círculo, el soldado vuelve a su puesto.")]
    public float guardZoneRadius = 2.5f;

    [Header("Regeneración")]
    public float healthRegenAmount = 1f;
    public float healthRegenInterval = 1f;
    public float timeUntilRegenStarts = 3f;

    [Header("Lógica Anti-Atasco")]
    public float maxStuckTime = 3.0f;
    private float timeStuck = 0f;

    private Transform targetEnemy;
    private Vector3? rallyPoint;
    private bool isFighting;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private AllyAttack attackScript;
    private AllyHealth myHealth;
    private NavMeshAgent agent;

    private float attackCooldown;
    private Coroutine healthRegenCoroutine;
    private float lastCombatTime;

    public Vector3 AttackCenter => transform.position + (Vector3)rangeCenterOffset;

    /// <summary>
    /// Inicializa las referencias a los componentes requeridos y configura los parámetros base del NavMeshAgent.
    /// </summary>
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        attackScript = GetComponent<AllyAttack>();
        myHealth = GetComponent<AllyHealth>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = speed;
            agent.stoppingDistance = 0;
        }

        if (anim != null) anim.speed = 0.75f;
        lastCombatTime = Time.time;
    }

    /// <summary>
    /// Establece el punto de reunión (bandera) de la unidad, reseteando su estado de combate y ordenándole regresar a su posición.
    /// </summary>
    /// <param name="point">Las coordenadas de destino en el mundo.</param>
    public void SetRallyPoint(Vector3 point)
    {
        rallyPoint = point;
        targetEnemy = null;
        isFighting = false;
        timeStuck = 0f;

        if (myHealth != null) myHealth.isOccupied = false;

        if (attackScript != null) attackScript.StopAttack();
    }

    /// <summary>
    /// Bucle principal de la IA que evalúa condiciones de atasco, enfriamientos de ataque y redirige a las lógicas de combate o reposo según corresponda.
    /// </summary>
    void Update()
    {
        if (agent == null) return;

        if (CheckForStuck()) return;

        if (!agent.isOnNavMesh)
        {
            SetRunningAnimation(false);
            return;
        }

        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            if (anim != null && !isFighting && attackCooldown <= 0) anim.ResetTrigger("attack");
        }

        HandleTargeting();

        if (targetEnemy != null)
        {
            HandleInCombatLogic();
        }
        else
        {
            HandleOutOfCombatLogic();
        }

        SetRunningAnimation(agent.velocity.sqrMagnitude > 0.01f);
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            HandleSpriteFlip(agent.steeringTarget);
        }
    }

    /// <summary>
    /// Verifica si la unidad se ha quedado bloqueada intentando llegar a su destino y la teletransporta (Warp) si supera el tiempo máximo permitido.
    /// </summary>
    /// <returns>Verdadero si la unidad estaba atascada y ha sido reubicada; falso en caso contrario.</returns>
    private bool CheckForStuck()
    {
        if (targetEnemy != null || !rallyPoint.HasValue)
        {
            timeStuck = 0f;
            return false;
        }

        bool isFarFromDestination = Vector2.Distance(transform.position, rallyPoint.Value) > 0.2f;
        bool isNotMoving = (agent.isOnNavMesh && agent.velocity.sqrMagnitude < 0.01f && !agent.pathPending) || !agent.isOnNavMesh;

        if (isFarFromDestination && isNotMoving)
        {
            timeStuck += Time.deltaTime;
            if (timeStuck >= maxStuckTime)
            {
                agent.Warp(rallyPoint.Value);
                agent.isStopped = true;
                agent.ResetPath();
                timeStuck = 0f;
                return true;
            }
        }
        else
        {
            timeStuck = 0f;
        }
        return false;
    }

    /// <summary>
    /// Controla el comportamiento de la unidad cuando no tiene enemigos fijados, gestionando su regreso al punto de reunión y permitiendo la regeneración de vida.
    /// </summary>
    private void HandleOutOfCombatLogic()
    {
        MaybeStartHealthRegen();

        if (!rallyPoint.HasValue)
        {
            if (!agent.isStopped) agent.isStopped = true;
            return;
        }

        float distToRally = Vector2.Distance(transform.position, rallyPoint.Value);

        if (distToRally > 0.2f)
        {
            if (agent.isStopped || Vector2.Distance(agent.destination, rallyPoint.Value) > 0.1f)
            {
                agent.SetDestination(rallyPoint.Value);
                agent.isStopped = false;
            }
        }
        else
        {
            if (!agent.isStopped) agent.isStopped = true;
        }
    }

    /// <summary>
    /// Determina las acciones de la unidad mientras tiene un objetivo fijado, alternando entre persecución y ejecución de ataques según su tipo (cuerpo a cuerpo o distancia).
    /// </summary>
    private void HandleInCombatLogic()
    {
        timeStuck = 0f;
        lastCombatTime = Time.time;
        StopHealthRegen();

        float distanceToEnemy = Vector2.Distance(AttackCenter, targetEnemy.position);

        if (isRanged)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            isFighting = true;
            HandleSpriteFlip(targetEnemy.position);
            TryAttack();
        }
        else
        {
            if (distanceToEnemy > attackRange)
            {
                if (agent.isStopped || Vector3.Distance(agent.destination, targetEnemy.position) > 0.1f)
                {
                    agent.SetDestination(targetEnemy.position);
                }
                agent.isStopped = false;
                isFighting = false;
            }
            else
            {
                if (!agent.isStopped)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
                isFighting = true;
                HandleSpriteFlip(targetEnemy.position);
                TryAttack();
            }
        }
    }

    /// <summary>
    /// Cancela cualquier acción agresiva en curso, libera el estado de ocupación de la unidad y la envía de vuelta a su formación.
    /// </summary>
    private void StopFightingAndSearchNew()
    {
        targetEnemy = null;
        isFighting = false;

        if (myHealth != null) myHealth.isOccupied = false;

        if (attackScript != null) attackScript.StopAttack();

        if (rallyPoint.HasValue && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(rallyPoint.Value);
            agent.isStopped = false;
        }
        lastCombatTime = Time.time;
    }

    /// <summary>
    /// Intercepta la notificación de un enemigo cercano y evalúa si es un objetivo viable basándose en el rango de ataque o la zona de guardia.
    /// </summary>
    /// <param name="enemy">El transform del enemigo detectado.</param>
    public void OnEnemyEngaged(Transform enemy)
    {
        if (targetEnemy == null || IsEnemyDead(targetEnemy))
        {
            if (isRanged)
            {
                if (Vector2.Distance(AttackCenter, enemy.position) > attackRange) return;
            }
            else
            {
                if (rallyPoint.HasValue && Vector2.Distance(rallyPoint.Value, enemy.position) > guardZoneRadius) return;
            }

            targetEnemy = enemy;
            lastCombatTime = Time.time;
            StopHealthRegen();
        }
    }

    /// <summary>
    /// Actualiza el temporizador de combate y detiene la regeneración pasiva de salud inmediatamente al recibir daño.
    /// </summary>
    public void OnReceivedDamage()
    {
        lastCombatTime = Time.time; 
        StopHealthRegen();
    }

    #region Métodos de Ayuda

    /// <summary>
    /// Evalúa continuamente si el enemigo objetivo actual sigue siendo válido (vivo y dentro de rango) o si es necesario buscar uno nuevo.
    /// </summary>
    private void HandleTargeting()
    {
        if (targetEnemy != null)
        {
            bool shouldDropTarget = IsEnemyDead(targetEnemy);

            if (!shouldDropTarget)
            {
                if (isRanged)
                {
                    float distToEnemy = Vector2.Distance(AttackCenter, targetEnemy.position);
                    if (distToEnemy > attackRange) shouldDropTarget = true;
                }
                else
                {
                    if (rallyPoint.HasValue)
                    {
                        float enemyDistToFlag = Vector2.Distance(rallyPoint.Value, targetEnemy.position);
                        if (enemyDistToFlag > guardZoneRadius)
                        {
                            shouldDropTarget = true;
                        }
                    }
                }
            }

            if (shouldDropTarget)
            {
                StopFightingAndSearchNew();
            }
        }

        if (targetEnemy == null)
        {
            ScanForNextEnemy();
        }
    }

    /// <summary>
    /// Utiliza físicas 2D (OverlapCircle) para escanear el área circundante y seleccionar al enemigo válido más cercano.
    /// </summary>
    private void ScanForNextEnemy()
    {
        float scanRadius = isRanged ? attackRange : guardZoneRadius;
        Vector3 scanCenter = (!isRanged && rallyPoint.HasValue) ? rallyPoint.Value : AttackCenter;

        Collider2D[] hits = Physics2D.OverlapCircleAll(scanCenter, scanRadius);

        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(enemyTag))
            {
                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.isDead || enemy.currentHealth <= 0) continue;

                EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
                if (enemyMovement != null && enemyMovement.CurrentTarget != null && enemyMovement.CurrentTarget != this.transform)
                {
                    AllyHealth otherAlly = enemyMovement.CurrentTarget.GetComponent<AllyHealth>();
                    if (otherAlly != null && !otherAlly.isDead)
                    {
                        continue;
                    }
                }

                if (isRanged)
                {
                    if (Vector2.Distance(AttackCenter, enemy.transform.position) > attackRange) continue;
                }
                else
                {
                    if (rallyPoint.HasValue && Vector2.Distance(rallyPoint.Value, enemy.transform.position) > guardZoneRadius) continue;
                }

                float dist = Vector2.Distance(AttackCenter, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = enemy.transform;
                }
            }
        }

        targetEnemy = closest;
    }

    /// <summary>
    /// Comprueba si ha pasado el tiempo suficiente desde el último combate para iniciar el proceso de curación pasiva.
    /// </summary>
    private void MaybeStartHealthRegen()
    {
        if (healthRegenCoroutine == null && Time.time - lastCombatTime > timeUntilRegenStarts)
        {
            if (myHealth != null && myHealth.health < myHealth.maxHealth)
            {
                healthRegenCoroutine = StartCoroutine(RegenerateHealth());
            }
        }
    }

    /// <summary>
    /// Interrumpe de manera segura la corrutina de curación pasiva.
    /// </summary>
    private void StopHealthRegen()
    {
        if (healthRegenCoroutine != null)
        {
            StopCoroutine(healthRegenCoroutine);
            healthRegenCoroutine = null;
        }
    }

    /// <summary>
    /// Corrutina encargada de restaurar salud periódicamente a la unidad siempre que no sufra interrupciones de combate.
    /// </summary>
    private IEnumerator RegenerateHealth()
    {
        while (myHealth != null && myHealth.health < myHealth.maxHealth)
        {
            yield return new WaitForSeconds(healthRegenInterval);
            
            if (myHealth != null && myHealth.health < myHealth.maxHealth)
            {
                myHealth.Heal(healthRegenAmount);
            }
        }
        healthRegenCoroutine = null;
    }

    /// <summary>
    /// Valida los tiempos de enfriamiento y ordena al script de ataque que ejecute una ofensiva contra el objetivo actual.
    /// </summary>
    private void TryAttack()
    {
        if (targetEnemy == null || attackCooldown > 0) return;

        if (attackScript != null)
        {
            attackScript.PerformAttack(targetEnemy);
            attackCooldown = attackScript.attackInterval;
            lastCombatTime = Time.time;
        }
    }

    /// <summary>
    /// Determina de forma segura si un objetivo enemigo ha sido destruido, desactivado o derrotado.
    /// </summary>
    /// <param name="enemyTransform">El transform del enemigo a evaluar.</param>
    /// <returns>Verdadero si el enemigo está muerto o inactivo; falso si sigue siendo una amenaza.</returns>
    private bool IsEnemyDead(Transform enemyTransform)
    {
        if (enemyTransform == null || !enemyTransform.gameObject.activeInHierarchy) return true;
        EnemyHealth eh = enemyTransform.GetComponent<EnemyHealth>();
        return eh == null || eh.isDead || eh.currentHealth <= 0;
    }

    /// <summary>
    /// Orientación visual del personaje, volteando el sprite en el eje X para que siempre mire hacia su destino u objetivo.
    /// </summary>
    /// <param name="targetPos">La posición en el mundo hacia la cual debe mirar el sprite.</param>
    private void HandleSpriteFlip(Vector3 targetPos)
    {
        if (spriteRenderer == null) return;
        if (Mathf.Abs(targetPos.x - transform.position.x) < 0.1f) return;
        spriteRenderer.flipX = (targetPos.x < transform.position.x);
    }

    /// <summary>
    /// Sincroniza el parámetro de estado de movimiento en el Animator para reproducir las animaciones de correr o estar inactivo.
    /// </summary>
    /// <param name="isRunning">Booleano que indica si la unidad se está desplazando.</param>
    private void SetRunningAnimation(bool isRunning)
    {
        if (anim != null && anim.GetBool("isRunning") != isRunning)
        {
            anim.SetBool("isRunning", isRunning);
        }
    }

    /// <summary>
    /// Dibuja indicadores visuales en la vista de Escena de Unity para facilitar la calibración de los rangos y zonas de guardia.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AttackCenter, attackRange);

        if (!isRanged)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = rallyPoint.HasValue ? rallyPoint.Value : AttackCenter;
            Gizmos.DrawWireSphere(center, guardZoneRadius);
        }
    }
    #endregion
}