using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gestiona la selección individual de unidades en el campo de batalla, proporcionando retroalimentación visual mediante círculos de selección y enviando los datos de combate (salud y daño) al gestor de interfaz de usuario.
/// </summary>
public class UnitSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visuales")]
    [Tooltip("El SpriteRenderer o el objeto del círculo seleccionador debajo de los pies de la unidad.")]
    public GameObject selectionCircle;

    [Header("Referencias a UI")]
    [Tooltip("El nombre que queremos que se muestre en la UI cuando se seleccione.")]
    public string unitName = "Unidad";

    [Tooltip("La foto o icono de la unidad que se mostrará en la UI.")]
    public Sprite unitPortrait;

    private int currentHealth;
    private int maxHealth;
    private int attackDamage;

    private bool isSelected = false;

    /// <summary>
    /// Inicializa el estado visual del selector y realiza una lectura inicial de las estadísticas de la unidad.
    /// </summary>
    void Start()
    {
        if (selectionCircle != null)
        {
            selectionCircle.SetActive(false);
        }

        UpdateStats();
    }

    /// <summary>
    /// Activa el resaltado visual y actualiza el cursor cuando el puntero entra en el área de la unidad.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetHoverCursor();
        }

        if (selectionCircle != null)
        {
            selectionCircle.SetActive(true);
        }
    }

    /// <summary>
    /// Restaura el cursor y desactiva el resaltado visual al salir el puntero, a menos que la unidad esté seleccionada permanentemente.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetDefaultCursor();
        }

        if (!isSelected && selectionCircle != null)
        {
            selectionCircle.SetActive(false);
        }
    }

    /// <summary>
    /// Procesa la selección de la unidad mediante clic izquierdo, actualizando sus estadísticas actuales y notificando al UnitUIManager para mostrar su información en pantalla.
    /// </summary>
    /// <param name="eventData">Datos del evento de clic.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (UnitUIManager.Instance != null)
        {
            // Refrescamos las estadísticas antes de enviar la información a la UI.
            UpdateStats();

            UnitUIManager.Instance.SelectUnit(this);
            isSelected = true;

            if (selectionCircle != null)
            {
                selectionCircle.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("UnitUIManager no encontrado en la escena.");
        }
    }

    /// <summary>
    /// Desactiva el estado de selección de la unidad y oculta su indicador visual.
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        if (selectionCircle != null)
        {
            selectionCircle.SetActive(false);
        }
    }

    /// <summary>
    /// Realiza una búsqueda de componentes de salud y ataque (tanto en aliados como en enemigos) para sincronizar los valores internos con el estado actual de la entidad.
    /// </summary>
    public void UpdateStats()
    {
        // --- Lógica para unidades aliadas ---
        AllyHealth aHealth = GetComponent<AllyHealth>();
        AllyAttack aAttack = GetComponent<AllyAttack>();

        if (aHealth != null)
        {
            currentHealth = (int)aHealth.health;
            maxHealth = (int)aHealth.maxHealth;
        }

        if (aAttack != null)
        {
            attackDamage = aAttack.attackDamage;
        }

        // --- Lógica para unidades enemigas ---
        EnemyHealth eHealth = GetComponent<EnemyHealth>();
        EnemyAttack eAttack = GetComponent<EnemyAttack>();

        if (eHealth != null)
        {
            currentHealth = eHealth.currentHealth;
            maxHealth = eHealth.maxHealth;
        }

        if (eAttack != null)
        {
            attackDamage = eAttack.attackDamage;
        }
    }

    // Métodos de acceso para la consulta de datos desde la interfaz de usuario.
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetAttackDamage() => attackDamage;
    public Sprite GetUnitPortrait() => unitPortrait;
}