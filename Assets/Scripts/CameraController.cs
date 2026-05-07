using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona el control de la cámara ortográfica mediante el nuevo sistema de Input, permitiendo el desplazamiento (pan) con el botón derecho y el zoom dinámico, limitando automáticamente la visión para que nunca exceda los bordes del mapa.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Ajustes de Zoom")]
    public float zoomSpeed = 0.05f;
    public float minZoom = 2f;
    public float maxZoomSetting = 12f;

    [Header("Límites del Mapa")]
    public PolygonCollider2D mapBounds;

    private Camera cam;
    private Vector3 dragOrigin;
    private float mapMinX, mapMaxX, mapMinY, mapMaxY;
    private float absoluteMaxZoom;

    /// <summary>
    /// Inicializa la referencia de la cámara y activa el cálculo de los límites físicos del escenario si existe un colisionador de mapa asignado.
    /// </summary>
    void Start()
    {
        cam = GetComponent<Camera>();
        if (mapBounds != null)
        {
            CalculateBounds();
        }
    }

    /// <summary>
    /// Analiza las dimensiones del PolygonCollider2D para determinar las coordenadas extremas y calcula el zoom máximo físicamente posible para evitar que el área de visión sea mayor que el propio escenario.
    /// </summary>
    void CalculateBounds()
    {
        mapMinX = mapBounds.bounds.min.x;
        mapMaxX = mapBounds.bounds.max.x;
        mapMinY = mapBounds.bounds.min.y;
        mapMaxY = mapBounds.bounds.max.y;

        // Calculamos el zoom máximo físicamente posible para que la cámara
        // nunca sea más grande que el mapa.
        float mapWidth = mapMaxX - mapMinX;
        float mapHeight = mapMaxY - mapMinY;

        float maxVerticalZoom = mapHeight / 2f;
        float maxHorizontalZoom = (mapWidth / 2f) / cam.aspect;

        // El zoom real será el menor entre el ajuste de usuario y el límite físico del mapa
        absoluteMaxZoom = Mathf.Min(maxZoomSetting, maxVerticalZoom, maxHorizontalZoom);
    }

    /// <summary>
    /// Ejecuta de forma secuencial la lógica de zoom, desplazamiento y restricción de posición en el ciclo LateUpdate para garantizar una transición suave después de que los objetos se hayan movido.
    /// </summary>
    void LateUpdate()
    {
        HandleZoom();
        HandlePan();
        ConstrainCamera();
    }

    /// <summary>
    /// Intercepta la lectura del valor de la rueda del ratón para ajustar el tamaño ortográfico de la cámara, aplicando el límite máximo calculado para prevenir errores de visualización.
    /// </summary>
    void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            // Limitamos el zoom para que nunca rompa el clamping
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, absoluteMaxZoom);
        }
    }

    /// <summary>
    /// Gestiona el desplazamiento lateral de la cámara (Panning) calculando la diferencia entre el punto de origen del clic derecho y la posición actual del ratón en el mundo.
    /// </summary>
    void HandlePan()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            dragOrigin = GetMouseWorldPosition();
        }

        if (Mouse.current.rightButton.isPressed)
        {
            Vector3 currentPos = GetMouseWorldPosition();
            Vector3 direction = dragOrigin - currentPos;
            transform.position += direction;
        }
    }

    /// <summary>
    /// Restringe la posición X e Y de la cámara basándose en su tamaño actual (zoom) y aspecto, asegurando que los bordes del visor nunca sobrepasen los límites del colisionador del mapa.
    /// </summary>
    void ConstrainCamera()
    {
        if (mapBounds == null) return;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minX = mapMinX + camWidth;
        float maxX = mapMaxX - camWidth;
        float minY = mapMinY + camHeight;
        float maxY = mapMaxY - camHeight;

        // Clamping final de la posición de la cámara
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    /// <summary>
    /// Convierte la posición bidimensional del ratón en pantalla a coordenadas tridimensionales dentro del espacio del mundo del juego.
    /// </summary>
    /// <returns>La posición del cursor en el mundo de juego (Z=0).</returns>
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = 10f; 
        return cam.ScreenToWorldPoint(mousePos);
    }
}