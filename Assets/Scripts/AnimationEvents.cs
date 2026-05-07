using UnityEngine;

/// <summary>
/// Actúa como mediador entre el sistema de animaciones de Unity y la lógica de combate, permitiendo que los eventos disparados desde los clips de animación ejecuten acciones específicas en los scripts de ataque de aliados, enemigos o torres.
/// </summary>
public class AnimationEvents : MonoBehaviour
{
    private AllyAttack allyAttack;
    private EnemyAttack enemyAttack;
    private ArcherTower archerTower;
    private MageTower mageTower;

    /// <summary>
    /// Localiza y almacena las referencias de los componentes de ataque en los objetos padre al inicializarse, permitiendo que el objeto que contiene el Animator se comunique con la lógica principal de la entidad.
    /// </summary>
    void Start()
    {
        // Buscamos los scripts de ataque en el padre (o en este mismo objeto por si acaso)
        allyAttack = GetComponentInParent<AllyAttack>();
        enemyAttack = GetComponentInParent<EnemyAttack>();
        archerTower = GetComponentInParent<ArcherTower>();
        mageTower = GetComponentInParent<MageTower>();
    }

    /// <summary>
    /// Método público diseñado para ser invocado exclusivamente por un 'Animation Event' desde un clip de animación. Identifica qué tipo de entidad está atacando y redirige la orden de ejecución al script correspondiente.
    /// </summary>
    public void TriggerAttackEvent()
    {
        if (allyAttack != null)
        {
            allyAttack.TriggerAttackEvent();
        }
        else if (enemyAttack != null)
        {
            enemyAttack.TriggerAttackEvent();
        }
        else if (archerTower != null)
        {
            archerTower.TriggerAttackEvent();
        }
        else if (mageTower != null)
        {
            mageTower.TriggerAttackEvent();
        }
    }
}