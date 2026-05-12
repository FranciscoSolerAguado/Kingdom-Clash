# 🛡️ Kingdom Clash - Unity Project
![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-7.3-purple?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-PC-blue?style=for-the-badge)

<img width="256" height="256" alt="Avatars_01" src="https://github.com/user-attachments/assets/9b86cb91-0a89-400f-b777-f630e57b5655" />

Este es un videojuego de estrategia del género **Tower Defense** desarrollado en **Unity 2D**. El proyecto implementa una arquitectura sólida para la gestión de combate, economía y progresión, combinando mecánicas clásicas con sistemas de despliegue táctico de unidades.

## 🚀 Características Principales

### ⚔️ Sistema de Combate y Torres
* **Defensa Multi-clase:** Gestión de torres de arqueros, magos y barracas, cada una con lógicas de ataque diferenciadas.
<p>
  <img width="130" height="150" src="https://github.com/user-attachments/assets/db6ea83c-c701-4c91-ab13-d7add80e57c5" />
  <br>
  <small><i>Torre de barracas: despliega tres guerreros que se colocan en el camino y luchan cuerpo a cuerpo.</i></small>
</p>

<p>
  <img width="130" height="150" src="https://github.com/user-attachments/assets/ba016138-2bb0-4c4d-bec1-7f7d55f89a74" />
  <br>
  <small><i>Torre de arquero: despliega una torre fija que los enemigos no pueden dañar y dispara a distancia.</i></small>
</p>

<p>
  <img width="130" height="150" src="https://github.com/user-attachments/assets/c92f0666-6f71-4f95-a335-0a3742f2532e" />
  <br>
  <small><i>Torre de arqueros fija: despliega dos arqueros en el camino que atacan a distancia.</i></small>
</p>

<p>
  <img width="130" height="150" src="https://github.com/user-attachments/assets/94e4fd6e-1057-4cb9-bfbf-efc057a874cb" />
  <br>
  <small><i>Torre del mago: dispara proyectiles mágicos a distancia, similar a la torre de arquero.</i></small>
</p>


  
* **Progresión Dinámica:** Sistema de mejoras (`TowerUpgrade`) que escala estadísticas de daño, rango y salud de forma proporcional.
    <img width="160" height="180" alt="Captura de pantalla 2026-04-23 094021" src="https://github.com/user-attachments/assets/82768407-74a9-4f5e-be87-3e55513a6827" />
    <small><i>Este es el menu de la torre barracas, el boton de arriba permite mejorar la torre, la bandera de abajo a la izquieda permite mover las unidades dentro del rango que tiene la torre, y el boton del dolar la vende y la destruye, dejando el hueco vacío para construir otro.</i></small>
<br>
    
* **Sincronización de Impacto:** Uso de eventos de animación (`AnimationEvents`) para garantizar que el daño se aplique en el frame exacto del contacto visual.
  <img width="187" height="180" alt="Captura de pantalla 2026-04-23 102335" src="https://github.com/user-attachments/assets/19010f30-5fdd-438f-9719-f588704bf64d" />
<br>

### 🎖️ Despliegue Táctico
* **Ghost Preview:** Previsualización semitransparente de unidades antes del despliegue para una colocación precisa.
  <img width="468" height="115" alt="Captura de pantalla 2026-04-23 104647" src="https://github.com/user-attachments/assets/772ac592-e47e-4bb4-b738-15a3b6b59aee" />
  <small><i>Este es el selector de unidades, para desplegarlas en el camino.<i/><small/>
  <img width="122" height="92" alt="Captura de pantalla 2026-04-23 102839" src="https://github.com/user-attachments/assets/7c120949-5113-4d67-88e1-9157241511f0" />
  
  <img width="122" height="92" alt="Captura de pantalla 2026-04-23 102919" src="https://github.com/user-attachments/assets/9b405bb1-0ae9-4990-9cfb-13b1bd943ed7" />
  <small><i>Aqui lo que se quiere referenciar es que cuando el jugador quiere desplegar una unidad, debe hacer en un lugar apropiado (el path) y no fuera de este.<i/><small/>
  
  <br>
* **Formaciones Automáticas:** Algoritmos matemáticos para posicionar escuadrones en formaciones circulares dinámicas al ser invocados.
  <img width="156" height="112" alt="Captura de pantalla 2026-04-23 103255" src="https://github.com/user-attachments/assets/dc8f0a38-6a6c-4b74-8772-2002100f796b" />
  <br>
* **Validación de Terreno:** Sistema de verificación de proximidad a caminos (`Path`) y detección de colisiones con estructuras existentes.
  <img width="578" height="222" alt="Captura de pantalla 2026-04-23 105412" src="https://github.com/user-attachments/assets/bf922f4b-24a8-4aaa-80cf-92d13f4550a4" />

  <img width="578" height="222" alt="Captura de pantalla 2026-04-23 103134" src="https://github.com/user-attachments/assets/e84e526b-db12-4ca4-af9e-2450890fdd15" />
    <small><i>El relieve azul sería la superficie caminable para las unidades y los enemigos, aunque estos realmente se mueven por el camino del medio (en la primera imagen el camino verde), lo demas son colisiones que no pueden atravesar.<i/><small/>

  <br>
### 💰 Economía y Progresión
* **Minería Activa:** Sistema de recolección de oro mediante unidades mineras con límites de población por nivel.
  <img width="240" height="162" alt="Captura de pantalla 2026-04-23 103544" src="https://github.com/user-attachments/assets/f03b1e63-29f7-405b-8677-b8206b618bad" />
  
  <img width="252" height="103" alt="Captura de pantalla 2026-04-23 103601" src="https://github.com/user-attachments/assets/ba1fbd64-fe77-4b6a-bc12-9504f99206df" />
  <small><i>Esto que se ve aquí es el menu informativo dentro de un nivel este muestra el oro, vidas, oleada actual y la cantidad de mineros que hay activos, en este nivel en concreto solo se permite contratar hasta 2, en otros 1 o hasta 3.<i/><small/>
  <br>
* **Save System:** Persistencia de datos local para el guardado de estrellas obtenidas y desbloqueo de niveles.
  <img width="267" height="230" alt="Captura de pantalla 2026-04-23 103730" src="https://github.com/user-attachments/assets/37306c08-f118-4339-bf46-282de2408e2a" />
    <small><i>En esta parte se puede comprobar que se puede guardar hasta tres partidas diferentes, que te muestran por que nivel vas, las estrellas totales conseguidas y también permite borrar las partidas.<i/><small/>
    
  <img width="1292" height="633" alt="Captura de pantalla 2026-04-23 103748" src="https://github.com/user-attachments/assets/81fe0fad-0717-4090-bc4c-e58d9707f28b" />
      <small><i>Este es el selector de niveles del juego, como se puede ver en la imagen, en esta partida se han completado los dos primeros niveles con 3 estrellas y el tercero esta disponible para jugarlo. También se pueden ver los botones del bestiario y el enlace externo a la API en swagger que es de donde se cogen los datos para mostrarlos en el bestiario.<i/><small/>
<br>

* **Dificultad Escalable:** Configuración individual por nivel (`LevelUpgradeSettings`) para limitar el poder máximo de las torres.
        <small><i>Esto basicamente es una opción que puedo configurar yo desde el editor en Unity, esta me permite configurar cuantas mejoras de nivel permite cada clase de torre en ese nivel especifico, por ejemplo en el primer nivel no se puede mejorar ninguna torre, y en el segundo nivel se le permite hacer una subida de nivel a cada clase de torre, es decir, se pueden subir hasta nivel 2, asi progresivamente se consigue que cada nivel sea más difícil pero a la vez el jugador también pueda progresar.<i/><small/>
  
### 🌐 Integración con API Externa (Bestiario)
El proyecto incluye un sistema de **Bestiario Dinámico** que consume datos de una API REST externa desarrollada en **Node.js**. Esto permite gestionar las estadísticas y el lore de los enemigos de forma centralizada.

* **Backend:** Servidor en Node.js utilizando `Express` para el enrutamiento.
* **Base de Datos:** Persistencia mediante `SQLite3` para el almacenamiento de perfiles de enemigos (vida, daño, descripción).
* **Documentación:** Implementación de `Swagger` para la visualización y prueba interactiva de los endpoints.
* **Consumo en Unity:** Uso de `UnityWebRequest` y corrutinas para la descarga asíncrona de datos JSON y su posterior parseo a objetos de C#.

<img width="561" height="307" alt="Captura de pantalla 2026-04-23 112336" src="https://github.com/user-attachments/assets/d38da3a1-32fe-414b-98e9-aa47180e2112" />

<small><i>Interfaz de Swagger UI donde se exponen los datos del bestiario que consume el cliente de Unity.</i></small>

<img width="1482" height="822" alt="Captura de pantalla 2026-04-23 112531" src="https://github.com/user-attachments/assets/47ce2729-974a-44d8-8904-4f0abccf1390" />

<small><i>El bestiario abierto en el juego, trayendose los datos directamente de la API.</i></small>
<br>
## 🛠️ Arquitectura Técnica

El proyecto destaca por una implementación limpia y modular:

* **Pattern Singleton:** Utilizado en gestores críticos como `GameManager`, `BuildManager` y `DeployManager` para asegurar un acceso global y eficiente.
* **Y-Sorting System:** Algoritmo de ordenación de sprites en tiempo real basado en la posición `Y` para una profundidad visual perfecta en perspectiva Top-Down.
* **UI Reactiva:** Uso de `TextMeshPro` y cursores contextuales que cambian según la disponibilidad de oro o tiempos de enfriamiento (*cooldowns*).
* **Optimización de Cámara:** Controlador de cámara con *clamping* inteligente basado en los límites físicos del mapa (`PolygonCollider2D`).

## 🎮 Instrucciones de Juego

1.  **Defiende:** Construye torres en los slots permitidos para frenar las oleadas.
2.  **Refuerza:** Despliega tropas aliadas en el camino para bloquear el avance de los enemigos más fuertes.
3.  **Gestiona:** Invierte en mineros para asegurar un flujo constante de oro.
4.  **Evoluciona:** Mejora tus torres estratégicamente según el tipo de enemigos de la oleada actual.

---

## 📂 Estructura de Scripts Destacados

| Script | Función Principal |
| :--- | :--- |
| `GameManager` | Cerebro del juego: vidas, oro, victoria/derrota y pausa. |
| `DeployManager` | Lógica de previsualización e instanciación de tropas. |
| `YSorter` | Control automático de profundidad y capas de renderizado. |
| `TooltipManager` | Interfaz informativa dinámica que sigue al cursor. |
| `CameraController` | Navegación y zoom restringido a los límites del nivel. |
| `BestiaryManager` | Conexión asíncrona con la API REST, parseo JSON y renderizado dinámico de la UI del Bestiario. |


---
