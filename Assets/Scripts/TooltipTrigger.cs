using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Proporciona un disparador para la visualización de tooltips. Al adjuntarse a un objeto de la interfaz, 
/// detecta la presencia del cursor y solicita al TooltipManager la apertura de la ventana informativa tras un breve retraso.
/// </summary>
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)]
    [Tooltip("El texto que aparecerá en el tooltip cuando el ratón pase por encima.")]
    public string content;

    [Tooltip("Tiempo en segundos que hay que mantener el ratón encima antes de que aparezca el tooltip.")]
    public float delay = 0.5f;

    [Header("Opcional: Integración con DeployButton")]
    [Tooltip("Si se asigna un DeployButton, el tooltip cambiará a 'No disponible' si la unidad está bloqueada.")]
    public DeployButton deployButton;

    private Coroutine showCoroutine;
    private bool isPointerOver = false;

    /// <summary>
    /// Detecta la entrada del puntero en el área del objeto e inicia la corrutina de espera para mostrar el tooltip.
    /// </summary>
    /// <param name="eventData">Datos del evento proporcionados por el EventSystem.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        if (showCoroutine == null)
        {
            showCoroutine = StartCoroutine(ShowTooltipDelayed());
        }
    }

    /// <summary>
    /// Detecta la salida del puntero y cancela inmediatamente cualquier proceso de visualización activo, ocultando el tooltip si ya era visible.
    /// </summary>
    /// <param name="eventData">Datos del evento proporcionados por el EventSystem.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        Cancel();
    }

    /// <summary>
    /// Asegura la limpieza de los estados visuales y la detención de corrutinas si el objeto se desactiva inesperadamente.
    /// </summary>
    private void OnDisable()
    {
        isPointerOver = false;
        Cancel();
    }

    /// <summary>
    /// Detiene la corrutina de retardo y ordena al TooltipManager que oculte la ventana informativa.
    /// </summary>
    private void Cancel()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    /// <summary>
    /// Corrutina que gestiona el tiempo de espera (idle time). Antes de mostrar el contenido, verifica si 
    /// el objeto de origen (como un botón de despliegue) está bloqueado para adaptar el mensaje dinámicamente.
    /// </summary>
    private IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSeconds(delay);

        if (isPointerOver && TooltipManager.Instance != null)
        {
            string finalContent = content;

            // Integración lógica: Si el botón está bloqueado por falta de requisitos, cambiamos el texto.
            if (deployButton != null)
            {
                UnityEngine.UI.Button btn = deployButton.GetComponent<UnityEngine.UI.Button>();
                if (btn != null && !btn.interactable)
                {
                    finalContent = "No disponible aún.";
                }
            }

            TooltipManager.Instance.ShowTooltip(finalContent);
        }
        
        showCoroutine = null;
    }
}