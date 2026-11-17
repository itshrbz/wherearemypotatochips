# Bugs

Formato y niveles de gravedad: skill `potato-playtest`. Cristhian abre los bugs; el dueño del bloque los cierra; Cristhian confirma.

Gravedad: **Bloqueante** (no se puede terminar o se cuelga) · **Alta** (rompe la experiencia) · **Media** (se nota pero se puede seguir) · **Baja** (detalle).

| ID | Gravedad | Dónde | Qué pasa | Pasos | Bloque | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| B-001 | | | | | | Abierto |
| B-002 | Alta | Todo el juego | Con mando no se puede mover, mirar ni saltar: `PlayerInput.inputactions` solo tiene teclado y mouse para Move, Look y Jump (correr, agacharse, interactuar y pausa sí tienen botón) | 1. Conectar un mando 2. Jugar desde 00_Boot 3. Mover los sticks en la tienda | E | Para probar (polish/noche: stick izquierdo, stick derecho y botón sur en el asset; el stick derecho se escala por tiempo en PlayerController) |
| B-003 | Baja | Tienda y réplicas | Compradora llamada Old Man; chocolate como Gum, caramelo y cupcake como Soda | Atender intro y tres réplicas | E | Para probar (polish/correcciones-tiendas-mapas) |
| B-004 | Media | 02_Store | Dinteles superpuestos que parpadean sobre la puerta de almacén | Mirar la puerta moviéndose frente al marco | E | Para probar (polish/correcciones-tiendas-mapas) |
| B-005 | Alta | Réplicas | Comprador dentro del mostrador, paso posterior estrecho y mobiliario básico | Entrar a una réplica e intentar llegar a la caja de pie | B/E | Para probar (polish/correcciones-tiendas-mapas) |
| B-006 | Alta | Pools / Warehouse | Pared norte desaparece al colocar réplica; se puede subir al techo; superficies de réplicas coincidentes | Recorrer pared final de Pools, saltar en parkour de Warehouse y revisar suelos de las tres tiendas | B/E | Para probar (polish/correcciones-tiendas-mapas) |
| B-007 | Media | Warehouse / Pools | Rectángulos claros de humedad y calcomanías planas sobresaliendo de pilares | Mirar gotas de lado y rodear pilares con humedad | B | Para probar (polish/correcciones-tiendas-mapas) |
| B-009 | Media | Jefe / navegación | Aviso al instanciar agente: Failed to create agent because it is not close enough to the NavMesh; observado antes y después de hornear, sin bloquear ventas ni transiciones | Iniciar un espacio con jefe y revisar consola; comprobar persecución manual | C | Pendiente para Grover; no se cambió el jefe en esta corrección |

## Comentarios de playtest

Lo que no es un bug pero importa: qué aburre, qué confunde, qué gusta.

- Comentario en itch después de la jam: el juego se sintió como un prototipo con bugs. El objetivo de este plan es cambiar esa impresión.
