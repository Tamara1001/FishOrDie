# 🐟 Fish or Die
**"Pesca, sobrevive o conviértete en la carnada."**

**Género:** Party Game / Supervivencia Local Multijugador  
**Jugadores:** 2 a 6 Jugadores (Pantalla Compartida)  
**Input:** Low Input (Un solo botón por jugador)  
**Motor:** Unity 2D  

---

## 📖 1. Visión General y Lore
**Fish or Die** es un frenético torneo de pesca ambientado en las costas del río Paraná, en Rosario. Hasta 6 pescadores se inscriben atraídos por un premio millonario en efectivo. Lo que las letras chicas del contrato no mencionan es que el torneo es, en realidad, un ritual de ofrenda para el mítico **Monstruo del Paraná**. 

El objetivo es sencillo: atrapar la mayor cantidad de peces para acumular dinero. Al final de cada ronda (cuando el temporizador llega a cero), el jugador con la puntuación más baja es arrastrado a las profundidades por el monstruo. Los sobrevivientes avanzan a la siguiente ronda hasta que solo quede un pescador en pie, quien se llevará el dinero y su vida.

---

## 🎮 2. Mecánicas Principales de Juego (Core Gameplay)

### 2.1. Controles "Low Input"
El juego está diseñado para el caos multijugador en un solo teclado o dispositivo. Cada jugador tiene asignada **una única tecla**. Esta tecla sirve tanto para iniciar el intento de pesca como para ejecutar el minijuego de captura.

### 2.2. Skill Check de Precisión
Cuando un pez se acerca a la línea de un jugador, este debe presionar su botón para engancharlo. Al hacerlo, se activa un minijuego rítmico (Skill Check).
* Consiste en un anillo expansivo (o indicador visual) que el jugador debe detener exactamente cuando se superpone con la "zona verde" de éxito.
* A diferencia de otros juegos (como Stardew Valley), no se trata de mantener presionado, sino de reflejos y *timing* puro.

### 2.3. Sistema de Eliminación (Escalabilidad de Rondas)
El juego escala dinámicamente entre 2 y 6 jugadores.
* Al agotarse el temporizador de la ronda, el juego evalúa los puntajes.
* El jugador con menor recaudación es eliminado de la partida.
* Los contadores se reinician y los sobrevivientes disputan la siguiente ronda, culminando en un tenso duelo 1v1.

### 2.4. Aparición Justa: "El Sistema de Profundidad"
En juegos de pantalla compartida, los jugadores de los extremos suelen tener ventaja al ver primero lo que entra a la pantalla. Para solucionarlo, se implementó un sistema de profundidad y carriles:
* Los peces se generan en lo profundo del río (oscuros, diminutos e in-capturables).
* El sistema asigna en secreto un "Jugador Objetivo" a cada pez.
* Solo cuando el pez pasa exactamente frente al carril de ese jugador, emerge a la superficie (brillante, escala 1:1, capturable).
* Esto garantiza que las oportunidades de pesca sean matemáticamente equitativas sin importar la posición del jugador en el muelle.

---

## 🐟 3. Contenido del Juego

### Los Peces (Recompensas Generadas Proceduralmente)
El tamaño, peso y valor final de cada pez no es estático, sino que se calcula de forma procedural al momento de pescarlo, sumando un factor sorpresa.

1. **Mojarra (Común - 60%)**
   * **Tamaño:** 5 - 15 cm | **Peso:** ~1 kg | **Valor:** $5/kg
   * **Dificultad:** Muy Fácil.
   * **Rol:** Pez de farmeo o rescate. Otorga un flujo bajo pero constante de puntos para evitar quedar último.
2. **Vieja del Agua (Poco Frecuente - 30%)**
   * **Tamaño:** 30 - 60 cm | **Peso:** 2 - 5 kg | **Valor:** $8/kg
   * **Dificultad:** Media.
   * **Rol:** Escalabilidad moderada de puntos.
3. **Dorado (Raro - 10%)**
   * **Tamaño:** 50 - 100 cm | **Peso:** 5 - 20 kg | **Valor:** $25/kg
   * **Dificultad:** Muy Difícil (Área de éxito minúscula y rápida).
   * **Rol:** El *Jackpot*. Su alto valor multiplicador puede dar vuelta la partida por completo, generando un momento de máxima tensión.

### El Enemigo
* **El Monstruo del Paraná:** Funciona como el ejecutor de las rondas. Aparece al final del temporizador (como un tentáculo o una sombra en el agua) exclusivamente para arrastrar al perdedor.

---

## 🍎 4. Game Feel (Juice) y Experiencia de Usuario (UX)

Para que el juego se sienta como un producto pulido y visceral, se programaron múltiples sistemas matemáticos de *feedback* visual (comúnmente llamado *Juice*), sin depender de animadores complejos:

### 4.1. Impacto Físico y Tensión
* **Squash & Stretch (Respuesta Táctil):** Cada vez que un jugador presiona su tecla, su avatar se "achata" y rebota orgánicamente en 0.15s, confirmando visualmente el input.
* **Vibración de Pesca:** Al enganchar un pez y abrir el minijuego, el pescador vibra suavemente. Si el jugador presiona activamente la tecla para forcejear con el pez, el temblor se multiplica x4, transmitiendo el peso y la tensión de la caña.
* **Eliminación Visceral:** Al ser devorado por el Monstruo del Paraná, el jugador no desaparece de forma aburrida. Su avatar gira sin control a 720 grados y es jalado violentamente hacia el fondo del agua.
* **Rastro de Velocidad (Ghost Trails):** Los peces con dificultad extrema (como el Dorado) dejan un rastro procedural de clones semitransparentes a su paso que se desvanecen independientemente, enfatizando su rareza y velocidad.

### 4.2. Interfaz Viva (UI Juice)
* **Score Rolling (Contadores Dinámicos):** El dinero no se actualiza instantáneamente. Cuando se atrapa un pez, el puntaje "rueda" velozmente hacia la nueva cifra, el número hace un efecto *Pop* (agranda su escala) y el fondo del panel emite un destello fotográfico blanco.
* **Reloj de Pánico:** Durante los últimos 5 segundos de la ronda, el temporizador global se tiñe de rojo y comienza a latir (pulse) agresivamente interpolando con los milisegundos restantes.
* **Transiciones Fluidas (Screen Fader):** Un Singleton maneja fundidos a negro matemáticos independientes del tiempo (`Time.unscaledDeltaTime`). Cada inicio de ronda, eliminación o carga de escena está acolchonada por suaves *Fade In* y *Fade Out*.
* **Feedback Flotante en Mundo (World Space):** Al pescar, íconos de ¡Atrapado! o ¡Escapó! flotan desde el personaje hacia el cielo, tintados de verde o rojo, pero con un borde del color de ese jugador en particular. Del mismo modo, el puntaje obtenido (+15) flota como un elemento 3D libre de la UI.

---

## ⚙️ 5. Arquitectura Técnica y Código

El proyecto fue estructurado por y para programadores con un enfoque en **Patrones de Diseño** y código modular.

### 5.1. Máquina de Estados (Game Flow)
El flujo es orquestado por un `GameManager` (Singleton, `DontDestroyOnLoad`) que administra una FSM (Finite State Machine).
* **Estados:** `MainMenu`, `Playing`, `RoundTransition`, `Paused`, `Victory`.
* El `GameManager` no interactúa con entidades del nivel. La lógica de pesca y eliminación la maneja el `RoundManager`, un script local a la escena del juego. Esto garantiza **Separación de Responsabilidades** e impide *Spaghetti Code*.

### 5.2. UI Desacoplada (Event-Driven / MVC)
Los scripts del mundo (`PlayerController`) no tienen referencias a `Canvas`, `Text` o elementos de UI. 
* Cuando ocurre un evento (Ej: Pez atrapado), el `RoundManager` emite un evento C# estático (`OnFishCatchDetails`). 
* El script `RoundHUD` está suscrito a este evento y delega la actualización visual al slot correspondiente. La UI se puede apagar o borrar sin romper el código del juego.

### 5.3. Sistema de Generación (RiverSpawner)
Controla el flujo de los peces usando Corrutinas. Lee los porcentajes de rareza del ecosistema, instancia a los peces fuera de cámara, y les asigna su `targetX` dinámico leyendo la lista actual de jugadores vivos del `RoundManager`.

---

## ⌨️ 6. Input System y Configuración

### Solución de Hardware (Anti-Ghosting)
Para que 6 personas puedan jugar en un mismo teclado sin sufrir bloqueos físicos del hardware (*Keyboard Ghosting*), se implementó el **New Input System** de Unity.
* El juego permite remapeo total interactivo (Re-binding) desde el menú de opciones utilizando `PerformInteractiveRebinding()`.
* Los jugadores pueden asignar teclas extendidas (Numpad, F13-F24, botones de Stream Deck o Gamepads auxiliares).

### Persistencia (`MatchSettings`)
Las opciones de la partida no viajan entre escenas colgando de GameObjects pesados. Se estructuró un script estático `MatchSettings` respaldado por `PlayerPrefs`. Guarda: Cantidad de jugadores, Nombres, Colores y el *Binding Path* (Ruta de la tecla) elegido. El `PlayerSpawner` lee esta clase estática al iniciar la ronda para configurar los personajes dinámicamente.

---

## 📦 7. Data Management (Scriptable Objects)
Los datos de los peces están encapsulados en `FishData` (Scriptable Objects). En lugar de quemar variables en los scripts (Hardcoding), los diseñadores del juego pueden crear nuevos peces haciendo clic derecho en Unity y ajustando las curvas de probabilidad, pesos mínimos/máximos y valor por kg desde el Inspector.

## 🛠️ 8. Estructura del Proyecto y Reglas de Prefabs
* **Anidamiento visual:** Todos los componentes estéticos (Ej: El dibujo del pescador) son hijos de un objeto padre vacío. Esto permite hacer un `flipX` (voltear al personaje para que mire al centro) rotando o escalando negativamente el hijo sin invertir los colliders o canvas de la física del padre.
* **Textos en Mundo:** Los popups de daño o puntos **nunca** utilizan Canvas anidados con `Scale With Screen Size`. Se utilizan componentes nativos de `TextMeshPro` 3D (World Space) instanciados en tiempo de ejecución para evitar fallos de resolución.

---

## 🚀 9. Guía para Desarrolladores (Puesta en Marcha)

1. **Dependencias:** Unity 2022 LTS (o superior) y el paquete *Input System* (v1.7+).
2. **Escena de Entrada:** Para correr el juego completo, abrí `Scenes/MainMenu`. Esto inicializa el `MatchSettings` y carga los periféricos.
3. **Pruebas Rápidas:** El código está fortificado. Si le das *Play* directamente en la escena `Scenes/Gameplay`, el juego detectará la falta del `GameManager` y autogenerará un flujo estándar seguro para que puedas debugear mecánicas de pesca sin pasar por el menú cada vez.
