using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona una variante de torre defensiva que despliega y coordina un escuadrón de arqueros móviles en el mapa, controlando su reaparición, mejora de estadísticas y posicionamiento táctico sobre los caminos.
/// </summary>
public class FixedArcherTower : MonoBehaviour, IMoveableTower
{
    [Header("Configuración")]
    public GameObject archerPrefab;
    public Transform spawnPoint; 
    public float respawnTime = 12f;
    public float rallyRange = 4f; 

    [Header("Estadísticas Base de las Unidades")]
    public int unitDamage = 15;
    public int unitMaxHealth = 50;

    [Header("Estado")]
    public GameObject[] archers = new GameObject[2];
    private Vector3 rallyPoint;

    public float RallyRange => rallyRange;

    /// <summary>
    /// Calcula automáticamente el punto de encuentro óptimo cercano al camino e inicializa la generación del escuadrón de arqueros al construirse la torre.
    /// </summary>
    void Start()
    {
        CalculateInitialRallyPoint();

        for (int i = 0; i < archers.Length; i++)
        {
            SpawnArcher(i);
        }
    }

    /// <summary>
    /// Aplica multiplicadores a las estadísticas base y actualiza de inmediato el daño, la salud actual y la salud máxima de los arqueros que ya se encuentran desplegados.
    /// </summary>
    /// <param name="damageMult">Multiplicador escalar para el daño de ataque.</param>
    /// <param name="healthMult">Multiplicador escalar para la salud máxima.</param>
    public void UpgradeUnitStats(float damageMult, float healthMult)
    {
        unitDamage = Mathf.RoundToInt(unitDamage * damageMult);
        unitMaxHealth = Mathf.RoundToInt(unitMaxHealth * healthMult);

        for (int i = 0; i < archers.Length; i++)
        {
            if (archers[i] != null)
            {
                AllyAttack atk = archers[i].GetComponent<AllyAttack>();
                if (atk != null) atk.attackDamage = unitDamage;

                AllyHealth hp = archers[i].GetComponent<AllyHealth>();
                if (hp != null)
                {
                    int extraHealth = unitMaxHealth - hp.maxHealth;
                    hp.maxHealth = unitMaxHealth;
                    hp.health += extraHealth;

                    if (hp.healthBar != null)
                    {
                        hp.healthBar.maxValue = hp.maxHealth;
                        hp.healthBar.value = hp.health;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Intercepta la notificación de muerte de un arquero del escuadrón e inicia el proceso temporal para generar su reemplazo.
    /// </summary>
    /// <param name="index">El índice de la matriz correspondiente a la unidad caída.</param>
    public void OnArcherDied(int index)
    {
        StartCoroutine(RespawnArcherRoutine(index));
    }

    /// <summary>
    /// Corrutina que gestiona el tiempo de espera (cooldown) de penalización antes de desplegar una nueva unidad en el campo de batalla.
    /// </summary>
    IEnumerator RespawnArcherRoutine(int index)
    {
        yield return new WaitForSeconds(respawnTime);
        SpawnArcher(index);
    }

    /// <summary>
    /// Instancia un nuevo arquero, inicializa sus atributos vitales y de combate según las mejoras actuales de la torre, y le ordena dirigirse a su posición de guardia.
    /// </summary>
    void SpawnArcher(int index)
    {
        if (archerPrefab == null || spawnPoint == null) return;

        GameObject newArcher = Instantiate(archerPrefab, spawnPoint.position, Quaternion.identity);

        Vector3 pos = newArcher.transform.position;
        pos.z = 0;
        newArcher.transform.position = pos;

        archers[index] = newArcher;

        // --- ASIGNAMOS ESTADÍSTICAS AL NACER ---
        AllyHealth health = newArcher.GetComponent<AllyHealth>();
        if (health != null)
        {
            health.maxHealth = unitMaxHealth;
            health.health = unitMaxHealth;
            health.Initialize(this, index);
        }

        AllyAttack attack = newArcher.GetComponent<AllyAttack>();
        if (attack != null)
        {
            attack.attackDamage = unitDamage;
        }

        AllyMovement movement = newArcher.GetComponent<AllyMovement>();
        if (movement != null)
        {
            Vector3 formationPos = GetFormationPosition(index);
            movement.SetRallyPoint(formationPos);
        }
    }

    /// <summary>
    /// Establece unas nuevas coordenadas de reunión global para el escuadrón y fuerza a las unidades supervivientes a reposicionarse en la nueva zona.
    /// </summary>
    /// <param name="position">Las nuevas coordenadas seleccionadas por el jugador en el mapa.</param>
    public void SetRallyPoint(Vector3 position)
    {
        rallyPoint = position;
        rallyPoint.z = 0;

        for (int i = 0; i < archers.Length; i++)
        {
            if (archers[i] != null)
            {
                AllyMovement movement = archers[i].GetComponent<AllyMovement>();
                if (movement != null)
                {
                    Vector3 formationPos = GetFormationPosition(i);
                    movement.SetRallyPoint(formationPos);
                }
            }
        }
    }

    /// <summary>
    /// Calcula y devuelve la posición geométrica exacta para un arquero específico, garantizando una separación paralela en la formación de guardia.
    /// </summary>
    Vector3 GetFormationPosition(int index)
    {
        float spacing = 0.4f;
        Vector3 offset = Vector3.zero;
        switch (index)
        {
            case 0: offset = new Vector3(spacing, -0.5f, 0); break;
            case 1: offset = new Vector3(-spacing, -0.5f, 0); break;
        }

        Vector3 formationPoint = rallyPoint + offset;
        formationPoint.z = 0;
        return formationPoint;
    }

    /// <summary>
    /// Analiza todos los trazados de camino (Paths) existentes en la escena y emplea cálculo de vectores para fijar automáticamente el punto inicial de reunión más cercano a la torre.
    /// </summary>
    void CalculateInitialRallyPoint()
    {
        // 1. Buscamos TODOS los caminos que haya en la escena
        Path[] allPaths = FindObjectsOfType<Path>();
        Vector3 referencePosition = transform.position;

        if (allPaths != null && allPaths.Length > 0)
        {
            float minDst = Mathf.Infinity;
            Vector3 bestPoint = referencePosition;
            bool pathFound = false;

            // 2. Evaluamos qué camino está más cerca de la torre
            foreach (Path path in allPaths)
            {
                if (path != null && path.points != null && path.points.Length > 1)
                {
                    for (int i = 0; i < path.points.Length - 1; i++)
                    {
                        Vector3 p1 = path.points[i].position;
                        Vector3 p2 = path.points[i + 1].position;
                        Vector3 closestOnSegment = GetClosestPointOnSegment(p1, p2, referencePosition);
                        float dst = Vector3.Distance(referencePosition, closestOnSegment);

                        if (dst < minDst)
                        {
                            minDst = dst;
                            bestPoint = closestOnSegment;
                            pathFound = true;
                        }
                    }
                }
            }

            if (pathFound)
            {
                if (Vector3.Distance(referencePosition, bestPoint) > rallyRange)
                {
                    Vector3 dir = (bestPoint - referencePosition).normalized;
                    rallyPoint = referencePosition + dir * rallyRange;
                }
                else
                {
                    rallyPoint = bestPoint;
                }

                rallyPoint.z = 0;
                return;
            }
        }

        // Fallback por si no hubiera ningún camino en la escena
        rallyPoint = referencePosition + Vector3.down * 2f;
        rallyPoint.z = 0;
    }

    /// <summary>
    /// Función matemática auxiliar que proyecta un punto sobre un segmento de línea recta entre dos vectores para encontrar la coordenada exacta de mayor proximidad.
    /// </summary>
    Vector3 GetClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ap = p - a;
        Vector3 ab = b - a;
        float t = Vector3.Dot(ap, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    /// <summary>
    /// Dibuja indicadores visuales en la ventana de Escena del editor de Unity para previsualizar el rango de movimiento táctico y los puntos de formación relativos.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(rallyPoint, 0.3f);
        Gizmos.DrawWireSphere(transform.position, rallyRange);

        for(int i=0; i<2; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetFormationPosition(i), 0.2f);
        }
    }
}