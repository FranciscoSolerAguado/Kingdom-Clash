using UnityEngine;

/// <summary>
/// Proporciona una interfaz de interacción básica para objetos con colisionadores 2D, permitiendo ejecutar acciones globales como el retorno a la pantalla de título mediante la detección directa de clics del ratón.
/// </summary>
public class ClickableButton2D : MonoBehaviour
{
    /// <summary>
    /// Callback nativo de Unity que se dispara automáticamente cuando el usuario presiona el botón izquierdo del ratón mientras el puntero se encuentra sobre el colisionador físico del objeto.
    /// </summary>
    void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("¡Clic detectado en el botón 2D! Volviendo al menú...");
            GameManager.Instance.GoToTitleScreen();
        }
        else
        {
            Debug.LogWarning("GameManager no encontrado en la escena.");
        }
    }
}