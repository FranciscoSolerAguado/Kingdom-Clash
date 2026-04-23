# 🛡️ Kingdom Clash - Unity Project
![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-7.3-purple?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Mobile-blue?style=for-the-badge)
<img width="256" height="256" alt="Avatars_01" src="https://github.com/user-attachments/assets/9b86cb91-0a89-400f-b777-f630e57b5655" />

Este es un videojuego de estrategia del género **Tower Defense** desarrollado en **Unity 2D**. El proyecto implementa una arquitectura sólida para la gestión de combate, economía y progresión, combinando mecánicas clásicas con sistemas de despliegue táctico de unidades.

## 🚀 Características Principales

### ⚔️ Sistema de Combate y Torres
* **Defensa Multi-clase:** Gestión de torres de arqueros, magos y barracas, cada una con lógicas de ataque diferenciadas.
* **Progresión Dinámica:** Sistema de mejoras (`TowerUpgrade`) que escala estadísticas de daño, rango y salud de forma proporcional.
* **Sincronización de Impacto:** Uso de eventos de animación (`AnimationEvents`) para garantizar que el daño se aplique en el frame exacto del contacto visual.

### 🎖️ Despliegue Táctico
* **Ghost Preview:** Previsualización semitransparente de unidades antes del despliegue para una colocación precisa.
* **Formaciones Automáticas:** Algoritmos matemáticos para posicionar escuadrones en formaciones circulares dinámicas al ser invocados.
* **Validación de Terreno:** Sistema de verificación de proximidad a caminos (`Path`) y detección de colisiones con estructuras existentes.

### 💰 Economía y Progresión
* **Minería Activa:** Sistema de recolección de oro mediante unidades mineras con límites de población por nivel.
* **Save System:** Persistencia de datos local para el guardado de estrellas obtenidas y desbloqueo de niveles.
* **Dificultad Escalable:** Configuración individual por nivel (`LevelUpgradeSettings`) para limitar el poder máximo de las torres.

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

---
