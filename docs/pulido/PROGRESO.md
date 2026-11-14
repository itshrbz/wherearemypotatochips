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
| A3 | Color de cada espacio | Rafael | 1 | A2 | Pendiente | polish/look | |
| A4 | Luces y rendimiento | Rafael | 1 | A3 | Pendiente | polish/look | |
| A5 | Capturas de después | Rafael | 1 | A4 | Pendiente | polish/look | |
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
| B1 | Kit de materiales | Grover | 2 | Puerta 1 | Pendiente | polish/niveles | |
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
