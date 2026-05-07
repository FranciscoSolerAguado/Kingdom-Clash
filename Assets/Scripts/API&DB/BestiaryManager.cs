using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Representa la estructura de datos de un enemigo, mapeada directamente desde la respuesta JSON proporcionada por la API.
/// </summary>
[System.Serializable]
public class Enemigo { public int id; public string nombre; public int vida; public int dano; public string descripcion; }

/// <summary>
/// Contenedor serializable diseñado para facilitar el parseo del arreglo de datos recibido desde el endpoint de la API.
/// </summary>
[System.Serializable]
public class RespuestaAPI { public List<Enemigo> data; }

/// <summary>
/// Gestiona la conexión asíncrona con la API REST del bestiario, deserializa la respuesta JSON y coordina la construcción y actualización dinámica de la interfaz de usuario.
/// </summary>
public class BestiaryManager : MonoBehaviour
{
    [Header("Configuración de la API")]
    public string apiUrl = "https://kingdomclashapi.onrender.com/enemigos";
    public string swaggerUrl = "https://kingdomclashapi.onrender.com/api-docs";
    private List<Enemigo> listaEnemigos = new List<Enemigo>();

    [Header("Recursos Locales (Imágenes)")]
    [Tooltip("Iconos pequeños recortados para los botones cuadrados de la lista.")]
    public Sprite[] iconosEnemigos; 
    
    [Tooltip("Imágenes completas/grandes para mostrar en la ficha derecha.")]
    public Sprite[] imagenesDetalleEnemigos; 

    [Header("Referencias de UI: Lista Izquierda")]
    public Transform contenedorBotones; 
    public GameObject prefabBotonEnemigo;

    [Header("Referencias de UI: Ficha Derecha")]
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoDano;
    public TextMeshProUGUI textoLore;
    public Image imagenDetalle; 
    
    [Header("Paneles")]
    [Tooltip("El panel principal del Bestiario que se activa/desactiva.")]
    public GameObject bestiaryPanel; 

    /// <summary>
    /// Inicializa el estado del gestor asegurando que el panel del bestiario se encuentre oculto al iniciar la ejecución.
    /// </summary>
    void Start()
    {
        if (bestiaryPanel != null) bestiaryPanel.SetActive(false); 
    }

    /// <summary>
    /// Activa visualmente la interfaz del bestiario y desencadena la corrutina encargada de solicitar los datos actualizados al servidor backend.
    /// </summary>
    public void AbrirBestiario()
    {
        if (bestiaryPanel != null)
        {
            bestiaryPanel.SetActive(true);
            StartCoroutine(DescargarBestiario());
        }
    }

    /// <summary>
    /// Desactiva y oculta el panel principal del bestiario de la pantalla del jugador.
    /// </summary>
    public void CerrarBestiario()
    {
        if (bestiaryPanel != null)
        {
            bestiaryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Ejecuta una llamada al sistema operativo para abrir el navegador web predeterminado en la URL que aloja la documentación Swagger de la API.
    /// </summary>
    public void AbrirSwagger()
    {
        Debug.Log("Abriendo documentación en el navegador...");
        Application.OpenURL(swaggerUrl);
    }

    /// <summary>
    /// Corrutina que realiza una petición HTTP GET a la API REST, gestiona la espera de la respuesta de red, comprueba si hay errores y procesa el texto JSON resultante.
    /// </summary>
    private IEnumerator DescargarBestiario()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                RespuestaAPI respuesta = JsonUtility.FromJson<RespuestaAPI>(webRequest.downloadHandler.text);
                listaEnemigos = respuesta.data;
                CrearListaVisual();
            }
            else
            {
                Debug.LogError("No se pudo conectar con la API. Asegúrate de que el servidor Node.js esté corriendo.");
            }
        }
    }

    /// <summary>
    /// Limpia el contenedor de la interfaz y genera instancias del prefab de botón por cada enemigo recuperado, asignando el icono correspondiente y vinculando el evento de clic a la ficha de detalles.
    /// </summary>
    private void CrearListaVisual()
    {
        foreach (Transform child in contenedorBotones) { Destroy(child.gameObject); }

        for (int i = 0; i < listaEnemigos.Count; i++)
        {
            Enemigo enemigo = listaEnemigos[i];
            int indiceGuardado = i; 

            GameObject nuevoBoton = Instantiate(prefabBotonEnemigo, contenedorBotones);
            
            Transform objetoImagenHija = nuevoBoton.transform.Find("Image");
            
            // Asignamos el ICONO PEQUEÑO al botón de la lista
            if (objetoImagenHija != null && indiceGuardado < iconosEnemigos.Length)
            {
                Image icono = objetoImagenHija.GetComponent<Image>();
                icono.sprite = iconosEnemigos[indiceGuardado];
                icono.color = Color.white; 
            }

            Button componenteBoton = nuevoBoton.GetComponent<Button>();
            componenteBoton.onClick.AddListener(() => MostrarDetalles(enemigo, indiceGuardado));
        }

        if (listaEnemigos.Count > 0) MostrarDetalles(listaEnemigos[0], 0);
    }

    /// <summary>
    /// Vuelca la información de texto del servidor y los recursos gráficos locales sobre el panel lateral derecho para visualizar las estadísticas y el lore completo de un enemigo específico.
    /// </summary>
    /// <param name="enemigoSeleccionado">El objeto de datos del enemigo a visualizar.</param>
    /// <param name="indiceDeImagen">El índice numérico para localizar la imagen de arte a tamaño completo en el array local.</param>
    public void MostrarDetalles(Enemigo enemigoSeleccionado, int indiceDeImagen)
    {
        if (textoNombre != null) textoNombre.text = enemigoSeleccionado.nombre;
        if (textoVida != null) textoVida.text = "Vida: " + enemigoSeleccionado.vida;
        if (textoDano != null) textoDano.text = "Daño: " + enemigoSeleccionado.dano;
        if (textoLore != null) textoLore.text = enemigoSeleccionado.descripcion;

        // Asignamos la IMAGEN GRANDE a la ficha de detalles
        if (imagenDetalle != null && indiceDeImagen < imagenesDetalleEnemigos.Length)
        {
            imagenDetalle.sprite = imagenesDetalleEnemigos[indiceDeImagen];
            imagenDetalle.color = Color.white;
        }
        else if (imagenDetalle != null)
        {
            // Ocultamos el cuadro si por algún casual olvidamos poner la imagen en el array
            imagenDetalle.color = Color.clear; 
        }
    }
}