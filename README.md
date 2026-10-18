# Where Are My Potato Chips?

Juego de terror liminal en primera persona hecho en 48 horas para la **GameJam+ 2026**, con el tema **"Something's Off"**.

Eres el dueño de una tienda de barrio. Un cliente te pide papas fritas que no tienes y vas a buscarlas al almacén. Pero detrás de la puerta no está tu almacén: hay pasillos que no terminan, piscinas vacías y un parque de juegos sin niños. Cada espacio termina en una copia de tu tienda donde alguien espera para pagar. Y algo te sigue.

## Cómo jugar

| Acción | Teclado y mouse | Mando |
| --- | --- | --- |
| Moverse | WASD | Stick izquierdo |
| Mirar | Mouse | Stick derecho |
| Correr | Shift (gasta energía) | |
| Agacharse / esconderse | Ctrl o C | Botón Este |
| Saltar | Espacio | |
| Interactuar | E | Botón Oeste |
| Pausa | Esc | Start |

- Atiende a los clientes: escanea el producto y cóbralo en la caja.
- Para esconderte, agáchate dentro de un escondite.
- Si el jefe te atrapa, el espacio vuelve a empezar.
- La sensibilidad del mouse se cambia en la pausa.

## Equipo

| Nombre | Rol |
| --- | --- |
| Grover Amador | Programación: IA del jefe y controlador del jugador |
| Rafael Taborga | Programación: flujo del juego, ventas, interfaz y builds |
| Alan Herbas | Diseño: jefe procedural, compradores, audio y nivel de Piscinas |
| Cristhian Bullain | Diseño: kit modular, tienda réplica, interfaz, Almacén y Parque |

Créditos completos en [CREDITS.md](CREDITS.md).

## Abrir el proyecto

1. Unity **6000.5.5f1** (Unity 6.5) con Universal Render Pipeline.
2. Clonar el repositorio y abrir la carpeta `Where_are_my_potato_chips` desde Unity Hub.
3. Abrir `Assets/00_Scenes/00_Boot` y dar Play.

En el editor: **F2** salta al siguiente espacio y **F3** reinicia el actual.

## Estructura

| Carpeta | Contenido |
| --- | --- |
| `00_Scenes` | Boot, menú, tienda y los tres espacios liminales |
| `01_Scripts` | Código del juego (Flow, Player, Enemy, Sale, Interaction, UI, Audio) |
| `02_Prefabs` | Jugador, interfaz, kit modular y tienda réplica |
| `07_data` | Ventas de cada comprador (SaleVariant) |
| `Resources` | `GameFlowSettings`: escenas, prefabs, mensajes del ticket y créditos |

## Herramientas

Unity 6.5, Universal Render Pipeline, Input System, AI Navigation, Animation Rigging, ProBuilder y TextMesh Pro.
Parte del código se escribió con ayuda de Claude (Anthropic).
