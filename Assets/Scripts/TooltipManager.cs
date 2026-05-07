using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona la visualización de ventanas informativas flotantes (tooltips) en la interfaz de usuario, controlando su contenido dinámico y asegurando que la ventana siga al cursor sin desbordar los límites de la pantalla.
/// </summary>
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Referencias")]
    [Tooltip("El panel o ventana del tooltip (RectTransform).")]
    public RectTransform tooltipWindow;
    
    [Tooltip("El componente de texto donde se mostrará la información.")]
    public TextMeshProUGUI tooltipText;

    [Header("Ajustes")]
    [Tooltip("Desplazamiento respecto a la posición del cursor.")]
    public Vector2 offset = new Vector2(15f, -15f);

    private RectTransform canvasRectTransform;
    private Camera mainCamera;

    /// <summary>
    /// Configura el patrón Singleton para garantizar el acceso global al sistema de tooltips desde cualquier parte del juego.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Localiza la cámara principal y el RectTransform del Canvas padre para realizar conversiones de coordenadas de pantalla a espacio local de forma precisa.
    /// </summary>
    private void Start()
    {
        mainCamera = Camera.main;
        if (tooltipWindow != null)
        {
            tooltipWindow.gameObject.SetActive(false);
            Canvas parentCanvas = tooltipWindow.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
            }
        }
    }

    /// <summary>
    /// Mantiene el tooltip sincronizado con la posición actual del ratón en cada frame mientras la ventana esté activa.
    /// </summary>
    private void Update()
    {
        if (tooltipWindow != null && tooltipWindow.gameObject.activeSelf)
        {
            UpdatePosition();
        }
    }

    /// <summary>
    /// Activa la ventana del tooltip con el texto proporcionado, forzando que se renderice sobre cualquier otro elemento de la interfaz.
    /// </summary>
    /// <param name="text">Cadena de texto descriptiva a mostrar.</param>
    public void ShowTooltip(string text)
    {
        if (tooltipWindow == null || tooltipText == null) return;
        if (string.IsNullOrEmpty(text))
        {
            HideTooltip();
            return;
        }

        tooltipText.text = text;

        // Forzamos que se dibuje por encima de todo el Canvas
        tooltipWindow.SetAsLastSibling();
        tooltipWindow.gameObject.SetActive(true);

        // Posicionamos el tooltip inmediatamente para evitar un frame de 'salto' visual
        UpdatePosition();
    }

    /// <summary>
    /// Desactiva visualmente la ventana del tooltip y detiene el seguimiento del cursor.
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipWindow != null)
        {
            tooltipWindow.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Calcula la posición local del ratón dentro del Canvas y ajusta el pivote y el offset de la ventana para evitar que el tooltip se oculte tras los bordes de la pantalla.
    /// </summary>
    private void UpdatePosition()
    {
        if (canvasRectTransform == null) return;

        // 1. Obtención de la posición del cursor en píxeles de pantalla
        Vector2 mousePos = Input.mousePosition;

        // 2. Conversión de coordenadas de pantalla a punto local en el RectTransform del Canvas
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out localPoint);

        // 3. Lógica de ajuste de bordes: calculamos el pivote según el cuadrante de la pantalla
        float pivotX = mousePos.x / Screen.width;
        float pivotY = mousePos.y / Screen.height;

        float newPivotX = pivotX > 0.5f ? 1.0f : 0.0f;
        float newPivotY = pivotY > 0.5f ? 1.0f : 0.0f;
        
        tooltipWindow.pivot = new Vector2(newPivotX, newPivotY);

        // Invertimos el offset si estamos en el lado derecho o superior para no tapar el cursor
        Vector2 currentOffset = offset;
        if (newPivotX == 1.0f) currentOffset.x = -Mathf.Abs(offset.x);
        if (newPivotY == 1.0f) currentOffset.y = Mathf.Abs(offset.y); 

        // 4. Aplicación de la posición final calculada
        tooltipWindow.localPosition = localPoint + currentOffset;
    }
}