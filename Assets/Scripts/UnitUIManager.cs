using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la interfaz de usuario dedicada a mostrar la información detallada de las unidades. Controla la visibilidad del contenedor de estadísticas, actualiza los textos de salud y daño en tiempo real, y maneja los eventos de deselección por entrada del usuario o muerte de la unidad.
/// </summary>
public class UnitUIManager : MonoBehaviour
{
    public static UnitUIManager Instance;

    [Header("Referencias a la UI")]
    [Tooltip("El contenedor principal que engloba la información visual. Se activa al seleccionar y se apaga al deseleccionar.")]
    public GameObject unitInfoContainer;
    
    [Tooltip("Campo de texto para mostrar el nombre identificativo de la unidad.")]
    public TextMeshProUGUI unitNameText;
    
    [Tooltip("Campo de texto para la salud actual y máxima (ej. 100/100).")]
    public TextMeshProUGUI healthText;
    
    [Tooltip("Campo de texto para el valor de daño de ataque.")]
    public TextMeshProUGUI attackText;
    
    [Tooltip("Componente de imagen para el retrato o icono de la unidad.")]
    public Image unitPortraitImage;

    [Tooltip("Elemento decorativo de fondo para el retrato.")]
    public GameObject portraitBackground;

    private UnitSelector currentSelectedUnit;

    /// <summary>
    /// Configura la instancia única del gestor (Singleton) para permitir que las unidades se registren al ser pulsadas.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Asegura que el panel de información esté oculto al iniciar la partida.
    /// </summary>
    private void Start()
    {
        if (unitInfoContainer != null)
        {
            unitInfoContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Monitoriza el estado de la unidad seleccionada y las entradas del jugador para cerrar el panel si se pulsa en el escenario vacío, se presiona Escape o si la unidad es destruida.
    /// </summary>
    private void Update()
    {
        if (currentSelectedUnit != null)
        {
            // Actualización constante de estadísticas para reflejar daño recibido o mejoras.
            currentSelectedUnit.UpdateStats();
            
            // Si la unidad muere, limpiamos la selección automáticamente.
            if (currentSelectedUnit.GetCurrentHealth() <= 0)
            {
                DeselectCurrent();
                return;
            }
            
            UpdateUI();
        }
        else if (unitInfoContainer != null && unitInfoContainer.activeSelf)
        {
            DeselectCurrent();
        }

        // Detección de clics en el vacío para deseleccionar.
        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
                
                if (hit.collider == null || hit.collider.GetComponent<UnitSelector>() == null)
                {
                    DeselectCurrent();
                }
            }
        }
        
        // Atajos rápidos para cerrar la información.
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectCurrent();
        }
    }

    /// <summary>
    /// Activa el contenedor de información y vincula una nueva unidad al panel, deseleccionando la unidad previa si existía.
    /// </summary>
    /// <param name="newUnit">Referencia al selector de la unidad pulsada.</param>
    public void SelectUnit(UnitSelector newUnit)
    {
        if (currentSelectedUnit != null && currentSelectedUnit != newUnit)
        {
            currentSelectedUnit.Deselect();
        }

        currentSelectedUnit = newUnit;
        
        if (unitInfoContainer != null)
        {
            unitInfoContainer.SetActive(true);
        }

        UpdateUI();
    }

    /// <summary>
    /// Limpia la referencia de la unidad actual, desactiva sus indicadores visuales y oculta el panel de la interfaz.
    /// </summary>
    public void DeselectCurrent()
    {
        if (currentSelectedUnit != null)
        {
            currentSelectedUnit.Deselect();
            currentSelectedUnit = null;
        }

        if (unitInfoContainer != null)
        {
            unitInfoContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Sincroniza los elementos visuales (textos y sprites) del panel con los datos actuales de la unidad seleccionada.
    /// </summary>
    private void UpdateUI()
    {
        if (currentSelectedUnit == null) return;

        if (unitNameText != null) 
            unitNameText.text = currentSelectedUnit.unitName;
        
        if (healthText != null) 
            healthText.text = $"{currentSelectedUnit.GetCurrentHealth()} / {currentSelectedUnit.GetMaxHealth()}";
            
        if (attackText != null) 
            attackText.text = $"{currentSelectedUnit.GetAttackDamage()}";
            
        if (unitPortraitImage != null)
        {
            Sprite portrait = currentSelectedUnit.GetUnitPortrait();
            if (portrait != null)
            {
                unitPortraitImage.sprite = portrait;
                unitPortraitImage.gameObject.SetActive(true);
                if (portraitBackground != null) portraitBackground.SetActive(true);
            }
            else
            {
                unitPortraitImage.gameObject.SetActive(false);
                if (portraitBackground != null) portraitBackground.SetActive(false);
            }
        }
    }
}