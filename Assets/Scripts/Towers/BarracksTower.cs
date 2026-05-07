using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona la torre de cuartel, encargada de generar y mantener un escuadrón de soldados cuerpo a cuerpo, controlando sus estadísticas, tiempos de reaparición y posiciones de formación en el mapa.
/// </summary>
public class BarracksTower : MonoBehaviour, IMoveableTower
{
    [Header("Configuración")]
    public GameObject warriorPrefab;
    public Transform spawnPoint;
    public float respawnTime = 10f;
    public float rallyRange = 3f;

    [Header("Estadísticas Base de las Unidades")]
    [Tooltip("El daño y vida con los que nacerán los soldados en esta torre")]
    public int unitDamage = 15;
    public int unitMaxHealth = 50;

    [Header("Estado")]
    public GameObject[] warriors = new GameObject[3];
    private Vector3 rallyPoint;

    public float RallyRange => rallyRange;

    /// <summary>
    /// Calcula automáticamente el punto de reunión inicial más cercano sobre el camino y genera el escuadrón base de guerreros al construirse la torre.
    /// </summary>
    void Start()
    {
        CalculateInitialRallyPoint();

        for (int i = 0; i < warriors.Length; i++)
        {
            SpawnWarrior(i);
        }
    }

    /// <summary>
    /// Aplica multiplicadores a las estadísticas base de la torre y actualiza instantáneamente el daño, la salud actual y la salud máxima de los soldados que ya están vivos en el mapa.
    /// </summary>
    /// <param name="damageMult">Multiplicador para el daño de ataque.</param>
    /// <param name="healthMult">Multiplicador para la salud máxima.</param>
    public void UpgradeUnitStats(float damageMult, float healthMult)
    {
        unitDamage = Mathf.RoundToInt(unitDamage * damageMult);
        unitMaxHealth = Mathf.RoundToInt(unitMaxHealth * healthMult);

        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] != null)
            {
                AllyAttack atk = warriors[i].GetComponent<AllyAttack>();
                if (atk != null) atk.attackDamage = unitDamage;

                AllyHealth hp = warriors[i].GetComponent<AllyHealth>();
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
    /// Recibe la notificación de muerte de una unidad específica de la formación e inicia el proceso para su eventual reaparición.
    /// </summary>
    /// <param name="index">El índice de la matriz correspondiente al soldado caído.</param>
    public void OnWarriorDied(int index)
    {
        StartCoroutine(RespawnWarriorRoutine(index));
    }

    /// <summary>
    /// Corrutina que espera el tiempo de penalización configurado antes de reponer a un soldado caído.
    /// </summary>
    IEnumerator RespawnWarriorRoutine(int index)
    {
        yield return new WaitForSeconds(respawnTime);
        SpawnWarrior(index);
    }

    /// <summary>
    /// Instancia un nuevo guerrero, inicializa sus atributos de combate según el nivel actual de la torre, lo vincula a su índice correspondiente y le ordena desplazarse a su posición designada.
    /// </summary>
    void SpawnWarrior(int index)
    {
        if (warriorPrefab == null || spawnPoint == null) return;

        float miniSpacing = 0.2f;
        Vector3 spawnOffset = Vector3.zero;

        switch (index)
        {
            case 0: spawnOffset = new Vector3(miniSpacing, 0, 0); break;
            case 1: spawnOffset = new Vector3(-miniSpacing, 0, 0); break;
            case 2: spawnOffset = new Vector3(0, -miniSpacing, 0); break;
        }

        Vector3 finalSpawnPos = spawnPoint.position + spawnOffset;
        finalSpawnPos.z = 0;

        GameObject newWarrior = Instantiate(warriorPrefab, finalSpawnPos, Quaternion.identity);
        warriors[index] = newWarrior;

        AllyHealth health = newWarrior.GetComponent<AllyHealth>();
        if (health != null)
        {
            health.maxHealth = unitMaxHealth;
            health.health = unitMaxHealth;
            health.Initialize(this, index);
        }

        AllyAttack attack = newWarrior.GetComponent<AllyAttack>();
        if (attack != null)
        {
            attack.attackDamage = unitDamage;
        }

        AllyMovement movement = newWarrior.GetComponent<AllyMovement>();
        if (movement != null)
        {
            Vector3 formationPos = GetFormationPosition(index);
            movement.SetRallyPoint(formationPos);
        }
    }

    /// <summary>
    /// Establece un nuevo punto de reunión (bandera) para el escuadrón y ordena a todas las unidades activas movilizarse hacia sus nuevas coordenadas de guardia.
    /// </summary>
    /// <param name="position">Las nuevas coordenadas globales establecidas por el jugador.</param>
    public void SetRallyPoint(Vector3 position)
    {
        rallyPoint = position;
        rallyPoint.z = 0;

        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] != null)
            {
                AllyMovement movement = warriors[i].GetComponent<AllyMovement>();
                if (movement != null)
                {
                    Vector3 formationPos = GetFormationPosition(i);
                    movement.SetRallyPoint(formationPos);
                }
            }
        }
    }

    /// <summary>
    /// Calcula las coordenadas exactas para un soldado específico dentro de la zona de reunión, estableciendo una formación triangular con separación suficiente para evitar superposiciones físicas.
    /// </summary>
    Vector3 GetFormationPosition(int index)
    {
        float spacing = 0.6f;
        Vector3 offset = Vector3.zero;

        switch (index)
        {
            case 0: offset = new Vector3(spacing, 0, 0); break;
            case 1: offset = new Vector3(-spacing, 0, 0); break;
            case 2: offset = new Vector3(0, -spacing * 1f, 0); break;
        }

        Vector3 formationPoint = rallyPoint + offset;
        formationPoint.z = 0;
        return formationPoint;
    }

    /// <summary>
    /// Escanea todos los objetos de tipo Path en la escena y utiliza cálculos trigonométricos para ubicar automáticamente el punto de encuentro inicial de las unidades en el tramo de ruta más cercano a la torre.
    /// </summary>
    void CalculateInitialRallyPoint()
    {
        // 1. Buscamos TODOS los caminos que haya en la escena
        Path[] allPaths = FindObjectsOfType<Path>();
        Vector3 referencePosition = spawnPoint != null ? spawnPoint.position : transform.position;

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
    /// Función matemática auxiliar que determina el punto más próximo dentro de un segmento de línea recta en relación a unas coordenadas externas.
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
    /// Dibuja guías visuales en la vista de Escena del editor de Unity para facilitar la visualización del rango de operación de la torre y las posiciones teóricas de la formación.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rallyPoint, 0.3f);
        Gizmos.DrawWireSphere(transform.position, rallyRange);

        for (int i = 0; i < 3; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetFormationPosition(i), 0.2f);
        }
    }
}