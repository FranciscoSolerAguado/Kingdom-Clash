using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Normaliza la escala de una entidad para prevenir deformaciones visuales indeseadas al rotar o escalar el objeto padre, asegurando que los componentes gráficos mantengan proporciones uniformes mientras ignoran elementos de interfaz como barras de vida.
/// </summary>
public class FixScale : MonoBehaviour 
{
    [Header("Configuración")]
    [Tooltip("Define aquí la escala absoluta que debe tener este objeto. Si es 0, se adaptará al tamaño máximo que alcance.")]
    public float fixedScale = 0f;

    [Tooltip("Actívalo para forzar que el Sprite/Animator tenga escala (1,1,1).")]
    public bool protectVisuals = true; 

    private float targetScaleValue;
    private bool isInitialized = false;
    private Transform visualChild;

    /// <summary>
    /// Inicializa los valores de escala objetivo e identifica mediante una búsqueda selectiva el componente visual principal (Animator o SpriteRenderer), descartando automáticamente elementos de UI.
    /// </summary>
    void Start() {
        // Configuración de la escala del padre
        if (fixedScale > 0)
        {
            targetScaleValue = fixedScale;
            isInitialized = true;
        }
        else
        {
            targetScaleValue = Mathf.Abs(transform.localScale.x);
            if (targetScaleValue < 0.1f) targetScaleValue = 0.1f;
            isInitialized = true;
        }

        // Buscamos el hijo que contiene el gráfico (Sprite o Animator) e ignoramos la UI
        if (protectVisuals)
        {
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null && anim.transform != transform)
            {
                visualChild = anim.transform;
            }
            else
            {
                SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.transform != transform)
                {
                    visualChild = sr.transform;
                }
            }
        }
    }

    /// <summary>
    /// Sincroniza la escala en el ciclo LateUpdate para garantizar que el cambio de dirección (escala negativa) no afecte a la proporción visual del gráfico ni a la legibilidad de los componentes hijos.
    /// </summary>
    void LateUpdate()
    {
        if (!isInitialized) return;

        // 1. Gestión de la escala del objeto principal (Padre)
        if (fixedScale == 0)
        {
            float currentAbs = Mathf.Abs(transform.localScale.x);
            if (currentAbs > targetScaleValue) targetScaleValue = currentAbs;
        }

        float currentSign = Mathf.Sign(transform.localScale.x);
        if (currentSign == 0) currentSign = 1;

        // Aplicamos la escala manteniendo el signo para el volteo pero fijando el valor absoluto
        transform.localScale = new Vector3(currentSign * targetScaleValue, targetScaleValue, targetScaleValue);

        // 2. Normalización de los visuales hijos
        if (protectVisuals && visualChild != null)
        {
            // Verificamos que el componente no sea UI antes de forzar su escala a la unidad
            if (visualChild.GetComponent<RectTransform>() == null && visualChild.GetComponent<Canvas>() == null)
            {
                visualChild.localScale = Vector3.one;
            }
        }
    }
}