# Bugs

Formato y niveles de gravedad: skill `potato-playtest`. Cristhian abre los bugs; el dueño del bloque los cierra; Cristhian confirma.

Gravedad: **Bloqueante** (no se puede terminar o se cuelga) · **Alta** (rompe la experiencia) · **Media** (se nota pero se puede seguir) · **Baja** (detalle).

| ID | Gravedad | Dónde | Qué pasa | Pasos | Bloque | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| B-001 | | | | | | Abierto |
| B-002 | Alta | Todo el juego | Con mando no se puede mover, mirar ni saltar: `PlayerInput.inputactions` solo tiene teclado y mouse para Move, Look y Jump (correr, agacharse, interactuar y pausa sí tienen botón) | 1. Conectar un mando 2. Jugar desde 00_Boot 3. Mover los sticks en la tienda | E | Para probar (polish/noche: stick izquierdo, stick derecho y botón sur en el asset; el stick derecho se escala por tiempo en PlayerController) |

## Comentarios de playtest

Lo que no es un bug pero importa: qué aburre, qué confunde, qué gusta.

- Comentario en itch después de la jam: el juego se sintió como un prototipo con bugs. El objetivo de este plan es cambiar esa impresión.
