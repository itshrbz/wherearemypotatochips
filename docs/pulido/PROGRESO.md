# Progreso

Estados: **Pendiente** → **En curso** → **Para probar** → **Hecha**. Solo Cristhian marca "Hecha". Las IAs actualizan esta tabla al empezar y al terminar cada tarea, y agregan la checklist de prueba al final del archivo.

| ID | Tarea | Persona | Fase | Depende de | Estado | Rama | Notas |
| --- | --- | --- | --- | --- | --- | --- | --- |
| S1 | Subir el setup del equipo | Rafael | 0 | — | Pendiente | polish/setup | |
| S2 | Instalar Claude Code / Codex y plugins | Grover, Alan | 0 | S1 | Pendiente | — | |
| T0 | Instalar y practicar ramas | Cristhian | 0 | S1 | Pendiente | — | |
| T1 | Playtest base y BUGS.md inicial | Cristhian | 0 | T0 | Pendiente | main | |
| A1 | Auditoría visual | Rafael | 1 | S1 | Para probar | polish/look | Informe en `notas/A1_auditoria_visual.md`. Propuesta aplicada sin esperar aprobación (pedido de Rafael): revisar sección 3 |
| A2 | Look de cámara vieja | Rafael | 1 | A1 | Para probar | polish/look | `Look_Global` + `RetroLook`. Falta medir fluidez en build WebGL |
| A3 | Color de cada espacio | Rafael | 1 | A2 | Para probar | polish/look | Presets nuevos en `AmbienteTools` (archivo de Grover, tocado por pedido de A3). Solo RenderSettings y Volumes en sus escenas |
| A4 | Luces y rendimiento | Rafael | 1 | A3 | Para probar | polish/look | Sombras en tiempo real: de 5 luces a 1. **Pendiente:** build WebGL y prueba en la laptop más débil (no se hizo: cambiar a WebGL reimporta todo y toca ProjectSettings con cambios locales de Rafael) |
| A5 | Capturas de después | Rafael | 1 | A4 | Para probar | polish/look | Comparación y valores finales en `notas/A5_comparacion.md`; decisiones tomadas sin aprobación en su sección final |
| C1 | Auditoría del jefe | Grover | 1 | S2 | Pendiente | polish/jefe | |
| C2 | Dificultad por espacio en datos | Grover | 1 | C1 | Pendiente | polish/jefe | |
| C3 | Avisos antes del ataque | Grover | 1 | C2 | Pendiente | polish/jefe | Necesita `boss_warn` de D1 |
| C4 | Escondites justos | Grover | 1 | C1 | Pendiente | polish/jefe | |
| C5 | Muerte y reinicio rápido | Grover | 1 | C1 | Pendiente | polish/jefe | |
| C6 | Navegación y atascos | Grover | 1 | C1 | Pendiente | polish/jefe | |
| D1 | Lista de sonidos que faltan | Alan | 1 | S2 | Pendiente | polish/audio | |
| D2 | Mixer y volumen por grupo | Alan | 1 | D1 | Pendiente | polish/audio | |
| D3 | Todo pasa por el mixer | Alan | 1 | D2 | Pendiente | polish/audio | |
| D4 | Snapshots según lo que pasa | Alan | 1 | D3 | Pendiente | polish/audio | |
| D5 | Ambiente por capas y reverb | Alan | 1 | D3 | Pendiente | polish/audio | |
| D6 | Pasos según el suelo | Alan | 1 | D3 | Pendiente | polish/audio | |
| T3 | Puerta 1 | Cristhian | 1 | A5, C6, D6 | Pendiente | main | |
| E1 | Menú principal | Rafael | 2 | Puerta 1 | Pendiente | polish/inicio | |
| E2 | Opciones completas | Rafael | 2 | A2, D2 | Pendiente | polish/inicio | |
| E3 | Intro, tienda y primer susto | Rafael | 2 | Puerta 1 | Pendiente | polish/inicio | |
| E4 | Juice de ventas y feedback | Rafael | 2 | Puerta 1 | Pendiente | polish/inicio | |
| E5 | Subtítulos | Rafael | 2 | E2 | Pendiente | polish/inicio | |
| E6 | Español e inglés | Rafael | 2 | E2, E3, E5 | Pendiente | polish/inicio | |
| E7 | Final y créditos | Rafael | 2 | E6 | Pendiente | polish/inicio | |
| B1 | Kit de materiales | Grover | 2 | Puerta 1 | Para probar | polish/niveles | 14 materiales `Kit_*` (URP Lit) y 9 `Kit_Decal_*` (Unlit transparente, para quads) en `03_Materials/Kit/`. Texturas puestas en los materiales grises de los 3 espacios y en los prefabs `Kit_*`. Falta `Bloque_Pintado` (se usó yeso). Sin usar todavía: alfombra, palet, madera pintada, goma, inox (para B2–B4). Capturas `capturas/despues/B1_*.png` |
| B2 | Vestir el almacén | Grover | 2 | B1 | Pendiente | polish/niveles | Etiquetas de suelo de D6 |
| B3 | Vestir las piscinas | Grover | 2 | B1 | Pendiente | polish/niveles | |
| B4 | Vestir el parque | Grover | 2 | B1 | Pendiente | polish/niveles | |
| B5 | Rendimiento de los niveles | Grover | 2 | B2, B3, B4 | Pendiente | polish/niveles | |
| F1 | Sistema base de anomalías | Alan | 2 | Puerta 1 | Pendiente | polish/anomalias | Opcional |
| F2 | Entre 6 y 8 anomalías | Alan | 2 | F1 | Pendiente | polish/anomalias | Opcional |
| F3 | Notar la anomalía | Alan | 2 | F2 | Pendiente | polish/anomalias | Opcional |
| T4 | Playtest con 5 personas (Puerta 2) | Cristhian | 2–3 | E7, B5 | Pendiente | main | |
| G1 | WebGL liviano | Rafael | 3 | Puerta 2 | Pendiente | polish/release | |
| G2 | Build de Windows y script | Rafael | 3 | G1 | Pendiente | polish/release | |
| G3 | Página de itch | Rafael | 3 | Puerta 2 | Pendiente | polish/release | |
| G4 | Tráiler y clips | Rafael | 3 | G3 | Pendiente | polish/release | |
| T5 | Prueba final (Puerta 3) | Cristhian | 3 | G2 | Pendiente | main | |
| G5 | Versión 1.0 | Rafael | 3 | T5 | Pendiente | main | |

## Pedidos entre bloques

Cuando una tarea necesita algo en un archivo o escena de otro dueño, se anota aquí.

| De | Para | Qué | Estado |
| --- | --- | --- | --- |
| C3 (Grover) | D1 (Alan) | Sonido `boss_warn` en el `SfxBank` | Pendiente |
| D6 (Alan) | B2–B4 (Grover) | Etiqueta o capa de cada tipo de suelo | Pendiente |
| D2 (Alan) | E2 (Rafael) | API `AudioMix` para el menú de opciones | Pendiente |
| A4 (Rafael) | B4 (Grover) | Postes de farol en el parque bajo las luces `Luces_Faroles/LuzFarol_*` (hoy la luz no tiene modelo) | Pendiente |
| A4 (Rafael) | G1 (Rafael) | Nivel de calidad para WebGL: 1 cascada, sombra dura 1024, sin sombras de luces extra, escala 0.85 (no se tocó `PC_RPAsset` por los cambios locales) | Pendiente |
| A3/A4 (Rafael) | Grover | Aviso: en `polish/look` cambié niebla, luz ambiente, Volumes y luces de 10/11/12 (nada de geometría) y `AmbienteTools.cs`. Hacer merge de `main` antes de tocar esas escenas en `polish/niveles` | Pendiente |

## Checklists de prueba

<!-- Las IAs agregan aquí la checklist de cada tarea al dejarla "Para probar" (skill potato-playtest). -->

### Prueba de A1 — Auditoría visual
Rama: polish/look  ·  Build: ninguna (es un informe)

Preparar: abrir `docs/pulido/notas/A1_auditoria_visual.md` en GitHub o VS Code (vista previa de Markdown).

- [ ] Abrir el informe → debería mostrar seis capturas de "antes" (menú, tienda, almacén, piscinas, parque, final) que se ven completas, no negras
- [ ] Jugar la tienda y un espacio en el editor → debería coincidir con lo que dicen las capturas (sin grano ni viñeta en juego, el parque con cielo azul de día)
- [ ] Leer la sección 3 (propuesta) → cada espacio tiene un color, niebla y luces descritos; anotar en BUGS.md o hablar con Rafael si algo no convence

Riesgos: ninguno, no cambia el juego.

### Prueba de A2 — Look de cámara vieja
Rama: polish/look  ·  Build: editor (y WebGL cuando se haga la build)

Preparar: abrir `00_Boot` y dar Play. F2 salta de espacio, F3 reinicia, **F4 cambia la intensidad del look (1 → 0.5 → 0)** (solo en editor y builds de desarrollo, hasta que E2 agregue el control en opciones).

- [ ] Jugar hasta la tienda → debería verse grano fino, bordes un poco más oscuros, una leve curvatura en los bordes y un poco de color separado en las esquinas, como video casero
- [ ] Pasar a un espacio con F2 → el mismo look sigue activo (no desaparece al cambiar de escena)
- [ ] Apretar F4 dos veces (look en 0) → la imagen queda limpia: sin grano, sin curvatura, sin viñeta del look. En la consola aparece "RetroLook = 0"
- [ ] Apretar F4 otra vez → vuelve el look completo al instante
- [ ] Hacer que el jefe te persiga → la viñeta roja de persecución se sigue viendo encima del look
- [ ] Build WebGL: recorrer un espacio 1 minuto → no hay tirones nuevos comparado con la build anterior
- [ ] No hay errores en la consola (los de "Pipeline" o "editor status" vienen del plugin de Unity, no del juego)

Riesgos: ahora el post-proceso está encendido en la cámara del jugador, así que los perfiles `Ambiente_*` de cada espacio empiezan a verse (antes no se veían). Si algo se ve muy distinto en un espacio, es eso; se ajusta en A3.

### Prueba de A3 — Color de cada espacio
Rama: polish/look  ·  Build: editor

Preparar: abrir `00_Boot` y dar Play. F2 salta al siguiente espacio.

- [ ] En la tienda → la luz se ve cálida, tirando a naranja de foco; se siente un lugar seguro
- [ ] Almacén → todo tira a amarillo verdoso enfermo, con niebla amarillenta al fondo de los pasillos
- [ ] Piscinas → cian frío y claro, con niebla celeste
- [ ] Parque → cielo de atardecer apagado (violeta arriba, ámbar en el horizonte, sin sol visible) y todo en violeta y gris; ya no se ve el cielo azul de día
- [ ] Entrar a la tienda réplica al final de cualquier espacio → el color cambia suave a uno casi neutro y un poco frío, distinto del espacio de afuera
- [ ] Con F4 en 0 → cada espacio sigue teniendo su color (solo se va el grano, la viñeta y la curvatura)
- [ ] Poner las capturas de los tres espacios lado a lado → se distinguen a primera vista y se ven del mismo juego

Riesgos: la niebla del almacén y del parque es menos densa que antes; mirar si el jefe se ve demasiado de lejos.

### Prueba de A4 — Luces y rendimiento
Rama: polish/look  ·  Build: editor y WebGL

Preparar: abrir `00_Boot` y dar Play. F2 salta de espacio.

- [ ] Almacén: caminar 1 minuto mirando el techo → la mayoría de los tubos está fija, varios tiemblan apenas, algunos parpadean en ráfagas cada tanto y unos pocos están casi siempre apagados y se prenden a ratos
- [ ] Almacén → la luz general es amarilla y pareja, no hay pasillos negros
- [ ] Piscinas → las paredes junto al agua tienen un brillo cian que sube desde la piscina, y no se ve ningún foco ni punto brillante sobre el agua
- [ ] Parque → el sol no se ve pero ilumina de costado en ámbar con sombras largas; en el camino hay manchas de luz cálida de farol y una de ellas se apaga y prende sola
- [ ] Tienda → luz cálida pareja; una de las fluorescentes parpadea de vez en cuando
- [ ] Build WebGL en la laptop más débil del equipo: recorrer el almacén (el espacio con más luces) 2 minutos → no hay tirones ni caídas fuertes de fluidez; anotar FPS aproximados si se puede
- [ ] No hay errores en la consola

Riesgos: la luz del almacén ahora sale casi toda del ambiente; si el jefe se ve demasiado de lejos o demasiado poco, avisar.

### Prueba de A5 — Antes y después
Rama: polish/look  ·  Build: ninguna (documento) + editor para comparar

Preparar: abrir `docs/pulido/notas/A5_comparacion.md` con vista previa de Markdown.

- [ ] Ver la tabla de comparación → cada espacio de "después" se distingue de los otros a primera vista (cálido, amarillo, cian, violeta)
- [ ] Jugar desde `00_Boot` hasta un espacio → lo que se ve en juego coincide con las capturas de "después"
- [ ] Leer "Decisiones tomadas sin aprobación" → Rafael y el equipo marcan cuáles aprueban; lo que no, se anota en BUGS.md como cambio pedido para el bloque A
- [ ] Revisar "Pendiente" → la build WebGL queda por hacer; probarla antes de cerrar la Puerta 1

Riesgos: ninguno nuevo, es un documento.

### Prueba de B1 — Kit de materiales
Rama: polish/niveles  ·  Build: editor (y WebGL para el último punto)

Preparar: abrir `00_Boot`, dar Play y apretar F2 para entrar al primer espacio; F2 de nuevo pasa al siguiente. Comparar con `capturas/despues/B1_almacen.png`, `B1_piscinas.png` y `B1_parque.png`.

- [ ] Almacén: mirar piso, paredes y techo → el piso es concreto con piedritas, las paredes y el laberinto son de yeso amarillento con manchas; no queda ninguna superficie gris lisa
- [ ] Almacén: acercarse a una góndola, el mostrador y las cajas de cartón → la góndola y el mostrador son de metal pintado gastado, las cajas tienen textura de cartón
- [ ] Piscinas: mirar paredes, piso y el fondo del agua → paredes y columnas de azulejo celeste chico, piso de losas beige, techo claro
- [ ] Parque: mirar el suelo, el camino de la entrada y la cerca → el suelo tiene textura de pasto seco, el camino es de tierra y la cerca es de metal
- [ ] En los tres espacios, caminar pegado a una pared larga → la textura mantiene el mismo tamaño, sin estirarse ni cortarse en franjas
- [ ] Tienda réplica al final de cada espacio → ahora también tiene piso de concreto y góndolas de metal; anotar si se ve demasiado distinta de la tienda real
- [ ] Build WebGL: entrar a cada espacio → carga sin trabarse más que antes y las texturas no se ven borrosas ni con cuadros
- [ ] No hay errores en la consola (solo en el editor)

Riesgos: los prefabs `Kit_*` también se usan en `_Sandbox/Bullain` y `_Sandbox/Cristhian`, que van a cambiar igual. Los bloques del laberinto pasaron de marrón oscuro a amarillo: el almacén se ve más claro que antes. El pasto del parque tiene un tinte cálido; si con la luz de A3 se ve muy oscuro, subir el color de `Kit_Pasto_Seco`.
