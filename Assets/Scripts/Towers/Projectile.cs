using UnityEngine;

/// <summary>
/// Gestiona la lógica de los proyectiles disparados por las torres o unidades, encargándose del seguimiento dinámico del objetivo, el cálculo de rotación según la trayectoria y la aplicación de daño al impactar.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 15f;
    public int damage = 10;
    public string targetTag = "Enemy";

    private Transform target;
    private BoxCollider2D targetBodyCollider;
    private bool hasHit = false;

    /// <summary>
    /// Asigna el objetivo al que el proyectil debe perseguir e intenta localizar su colisionador principal para calcular un punto de impacto centrado y preciso.
    /// </summary>
    /// <param name="newTarget">El Transform del objetivo a seguir.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetBodyCollider = target.GetComponent<BoxCollider2D>();
            if (targetBodyCollider == null) targetBodyCollider = target.GetComponentInChildren<BoxCollider2D>();
        }
    }

    /// <summary>
    /// Realiza una validación de seguridad sobre las etiquetas de los objetivos para normalizar posibles variaciones en la nomenclatura del inspector.
    /// </summary>
    void Start()
    {
        if (targetTag == "Ally") targetTag = "ally";
        if (targetTag == "enemy") targetTag = "Enemy";
    }

    /// <summary>
    /// Actualiza en cada frame la posición del proyectil hacia su destino. Calcula la rotación necesaria para que el eje visual coincida con la dirección del movimiento y detecta la proximidad del impacto.
    /// </summary>
    void Update()
    {
        if (hasHit) return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 destination;
        if (targetBodyCollider != null)
        {
            // Apunta al centro geométrico del colisionador para un impacto visual realista
            destination = targetBodyCollider.bounds.center;
        }
        else
        {
            destination = target.position + Vector3.up * 0.5f;
        }

        float step = speed * Time.deltaTime;
        float distToDest = Vector3.Distance(transform.position, destination);

        // Si el proyectil está lo suficientemente cerca en este frame, procesa el impacto
        if (distToDest <= step)
        {
            HitTarget(target.gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, destination, step);

        // Alineación de la rotación del proyectil con el vector de dirección
        Vector3 dir = destination - transform.position;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    /// <summary>
    /// Detecta colisiones físicas mediante triggers para permitir que el proyectil impacte con objetivos que se crucen en su trayectoria, validando que pertenezcan a la facción opuesta.
    /// </summary>
    /// <param name="col">El colisionador con el que se ha producido el contacto.</param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasHit) return;

        if (col.CompareTag(targetTag))
        {
            // Evita procesar aquí si es el objetivo principal (ya se gestiona en Update) para evitar doble impacto
            if (col.transform == target || col.transform.IsChildOf(target))
            {
                return;
            }

            if (col is BoxCollider2D || col is CapsuleCollider2D)
            {
                HitTarget(col.gameObject);
            }
        }
    }

    /// <summary>
    /// Ejecuta la lógica final del impacto: identifica la facción del objeto golpeado, aplica la deducción de salud correspondiente y desactiva el proyectil antes de su destrucción.
    /// </summary>
    /// <param name="obj">El GameObject impactado.</param>
    void HitTarget(GameObject obj)
    {
        if (hasHit) return;
        hasHit = true;

        if (targetTag == "Enemy")
        {
            EnemyHealth health = obj.GetComponent<EnemyHealth>() ?? obj.GetComponentInParent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        else if (targetTag == "ally")
        {
            AllyHealth health = obj.GetComponent<AllyHealth>() ?? obj.GetComponentInParent<AllyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        // Desactiva colisiones para evitar efectos secundarios durante la breve animación de impacto
        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.enabled = false;

        if (obj != null)
        {
            // Emparenta el proyectil al objetivo para que "se mueva" con él antes de desaparecer
            transform.SetParent(obj.transform);
        }

        Destroy(gameObject, 0.15f);
    }
}