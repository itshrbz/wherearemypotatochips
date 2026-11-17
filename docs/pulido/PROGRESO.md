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
| C1 | Auditoría del jefe | Grover | 1 | S2 | Para probar | polish/boss | Informe en `notas/C1_jefe.md` (hecho leyendo código, sin jugar). Grover aprobó el orden C2 → C6. Curva elegida: por posición en la partida (opción B) |
| C2 | Dificultad por espacio en datos | Grover | 1 | C1 | Para probar | polish/boss | Assets `Boss_1_Ensena`, `Boss_2_Presiona`, `Boss_3_Aprieta` en `07_data/Boss/`, asignados al prefab. Probado en Play (por script): 2.º espacio 2.6/3.9, 3.º 2.85/4.1; el 1.º no tiene jefe. Sin probar a mano: escapar corriendo, el respiro de 4 s |
| C3 | Avisos antes del ataque | Grover | 1 | C2 | Para probar | polish/boss | Probado por script: al descubrirte se queda quieto 0.6 s y después corre a 4.29 (< 4.5 del jugador); las luces a menos de 9 m fallan y las lejanas no. Sin probar: destello y sonido (falta `boss_warn`/`boss_alert` de D1) |
| C4 | Escondites justos | Grover | 1 | C1 | Para probar | polish/boss | Probado por script: escondido a 6 m no te ve (sospecha 0); si te ve esconderte, va al escondite y te saca. Sin probar: oír a través de paredes, respiración (Alan) |
| C5 | Muerte y reinicio rápido | Grover | 1 | C1 | Para probar | polish/boss | Probado: susto → negro con frase ("It was right behind you.", "Again.") → de vuelta en ~2 s, con la dificultad intacta |
| C6 | Navegación y atascos | Grover | 1 | C1 | Para probar | polish/boss | Probado por script: trepa la plataforma del almacén y bloques de 2 m y te alcanza; no atrapa a través de `Kit_Pared`. Arreglado: arriba de algo inalcanzable (bloque del laberinto, pared de piscinas) no se recolocaba; ahora `CheckUnreachable` lo recoloca a los 4 s sin ruta hasta el jugador (lejos y, si se puede, donde no lo ves). Falta la prueba de 10 min por espacio |
| D1 | Lista de sonidos que faltan | Alan | 1 | S2 | Pendiente | polish/audio | |
| D2 | Mixer y volumen por grupo | Alan | 1 | D1 | Pendiente | polish/audio | |
| D3 | Todo pasa por el mixer | Alan | 1 | D2 | Pendiente | polish/audio | |
| D4 | Snapshots según lo que pasa | Alan | 1 | D3 | Pendiente | polish/audio | |
| D5 | Ambiente por capas y reverb | Alan | 1 | D3 | Pendiente | polish/audio | |
| D6 | Pasos según el suelo | Alan | 1 | D3 | Pendiente | polish/audio | |
| T3 | Puerta 1 | Cristhian | 1 | A5, C6, D6 | Pendiente | main | |
| E1 | Menú principal | Rafael | 2 | Puerta 1 | Para probar | polish/noche | Adelantada antes de la Puerta 1. Fondo: pasillo del almacén en bucle de 8 m hecho con el kit modular (`Fondo_Menu`), 9 luces sin sombra, niebla amarilla y perfil `Ambiente_10_Warehouse`. Botones en inglés como el resto de la UI (la traducción va en E6). Probado con teclado y mando simulados; el mouse simulado falla en la parte baja de la vista del editor (Back, Quit), con clic directo funcionan: **probar con mouse real** |
| E2 | Opciones completas | Rafael | 2 | A2, D2 | Pendiente | polish/inicio | |
| E3 | Intro, tienda y primer susto | Rafael | 2 | Puerta 1 | Para probar | polish/noche | Intro, `StoreLife` y `StoreScare` (susto con la cola de compradores). La calle vista desde la tienda pasó a la 1.1. Faltan `scare_sting` y `car_pass` en el SfxBank (pedido a Alan) |
| E4 | Juice de ventas y feedback | Rafael | 2 | Puerta 1 | Para probar | polish/noche | Versión corta: ticket que se imprime de la caja (`ReceiptTicket`), `CameraEffects.Shake` y `cash_register` del SfxBank al cobrar |
| E5 | Subtítulos | Rafael | 2 | E2 | Pendiente | polish/inicio | |
| E6 | Español e inglés | Rafael | 2 | E2, E3, E5 | Pendiente | polish/inicio | |
| E7 | Final y créditos | Rafael | 2 | E6 | Pendiente | polish/inicio | |
| B1 | Kit de materiales | Rafael | 2 | Puerta 1 | Para probar | polish/materiales | Lo tomó Rafael para adelantarle trabajo a Grover. 14 materiales en `03_Materials/Kit/`, 11 calcomanías con prefab en `02_Prefabs/Calcomanias/`, créditos en `CREDITS.md`. Decisiones en `notas/B1_kit_materiales.md` (sección "Cómo quedó"). Sin escena de prueba: no se tocaron escenas |
| B2 | Vestir el almacén | Grover | 2 | B1 | Para probar | polish/noche | Kit aplicado (paredes con `Kit_PapelAmarillo`, Yeso teñido de amarillo; alfombra, techo de placas), 16 estantes, 12 pilas de cajas, extintores, ~50 calcomanías y 4 detalles mal. Suelo con tag `Suelo_Alfombra` |
| B3 | Vestir las piscinas | Grover | 2 | B1 | Para probar | polish/noche | Azulejo blanco y de piscina, yeso, 10 escaleras de piscina, bancos, silla de salvavidas, extintores, carteles y 4 detalles mal. Suelo con tag `Suelo_Azulejo` |
| B4 | Vestir el parque | Grover | 2 | B1 | Para probar | polish/noche | Pasto seco, tierra en el camino, cerca de metal, postes bajo los 6 faroles, 10 bancos, 6 basureros, carteles y 5 detalles mal. Tags `Suelo_Pasto` y `Suelo_Tierra` |
| B5 | Rendimiento de los niveles | Grover | 2 | B2, B3, B4 | Pendiente | polish/niveles | |
| F1 | Sistema base de anomalías | Alan | 2 | Puerta 1 | Pendiente | polish/anomalias | Opcional |
| F2 | Entre 6 y 8 anomalías | Alan | 2 | F1 | Pendiente | polish/anomalias | Opcional |
| F3 | Notar la anomalía | Alan | 2 | F2 | Pendiente | polish/anomalias | Opcional |
| T4 | Playtest con 5 personas (Puerta 2) | Cristhian | 2–3 | E7, B5 | Pendiente | main | |
| G1 | WebGL liviano | Rafael | 3 | Puerta 2 | Pendiente | polish/release | |
| G2 | Build de Windows y script | Rafael | 3 | G1 | Pendiente | polish/release | |
| G3 | Página de itch | Rafael | 3 | Puerta 2 | Para probar | polish/itch | Adelantada antes de la Puerta 2 (solo texto). Rama `polish/itch` en vez de `polish/release` para no mezclarla con G1/G2. Lo que depende de tareas pendientes está entre `[corchetes]` en `docs/itch/` |
| G4 | Tráiler y clips | Rafael | 3 | G3 | Pendiente | polish/release | |
| T5 | Prueba final (Puerta 3) | Cristhian | 3 | G2 | Pendiente | main | |
| G5 | Versión 1.0 | Rafael | 3 | T5 | Pendiente | main | |

## Pedidos entre bloques

Están separados por persona en [detallesfaltantes/](detallesfaltantes/): [ALAN.md](detallesfaltantes/ALAN.md), [RAFAEL.md](detallesfaltantes/RAFAEL.md) y [GROVER.md](detallesfaltantes/GROVER.md). Cuando una tarea necesita algo en un archivo o escena de otro dueño, se anota en el archivo de esa persona.

Pedidos anteriores a la separación por persona:

| De | Para | Qué | Estado |
| --- | --- | --- | --- |
| C3 (Grover) | D1 (Alan) | Sonido `boss_warn` en el `SfxBank` | Pendiente |
| D6 (Alan) | B2–B4 (Grover) | Etiqueta o capa de cada tipo de suelo | Hecho (polish/noche) |
| D2 (Alan) | E2 (Rafael) | API `AudioMix` para el menú de opciones | Pendiente |
| A4 (Rafael) | B4 (Grover) | Postes de farol en el parque bajo las luces `Luces_Faroles/LuzFarol_*` (hoy la luz no tiene modelo) | Hecho (polish/noche) |
| A4 (Rafael) | G1 (Rafael) | Nivel de calidad para WebGL: 1 cascada, sombra dura 1024, sin sombras de luces extra, escala 0.85 (no se tocó `PC_RPAsset` por los cambios locales) | Pendiente |
| B1 (Rafael) | B2–B4 (Grover) | Usar el kit: materiales de `03_Materials/Kit/` y calcomanías de `02_Prefabs/Calcomanias/` en las escenas de cada espacio (B1 no tocó escenas) | Hecho (polish/noche) |
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

### Prueba de C1 — Auditoría del jefe
Rama: polish/boss  ·  Build: ninguna (es un informe) + editor para confirmar lo "probable"

Preparar: abrir `docs/pulido/notas/C1_jefe.md` con vista previa de Markdown. Para confirmar, abrir `00_Boot`, dar Play y usar F2 para llegar a un espacio con jefe.

- [ ] Dejar que el jefe te vea → **no suena ningún aviso** y en la consola aparece "Sfx: el id 'boss_alert' no existe" (P1)
- [ ] Acercarte al jefe sin que te vea → no se oye respiración, solo pasos (P2)
- [ ] Que te persiga y correr en línea recta → aunque cortes la vista, sigue detrás mientras corras cerca (P4)
- [ ] Jugar el tercer espacio de la partida y que te persiga → no se puede escapar corriendo (P3)
- [ ] Dejar que te atrape y cronometrar desde el susto hasta poder moverte → unos 4 s (P11)
- [ ] Almacén: subirte a un bloque alto mientras te persigue → el jefe se queda abajo (P9)
- [ ] Anotar en el informe o en BUGS.md lo que no coincida

Riesgos: ninguno, no cambia el juego.

### Prueba de C2 — Dificultad del jefe en datos
Rama: polish/boss  ·  Build: editor

Preparar (una sola vez, en el editor):
1. Abrir Unity y esperar a que compile. La consola no debe tener errores.
2. Crear la carpeta `Assets/07_data/Boss/`. Dentro, clic derecho → Create → Potato Chips → Boss Tuning, tres veces: `Boss_1_Ensena`, `Boss_2_Presiona`, `Boss_3_Aprieta`.
3. Poner estos valores (el jugador camina a 2.5 y corre a 4.5):

| Campo | Boss_1_Ensena | Boss_2_Presiona | Boss_3_Aprieta |
| --- | --- | --- | --- |
| Patrol Speed | 2.3 | 2.6 | 2.85 |
| Chase Speed | 3.6 | 3.9 | 4.1 |
| Burst Multiplier | 1.1 | 1.1 | 1.08 |
| Sight Range | 16 | 18 | 20 |
| Instant Sight Range | 3.5 | 4.5 | 5 |
| Hearing Range | 6 | 8 | 9 |
| Awareness Gain | 1.0 | 1.3 | 1.5 |
| Search Time | 5 | 6 | 7 |
| Hunt Chance | 0.3 | 0.45 | 0.6 |
| Chase Cooldown | 6 | 4 | 2 |

4. Abrir `02_Prefabs/Enemy/Boss.prefab`, en `BossAI` → `Tuning By Level` poner tamaño 3 y arrastrar los tres assets en ese orden. Guardar el prefab.

Hoy el primer espacio de la partida no tiene jefe (`LevelContext.BossEnabled`), así que `Boss_1_Ensena` solo se usa si eso cambia.

- [ ] Play desde `00_Boot`, F2 dos veces (segundo espacio) → seleccionar el jefe en la jerarquía: `Patrol Speed` vale 2.6 y `Chase Speed` 3.9
- [ ] F2 otra vez (tercer espacio) → `Patrol Speed` 2.85 y `Chase Speed` 4.1
- [ ] Que te persiga en el segundo espacio y correr en línea recta → el jefe no te alcanza mientras tengas energía
- [ ] Escapar (esconderte o cortar la vista 3 s) y volver a aparecer frente a él a más de 5 m → durante unos 4 s solo camina hacia ti (investiga), no corre; después sí persigue
- [ ] Cambiar un valor de un asset con el juego parado y volver a jugar → el cambio se nota (todo se ajusta desde el Inspector)
- [ ] Quitar los assets del prefab (tamaño 0) → el jefe usa los valores del prefab y no hay errores
- [ ] No hay errores nuevos en la consola

Riesgos: sin los assets asignados, el tercer espacio pierde el ×1.25 de antes y queda igual de difícil que el segundo.

### Prueba de C3 — Avisos antes del ataque
Rama: polish/boss  ·  Build: editor

Preparar: abrir `00_Boot`, dar Play y F2 dos veces para llegar a un espacio con jefe. Hasta que Alan agregue `boss_warn` o `boss_alert` al `SfxBank`, el sonido de aviso no suena (solo sale un warning en la consola).

- [ ] Acercarte al jefe sin que te vea, en el almacén o en la tienda réplica → las luces a unos 9 m de él empiezan a fallar, más cuanto más cerca está; al alejarse vuelven a la normalidad
- [ ] Dejar que te descubra → se frena, gira hacia ti, la pantalla da un destello rojo corto y (con el sonido ya puesto) suena el aviso; recién después corre
- [ ] Que te descubra a menos de 2 m → igual tienes medio segundo antes de que te pueda atrapar
- [ ] Correr cerca de él con una pared en medio → no hay destello: desde C4 oírte solo lo hace ir a mirar (suena su alerta, en 3D)
- [ ] Parque (las luces de farol) y piscinas → las luces que fallan no dejan el lugar a oscuras ni parpadean como estroboscopio
- [ ] No hay errores nuevos en la consola

Riesgos: luces apagándose rápido cerca del jefe; si molesta a alguien sensible a los destellos, subir `Min Interval` en `BossLightDisturbance` o bajar `Max Off Chance`.

### Prueba de C4 — Escondites justos
Rama: polish/boss  ·  Build: editor

Preparar: abrir `00_Boot`, dar Play y F2 dos veces para llegar a un espacio con jefe.

- [ ] Correr cerca del jefe sin que te vea → va a mirar hacia donde estabas (camina, no corre) y suena su alerta, pero no te persigue
- [ ] Correr a unos 6–8 m del jefe con una pared en medio → no te oye; a menos de 4 m sí
- [ ] Que te persiga, cortarle la vista y seguir corriendo → a los 3 s deja de perseguirte y te busca (antes seguía detrás mientras corrieras)
- [ ] Esconderte (agachado en un escondite) sin que te vea → el jefe puede pasar cerca o revisar el escondite, pero no te saca
- [ ] Esconderte mientras te está viendo → suena su alerta en ese momento, va directo al escondite y te saca
- [ ] Parque (51 escondites): caminar por varios escondites seguidos agachado → el texto "HIDDEN" no parpadea ni desaparece al pasar de uno a otro
- [ ] Piscinas: los escondites generados (cajas de cartón) → las cajas ya no se atraviesan, y igual cabes agachado delante de ellas
- [ ] Piscinas: recorrer el espacio → hay escondites en lugares útiles (rincones cerca de los caminos, no todos juntos ni en callejones sin salida)
- [ ] No hay errores nuevos en la consola

Riesgos: el jefe es más fácil de esquivar corriendo que antes; si queda demasiado fácil, subir `Hearing Range` en los `BossTuning`.

### Prueba de C5 — Muerte y reinicio rápido
Rama: polish/boss  ·  Build: editor

Preparar: abrir `00_Boot`, dar Play y F2 dos veces para llegar a un espacio con jefe.

- [ ] Dejar que te atrape → el susto (la cámara gira hacia su cara, zoom y temblor) se ve igual que antes
- [ ] Cronometrar desde que termina el susto hasta poder moverte de nuevo → menos de 3 s, con pantalla negra, una frase corta en el centro y fundido (sin párpados)
- [ ] Morir 4 o 5 veces → las frases cambian y no sale la misma dos veces seguidas
- [ ] Esconderte mientras te está viendo y dejar que te saque → la frase es "It saw you hide."
- [ ] F3 (reiniciar en el editor) y pasar de espacio con F2 → siguen con el despertar de párpados de siempre
- [ ] Las frases se pueden cambiar en `Boss.prefab` → `BossAI` → `Retry Lines`
- [ ] No hay errores nuevos en la consola

Riesgos: las frases están en inglés como el resto del juego; E6 (idiomas) tendrá que traducirlas.


### Prueba de B1 — Kit de materiales
Rama: polish/materiales  ·  Build: editor

Preparar: abrir `_Sandbox` (o cualquier escena) **sin guardar** al salir. En el Project, ir a `Assets/03_Materials/Kit/`.

- [ ] Ver la carpeta `Kit` en modo miniatura → hay 14 materiales con textura real (azulejo blanco, azulejo de piscina, alfombra, papel tapiz, techo de placas, concreto, pasto seco, metal verde, metal amarillo, piso de tienda, yeso, asfalto, madera pintada, tierra), ninguno rosado ni gris liso
- [ ] Crear un cubo y arrastrarle `Kit_AzulejoBlanco` → se ven las juntas del azulejo con relieve al mover la luz o la cámara
- [ ] Arrastrar `Assets/02_Prefabs/Calcomanias/Calco_Salida` delante de una pared → se ve el cartel verde EXIT con fondo transparente alrededor de las esquinas redondeadas, sin cuadro negro
- [ ] Arrastrar `Calco_Humedad1` sobre una pared clara → mancha marrón que gotea desde arriba y se desvanece hacia abajo y a los lados, sin borde cuadrado
- [ ] Revisar `Calco_NoCorrer`, `Calco_SoloEmpleados` y `Calco_PisoMojado` → todo el texto cabe dentro del cartel
- [ ] Abrir `CREDITS.md` → hay una tabla "Texturas del kit de materiales" con cada textura, su autor y licencia CC0
- [ ] No hay errores en la consola

Riesgos: ninguno en juego (no se tocó ninguna escena). Las texturas suman unos 12 MB al proyecto; en la build solo entran las que Grover use.

### Prueba de E1 — Menú principal
Rama: polish/inicio  ·  Build: editor (y WebGL cuando se haga)

Preparar: abrir `00_Boot` y dar Play.

- [ ] Esperar en el menú 20 segundos → la cámara avanza despacio por un pasillo amarillo sin fin, con un leve balanceo; nunca se ve un salto ni el final del pasillo
- [ ] Mirar las esquinas → arriba a la izquierda "PLAY" parpadea; abajo a la derecha corre un reloj desde AM 3:07 con la fecha NOV. 14 1997
- [ ] Esperar unos segundos mirando el título → de vez en cuando parpadea como un letrero viejo
- [ ] Con el **mouse**, entrar a Settings, Controls y Credits y volver con su botón Back → cada pantalla abre y vuelve al menú; probar también Quit (en el editor detiene el Play)
- [ ] Con el **teclado**: flechas o W/S para moverse, Enter para entrar, Esc para volver → siempre hay un botón resaltado y Esc vuelve al menú desde cualquier pantalla
- [ ] Con un **mando**: cruceta o stick para moverse, A para entrar, B para volver → funciona igual que el teclado
- [ ] En Settings, apagar "Camera effects" y volver → el pasillo sigue avanzando pero sin balanceo
- [ ] Apretar Play → aparece la historia y luego la tienda. En la build WebGL el botón Quit no aparece
- [ ] No hay errores en la consola (los de "editor status" o "api/exec" son del plugin de Unity)

Riesgos: la escena `01_Menu` ahora tiene 117 piezas del kit y 9 luces; si el menú va lento en la laptop más débil, avisar.

### Prueba de G3 — Página de itch
Rama: polish/itch  ·  Build: ninguna (textos)

Preparar: abrir `docs/itch/LEEME.md` con vista previa de Markdown.

- [ ] Leer `pagina_es.md` y `page_en.md` → dicen lo mismo en los dos idiomas, sin errores de ortografía, y la historia coincide con la intro del juego
- [ ] Comparar la tabla de controles con el juego → cada tecla hace lo que dice (las celdas del mando entre corchetes todavía no funcionan, es un bug conocido)
- [ ] Leer `capturas_y_gifs.md` → cada captura dice escena y momento; probar a sacar la 1 (tienda con el primer cliente) y ver que se puede seguir la instrucción
- [ ] Revisar los créditos con cada persona del equipo → el rol de cada uno es correcto
- [ ] Revisar la lista "Antes de publicar, confirmar" de `LEEME.md` → no falta nada que el equipo sepa

Riesgos: ninguno, no cambia el juego.

## Prueba de la noche · polish/noche

Una sola checklist para B1–B4, C1–C6, E1, E3, E4 y B-002. Rama: `polish/noche` · Build: editor.

Preparar: hacer checkout de `polish/noche`, abrir `00_Boot` y dar Play. Tener un mando conectado para la parte del final. Capturas de referencia en `capturas/noche/` (almacén, piscinas, parque).

- [ ] Menú: se ve el pasillo amarillo en bucle y PLAY funciona con mouse real (E1)
- [ ] Intro y tienda: tres líneas de intro; en cada cobro se oye la caja, la cámara da un golpe corto y sale un ticket de papel de la caja que se arranca solo (E4)
- [ ] Tienda: el tercer comprador pide papas y llega el susto; la puerta del almacén se abre (E3). En silencio en ese momento es normal: falta `scare_sting`
- [ ] Primer espacio (el que toque): sin jefe, solo tutorial
- [ ] En cada espacio: ninguna pared, suelo ni techo gris; hay estantes, bancos, escaleras, carteles y manchas (B2–B4)
- [ ] Almacén: el cartel "pasillo 7" aparece dos veces seguidas, hay una puerta pegada a la pared, un estante boca abajo en el techo y tres afiches de papas en fila
- [ ] Piscinas: una escalera que sube hasta el techo, un tobogán que entra a la pared, un banco dentro del agua y un cartel de "no correr" al revés
- [ ] Parque: cada farol tiene poste; hay un banco gigante, uno mirando la cerca, un cartel EXIT en un árbol y un farol del tamaño de un niño
- [ ] Ningún mueble nuevo bloquea un pasillo ni te deja atascado; no te caes del mapa
- [ ] Jefe: usa los ajustes de Grover por posición (`Boss_1_Ensena`, `Boss_2_Presiona`, `Boss_3_Aprieta`): el del tercer espacio aprieta más que el del segundo; si te atrapa, reinicio corto con una frase (C2, C5)
- [ ] Jefe: no se queda trabado en estantes, bancos ni escaleras nuevas (C6)
- [ ] Con mando, en cualquier espacio: stick izquierdo mueve, stick derecho mira a una velocidad cómoda, A/Cruz salta, B/Círculo agacha, clic de stick corre (B-002)
- [ ] Llegar a la tienda réplica de los tres espacios: se ve como la tienda real (pared, piso y techo de tienda), no gris
- [ ] Final y vuelta al menú sin errores en la consola

Riesgos: los muebles se colocaron por script; alguno puede quedar flotando o metido en una pared. Las texturas se ven estiradas en la vista de escena y bien en Play (el tiling se aplica al cargar). En la consola sigue el aviso de `boss_alert` (pedido a Alan).

### Prueba de C6 — Navegación y atascos
Rama: polish/boss  ·  Build: editor

Preparar: abrir `00_Boot`, dar Play y F2 dos veces para llegar a un espacio con jefe. El almacén ya tiene 4 saltos hechos a mano en la plataforma central; no hace falta generar más.

- [ ] Almacén: subirte a la plataforma central mientras te persigue → el jefe trepa y te alcanza
- [ ] Ponerte en un lugar al que el jefe no puede llegar (por ejemplo, encima de un obstáculo alto) → a los ~4 s desaparece y vuelve a aparecer a más de 10 m (si puede, donde no lo ves); si vuelve a acercarse, se repite
- [ ] Piscinas: bajar a la canaleta baja junto a la pared (cerca de z 85) y dejar que te persiga → anotar qué hace el jefe (problema conocido: hoy se queda quieto en el borde)
- [ ] Ponerte pegado a una pared fina con el jefe del otro lado → no te atrapa a través de la pared
- [ ] Jugar 10 minutos en cada espacio (almacén, piscinas, parque) dejando que te persiga y te busque → el jefe no se queda trabado contra paredes ni esquinas, y no atraviesa paredes
- [ ] Si se recoloca, nunca aparece delante de ti ni encima de un bloque sin salida
- [ ] No hay errores nuevos en la consola (el warning "Failed to create agent because it is not close enough to the NavMesh" en piscinas ya existía antes)

Riesgos: si te subes a un lugar inalcanzable, el jefe se va y vuelve; es a propósito, pero puede sentirse raro si pasa seguido. Avisar dónde pasa para agregar un salto ahí.

