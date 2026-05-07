using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona la navegación de los enemigos a través de una ruta predefinida, la detección de unidades aliadas mediante físicas 2D y las transiciones fluidas entre el avance por el mapa y el estado de combate.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    public float speed = 2f;

    [Tooltip("Ajuste visual (Offset) para forzar al enemigo a caminar más arriba o abajo del camino.")]
    public Vector3 pathOffset = Vector3.zero;

    [Header("Atributos de Combate")]
    public bool isRanged = false;
    [Tooltip("Distancia a la que un enemigo puede atacar.")]
    public float attackRange = 1.0f;
    [Tooltip("Radio matemático para detectar el cuerpo real del aliado, ignorando sus áreas de ataque gigantes.")]
    public float detectionRadius = 1.2f;
    [Tooltip("Mueve el centro de los círculos de ataque y detección (Ej: Y=0.5 para subirlo al pecho)")]
    public Vector2 rangeCenterOffset = new Vector2(0f, 0.5f);
    public int damageToPlayer = 1;

    private Path route;
    private int currentPointIndex = 0;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private bool isFighting = false;
    private Transform currentTarget;
    private AllyHealth currentAllyHealth;
    private EnemyAttack attackScript;

    public Transform CurrentTarget => currentTarget;

    private float attackCooldown = 0f;

    /// <summary>
    /// Calcula dinámicamente el centro real para los escaneos de área y cálculos de distancia de ataque, aplicando el offset vertical u horizontal configurado.
    /// </summary>
    public Vector3 AttackCenter => transform.position + (Vector3)rangeCenterOffset;

    /// <summary>
    /// Inicializa las referencias a los componentes de animación, ataque y renderizado visual al crear la entidad.
    /// </summary>
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        attackScript = GetComponent<EnemyAttack>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Bucle principal de la IA que evalúa los tiempos de enfriamiento, alterna entre el seguimiento de la ruta y la confrontación directa, y gestiona el desplazamiento físico hacia los objetivos.
    /// </summary>
    void Update()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            if (anim != null) anim.ResetTrigger("attack");
        }

        // Si no está peleando, usa el escáner matemático centrado en el AttackCenter
        if (!isFighting)
        {
            ScanForAllies();
        }

        if (isFighting)
        {
            if (currentTarget == null || currentAllyHealth == null || currentAllyHealth.isDead)
            {
                StopFighting();
                return;
            }

            HandleSpriteFlip(currentTarget.position);

            if (!isRanged)
            {
                // Calculamos la distancia desde nuestro pecho/centro hasta el objetivo
                float distance = Vector2.Distance(AttackCenter, currentTarget.position);

                if (distance > attackRange)
                {
                    // El movimiento físico sigue empujando el transform real
                    transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
                    SetRunningAnimation(true);
                }
                else
                {
                    SetRunningAnimation(false);
                    TryAttack();
                }
            }
            else
            {
                SetRunningAnimation(false);
                TryAttack();
            }
            return;
        }

        if (route == null || currentPointIndex >= route.points.Length)
        {
            SetRunningAnimation(false);
            return;
        }

        Vector3 targetPos = route.points[currentPointIndex].position + pathOffset;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        SetRunningAnimation(true);
        HandleSpriteFlip(targetPos);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f) currentPointIndex++;
        if (currentPointIndex >= route.points.Length) ReachGoal();
    }

    /// <summary>
    /// Utiliza físicas 2D (OverlapCircle) centradas en el AttackCenter para identificar y fijar como objetivo a la unidad aliada válida y disponible más cercana.
    /// </summary>
    private void ScanForAllies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(AttackCenter, detectionRadius);

        AllyHealth bestAlly = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("ally"))
            {
                AllyHealth ally = hit.GetComponentInParent<AllyHealth>();
                if (ally != null && !ally.isDead)
                {
                    if (!isRanged && ally.isOccupied) continue;

                    // Medimos la distancia desde nuestro AttackCenter al aliado
                    float dist = Vector2.Distance(AttackCenter, ally.transform.position);

                    if (dist <= detectionRadius && dist < minDistance)
                    {
                        minDistance = dist;
                        bestAlly = ally;
                    }
                }
            }
        }

        if (bestAlly != null)
        {
            currentAllyHealth = bestAlly;
            if (!isRanged) currentAllyHealth.isOccupied = true;

            isFighting = true;
            currentTarget = bestAlly.transform;
        }
    }

    /// <summary>
    /// Valida que el objetivo permanezca dentro del rango de ataque efectivo y ejecuta la ofensiva si los tiempos de enfriamiento lo permiten.
    /// </summary>
    private void TryAttack()
    {
        if (currentTarget == null || attackCooldown > 0) return;

        if (!isRanged)
        {
            float distance = Vector2.Distance(AttackCenter, currentTarget.position);
            if (distance > attackRange + 0.1f) return;
        }

        if (attackScript != null)
        {
            attackScript.PerformAttack(currentTarget);
            attackCooldown = attackScript.attackInterval;
        }
    }

    /// <summary>
    /// Detecta las colisiones con los volúmenes de fin de nivel (LevelLimit) para aplicar el daño correspondiente a la salud general del jugador.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LevelLimit"))
        {
            HandleDeathByLimit();
        }
    }

    /// <summary>
    /// Interrumpe de forma segura el estado de combate, liberando la condición de ocupación del aliado interceptado y retomando el avance estándar por el camino.
    /// </summary>
    private void StopFighting()
    {
        if (currentAllyHealth != null && !isRanged)
        {
            currentAllyHealth.isOccupied = false;
        }

        currentAllyHealth = null;
        isFighting = false;
        currentTarget = null;

        if (attackScript != null)
        {
            attackScript.StopAttack();
        }

        SetRunningAnimation(true);
    }

    /// <summary>
    /// Invierte el eje X del componente visual del enemigo para asegurar que siempre esté mirando hacia su próximo punto de ruta o hacia el objetivo de combate.
    /// </summary>
    private void HandleSpriteFlip(Vector3 targetPos)
    {
        if (spriteRenderer == null) return;
        if (Mathf.Abs(targetPos.x - transform.position.x) < 0.1f) return;
        spriteRenderer.flipX = (targetPos.x < transform.position.x);
    }

    /// <summary>
    /// Sincroniza el estado de movimiento actual con el controlador de animaciones (Animator) del modelo.
    /// </summary>
    private void SetRunningAnimation(bool isRunning) { if (anim != null) anim.SetBool("isRunning", isRunning); }
    
    /// <summary>
    /// Asigna la secuencia de puntos espaciales (waypoints) que conformarán la ruta de invasión de este enemigo durante el nivel.
    /// </summary>
    public void SetPath(Path newPath) { route = newPath; }
    
    /// <summary>
    /// Ejecuta la penalización sobre las vidas globales del jugador al final de la ruta y destruye al enemigo infiltrado.
    /// </summary>
    private void HandleDeathByLimit() { if (GameManager.Instance != null) GameManager.Instance.LoseLives(damageToPlayer); Destroy(gameObject); }
    
    /// <summary>
    /// Método de enrutamiento invocado automáticamente cuando el enemigo alcanza el último waypoint definido en su camino.
    /// </summary>
    void ReachGoal() { HandleDeathByLimit(); }

    /// <summary>
    /// Dibuja representaciones esféricas en la vista de Escena del editor de Unity para facilitar la depuración visual de los radios de detección y ataque.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Zona de visión/detección (Magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(AttackCenter, detectionRadius);

        // Zona de ataque (Rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AttackCenter, attackRange);
    }
}