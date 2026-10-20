# 🐟 Fish or Die

**"Pesca, sobrevive o conviértete en la carnada."**

Party game local de 2 a 6 jugadores en pantalla compartida, hecho en Unity 2D. Es un torneo de pesca en las costas del río Paraná, en Rosario: cada ronda, quien pesca menos es arrastrado al fondo por el Monstruo del Paraná. Queda un solo pescador en pie.

Cada jugador usa **una sola tecla**, y esa misma tecla sirve para tirar la caña y para el minijuego de captura.

## De un vistazo
- **Mi rol:** programación del juego. Proyecto de una gamelab.
- **Alcance:** 35 scripts de C# (unas 4.900 líneas) organizados por responsabilidad, en una única escena que maneja menú, lobby, rondas y victoria con una máquina de estados.
- **Para jugar en grupo:** lobby "Drop-In" (cada persona se une presionando su tecla), remapeo interactivo de teclas con detección de conflictos y soporte anti-ghosting con el Input System nuevo.
- **Game feel:** squash & stretch, vibración de caña, contadores que "ruedan", reloj de pánico, transiciones y música dinámica por stems que sube la tensión al final de la ronda.
- **Diseño justo:** un sistema de profundidad hace que los peces emerjan frente al carril de su jugador objetivo, así nadie tiene ventaja por la posición en pantalla.
- **Basado en datos:** los peces se definen con ScriptableObjects (probabilidad, peso, valor por kilo y dificultad).

## Cómo se juega
1. En el menú, entrá al lobby. Cada persona presiona la tecla que quiere usar y se une. Por defecto las teclas son A, S, D, F, G y H, y se pueden cambiar desde las opciones.
2. Cuando un pez pasa frente a tu carril, presioná tu tecla para engancharlo.
3. Se activa un skill check: un anillo se expande y tenés que presionar tu tecla justo cuando coincide con la zona verde.
4. Cada pez vale distinto (Mojarra, Vieja del Agua, Dorado). Al terminar la ronda, el que menos dinero juntó cae al agua.
5. Gana la última persona en pie.

## Tecnología
- Unity 6 (6000.3), URP 2D, Input System nuevo y TextMeshPro.
- Patrones: Singleton (`GameManager`, `AudioManager`), máquina de estados, eventos C# para desacoplar la UI del juego y Object Pooling para el audio.
- Persistencia de opciones y jugadores con una clase estática `MatchSettings` sobre `PlayerPrefs`.

## Documentación
El diseño completo (reglas, peces, game feel, arquitectura, input y audio) está en [`docs/DISENO_Y_ARQUITECTURA.md`](docs/DISENO_Y_ARQUITECTURA.md).

## Probarlo en el editor
1. Abrir el proyecto con Unity 6000.3 (ver `ProjectSettings/ProjectVersion.txt`).
2. Abrir la escena `Assets/_Project/Scenes/FishOrDie.unity` y darle Play. Todo el flujo ocurre en esa escena.

## Créditos
Programación: Tamara D'Angelo ([@Tamara1001](https://github.com/Tamara1001)).

## Licencias
- El **código** está bajo licencia MIT. Ver [`LICENSE`](LICENSE).
- El **arte, el audio y las fuentes** pertenecen a sus autoras y autores, se incluyen con su permiso y no están cubiertos por la licencia MIT.
